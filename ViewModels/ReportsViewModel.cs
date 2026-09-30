using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public sealed partial class ReportsViewModel : ViewModelBase
    {
        private readonly ISchoolRepository _schools;
        private readonly IAcademicYearRepository _years;
        private readonly ISchoolClassRepository _classes;
        private readonly ILearnerRepository _learners;
        private readonly ISubjectRepository _subjects;
        private readonly ILearnerGradeService _grades;
        private readonly IAttendanceRepository _attendance;
        private readonly IReadingAssessmentStore _reading;
        private readonly IDialogService _dialogs;
        private School? _school;
        private AcademicYear? _year;
        private SchoolClass? _selectedClass;
        private int _selectedTerm = 1;
        private DateTime? _selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private int _learnerCount, _subjectCount, _gradesRecorded, _presentCount, _absentCount, _readingCount;
        private int _loadVersion;

        public ReportsViewModel(ISchoolRepository schools, IAcademicYearRepository years,
            ISchoolClassRepository classes, ILearnerRepository learners, ISubjectRepository subjects,
            ILearnerGradeService grades, IAttendanceRepository attendance,
            IReadingAssessmentStore reading, IDialogService dialogs)
        {
            _schools = schools; _years = years; _classes = classes; _learners = learners;
            _subjects = subjects; _grades = grades; _attendance = attendance;
            _reading = reading; _dialogs = dialogs;
            Classes = new(); LearnerRows = new();
            _ = InitializeAsync();
        }

        public ObservableCollection<SchoolClass> Classes { get; }
        public ObservableCollection<ReportLearnerRow> LearnerRows { get; }
        public string SchoolYearText => _year?.DisplayName ?? "No active school year";
        public SchoolClass? SelectedClass
        {
            get => _selectedClass;
            set { if (SetProperty(ref _selectedClass, value)) _ = LoadReportAsync(); }
        }
        public int SelectedTerm
        {
            get => _selectedTerm;
            set { if (SetProperty(ref _selectedTerm, value)) _ = LoadReportAsync(); }
        }
        public DateTime? SelectedMonth
        {
            get => _selectedMonth;
            set { if (SetProperty(ref _selectedMonth, value)) _ = LoadReportAsync(); }
        }
        public int LearnerCount { get => _learnerCount; private set => SetProperty(ref _learnerCount, value); }
        public int SubjectCount { get => _subjectCount; private set => SetProperty(ref _subjectCount, value); }
        public int GradesRecorded { get => _gradesRecorded; private set => SetProperty(ref _gradesRecorded, value); }
        public int PresentCount { get => _presentCount; private set => SetProperty(ref _presentCount, value); }
        public int AbsentCount { get => _absentCount; private set => SetProperty(ref _absentCount, value); }
        public int ReadingCount { get => _readingCount; private set => SetProperty(ref _readingCount, value); }

        private async Task InitializeAsync()
        {
            try
            {
                _school = await _schools.GetActiveSchoolAsync();
                if (_school == null) { StatusMessage = "I-set up muna ang school."; return; }
                _year = await _years.GetCurrentAsync(_school.Id);
                OnPropertyChanged(nameof(SchoolYearText));
                if (_year == null) { StatusMessage = "Pumili muna ng active school year."; return; }
                foreach (var schoolClass in await _classes.GetByAcademicYearAsync(_school.Id, _year.Id))
                    Classes.Add(schoolClass);
                SelectedClass = Classes.FirstOrDefault();
                if (SelectedClass == null) StatusMessage = "Wala pang class sa active school year.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reports"); }
        }

        [RelayCommand]
        private async Task LoadReportAsync()
        {
            int version = ++_loadVersion;
            if (SelectedClass == null || _school == null || _year == null) return;
            var schoolClass = SelectedClass;
            int term = SelectedTerm;
            var month = SelectedMonth;
            try
            {
                IsBusy = true;
                StatusMessage = "Loading class report...";
                var learners = (await _learners.GetByClassAsync(schoolClass.Id))
                    .OrderBy(x => x.Sex == "Male" ? 0 : 1)
                    .ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
                var subjects = await _subjects.GetByClassAsync(schoolClass.Id);
                var counts = learners.ToDictionary(x => x.Id, _ => 0);
                // The grade service returns null for descriptive grades and incomplete terms.
                foreach (var subject in subjects)
                {
                    var grades = await _grades.GetTermGradesAsync(schoolClass, _year, subject, learners, term);
                    foreach (var learner in learners)
                        if (grades.TryGetValue(learner.Id, out int? grade) && grade.HasValue) counts[learner.Id]++;
                }
                var present = learners.ToDictionary(x => x.Id, _ => 0);
                var absent = learners.ToDictionary(x => x.Id, _ => 0);
                if (month.HasValue)
                {
                    var attendance = await _attendance.GetMonthAttendancesAsync(schoolClass.Id, month.Value.Year, month.Value.Month);
                    foreach (var record in attendance)
                    {
                        if (!present.ContainsKey(record.LearnerId)) continue;
                        if (record.AttendanceStatus.Equals("Present", StringComparison.OrdinalIgnoreCase)) present[record.LearnerId]++;
                        else if (record.AttendanceStatus.Equals("Absent", StringComparison.OrdinalIgnoreCase)) absent[record.LearnerId]++;
                    }
                }
                var reading = learners.ToDictionary(x => x.Id, _ => 0);
                foreach (var learner in learners)
                    reading[learner.Id] = (await _reading.GetForLearnerAsync(_school.Id, _year.Id, learner.Id)).Count;
                if (version != _loadVersion || SelectedClass?.Id != schoolClass.Id) return;
                LearnerRows.Clear();
                int index = 0;
                foreach (var learner in learners)
                    LearnerRows.Add(new ReportLearnerRow(++index, learner.OfficialName, learner.Sex,
                        counts[learner.Id], present[learner.Id], absent[learner.Id], reading[learner.Id]));
                LearnerCount = learners.Count; SubjectCount = subjects.Count;
                GradesRecorded = counts.Values.Sum(); PresentCount = present.Values.Sum();
                AbsentCount = absent.Values.Sum(); ReadingCount = reading.Values.Sum();
                StatusMessage = "Read-only overview mula sa naka-save na TeachFlex records.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reports"); }
            finally { if (version == _loadVersion) IsBusy = false; }
        }

        [RelayCommand]
        private void ExportReport()
        {
            if (_school == null || _year == null || SelectedClass == null || LearnerRows.Count == 0)
            { _dialogs.ShowWarning("Mag-load muna ng class report na may learners.", "Reports"); return; }
            var dialog = new SaveFileDialog
            {
                Title = "Export Class Overview", Filter = "PDF Document (*.pdf)|*.pdf",
                DefaultExt = ".pdf", AddExtension = true, OverwritePrompt = true,
                FileName = $"Class-Overview-{SelectedClass.Id}-Term-{SelectedTerm}.pdf"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
                var rows = LearnerRows.ToList();
                var school = _school; var year = _year; var schoolClass = SelectedClass;
                Document.Create(doc => doc.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); page.Margin(26);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8));
                    page.Header().Column(c =>
                    {
                        c.Item().Text(school.SchoolName).FontSize(14).Bold();
                        c.Item().Text($"CLASS OVERVIEW • {schoolClass.DisplayName} • S.Y. {year.DisplayName} • Term {SelectedTerm}");
                        c.Item().Text($"Attendance: {SelectedMonth:MMMM yyyy} • Grades recorded: {GradesRecorded} • Reading assessments: {ReadingCount}");
                    });
                    page.Content().PaddingTop(12).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.ConstantColumn(30); c.RelativeColumn(4); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        t.Header(h =>
                        {
                            foreach (var label in new[] { "#", "Learner", "Term grades recorded", "Present", "Absent", "Reading assessments" })
                                h.Cell().Border(0.5f).Padding(5).Text(label).Bold();
                        });
                        foreach (var row in rows)
                        {
                            foreach (var value in new[] { row.Number.ToString(), row.Name, row.GradesRecorded.ToString(), row.Present.ToString(), row.Absent.ToString(), row.ReadingAssessments.ToString() })
                                t.Cell().Border(0.5f).Padding(4).Text(value);
                        }
                    });
                    page.Footer().AlignCenter().Text("Read-only summary • Descriptive grades are available in their progress reports").FontSize(7);
                })).GeneratePdf(dialog.FileName);
                StatusMessage = "Nagawa ang class overview PDF.";
                _dialogs.ShowInformation($"Nagawa ang report:\n{dialog.FileName}", "Reports");
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Reports"); }
        }
    }

    public sealed record ReportLearnerRow(int Number, string Name, string Sex,
        int GradesRecorded, int Present, int Absent, int ReadingAssessments);
}
