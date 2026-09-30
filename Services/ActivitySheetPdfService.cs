using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public sealed class ActivitySheetPdfService
    {
        public void Export(ActivitySheet sheet, LessonPlan plan, string school, string year,
            string className, string subject, bool teacherCopy, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text(school).Bold().FontSize(11);
                    header.Item().AlignCenter().Text(teacherCopy ? "TEACHER COPY · ACTIVITY SHEET" : "LEARNER ACTIVITY SHEET").Bold().FontSize(16);
                    header.Item().AlignCenter().Text($"{year} · {className} · {subject} · Term {plan.TermNumber}, Week {plan.WeekNumber}, Session {sheet.Session}").FontSize(9);
                });
                page.Content().PaddingTop(18).Column(content =>
                {
                    content.Spacing(12);
                    content.Item().Text($"Name: ____________________________________     Date: _______________");
                    content.Item().Text(sheet.Title).Bold().FontSize(14);
                    content.Item().Text($"Learning Competency: {plan.LearningCompetency}").FontSize(9);
                    content.Item().Text("Panuto: " + sheet.Instructions);
                    if (!string.IsNullOrWhiteSpace(sheet.ImageBase64))
                    {
                        content.Item().Height(160).Image(Convert.FromBase64String(sheet.ImageBase64)).FitArea();
                        content.Item().Text($"Larawan: {sheet.ImageTitle} · {sheet.ImageAuthor} · {sheet.ImageLicense}\n" +
                            $"Source: {sheet.ImageSourceUrl}\nLisensiya: {sheet.ImageLicenseUrl}").FontSize(7);
                    }
                    content.Item().Text(sheet.Questions).FontSize(11);
                    if (teacherCopy)
                    {
                        content.Item().PaddingTop(12).LineHorizontal(1);
                        content.Item().Text("GABAY SA GURO / INAASAHANG SAGOT").Bold();
                        content.Item().Text(sheet.TeacherGuide);
                    }
                });
                page.Footer().AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
            })).GeneratePdf(path);
        }
    }
}
