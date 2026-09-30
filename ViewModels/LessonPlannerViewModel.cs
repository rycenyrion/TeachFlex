using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class LessonPlannerViewModel : ViewModelBase
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IAcademicYearRepository _yearRepository;
        private readonly ISchoolClassRepository _classRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ILessonPlanStore _store;
        private readonly ILessonPlanPdfService _pdf;
        private readonly IDialogService _dialogs;
        private readonly OfflineIlawLessonService _suggestions = new();
        private string _chatPrompt = "Create an ILAW lesson plan for the selected session in English. Include 3–5 specific activities with directions, item counts and actual questions, a group activity, and a formative check with answers.";
        private string _learnerNeeds = string.Empty;
        private string _curriculumNote = "Select a BOW subject, or paste the complete text from the PDF.";
        private int _selectedDay = 1;
        private School? _school;
        private AcademicYear? _year;
        private SchoolClass? _selectedClass;
        private Subject? _selectedSubject;
        private LessonPlan? _selectedSavedPlan;
        private string _planId = Guid.NewGuid().ToString("N");
        private bool _loadingEditor;
        private bool _suppressSubjectLoading;
        private int _subjectLoadVersion;
        private bool _isDirty;
        private DateTime? _lessonDate = DateTime.Today;
        private string _title = string.Empty;
        private string _learningCompetency = string.Empty;
        private string _objectives = string.Empty;
        private string _materials = string.Empty;
        private string _introduction = string.Empty;
        private string _activities = string.Empty;
        private string _assessment = string.Empty;
        private string _reflection = string.Empty;
        private string? _selectedCompetencyChoice;
        private string? _selectedContentStandardChoice;
        private string? _selectedPerformanceStandardChoice;
        private string _lastSuggestedTitle = string.Empty;
        private string _lastAutoCompetency = string.Empty;
        private string _lastAutoObjective = string.Empty;
        private bool _applyingStandard;
        private bool _lastAutoFromStandard;
        private bool _lastAutoContent;
        private bool _switchingSession;
        private readonly string[] _sessionTitles = Enumerable.Repeat(string.Empty, 4).ToArray();
        private readonly string[] _sessionCompetencies = Enumerable.Repeat(string.Empty, 4).ToArray();
        private readonly string[] _sessionObjectives = Enumerable.Repeat(string.Empty, 4).ToArray();
        private int _termNumber = 1;
        private int _weekNumber = 1;
        private string _contentStandard = string.Empty;
        private string _performanceStandard = string.Empty;
        private string _bowSource = string.Empty;
        private string _preLesson = string.Empty;
        private string _session1Motivation = string.Empty;
        private string _session2Motivation = string.Empty;
        private string _session3Motivation = string.Empty;
        private string _session4Motivation = string.Empty;
        private string _day1 = string.Empty;
        private string _day2 = string.Empty;
        private string _day3 = string.Empty;
        private string _day4 = string.Empty;
        private string _day1Assessment = string.Empty;
        private string _day2Assessment = string.Empty;
        private string _day3Assessment = string.Empty;
        private string _day4Assessment = string.Empty;
        private string _weeklyTest = string.Empty;
        private string _session1ExtendLearning = string.Empty;
        private string _session1Reflection = string.Empty;
        private string _session2ExtendLearning = string.Empty;
        private string _session2Reflection = string.Empty;
        private string _session3ExtendLearning = string.Empty;
        private string _session3Reflection = string.Empty;
        private string _session4ExtendLearning = string.Empty;
        private string _session4Reflection = string.Empty;
        private string _session5ExtendLearning = string.Empty;
        private string _session5Reflection = string.Empty;
        private string _extendLearning = string.Empty;


        public LessonPlannerViewModel(ISchoolRepository schoolRepository,
            IAcademicYearRepository yearRepository,
            ISchoolClassRepository classRepository,
            ISubjectRepository subjectRepository, ILessonPlanStore store,
            ILessonPlanPdfService pdf, IDialogService dialogs)
        {
            _schoolRepository = schoolRepository;
            _yearRepository = yearRepository;
            _classRepository = classRepository;
            _subjectRepository = subjectRepository;
            _store = store;
            _pdf = pdf;
            _dialogs = dialogs;
            Classes = new ObservableCollection<SchoolClass>();
            Subjects = new ObservableCollection<Subject>();
            SavedPlans = new ObservableCollection<LessonPlan>();
            TermChoices = new ObservableCollection<int>(Enumerable.Range(1, 3));
            TeachingDays = new ObservableCollection<int>(Enumerable.Range(1, 4));
            WeekChoices = new ObservableCollection<int>(Enumerable.Range(1, 20));
            BowSources = new ObservableCollection<string>();
            TitleChoices = new ObservableCollection<string>();
            ContentStandardChoices = new ObservableCollection<string>();
            PerformanceStandardChoices = new ObservableCollection<string>();
            CompetencyChoices = new ObservableCollection<string>();
            _ = LoadAsync();
        }

        public ObservableCollection<SchoolClass> Classes { get; }
        public ObservableCollection<Subject> Subjects { get; }
        public ObservableCollection<LessonPlan> SavedPlans { get; }
        public ObservableCollection<int> TermChoices { get; }
        public ObservableCollection<int> TeachingDays { get; }
        public ObservableCollection<int> WeekChoices { get; }
        public ObservableCollection<string> BowSources { get; }
        public ObservableCollection<string> TitleChoices { get; }
        public ObservableCollection<string> ContentStandardChoices { get; }
        public ObservableCollection<string> PerformanceStandardChoices { get; }
        public ObservableCollection<string> CompetencyChoices { get; }
        public string SchoolYearText => _year?.DisplayName ?? "No active school year";
        public string ChatPrompt { get => _chatPrompt; set => SetProperty(ref _chatPrompt, value); }
        public bool IsDirty { get => _isDirty; private set => SetProperty(ref _isDirty, value); }

        public SchoolClass? SelectedClass
        {
            get => _selectedClass;
            set
            {
                if (SetProperty(ref _selectedClass, value))
                {
                    MarkDirty();
                    RefreshBowSources();
                    RefreshCurriculumChoices();
                    if (!_suppressSubjectLoading) _ = LoadSubjectsAsync(value);
                }
            }
        }

        public Subject? SelectedSubject
        {
            get => _selectedSubject;
            set { if (SetProperty(ref _selectedSubject, value)) { MarkDirty(); RefreshBowSources(); } }
        }

        public LessonPlan? SelectedSavedPlan
        {
            get => _selectedSavedPlan;
            set
            {
                if (value == _selectedSavedPlan) return;
                if (IsDirty && !_dialogs.Confirm(
                        "Discard unsaved changes and open another lesson plan?",
                        "Unsaved Lesson Plan"))
                {
                    OnPropertyChanged(nameof(SelectedSavedPlan));
                    return;
                }
                if (SetProperty(ref _selectedSavedPlan, value) && value != null)
                    _ = OpenPlanAsync(value);
            }
        }

        private void MarkDirty() { if (!_loadingEditor) IsDirty = true; }
        public DateTime? LessonDate { get => _lessonDate; set { if (SetProperty(ref _lessonDate, value)) MarkDirty(); } }
        public string Title { get => _title; set { if (SetProperty(ref _title, value)) { if (!_loadingEditor && !_switchingSession) _sessionTitles[SelectedDay - 1] = value; if (!_switchingSession) MarkDirty(); } } }
        public string LearningCompetency
        {
            get => _learningCompetency;
            set
            {
                if (!SetProperty(ref _learningCompetency, value)) return;
                if (!_loadingEditor && !_switchingSession) _sessionCompetencies[SelectedDay - 1] = value;
                if (!_switchingSession) MarkDirty();
                if (!_loadingEditor && !_switchingSession && !_applyingStandard && CompetencyChoices.Contains(value))
                {
                    if (string.IsNullOrWhiteSpace(Title) || Title == _lastSuggestedTitle)
                    {
                        Title = value;
                        _lastSuggestedTitle = value;
                    }
                    SuggestObjectiveFromCompetency();
                }
            }
        }
        public string LearnerNeeds { get => _learnerNeeds; set { if (SetProperty(ref _learnerNeeds, value)) MarkDirty(); } }
        public string CurriculumNote { get => _curriculumNote; private set => SetProperty(ref _curriculumNote, value); }
        public int SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value))
                {
                    LoadSessionIntention();
                    OnPropertyChanged(nameof(CurrentDayActivities));
                    OnPropertyChanged(nameof(CurrentDayAssessment));
                    if (!_loadingEditor && (_lastAutoFromStandard || string.IsNullOrWhiteSpace(LearningCompetency)))
                    {
                        bool useContent = _lastAutoFromStandard ? _lastAutoContent : string.IsNullOrWhiteSpace(PerformanceStandard);
                        string standard = useContent ? ContentStandard : PerformanceStandard;
                        if (!string.IsNullOrWhiteSpace(standard))
                            ApplyStandardSelection(standard, useContent);
                    }
                }
            }
        }
        public string CurrentDayActivities
        {
            get => SelectedDay switch { 2 => Day2, 3 => Day3, 4 => Day4, _ => Day1 };
            set
            {
                switch (SelectedDay) { case 2: Day2 = value; break; case 3: Day3 = value; break; case 4: Day4 = value; break; default: Day1 = value; break; }
                OnPropertyChanged();
            }
        }
        public string CurrentDayAssessment
        {
            get => SelectedDay switch { 2 => Day2Assessment, 3 => Day3Assessment, 4 => Day4Assessment, _ => Day1Assessment };
            set
            {
                switch (SelectedDay) { case 2: Day2Assessment = value; break; case 3: Day3Assessment = value; break; case 4: Day4Assessment = value; break; default: Day1Assessment = value; break; }
                OnPropertyChanged();
            }
        }

        public string? SelectedCompetencyChoice
        {
            get => _selectedCompetencyChoice;
            set
            {
                if (!SetProperty(ref _selectedCompetencyChoice, value) || string.IsNullOrWhiteSpace(value)) return;
                LearningCompetency = value;
                if (string.IsNullOrWhiteSpace(Title) || Title == _lastSuggestedTitle)
                {
                    _lastSuggestedTitle = value;
                    Title = _lastSuggestedTitle;
                }
            }
        }
        public string? SelectedContentStandardChoice
        {
            get => _selectedContentStandardChoice;
            set
            {
                if (SetProperty(ref _selectedContentStandardChoice, value) && !string.IsNullOrWhiteSpace(value))
                    ContentStandard = value;
            }
        }
        public string? SelectedPerformanceStandardChoice
        {
            get => _selectedPerformanceStandardChoice;
            set
            {
                if (SetProperty(ref _selectedPerformanceStandardChoice, value) && !string.IsNullOrWhiteSpace(value))
                    PerformanceStandard = value;
            }
        }
        public string Objectives { get => _objectives; set { if (SetProperty(ref _objectives, value)) { if (!_loadingEditor && !_switchingSession) _sessionObjectives[SelectedDay - 1] = value; if (!_switchingSession) MarkDirty(); } } }

        private void LoadSessionIntention()
        {
            _switchingSession = true;
            try
            {
                Title = _sessionTitles[SelectedDay - 1] ?? string.Empty;
                LearningCompetency = _sessionCompetencies[SelectedDay - 1] ?? string.Empty;
                Objectives = _sessionObjectives[SelectedDay - 1] ?? string.Empty;
            }
            finally { _switchingSession = false; }
        }
        public string Materials { get => _materials; set { if (SetProperty(ref _materials, value)) MarkDirty(); } }
        public string Introduction { get => _introduction; set { if (SetProperty(ref _introduction, value)) MarkDirty(); } }
        public string Activities { get => _activities; set { if (SetProperty(ref _activities, value)) MarkDirty(); } }
        public string Assessment { get => _assessment; set { if (SetProperty(ref _assessment, value)) MarkDirty(); } }
        public string Reflection { get => _reflection; set { if (SetProperty(ref _reflection, value)) MarkDirty(); } }
        public int TermNumber { get => _termNumber; set { if (SetProperty(ref _termNumber, value)) { MarkDirty(); RefreshCurriculumChoices(); } } }
        public int WeekNumber { get => _weekNumber; set { if (SetProperty(ref _weekNumber, value)) { MarkDirty(); RefreshCurriculumChoices(); } } }
        public string ContentStandard
        {
            get => _contentStandard;
            set
            {
                if (SetProperty(ref _contentStandard, value))
                {
                    MarkDirty();
                    ApplyStandardSelection(value, true);
                }
            }
        }
        public string PerformanceStandard
        {
            get => _performanceStandard;
            set
            {
                if (SetProperty(ref _performanceStandard, value))
                {
                    MarkDirty();
                    ApplyStandardSelection(value, false);
                }
            }
        }
        public string BowSource { get => _bowSource; set { if (SetProperty(ref _bowSource, value)) { MarkDirty(); RefreshCurriculumChoices(); } } }
        public string PreLesson { get => _preLesson; set { if (SetProperty(ref _preLesson, value)) MarkDirty(); } }
        public string Session1Motivation { get => _session1Motivation; set { if (SetProperty(ref _session1Motivation, value)) MarkDirty(); } }
        public string Session2Motivation { get => _session2Motivation; set { if (SetProperty(ref _session2Motivation, value)) MarkDirty(); } }
        public string Session3Motivation { get => _session3Motivation; set { if (SetProperty(ref _session3Motivation, value)) MarkDirty(); } }
        public string Session4Motivation { get => _session4Motivation; set { if (SetProperty(ref _session4Motivation, value)) MarkDirty(); } }
        public string Day1 { get => _day1; set { if (SetProperty(ref _day1, value)) MarkDirty(); } }
        public string Day2 { get => _day2; set { if (SetProperty(ref _day2, value)) MarkDirty(); } }
        public string Day3 { get => _day3; set { if (SetProperty(ref _day3, value)) MarkDirty(); } }
        public string Day4 { get => _day4; set { if (SetProperty(ref _day4, value)) MarkDirty(); } }
        public string Day1Assessment { get => _day1Assessment; set { if (SetProperty(ref _day1Assessment, value)) MarkDirty(); } }
        public string Day2Assessment { get => _day2Assessment; set { if (SetProperty(ref _day2Assessment, value)) MarkDirty(); } }
        public string Day3Assessment { get => _day3Assessment; set { if (SetProperty(ref _day3Assessment, value)) MarkDirty(); } }
        public string Day4Assessment { get => _day4Assessment; set { if (SetProperty(ref _day4Assessment, value)) MarkDirty(); } }
        public string WeeklyTest { get => _weeklyTest; set { if (SetProperty(ref _weeklyTest, value)) MarkDirty(); } }
        public string Session1ExtendLearning { get => _session1ExtendLearning; set { if (SetProperty(ref _session1ExtendLearning, value)) MarkDirty(); } }
        public string Session1Reflection { get => _session1Reflection; set { if (SetProperty(ref _session1Reflection, value)) MarkDirty(); } }
        public string Session2ExtendLearning { get => _session2ExtendLearning; set { if (SetProperty(ref _session2ExtendLearning, value)) MarkDirty(); } }
        public string Session2Reflection { get => _session2Reflection; set { if (SetProperty(ref _session2Reflection, value)) MarkDirty(); } }
        public string Session3ExtendLearning { get => _session3ExtendLearning; set { if (SetProperty(ref _session3ExtendLearning, value)) MarkDirty(); } }
        public string Session3Reflection { get => _session3Reflection; set { if (SetProperty(ref _session3Reflection, value)) MarkDirty(); } }
        public string Session4ExtendLearning { get => _session4ExtendLearning; set { if (SetProperty(ref _session4ExtendLearning, value)) MarkDirty(); } }
        public string Session4Reflection { get => _session4Reflection; set { if (SetProperty(ref _session4Reflection, value)) MarkDirty(); } }
        public string Session5ExtendLearning { get => _session5ExtendLearning; set { if (SetProperty(ref _session5ExtendLearning, value)) MarkDirty(); } }
        public string Session5Reflection { get => _session5Reflection; set { if (SetProperty(ref _session5Reflection, value)) MarkDirty(); } }
        public string ExtendLearning { get => _extendLearning; set { if (SetProperty(ref _extendLearning, value)) MarkDirty(); } }

        private void SuggestObjectiveFromCompetency()
        {
            if (string.IsNullOrWhiteSpace(LearningCompetency)) return;
            if (!string.IsNullOrWhiteSpace(Objectives) && Objectives != _lastAutoObjective) return;
            _lastAutoObjective = "By the end of the lesson, learners will demonstrate this skill: " +
                LearningCompetency.Trim().TrimEnd('.') + ".";
            Objectives = _lastAutoObjective;
        }

        private void SuggestFromStandard(string standard, bool content)
        {
            var draft = StandardLessonSuggestionService.Create(standard, SelectedSubject?.SubjectName, SelectedDay);
            if (string.IsNullOrWhiteSpace(LearningCompetency) || LearningCompetency == _lastAutoCompetency)
            {
                LearningCompetency = draft.Competency;
                _lastAutoCompetency = draft.Competency;
            }
            if (string.IsNullOrWhiteSpace(Title) || Title == _lastSuggestedTitle)
            {
                Title = draft.Title;
                _lastSuggestedTitle = draft.Title;
            }
            if (string.IsNullOrWhiteSpace(Objectives) || Objectives == _lastAutoObjective)
            {
                _lastAutoObjective = LearningCompetency == draft.Competency
                    ? draft.Objective
                    : "By the end of the lesson, learners will demonstrate this skill: " + LearningCompetency.Trim().TrimEnd('.') + ".";
                Objectives = _lastAutoObjective;
            }
            CurriculumNote = "This TeachFlex draft is suggested from the standard; it is not a verified BOW competency. Review and edit the competency, title and objectives before saving.";
            _lastAutoFromStandard = true;
            _lastAutoContent = content;
        }

        private void ApplyStandardSelection(string value, bool content)
        {
            if (_loadingEditor || _applyingStandard || string.IsNullOrWhiteSpace(value)) return;
            var entry = BowCurriculumCatalog.Find(SelectedClass?.GradeLevel, BowSource);
            var standards = content ? entry?.Content : entry?.Performance;
            int standardIndex = standards?.FindIndex(item => string.Equals(item, value, StringComparison.Ordinal)) ?? -1;
            if (standardIndex < 0 && value.Trim().Length < 25) return;
            try
            {
                _applyingStandard = true;
                if (standards?.Count == 3 && standardIndex >= 0 && TermNumber != standardIndex + 1)
                    TermNumber = standardIndex + 1;

                // The catalog has term lists, but no verified week-to-competency
                // mapping. Even a one-item list must not imply a week assignment.
                if (entry != null)
                {
                    CurriculumNote = CompetencyChoices.Count > 0
                        ? "Select the exact BOW competency for this lesson. The catalog has no verified week mapping, so TeachFlex will not select one automatically."
                        : "No competency was extracted from this BOW. Review the PDF and enter the exact competency.";
                    return;
                }
                SuggestFromStandard(value, content);
            }
            finally { _applyingStandard = false; }
        }

        [RelayCommand]
        private void RefreshStandardSuggestions()
        {
            string standard = !string.IsNullOrWhiteSpace(PerformanceStandard)
                ? PerformanceStandard : ContentStandard;
            if (string.IsNullOrWhiteSpace(standard))
            {
                _dialogs.ShowWarning("Select or enter a Content Standard or Performance Standard first.", "ILAW Suggestions");
                return;
            }
            if ((!string.IsNullOrWhiteSpace(LearningCompetency) || !string.IsNullOrWhiteSpace(Objectives)) &&
                !_dialogs.Confirm("Replace the current competency, title and objectives with new suggestions?", "ILAW Suggestions")) return;
            LearningCompetency = Title = Objectives = string.Empty;
            _lastAutoCompetency = _lastAutoObjective = _lastSuggestedTitle = string.Empty;
            ApplyStandardSelection(standard, !string.IsNullOrWhiteSpace(ContentStandard) && standard == ContentStandard);
        }


        private void RefreshBowSources()
        {
            BowSources.Clear();
            if (SelectedClass == null) return;
            var sources = BowSourceCatalog.ForGrade(SelectedClass.GradeLevel).ToList();
            string subject = SelectedSubject?.SubjectName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(subject))
            {
                string key = new string(subject.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
                var matches = sources.Where(source =>
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(source);
                    string normalized = new string(filename.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
                    return normalized.Contains(key, StringComparison.Ordinal);
                }).ToList();
                if (matches.Count > 0) sources = matches;
            }
            foreach (string source in sources) BowSources.Add(source);
        }

        private void RefreshCurriculumChoices()
        {
            TitleChoices.Clear(); ContentStandardChoices.Clear();
            PerformanceStandardChoices.Clear(); CompetencyChoices.Clear();
            var entry = BowCurriculumCatalog.Find(SelectedClass?.GradeLevel, BowSource);
            if (entry == null)
            {
                CurriculumNote = "Select a BOW or enter a standard. TeachFlex can provide an editable suggestion even when no BOW competency is available.";
                return;
            }
            // Three distinct term standards in source order map to Term 1, 2 and 3.
            System.Collections.Generic.IEnumerable<string> contents = entry.Content.Count == 3 ? new[] { entry.Content[Math.Clamp(TermNumber, 1, 3) - 1] } : entry.Content;
            System.Collections.Generic.IEnumerable<string> performances = entry.Performance.Count == 3 ? new[] { entry.Performance[Math.Clamp(TermNumber, 1, 3) - 1] } : entry.Performance;
            foreach (string item in contents) ContentStandardChoices.Add(item);
            foreach (string item in performances) PerformanceStandardChoices.Add(item);
            if (!_loadingEditor)
            {
                if (string.IsNullOrWhiteSpace(ContentStandard) && ContentStandardChoices.Count == 1)
                    ContentStandard = ContentStandardChoices[0];
                if (string.IsNullOrWhiteSpace(PerformanceStandard) && PerformanceStandardChoices.Count == 1)
                    PerformanceStandard = PerformanceStandardChoices[0];
            }
            var competencies = entry.TermCompetencies.Count == 3
                ? entry.TermCompetencies[Math.Clamp(TermNumber, 1, 3) - 1]
                : entry.Competencies;
            foreach (string item in competencies)
            {
                CompetencyChoices.Add(item);
                // The BOW has no separate lesson title: offer a short, editable title from its text.
                string title = item;
                if (!TitleChoices.Contains(title)) TitleChoices.Add(title);
            }
            CurriculumNote = CompetencyChoices.Count == 0
                ? "No complete competency has been verified for this PDF. Paste the exact text from the BOW."
                : "Select a BOW competency for the selected term. Check the PDF for the week; the catalog has no verified week mapping.";
            if (!_loadingEditor && !_applyingStandard)
            {
                if (ContentStandardChoices.Contains(ContentStandard)) ApplyStandardSelection(ContentStandard, true);
                else if (PerformanceStandardChoices.Contains(PerformanceStandard)) ApplyStandardSelection(PerformanceStandard, false);
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading lesson plans...";
                _school = await _schoolRepository.GetActiveSchoolAsync();
                if (_school == null) { StatusMessage = "Complete School Setup first."; return; }
                _year = await _yearRepository.GetCurrentAsync(_school.Id);
                OnPropertyChanged(nameof(SchoolYearText));
                if (_year == null) { StatusMessage = "Create a school year first."; return; }
                foreach (SchoolClass schoolClass in await _classRepository
                             .GetByAcademicYearAsync(_school.Id, _year.Id))
                    Classes.Add(schoolClass);
                await RefreshPlansAsync();
                _loadingEditor = true;
                SelectedClass = Classes.FirstOrDefault();
                _loadingEditor = false;
                StatusMessage = "Select a class and subject, then write your lesson plan.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Lesson Planner"); }
            finally { IsBusy = false; }
        }

        private async Task LoadSubjectsAsync(SchoolClass? schoolClass, int? selectedSubjectId = null)
        {
            int version = ++_subjectLoadVersion;
            Subjects.Clear();
            if (schoolClass == null) { SelectedSubject = null; return; }
            try
            {
                var subjects = await _subjectRepository.GetByClassAsync(schoolClass.Id);
                if (SelectedClass != schoolClass || version != _subjectLoadVersion) return;
                _loadingEditor = true;
                foreach (Subject subject in subjects.Where(item => item.IsActive))
                    Subjects.Add(subject);
                SelectedSubject = Subjects.FirstOrDefault(subject =>
                    subject.Id == selectedSubjectId) ?? Subjects.FirstOrDefault();
            }
            catch (Exception ex) { StatusMessage = ex.Message; }
            finally { _loadingEditor = false; }
        }

        private async Task RefreshPlansAsync()
        {
            if (_school == null || _year == null) return;
            var plans = await _store.GetByYearAsync(_school.Id, _year.Id);
            SavedPlans.Clear();
            foreach (LessonPlan plan in plans) SavedPlans.Add(plan);
        }

        private async Task OpenPlanAsync(LessonPlan plan)
        {
            _loadingEditor = true;
            try
            {
                _planId = plan.Id;
                _suppressSubjectLoading = true;
                SelectedClass = Classes.FirstOrDefault(item => item.Id == plan.SchoolClassId);
                _suppressSubjectLoading = false;
                await LoadSubjectsAsync(SelectedClass, plan.SubjectId);
                _loadingEditor = true;
                string[] savedTitles = { plan.Session1Title, plan.Session2Title, plan.Session3Title, plan.Session4Title };
                string[] savedCompetencies = { plan.Session1Competency, plan.Session2Competency, plan.Session3Competency, plan.Session4Competency };
                string[] savedObjectives = { plan.Session1Objectives, plan.Session2Objectives, plan.Session3Objectives, plan.Session4Objectives };
                for (int i = 0; i < 4; i++)
                {
                    _sessionTitles[i] = string.IsNullOrWhiteSpace(savedTitles[i]) ? plan.Title : savedTitles[i];
                    _sessionCompetencies[i] = string.IsNullOrWhiteSpace(savedCompetencies[i]) ? plan.LearningCompetency : savedCompetencies[i];
                    _sessionObjectives[i] = string.IsNullOrWhiteSpace(savedObjectives[i]) ? plan.Objectives : savedObjectives[i];
                }
                SelectedDay = 1;
                LoadSessionIntention();
                LessonDate = plan.LessonDate;
                _lastAutoCompetency = string.Empty;
                _lastAutoObjective = string.Empty;
                _lastAutoFromStandard = false;
                Materials = plan.Materials;
                Introduction = plan.Introduction;
                Activities = plan.Activities;
                Assessment = plan.Assessment;
                Reflection = plan.Reflection;
                LearnerNeeds = plan.LearnerNeeds;
                TermNumber = plan.TermNumber; WeekNumber = plan.WeekNumber;
                ContentStandard = plan.ContentStandard;
                PerformanceStandard = plan.PerformanceStandard;
                BowSource = plan.BowSource;
                PreLesson = plan.PreLesson;
                Session1Motivation = string.IsNullOrWhiteSpace(plan.Session1Motivation) ? plan.PreLesson : plan.Session1Motivation;
                Session2Motivation = string.IsNullOrWhiteSpace(plan.Session2Motivation) ? plan.PreLesson : plan.Session2Motivation;
                Session3Motivation = string.IsNullOrWhiteSpace(plan.Session3Motivation) ? plan.PreLesson : plan.Session3Motivation;
                Session4Motivation = string.IsNullOrWhiteSpace(plan.Session4Motivation) ? plan.PreLesson : plan.Session4Motivation;
                Day1 = plan.Day1;
                Day2 = plan.Day2;
                Day3 = plan.Day3;
                Day4 = plan.Day4;
                Day1Assessment = plan.Day1Assessment;
                Day2Assessment = plan.Day2Assessment;
                Day3Assessment = plan.Day3Assessment;
                Day4Assessment = plan.Day4Assessment;
                WeeklyTest = plan.WeeklyTest;
                ExtendLearning = plan.ExtendLearning;
                Session1ExtendLearning = string.IsNullOrWhiteSpace(plan.Session1ExtendLearning) ? plan.ExtendLearning : plan.Session1ExtendLearning;
                Session1Reflection = string.IsNullOrWhiteSpace(plan.Session1Reflection) ? plan.Reflection : plan.Session1Reflection;
                Session2ExtendLearning = string.IsNullOrWhiteSpace(plan.Session2ExtendLearning) ? plan.ExtendLearning : plan.Session2ExtendLearning;
                Session2Reflection = string.IsNullOrWhiteSpace(plan.Session2Reflection) ? plan.Reflection : plan.Session2Reflection;
                Session3ExtendLearning = string.IsNullOrWhiteSpace(plan.Session3ExtendLearning) ? plan.ExtendLearning : plan.Session3ExtendLearning;
                Session3Reflection = string.IsNullOrWhiteSpace(plan.Session3Reflection) ? plan.Reflection : plan.Session3Reflection;
                Session4ExtendLearning = string.IsNullOrWhiteSpace(plan.Session4ExtendLearning) ? plan.ExtendLearning : plan.Session4ExtendLearning;
                Session4Reflection = string.IsNullOrWhiteSpace(plan.Session4Reflection) ? plan.Reflection : plan.Session4Reflection;
                Session5ExtendLearning = string.IsNullOrWhiteSpace(plan.Session5ExtendLearning) ? plan.ExtendLearning : plan.Session5ExtendLearning;
                Session5Reflection = string.IsNullOrWhiteSpace(plan.Session5Reflection) ? plan.Reflection : plan.Session5Reflection;

                OnPropertyChanged(nameof(CurrentDayActivities));
                OnPropertyChanged(nameof(CurrentDayAssessment));
                IsDirty = false;
                StatusMessage = "Lesson plan loaded.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Lesson plan could not be loaded.";
                _dialogs.ShowError(ex.Message, "Open Lesson Plan");
            }
            finally { _suppressSubjectLoading = false; _loadingEditor = false; }
        }

        [RelayCommand]
        private void NewPlan()
        {
            if (IsDirty && !_dialogs.Confirm("Discard unsaved changes and start a new plan?",
                    "Unsaved Lesson Plan")) return;
            _loadingEditor = true;
            _planId = Guid.NewGuid().ToString("N");
            _selectedSavedPlan = null;
            OnPropertyChanged(nameof(SelectedSavedPlan));
            LessonDate = DateTime.Today;
            Title = LearningCompetency = Objectives = Materials = string.Empty;
            _selectedCompetencyChoice = null; _lastSuggestedTitle = string.Empty;
            _selectedContentStandardChoice = _selectedPerformanceStandardChoice = null;
            OnPropertyChanged(nameof(SelectedContentStandardChoice));
            OnPropertyChanged(nameof(SelectedPerformanceStandardChoice));
            _lastAutoCompetency = _lastAutoObjective = string.Empty;
            _lastAutoFromStandard = false;
            for (int i = 0; i < 4; i++)
                _sessionTitles[i] = _sessionCompetencies[i] = _sessionObjectives[i] = string.Empty;
            OnPropertyChanged(nameof(SelectedCompetencyChoice));
            Introduction = Activities = Assessment = Reflection = string.Empty;
            TermNumber = WeekNumber = 1;
            SelectedDay = 1; LearnerNeeds = string.Empty;
            ContentStandard = string.Empty;
            PerformanceStandard = string.Empty;
            BowSource = string.Empty;
            PreLesson = string.Empty;
            Session1Motivation = Session2Motivation = Session3Motivation = Session4Motivation = string.Empty;
            Day1 = string.Empty;
            Day2 = string.Empty;
            Day3 = string.Empty;
            Day4 = string.Empty;
            Day1Assessment = string.Empty;
            Day2Assessment = string.Empty;
            Day3Assessment = string.Empty;
            Day4Assessment = string.Empty;
            WeeklyTest = string.Empty;
            ExtendLearning = string.Empty;
            Session1ExtendLearning = string.Empty;
            Session1Reflection = string.Empty;
            Session2ExtendLearning = string.Empty;
            Session2Reflection = string.Empty;
            Session3ExtendLearning = string.Empty;
            Session3Reflection = string.Empty;
            Session4ExtendLearning = string.Empty;
            Session4Reflection = string.Empty;
            Session5ExtendLearning = string.Empty;
            Session5Reflection = string.Empty;

            IsDirty = false;
            _loadingEditor = false;
            StatusMessage = "New lesson plan ready.";
        }

        private LessonPlan CurrentPlan() => new LessonPlan
        {
            Id = _planId,
            SchoolId = _school!.Id,
            AcademicYearId = _year!.Id,
            SchoolClassId = SelectedClass!.Id,
            SubjectId = SelectedSubject!.Id,
            LessonDate = LessonDate ?? DateTime.Today,
            Title = Title.Trim(),
            LearningCompetency = LearningCompetency.Trim(),
            Objectives = Objectives.Trim(),
            Session1Title = _sessionTitles[0]?.Trim() ?? string.Empty,
            Session1Competency = _sessionCompetencies[0]?.Trim() ?? string.Empty,
            Session1Objectives = _sessionObjectives[0]?.Trim() ?? string.Empty,
            Session2Title = _sessionTitles[1]?.Trim() ?? string.Empty,
            Session2Competency = _sessionCompetencies[1]?.Trim() ?? string.Empty,
            Session2Objectives = _sessionObjectives[1]?.Trim() ?? string.Empty,
            Session3Title = _sessionTitles[2]?.Trim() ?? string.Empty,
            Session3Competency = _sessionCompetencies[2]?.Trim() ?? string.Empty,
            Session3Objectives = _sessionObjectives[2]?.Trim() ?? string.Empty,
            Session4Title = _sessionTitles[3]?.Trim() ?? string.Empty,
            Session4Competency = _sessionCompetencies[3]?.Trim() ?? string.Empty,
            Session4Objectives = _sessionObjectives[3]?.Trim() ?? string.Empty,
            Materials = Materials.Trim(),
            Introduction = Introduction.Trim(),
            Activities = Activities.Trim(),
            Assessment = Assessment.Trim(),
            Reflection = string.Empty, // migrated to per-session reflections
            TermNumber = TermNumber, WeekNumber = WeekNumber,
            ContentStandard = ContentStandard.Trim(),
            PerformanceStandard = PerformanceStandard.Trim(),
            BowSource = BowSource.Trim(),
            PreLesson = PreLesson.Trim(),
            Session1Motivation = Session1Motivation.Trim(),
            Session2Motivation = Session2Motivation.Trim(),
            Session3Motivation = Session3Motivation.Trim(),
            Session4Motivation = Session4Motivation.Trim(),
            Day1 = Day1.Trim(),
            Day2 = Day2.Trim(),
            Day3 = Day3.Trim(),
            Day4 = Day4.Trim(),
            Day1Assessment = Day1Assessment.Trim(),
            Day2Assessment = Day2Assessment.Trim(),
            Day3Assessment = Day3Assessment.Trim(),
            Day4Assessment = Day4Assessment.Trim(),
            WeeklyTest = WeeklyTest.Trim(),
            Session1ExtendLearning = Session1ExtendLearning.Trim(),
            Session1Reflection = Session1Reflection.Trim(),
            Session2ExtendLearning = Session2ExtendLearning.Trim(),
            Session2Reflection = Session2Reflection.Trim(),
            Session3ExtendLearning = Session3ExtendLearning.Trim(),
            Session3Reflection = Session3Reflection.Trim(),
            Session4ExtendLearning = Session4ExtendLearning.Trim(),
            Session4Reflection = Session4Reflection.Trim(),
            Session5ExtendLearning = Session5ExtendLearning.Trim(),
            Session5Reflection = Session5Reflection.Trim(),
            ExtendLearning = string.Empty, // migrated to per-session ways forward
            LearnerNeeds = LearnerNeeds.Trim()
        };

        private bool Validate()
        {
            if (_school != null && _year != null && SelectedClass != null &&
                SelectedSubject != null && LessonDate.HasValue &&
                TermNumber is >= 1 and <= 3 && WeekNumber is >= 1 and <= 20 &&
                !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(LearningCompetency) &&
                !string.IsNullOrWhiteSpace(Objectives))
                return true;
            _dialogs.ShowWarning("Select class, subject, date, term (1-3), week (1-20), then enter title, competency and objectives.",
                "Incomplete Lesson Plan");
            return false;
        }

        private Task GenerateSuggestedLessonAsync(bool simplify)
        {
            if (SelectedClass == null || SelectedSubject == null ||
                string.IsNullOrWhiteSpace(LearningCompetency))
            {
                _dialogs.ShowWarning("Select the class, subject and learning competency.", "Offline Lesson Draft");
                return Task.CompletedTask;
            }
            if ((!string.IsNullOrWhiteSpace(CurrentDayActivities) || !string.IsNullOrWhiteSpace(CurrentDayAssessment)) &&
                !_dialogs.Confirm($"Replace the current Day {SelectedDay} draft?", "Generate Lesson Draft")) return Task.CompletedTask;
            try
            {
                IsBusy = true;
                StatusMessage = $"Generating Day {SelectedDay} suggestion...";
                var result = _suggestions.Generate(SelectedSubject.SubjectName, SelectedDay,
                    LearningCompetency, LearnerNeeds, simplify);
                if (string.IsNullOrWhiteSpace(Title))
                {
                    Title = result.Title;
                    _lastSuggestedTitle = result.Title;
                }
                if (string.IsNullOrWhiteSpace(Objectives)) Objectives = result.Objectives;
                if (string.IsNullOrWhiteSpace(Materials)) Materials = result.Resources;
                switch (SelectedDay)
                {
                    case 2: Session2Motivation = result.PreLesson; break;
                    case 3: Session3Motivation = result.PreLesson; break;
                    case 4: Session4Motivation = result.PreLesson; break;
                    default: Session1Motivation = result.PreLesson; break;
                }
                CurrentDayActivities = result.Activities;
                CurrentDayAssessment = result.Assessment;
                StatusMessage = $"Day {SelectedDay} suggestion is ready. Review and edit before saving.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Offline Lesson Draft"); }
            finally { IsBusy = false; }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task GenerateDayAsync() => GenerateSuggestedLessonAsync(false);

        [RelayCommand]
        private Task SimplifyDayAsync() => GenerateSuggestedLessonAsync(true);

        [RelayCommand]
        private void OpenLessonChat() => OpenExternalLessonChat("https://chatgpt.com/", "ChatGPT");

        [RelayCommand]
        private void OpenGeminiLessonChat() => OpenExternalLessonChat("https://gemini.google.com/", "Gemini");

        [RelayCommand]
        private void OpenCopilotLessonChat() => OpenExternalLessonChat("https://copilot.com/", "Copilot");

        [RelayCommand]
        private void OpenCanvaLessonChat() => OpenExternalLessonChat("https://www.canva.com/ai-assistant/", "Canva AI");

        private void OpenExternalLessonChat(string url, string serviceName)
        {
            if (SelectedClass == null || SelectedSubject == null || string.IsNullOrWhiteSpace(LearningCompetency))
            {
                _dialogs.ShowWarning("Select the class, subject and BOW learning competency first.", "AI Lesson Assistant");
                return;
            }
            string prompt = "Create an editable ILAW lesson plan draft entirely in English. Use English as the medium of instruction for the teacher directions, learner activities, questions, and answers. Preserve the exact wording of the BOW competency quoted below; do not translate or replace it.\n" +
                $"Grade: {SelectedClass.GradeLevel}; Subject: {SelectedSubject.SubjectName}; Term: {TermNumber}; Week: {WeekNumber}; Session: {SelectedDay} (one of four teaching sessions).\n" +
                $"Content Standard: {ContentStandard}\nPerformance Standard: {PerformanceStandard}\n" +
                $"Exact BOW Learning Competency (do not invent another): {LearningCompetency}\n" +
                $"Learner needs/materials: {LearnerNeeds}\n" +
                $"Teacher's additional request: {ChatPrompt}\n" +
                "Provide a lesson title, measurable objectives, a sample motivation, a lesson flow with specific directions and actual items, 3–5 activities including a group activity, a five-item formative check with answer key, resources, and ways forward. " +
                "Session 5 is reserved for the weekly test. Identify any missing BOW information; do not pretend to have seen an entire PDF. The teacher will review and adapt the draft.\n\n" +
                "IMPORTANT: For importing into TeachFlex, put each exact label on its own line in the order below. Do not use a markdown code fence. Write the content of every section in English.\n" +
                "[TITLE]\n<lesson title>\n[OBJECTIVES]\n<measurable objectives>\n[MOTIVATION]\n<sample pre-lesson motivation>\n" +
                "[ACTIVITIES]\n<3–5 specific activities with directions, item counts and actual items, including a group activity>\n" +
                "[FORMATIVE_CHECKS]\n<five-item formative check and answer key>\n[RESOURCES]\n<learning resources>\n" +
                "[WAYS_FORWARD]\n<short remediation or enrichment for this session>.";
            try
            {
                System.Windows.Clipboard.SetText(prompt);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
                    { UseShellExecute = true });
                StatusMessage = $"Opened {serviceName} and copied the ILAW prompt. Press Ctrl+V in the chat. Review the draft and paste it back into TeachFlex.";
            }
            catch (Exception ex) { _dialogs.ShowError("Could not open the chatbot or copy the prompt: " + ex.Message, "AI Lesson Assistant"); }
        }

        [RelayCommand]
        private Task GenerateWeekAsync()
        {
            if (SelectedClass == null || SelectedSubject == null ||
                string.IsNullOrWhiteSpace(LearningCompetency))
            {
                _dialogs.ShowWarning("Select the class, subject and learning competency first.", "Weekly ILAW");
                return Task.CompletedTask;
            }
            if (new[] { Day1, Day2, Day3, Day4, WeeklyTest }.Any(x => !string.IsNullOrWhiteSpace(x)) &&
                !_dialogs.Confirm("Replace the current Sessions 1–5 draft?", "Weekly ILAW")) return Task.CompletedTask;
            try
            {
                IsBusy = true;
                StatusMessage = "Generating four teaching days...";
                if (_lastAutoFromStandard)
                {
                    string standard = _lastAutoContent ? ContentStandard : PerformanceStandard;
                    for (int index = 0; index < 4; index++)
                    {
                        var draft = StandardLessonSuggestionService.Create(standard, SelectedSubject.SubjectName, index + 1);
                        if (string.IsNullOrWhiteSpace(_sessionTitles[index])) _sessionTitles[index] = draft.Title;
                        if (string.IsNullOrWhiteSpace(_sessionCompetencies[index])) _sessionCompetencies[index] = draft.Competency;
                        if (string.IsNullOrWhiteSpace(_sessionObjectives[index])) _sessionObjectives[index] = draft.Objective;
                    }
                }
                var lessons = new OfflineDailyLesson[4];
                for (int day = 1; day <= 4; day++)
                    lessons[day - 1] = _suggestions.Generate(SelectedSubject.SubjectName, day,
                        string.IsNullOrWhiteSpace(_sessionCompetencies[day - 1])
                            ? LearningCompetency : _sessionCompetencies[day - 1], LearnerNeeds, false);
                if (string.IsNullOrWhiteSpace(Title))
                {
                    Title = lessons[0].Title;
                    _lastSuggestedTitle = Title;
                }
                if (string.IsNullOrWhiteSpace(Objectives)) Objectives = lessons[0].Objectives;
                Materials = lessons[0].Resources;
                Session1Motivation = lessons[0].PreLesson;
                Session2Motivation = lessons[1].PreLesson;
                Session3Motivation = lessons[2].PreLesson;
                Session4Motivation = lessons[3].PreLesson;
                Day1 = lessons[0].Activities; Day1Assessment = lessons[0].Assessment;
                Day2 = lessons[1].Activities; Day2Assessment = lessons[1].Assessment;
                Day3 = lessons[2].Activities; Day3Assessment = lessons[2].Assessment;
                Day4 = lessons[3].Activities; Day4Assessment = lessons[3].Assessment;
                WeeklyTest = $"Coverage: {LearningCompetency}\n\nSession 5: Short weekly test on the skills taught in Sessions 1–4. Adjust the questions and number of items to the learners' needs.";
                OnPropertyChanged(nameof(CurrentDayActivities));
                OnPropertyChanged(nameof(CurrentDayAssessment));
                StatusMessage = "Weekly draft ready. Review and edit each session before saving.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Weekly ILAW"); }
            finally { IsBusy = false; }
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SavePlanAsync()
        {
            if (!Validate()) return;
            try
            {
                IsBusy = true;
                await _store.SaveAsync(CurrentPlan());
                IsDirty = false;
                await RefreshPlansAsync();
                _selectedSavedPlan = SavedPlans.FirstOrDefault(plan => plan.Id == _planId);
                OnPropertyChanged(nameof(SelectedSavedPlan));
                StatusMessage = "Lesson plan saved.";
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Save Lesson Plan"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task DeletePlanAsync()
        {
            if (_selectedSavedPlan == null) return;
            if (!_dialogs.Confirm($"Delete '{_selectedSavedPlan.Title}'?", "Delete Lesson Plan")) return;
            try
            {
                await _store.DeleteAsync(_selectedSavedPlan.Id);
                IsDirty = false;
                NewPlan();
                await RefreshPlansAsync();
                StatusMessage = "Lesson plan deleted.";
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Delete Lesson Plan"); }
        }

        [RelayCommand]
        private void ExportPdf()
        {
            if (!Validate()) return;
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf", DefaultExt = ".pdf",
                AddExtension = true,
                FileName = $"Lesson-Plan-{LessonDate:yyyy-MM-dd}.pdf"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                _pdf.Export(CurrentPlan(), SelectedClass!.DisplayName,
                    SelectedSubject!.SubjectName, _year!.DisplayName,
                    _school!.SchoolName, _school.SchoolHead, dialog.FileName);
                StatusMessage = "Lesson plan PDF created.";
                _dialogs.ShowInformation(dialog.FileName, "Lesson Plan PDF");
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Export Lesson Plan"); }
        }
    }
}
