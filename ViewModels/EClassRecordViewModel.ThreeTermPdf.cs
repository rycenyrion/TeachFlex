using System;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        [RelayCommand]
        private void ExportThreeTermSummaryPdf()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Export PDF");

                return;
            }

            if (!HasThreeTermSummary)
            {
                _dialogService.ShowWarning(
                    "Click Prepare Summary first.",
                    "Export PDF");

                return;
            }

            try
            {
                string suggestedFileName =
                    CreateSafePdfFileName(
                        $"All-Subject Three-Term Summary - " +
                        $"{SelectedClass.DisplayName}.pdf");

                SaveFileDialog saveDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Export Three-Term Grade Summary",

                        Filter =
                            "PDF Document (*.pdf)|*.pdf",

                        DefaultExt =
                            ".pdf",

                        AddExtension =
                            true,

                        FileName =
                            suggestedFileName
                    };

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                QuestPDF.Settings.License =
                    LicenseType.Community;

                string className =
                    SelectedClass.DisplayName;

                string subjectName =
                    "All assigned numerical subjects";

                string schoolYear =
                    SummarySchoolYear;

                TermSubjectSummarySection[] sections = TermSubjectSections.ToArray();
                Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(sections.Length > 0 &&
                                  sections[0].Subjects.Count > 7
                            ? PageSizes.A3.Landscape()
                            : PageSizes.A4.Landscape());
                        page.Margin(24);
                        page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(7));
                        page.Header().Column(header =>
                        {
                            header.Item().Element(container => ComposeFormalPdfHeader(
                                container, "ALL-SUBJECT TERM SUMMARY"));
                            header.Item().Text($"Class: {className}    School Year: {schoolYear}").SemiBold();
                        });
                        page.Content().PaddingTop(12).Column(content =>
                        {
                            content.Spacing(12);
                            foreach (TermSubjectSummarySection section in sections)
                            {
                                content.Item().AlignCenter().Text(section.Title).Bold().FontSize(11);
                                content.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(27);
                                        columns.RelativeColumn(2.2f);
                                        columns.ConstantColumn(34);
                                        foreach (string subject in section.Subjects)
                                            columns.RelativeColumn(1.2f);
                                        columns.ConstantColumn(50);
                                        columns.ConstantColumn(65);
                                    });
                                    table.Header(header =>
                                    {
                                        AddPdfHeaderCell(header, "No.");
                                        AddPdfHeaderCell(header, "Learner");
                                        AddPdfHeaderCell(header, "Sex");
                                        foreach (string subject in section.Subjects)
                                            AddPdfHeaderCell(header, subject);
                                        AddPdfHeaderCell(header, "Average");
                                        AddPdfHeaderCell(header, "Status");
                                    });
                                    foreach (TermSubjectSummaryRow row in section.Rows)
                                    {
                                        AddPdfDataCell(table, row.Number.ToString());
                                        AddPdfDataCell(table, row.LearnerName, false);
                                        AddPdfDataCell(table, row.Sex);
                                        foreach (TermSubjectGradeCell grade in row.Grades)
                                            AddPdfDataCell(table, grade.GradeText);
                                        AddPdfDataCell(table, row.AverageText);
                                        AddPdfDataCell(table, row.Status);
                                    }
                                });
                            }
                            content.Item().Element(container => ComposeFormalPdfSignatures(container));
                        });
                        page.Footer().AlignCenter().Text(footer =>
                        {
                            footer.Span("Page ");
                            footer.CurrentPageNumber();
                            footer.Span(" of ");
                            footer.TotalPages();
                        });
                    });
                }).GeneratePdf(saveDialog.FileName);

                StatusMessage =
                    "Three-term summary PDF exported.";

                _dialogService.ShowInformation(
                    "Your PDF document has been " +
                    "saved successfully.",
                    "Export PDF Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not export the " +
                    $"three-term summary PDF.\n\n" +
                    $"{exception.Message}",
                    "Export PDF Error");
            }
        }

        private static void AddPdfHeaderCell(
            TableCellDescriptor table,
            string text)
        {
            table.Cell()
                .Background(
                    "#DBEAFE")
                .Border(
                    0.5f)
                .BorderColor(
                    "#64748B")
                .Padding(
                    5)
                .AlignCenter()
                .AlignMiddle()
                .Text(
                    text)
                .SemiBold();
        }

        private static void AddPdfDataCell(
            TableDescriptor table,
            string text,
            bool centerText = true)
        {
            IContainer cell =
                table.Cell()
                    .Border(
                        0.5f)
                    .BorderColor(
                        "#94A3B8")
                    .Padding(
                        5)
                    .AlignMiddle();

            if (centerText)
            {
                cell =
                    cell.AlignCenter();
            }

            cell.Text(
                text);
        }

        private static string CreateSafePdfFileName(
            string fileName)
        {
            foreach (char invalidCharacter
                     in Path.GetInvalidFileNameChars())
            {
                fileName =
                    fileName.Replace(
                        invalidCharacter,
                        '-');
            }

            return fileName;
        }
    }
}
