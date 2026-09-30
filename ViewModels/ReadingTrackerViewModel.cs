using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class ReadingTrackerViewModel : ViewModelBase
    {
        private readonly ISchoolRepository _schools;
        private readonly IAcademicYearRepository _years;
        private readonly ISchoolClassRepository _classes;
        private readonly ILearnerRepository _learners;
        private readonly IReadingAssessmentStore _store;
        private readonly IDialogService _dialogs;
        private School? _school;
        private AcademicYear? _year;
        private SchoolClass? _selectedClass;
        private Learner? _selectedLearner;
        private ReadingAssessment? _selectedRecord;
        private int _classVersion;
        private int _recordVersion;
        private string _recordId = Guid.NewGuid().ToString("N");
        private DateTime? _assessmentDate = DateTime.Today;
        private string _assessmentTool = "";
        private string _passageOrMaterial = "";
        private int? _wordsCorrect;
        private int? _wordsAttempted;
        private int? _comprehensionCorrect;
        private int? _comprehensionTotal;
        private string _readingLevel = "";
        private string _intervention = "";
        private string _notes = "";

        public ReadingTrackerViewModel(ISchoolRepository schools, IAcademicYearRepository years,
            ISchoolClassRepository classes, ILearnerRepository learners,
            IReadingAssessmentStore store, IDialogService dialogs)
        {
            _schools = schools; _years = years; _classes = classes;
            _learners = learners; _store = store; _dialogs = dialogs;
            Classes = new(); Learners = new(); Records = new();
            _ = LoadAsync();
        }

        public ObservableCollection<SchoolClass> Classes { get; }
        public ObservableCollection<Learner> Learners { get; }
        public ObservableCollection<ReadingAssessment> Records { get; }
        public ObservableCollection<string> SuggestedTools { get; } = new();
        public ObservableCollection<string> SuggestedLevels { get; } = new();
        public string GradeGuide { get; private set; } = "Pumili ng class upang makita ang assessment guide.";
        public string SchoolYearText => _year?.DisplayName ?? "No active school year";
        public SchoolClass? SelectedClass
        {
            get => _selectedClass;
            set { if (SetProperty(ref _selectedClass, value)) { ConfigureGrade(value?.GradeLevel); _ = LoadLearnersAsync(value); } }
        }
        public Learner? SelectedLearner
        {
            get => _selectedLearner;
            set { if (SetProperty(ref _selectedLearner, value)) _ = LoadRecordsAsync(value); }
        }
        public ReadingAssessment? SelectedRecord
        {
            get => _selectedRecord;
            set { if (SetProperty(ref _selectedRecord, value) && value != null) LoadEditor(value); }
        }
        public DateTime? AssessmentDate { get => _assessmentDate; set => SetProperty(ref _assessmentDate, value); }
        public string AssessmentTool { get => _assessmentTool; set => SetProperty(ref _assessmentTool, value); }
        public string PassageOrMaterial { get => _passageOrMaterial; set => SetProperty(ref _passageOrMaterial, value); }
        public int? WordsCorrect { get => _wordsCorrect; set { if (SetProperty(ref _wordsCorrect, value)) OnPropertyChanged(nameof(PreviewAccuracy)); } }
        public int? WordsAttempted { get => _wordsAttempted; set { if (SetProperty(ref _wordsAttempted, value)) OnPropertyChanged(nameof(PreviewAccuracy)); } }
        public int? ComprehensionCorrect { get => _comprehensionCorrect; set => SetProperty(ref _comprehensionCorrect, value); }
        public int? ComprehensionTotal { get => _comprehensionTotal; set => SetProperty(ref _comprehensionTotal, value); }
        public string ReadingLevel { get => _readingLevel; set => SetProperty(ref _readingLevel, value); }
        public string Intervention { get => _intervention; set => SetProperty(ref _intervention, value); }
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
        public string PreviewAccuracy => WordsAttempted > 0 && WordsCorrect.HasValue
            ? $"{100d * WordsCorrect.Value / WordsAttempted.Value:0.0}%" : "—";
        public int AssessmentCount => Records.Count;
        public string LatestLevel => Records.FirstOrDefault()?.ReadingLevel is { Length: > 0 } level ? level : "—";
        public string ProgressText
        {
            get
            {
                var recent = Records.FirstOrDefault(x => x.Accuracy.HasValue && SameGrade(x));
                if (recent == null) return "Kailangan ng 2 assessment na may word scores.";
                var scored = Records.Where(x => x.Accuracy.HasValue && SameGrade(x) &&
                    string.Equals(x.AssessmentTool, recent.AssessmentTool, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x.AssessmentDate).ThenBy(x => x.UpdatedAtUtc).ToList();
                if (scored.Count < 2) return "Kailangan ng 2 assessment gamit ang parehong tool.";
                double change = scored[^1].Accuracy!.Value - scored[0].Accuracy!.Value;
                return $"{(change > 0 ? "+" : "")}{change:0.0} percentage points ({recent.AssessmentTool})";
            }
        }

        private bool SameGrade(ReadingAssessment record) =>
            string.Equals(record.GradeLevel, SelectedClass?.GradeLevel, StringComparison.OrdinalIgnoreCase);

        private void ConfigureGrade(string? grade)
        {
            SuggestedTools.Clear(); SuggestedLevels.Clear();
            var g = (grade ?? "").Trim().ToLowerInvariant();
            int number = 0;
            var digits = new string(g.Where(char.IsDigit).ToArray());
            int.TryParse(digits, out number);
            string[] tools;
            string[] levels;
            if (g.Contains("kinder"))
            {
                GradeGuide = "Kinder: obserbahan ang pagkilala sa titik at tunog, pakikinig, at pag-unawa sa kuwentong binasa ng guro. Itala ang checklist o bilang ng tamang sagot; hindi kailangan ang word accuracy kung hindi pa nagbabasa ng teksto ang bata.";
                tools = new[] { "Pagkilala sa titik at tunog", "Pakikinig at pag-unawa sa kuwento", "Pagkilala sa salita at larawan" };
                levels = new[] { "Nagsisimula", "Umuunlad", "Nakapagpapakita nang mag-isa" };
            }
            else if (number == 1)
            {
                GradeGuide = "Grade 1: pumili ng angkop na wika at materyal para sa titik/tunog, pantig, salita, maikling pangungusap o kuwento. Bilangin lamang ang mga salitang aktuwal na ipinabasa.";
                tools = new[] { "Tunog, pantig at salita", "Pagbasa ng maikling pangungusap", "Pagbasa at pag-unawa sa maikling kuwento" };
                levels = new[] { "Nagsisimula", "Umuunlad", "Nakapagbabasa nang mag-isa" };
            }
            else if (number is 2 or 3)
            {
                GradeGuide = "Grades 2–3: gumamit ng tekstong akma sa baitang at wika. Itala ang wastong nabasang salita at mga tanong sa pag-unawa kung bahagi ng assessment.";
                tools = new[] { "Pasalitang pagbasa ng tekstong akma sa baitang", "Pag-unawa sa binasang kuwento", "Pagbasa ng pangungusap at talata" };
                levels = new[] { "Nangangailangan ng gabay", "Umuunlad", "Nakapagbabasa nang mag-isa" };
            }
            else if (number is >= 4 and <= 6)
            {
                GradeGuide = "Grades 4–6: pumili ng akmang tekstong pampanitikan o impormatibo. Itala ang katumpakan at pag-unawa batay sa ginamit na materyal.";
                tools = new[] { "Pasalitang pagbasa ng tekstong akma sa baitang", "Pag-unawa sa tekstong impormatibo", "Pag-unawa sa tekstong pampanitikan" };
                levels = new[] { "Nangangailangan ng gabay", "Umuunlad", "Nakapagbabasa nang mag-isa" };
            }
            else if (number is >= 7 and <= 12)
            {
                GradeGuide = "Grades 7–12: gumamit ng tekstong akma sa asignatura at baitang; tasahin ang pag-unawa, paghinuha, at pagsusuri. Optional ang word accuracy kung hindi pasalitang pagbasa ang ginawa.";
                tools = new[] { "Pag-unawa at paghinuha sa teksto", "Pagsusuri ng tekstong impormatibo", "Pagsusuri ng tekstong pampanitikan" };
                levels = new[] { "Nangangailangan ng gabay", "Umuunlad", "Nakapagbabasa nang mag-isa" };
            }
            else
            {
                GradeGuide = "Pumili ng assessment na angkop sa baitang at sa aktuwal na materyal ng learner.";
                tools = new[] { "Reading assessment" }; levels = new[] { "Nangangailangan ng gabay", "Umuunlad", "Nakapagbabasa nang mag-isa" };
            }
            foreach (var tool in tools) SuggestedTools.Add(tool);
            foreach (var level in levels) SuggestedLevels.Add(level);
            OnPropertyChanged(nameof(GradeGuide));
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                _school = await _schools.GetActiveSchoolAsync();
                if (_school == null) { StatusMessage = "I-set up muna ang school."; return; }
                _year = await _years.GetCurrentAsync(_school.Id);
                OnPropertyChanged(nameof(SchoolYearText));
                if (_year == null) { StatusMessage = "Pumili muna ng active school year."; return; }
                foreach (var item in await _classes.GetByAcademicYearAsync(_school.Id, _year.Id)) Classes.Add(item);
                SelectedClass = Classes.FirstOrDefault();
                StatusMessage = "Pumili ng class at learner para makita ang reading history.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reading Tracker"); }
            finally { IsBusy = false; }
        }

        private async Task LoadLearnersAsync(SchoolClass? schoolClass)
        {
            int version = ++_classVersion;
            Learners.Clear(); Records.Clear(); SelectedLearner = null; NewRecord();
            if (schoolClass == null) return;
            try
            {
                var found = await _learners.GetByClassAsync(schoolClass.Id);
                if (version != _classVersion) return;
                foreach (var item in found.OrderBy(x => x.Sex == "Male" ? 0 : 1)
                             .ThenBy(x => x.LastName).ThenBy(x => x.FirstName)) Learners.Add(item);
                SelectedLearner = Learners.FirstOrDefault();
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reading Tracker"); }
        }

        private async Task LoadRecordsAsync(Learner? learner)
        {
            int version = ++_recordVersion;
            Records.Clear(); NewRecord(); UpdateSummary();
            if (learner == null || _school == null || _year == null) return;
            try
            {
                var found = await _store.GetForLearnerAsync(_school.Id, _year.Id, learner.Id);
                if (version != _recordVersion || SelectedLearner?.Id != learner.Id) return;
                foreach (var item in found) Records.Add(item);
                UpdateSummary();
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reading Tracker"); }
        }

        private void UpdateSummary()
        {
            OnPropertyChanged(nameof(AssessmentCount));
            OnPropertyChanged(nameof(LatestLevel));
            OnPropertyChanged(nameof(ProgressText));
        }

        private void LoadEditor(ReadingAssessment item)
        {
                _recordId = item.Id; AssessmentDate = item.AssessmentDate;
                AssessmentTool = item.AssessmentTool; PassageOrMaterial = item.PassageOrMaterial;
                WordsCorrect = item.WordsCorrect; WordsAttempted = item.WordsAttempted;
                ComprehensionCorrect = item.ComprehensionCorrect; ComprehensionTotal = item.ComprehensionTotal;
                ReadingLevel = item.ReadingLevel; Intervention = item.Intervention; Notes = item.Notes;
        }

        [RelayCommand]
        private void NewRecord()
        {
            _recordId = Guid.NewGuid().ToString("N");
                SelectedRecord = null; AssessmentDate = DateTime.Today;
                AssessmentTool = PassageOrMaterial = ReadingLevel = Intervention = Notes = "";
                WordsCorrect = WordsAttempted = ComprehensionCorrect = ComprehensionTotal = null;
        }

        [RelayCommand]
        private async Task SaveRecordAsync()
        {
            if (_school == null || _year == null || SelectedClass == null || SelectedLearner == null || AssessmentDate == null)
            { _dialogs.ShowWarning("Pumili ng class, learner at assessment date.", "Reading Tracker"); return; }
            if (string.IsNullOrWhiteSpace(AssessmentTool))
            { _dialogs.ShowWarning("Ilagay ang assessment tool o paraan ng pagbasa.", "Reading Tracker"); return; }
            if (new[] { WordsCorrect, WordsAttempted, ComprehensionCorrect, ComprehensionTotal }.Any(x => x < 0) ||
                (WordsCorrect.HasValue != WordsAttempted.HasValue) ||
                (WordsAttempted.HasValue && (WordsAttempted == 0 || WordsCorrect > WordsAttempted)) ||
                (ComprehensionCorrect.HasValue != ComprehensionTotal.HasValue) ||
                (ComprehensionTotal.HasValue && (ComprehensionTotal == 0 || ComprehensionCorrect > ComprehensionTotal)))
            { _dialogs.ShowWarning("Suriin ang scores: kailangan ang correct at total, at hindi puwedeng lumampas sa total.", "Reading Tracker"); return; }
            try
            {
                IsBusy = true;
                await _store.SaveAsync(new ReadingAssessment
                {
                    Id = _recordId, SchoolId = _school.Id, AcademicYearId = _year.Id,
                    SchoolClassId = SelectedClass.Id, LearnerId = SelectedLearner.Id,
                    GradeLevel = SelectedClass.GradeLevel,
                    AssessmentDate = AssessmentDate.Value.Date, AssessmentTool = AssessmentTool.Trim(),
                    PassageOrMaterial = PassageOrMaterial.Trim(), WordsCorrect = WordsCorrect,
                    WordsAttempted = WordsAttempted, ComprehensionCorrect = ComprehensionCorrect,
                    ComprehensionTotal = ComprehensionTotal, ReadingLevel = ReadingLevel.Trim(),
                    Intervention = Intervention.Trim(), Notes = Notes.Trim()
                });
                await LoadRecordsAsync(SelectedLearner);
                StatusMessage = "Nai-save ang reading assessment.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reading Tracker"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeleteRecordAsync()
        {
            if (SelectedRecord == null || _school == null || _year == null || SelectedLearner == null) return;
            if (!_dialogs.Confirm("Burahin ang napiling reading assessment?", "Reading Tracker")) return;
            try
            {
                await _store.DeleteAsync(SelectedRecord.Id, _school.Id, _year.Id);
                await LoadRecordsAsync(SelectedLearner);
                StatusMessage = "Nabura ang reading assessment.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reading Tracker"); }
        }
    }
}
