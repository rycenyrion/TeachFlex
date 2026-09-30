using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        private int _gradeSlipTerm = 1;
        public int GradeSlipTerm
        {
            get => _gradeSlipTerm;
            set => SetProperty(ref _gradeSlipTerm, value);
        }

        [RelayCommand]
        private async Task ExportGradeSlipAsync()
        {
            var school = _currentSchool;
            var year = _currentAcademicYear;
            var schoolClass = SelectedClass;
            var learner = SelectedSf9Learner;
            int term = GradeSlipTerm;
            if (school == null || year == null || schoolClass == null || learner == null || term is < 1 or > 3)
            {
                _dialogService.ShowWarning("Pumili ng class, learner at term.", "Grade Slip");
                return;
            }
            string grade = schoolClass.GradeLevel.Trim();
            if (grade.Contains("Kinder", StringComparison.OrdinalIgnoreCase) ||
                grade.Equals("Grade 1", StringComparison.OrdinalIgnoreCase) ||
                grade.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                grade.Equals("G1", StringComparison.OrdinalIgnoreCase))
            {
                _dialogService.ShowInformation("Descriptive ang Kinder at Grade 1. Gamitin ang kanilang progress report sa SF9 para sa parent copy.", "Grade Slip");
                return;
            }

            var subjects = await _subjectRepository.GetByClassAsync(schoolClass.Id);
            if (subjects.Count == 0)
            {
                _dialogService.ShowWarning("Walang subjects na nakatalaga sa napiling class.", "Grade Slip");
                return;
            }
            var all = new List<(Subject Subject, List<(AssessmentItem Item, decimal? Score)> Rows)>();
            foreach (var subject in subjects.OrderBy(x => x.DisplayOrder).ThenBy(x => x.SubjectName))
            {
                var items = await _assessmentRepository.GetItemsAsync(schoolClass.Id, subject.Id, term);
                var scores = await _assessmentRepository.GetScoresAsync(schoolClass.Id, subject.Id, term);
                var rows = items.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
                    .Select(x => (Item: x, Score: scores.FirstOrDefault(s => s.AssessmentItemId == x.Id && s.LearnerId == learner.Id)?.Score)).ToList();
                all.Add((subject, rows));
            }
            var dialog = new SaveFileDialog
            {
                Title = "Export All-Subject Grade Slip", Filter = "PDF Document (*.pdf)|*.pdf",
                DefaultExt = ".pdf", AddExtension = true, OverwritePrompt = true,
                FileName = $"Grade-Slip-{learner.Id}-Term-{term}.pdf"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                IsBusy = true;
                string teacher = schoolClass.Adviser == null ? "________________________" :
                    $"{schoolClass.Adviser.FirstName} {schoolClass.Adviser.LastName}";
                QuestPDF.Settings.License = LicenseType.Community;
                Document.Create(doc =>
                {
                    for (int index = 0; index < all.Count; index += 2)
                    {
                        var pair = all.Skip(index).Take(2).ToList();
                        doc.Page(page => ComposeSlipPage(page, school, year.DisplayName, schoolClass, learner, teacher, term, pair));
                    }
                }).GeneratePdf(dialog.FileName);
                StatusMessage = $"Nagawa ang Grade Slip PDF para sa {all.Count} subjects.";
                _dialogService.ShowInformation($"Nagawa ang Grade Slip para sa {all.Count} subjects:\n{dialog.FileName}", "Grade Slip");
            }
            catch (Exception ex)
            {
                StatusMessage = "Hindi nagawa ang Grade Slip PDF.";
                _dialogService.ShowError(ex.Message, "Grade Slip");
            }
            finally { IsBusy = false; }
        }

        private static void ComposeSlipPage(PageDescriptor page, School school, string schoolYear,
            SchoolClass schoolClass, Learner learner, string teacher, int term,
            List<(Subject Subject, List<(AssessmentItem Item, decimal? Score)> Rows)> pair)
        {
            page.Size(PageSizes.A4);
            page.Margin(22);
            page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(8));
            page.Header().Row(row =>
            {
                row.ConstantItem(50).Height(60).Element(c => SlipLogo(c, school.DepEdLogoPath));
                row.RelativeItem().AlignCenter().Column(c =>
                {
                    c.Item().AlignCenter().Text("Republic of the Philippines • Department of Education").FontSize(8);
                    c.Item().AlignCenter().Text($"{school.Region} • {school.Division}").FontSize(7);
                    c.Item().AlignCenter().Text(school.SchoolName).FontSize(11).Bold();
                    c.Item().AlignCenter().Text("GRADE SLIP • PARENT COPY").FontSize(10).Bold();
                });
                row.ConstantItem(50).Height(60).Element(c => SlipLogo(c, school.SchoolLogoPath));
            });
            page.Content().PaddingTop(8).Column(column =>
            {
                column.Spacing(7);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Learner: {learner.OfficialName}");
                        c.Item().Text($"LRN: {learner.Lrn}");
                        c.Item().Text($"Term: {term}     S.Y.: {schoolYear}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Grade & Section: {schoolClass.DisplayName}");
                        c.Item().Text($"Teacher: {teacher}");
                    });
                });
                for (int index = 0; index < pair.Count; index++)
                {
                    var data = pair[index];
                    if (index > 0) column.Item().PaddingTop(8).LineHorizontal(0.8f);
                    column.Item().MinHeight(300).Element(c => ComposeSubjectSlip(c, data.Subject, data.Rows));
                }
            });
            page.Footer().AlignCenter().Text("Blangkong score: wala pang naitalang score sa ECR. • TeachFlex parent copy").FontSize(7);
        }

        private static void ComposeSubjectSlip(IContainer container, Subject subject,
            List<(AssessmentItem Item, decimal? Score)> scores)
        {
            container.Column(column =>
            {
                column.Spacing(5);
                column.Item().Text($"SUBJECT: {subject.SubjectName}").FontSize(10).Bold();
                var written = scores.Where(x => IsWrittenWork(x.Item) &&
                    (x.Item.HighestPossibleScore > 0 || x.Score.HasValue)).ToList();
                var performance = scores.Where(x => IsPerformanceTask(x.Item) &&
                    (x.Item.HighestPossibleScore > 0 || x.Score.HasValue)).ToList();
                var exams = scores.Where(x => x.Item.Category.Contains("Summative", StringComparison.OrdinalIgnoreCase) ||
                    x.Item.Category.Contains("Examination", StringComparison.OrdinalIgnoreCase)).ToList();
                string w = subject.WrittenOralWorksWeight > 0 ? $" {subject.WrittenOralWorksWeight}%" : "";
                string p = subject.PerformanceTasksWeight > 0 ? $" {subject.PerformanceTasksWeight}%" : "";
                string e = subject.SummativeTermExamWeight > 0 ? $" {subject.SummativeTermExamWeight}%" : "";
                column.Item().Element(c => SlipTable(c, $"WRITTEN/ORAL WORKS (WWs){w}", written, 1));
                column.Item().Element(c => SlipTable(c, $"PRODUCT/PERFORMANCE TASKS (PTs){p}", performance, 1));
                column.Item().Element(c => SlipTable(c, $"SUMMATIVE TEST & TERM EXAMINATION{e}", exams, 3));
                var other = scores.Where(x => !IsWrittenWork(x.Item) && !IsPerformanceTask(x.Item))
                    .Except(exams).ToList();
                if (other.Count > 0) column.Item().Element(c => SlipTable(c, "OTHER ASSESSMENTS", other, other.Count));
                column.Item().PaddingTop(5).AlignCenter().Text("______________________________________________    Parent's/Guardian's Name & Signature");
                if (scores.Count == 0)
                    column.Item().Text("Wala pang naka-save na assessment para sa subject at term na ito. I-check ang ECR ng subject na ito.").FontSize(8);
            });
        }

        private static bool IsWrittenWork(AssessmentItem item) =>
            item.Category.Contains("Written", StringComparison.OrdinalIgnoreCase) ||
            item.AssessmentName.StartsWith("WW", StringComparison.OrdinalIgnoreCase);

        private static bool IsPerformanceTask(AssessmentItem item) =>
            item.Category.Contains("Performance", StringComparison.OrdinalIgnoreCase) ||
            item.AssessmentName.StartsWith("PT", StringComparison.OrdinalIgnoreCase);

        private static void SlipLogo(IContainer container, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                container.Image(path).FitArea();
        }

        private static void SlipTable(IContainer container, string title,
            List<(AssessmentItem Item, decimal? Score)> rows, int minimum)
        {
            int count = Math.Max(minimum, rows.Count);
            container.Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    for (int i = 0; i < count; i++) c.RelativeColumn();
                });
                table.Cell().ColumnSpan((uint)(count + 1)).Border(0.5f).Padding(2).AlignCenter().Text(title).Bold();
                table.Cell().Border(0.5f).Padding(2).Text("Assessment");
                for (int i = 0; i < count; i++)
                    table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(i < rows.Count ? rows[i].Item.AssessmentName : rows.Count == 0 ? "—" : "").FontSize(7);
                table.Cell().Border(0.5f).Padding(2).Text("Highest Possible Score");
                for (int i = 0; i < count; i++)
                    table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(i < rows.Count && rows[i].Item.HighestPossibleScore > 0 ? rows[i].Item.HighestPossibleScore.ToString("0.##") : "");
                table.Cell().Border(0.5f).Padding(2).Text("Learner's Score");
                for (int i = 0; i < count; i++)
                    table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(i < rows.Count ? rows[i].Score?.ToString("0.##") ?? "" : "");
            });
        }
    }
}
