using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ILessonPlanPdfService
    {
        void Export(LessonPlan plan, string className, string subjectName,
            string schoolYear, string schoolName, string schoolHead, string outputPath);
    }

    public sealed class LessonPlanPdfService : ILessonPlanPdfService
    {
        private const string Slate = "E7EDF3";
        private const string Light = "F5F8FB";

        public void Export(LessonPlan plan, string className, string subjectName,
            string schoolYear, string schoolName, string schoolHead, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
            Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(7.5f));
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("Republic of the Philippines").FontSize(8);
                    header.Item().AlignCenter().Text("Department of Education").Bold().FontSize(11);
                    header.Item().AlignCenter().Text(string.IsNullOrWhiteSpace(schoolName) ? "School Name" : schoolName).Bold();
                    header.Item().AlignCenter().PaddingBottom(7).Text("WEEKLY ILAW LESSON PLAN").Bold().FontSize(13);
                });
                page.Content().PaddingTop(5).Column(content =>
                {
                    content.Spacing(7);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); });
                        Info(table, "Learning Area", subjectName);
                        Info(table, "Grade and Section", className);
                        Info(table, "Term / Week", $"Term {plan.TermNumber} · Week {plan.WeekNumber}");
                        Info(table, "School Year / Start Date", $"{schoolYear} · {plan.LessonDate:MMMM d, yyyy}");
                        Info(table, "Lesson Title (Session 1)", Fallback(plan.Session1Title, plan.Title));
                        Info(table, "BOW Source", plan.BowSource);
                    });
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.RelativeColumn(1.15f); for (int i = 0; i < 5; i++) columns.RelativeColumn(1); });
                        Band(table, "I · INTENTIONS");
                        RowSpan(table, "Content Standard", plan.ContentStandard);
                        RowSpan(table, "Performance Standard", plan.PerformanceStandard);
                        HeaderCell(table, "Part");
                        for (int i = 1; i <= 4; i++) HeaderCell(table, $"SESSION {i}");
                        HeaderCell(table, "SESSION 5 · WEEKLY TEST");
                        DayRow(table, "Lesson Title", Fallback(plan.Session1Title, plan.Title),
                            Fallback(plan.Session2Title, plan.Title), Fallback(plan.Session3Title, plan.Title),
                            Fallback(plan.Session4Title, plan.Title), "");
                        DayRow(table, "Learning Competency", Fallback(plan.Session1Competency, plan.LearningCompetency),
                            Fallback(plan.Session2Competency, plan.LearningCompetency), Fallback(plan.Session3Competency, plan.LearningCompetency),
                            Fallback(plan.Session4Competency, plan.LearningCompetency), "");
                        DayRow(table, "Learning Objectives", Fallback(plan.Session1Objectives, plan.Objectives),
                            Fallback(plan.Session2Objectives, plan.Objectives), Fallback(plan.Session3Objectives, plan.Objectives),
                            Fallback(plan.Session4Objectives, plan.Objectives), "");
                        Band(table, "L · LEARNING EXPERIENCES");
                        HeaderCell(table, "Part");
                        for (int i = 1; i <= 4; i++) HeaderCell(table, $"SESSION {i}");
                        HeaderCell(table, "SESSION 5 · WEEKLY TEST");
                        DayRow(table, "Pre-Lesson / Motivation",
                            Motivation(plan.Session1Motivation, plan.PreLesson), Motivation(plan.Session2Motivation, plan.PreLesson),
                            Motivation(plan.Session3Motivation, plan.PreLesson), Motivation(plan.Session4Motivation, plan.PreLesson), "");
                        DayRow(table, "Lesson Flow / Activities", plan.Day1, plan.Day2, plan.Day3, plan.Day4, "");
                        RowSpan(table, "Learning Resources", plan.Materials);
                        Band(table, "A · ASSESSMENT");
                        DayRow(table, "Formative Checks", plan.Day1Assessment, plan.Day2Assessment, plan.Day3Assessment, plan.Day4Assessment, "");
                        DayRow(table, "Weekly Test", "", "", "", "", plan.WeeklyTest);
                        Band(table, "W · WAYS FORWARD");
                        DayRow(table, "Extend Learning / Remediation",
                            Fallback(plan.Session1ExtendLearning, plan.ExtendLearning), Fallback(plan.Session2ExtendLearning, plan.ExtendLearning),
                            Fallback(plan.Session3ExtendLearning, plan.ExtendLearning), Fallback(plan.Session4ExtendLearning, plan.ExtendLearning),
                            Fallback(plan.Session5ExtendLearning, plan.ExtendLearning));
                        DayRow(table, "Teacher Reflection",
                            Fallback(plan.Session1Reflection, plan.Reflection), Fallback(plan.Session2Reflection, plan.Reflection),
                            Fallback(plan.Session3Reflection, plan.Reflection), Fallback(plan.Session4Reflection, plan.Reflection),
                            Fallback(plan.Session5Reflection, plan.Reflection));
                    });
                    content.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Prepared by: __________________________");
                        row.RelativeItem().Text("Checked by: __________________________");
                        row.RelativeItem().Text($"Approved by: {(string.IsNullOrWhiteSpace(schoolHead) ? "__________________________" : schoolHead)}");
                    });
                });
                page.Footer().AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
            })).GeneratePdf(outputPath);
        }

        private static string Value(string? text) => string.IsNullOrWhiteSpace(text) ? "—" : text.Trim();
        private static string Motivation(string? session, string? legacy) =>
            string.IsNullOrWhiteSpace(session) ? legacy ?? string.Empty : session;
        private static string Fallback(string? session, string? legacy) =>
            string.IsNullOrWhiteSpace(session) ? legacy ?? string.Empty : session;
        private static IContainer Box(IContainer cell) => cell.Border(0.5f).BorderColor("AAB5C1").Padding(4);
        private static void Info(TableDescriptor table, string label, string value)
        {
            Box(table.Cell()).Background(Light).Text(label).Bold();
            Box(table.Cell()).Text(Value(value));
        }
        private static void Band(TableDescriptor table, string heading) =>
            Box(table.Cell().ColumnSpan(6)).Background(Slate).Text(heading).Bold();
        private static void HeaderCell(TableDescriptor table, string heading) =>
            Box(table.Cell()).Background(Light).Text(heading).Bold();
        private static void RowSpan(TableDescriptor table, string label, string value)
        {
            Box(table.Cell()).Background(Light).Text(label).Bold();
            Box(table.Cell().ColumnSpan(5)).Text(Value(value));
        }
        private static void DayRow(TableDescriptor table, string label, string a, string b, string c, string d, string e)
        {
            Box(table.Cell()).Background(Light).Text(label).Bold();
            foreach (string value in new[] { a, b, c, d, e }) Box(table.Cell()).Text(Value(value));
        }
    }
}
