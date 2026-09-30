using System;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        [RelayCommand]
        private void ExportPaceSummaryPdf()
        {
            if (SelectedClass == null ||
                SelectedSubject == null)
            {
                _dialogService.ShowWarning(
                    "Select a Grade 1 class and " +
                    "learning area first.",
                    "Export PACE PDF");

                return;
            }

            if (PaceLearners.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Grade 1 PACE record first.",
                    "Export PACE PDF");

                return;
            }

            try
            {
                SaveFileDialog saveDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Export Grade 1 PACE Summary",

                        Filter =
                            "PDF Document (*.pdf)|*.pdf",

                        DefaultExt =
                            ".pdf",

                        AddExtension =
                            true,

                        FileName =
                            CreateSafePacePdfFileName(
                                $"Grade 1 PACE Summary - " +
                                $"{SelectedClass.DisplayName} - " +
                                $"{SelectedSubject.SubjectName} - " +
                                $"{SelectedTermText}.pdf")
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
                    SelectedSubject.SubjectName;

                string termName =
                    SelectedTermText;

                string schoolYear =
                    SummarySchoolYear;

                PaceLearnerRow[] learners =
                    PaceLearners.ToArray();

                Document.Create(
                    document =>
                    {
                        document.Page(
                            page =>
                            {
                                page.Size(
                                    PageSizes.A4
                                        .Landscape());

                                page.Margin(
                                    30);

                                page.DefaultTextStyle(
                                    style =>
                                        style.FontFamily(
                                                "Arial")
                                            .FontSize(
                                                9));

                                page.Header()
                                    .Column(
                                        header =>
                                        {
                                            header.Item()
                                                .Element(
                                                    container =>
                                                        ComposeFormalPdfHeader(
                                                            container,
                                                            "GRADE 1 " +
                                                            "PACE SUMMARY"));

                                            header.Item()
                                                .PaddingTop(
                                                    10)
                                                .Text(
                                                    $"Class: " +
                                                    $"{className}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"Learning Area: " +
                                                    $"{subjectName}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"Term: " +
                                                    $"{termName}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"School Year: " +
                                                    $"{schoolYear}")
                                                .SemiBold();
                                        });

                                page.Content()
                                    .PaddingTop(
                                        14)
                                    .Column(
                                        content =>
                                        {
                                            content.Item()
                                                .Table(
                                                    table =>
                                                    {
                                                        table
                                                            .ColumnsDefinition(
                                                                columns =>
                                                                {
                                                                    columns
                                                                        .ConstantColumn(
                                                                            35);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            3);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            1);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            0.7f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            0.7f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            0.7f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            0.7f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            0.7f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            1.5f);

                                                                    columns
                                                                        .RelativeColumn(
                                                                            1.4f);
                                                                });

                                                        table.Header(
                                                            header =>
                                                            {
                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "#");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "Learner");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "Sex");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "A");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "B");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "C");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "D");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "E");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "Rated / Total");

                                                                AddPacePdfHeaderCell(
                                                                    header,
                                                                    "Status");
                                                            });

                                                        foreach (
                                                            PaceLearnerRow learner
                                                            in learners)
                                                        {
                                                            int total =
                                                                learner
                                                                    .Ratings
                                                                    .Count;

                                                            int rated =
                                                                learner
                                                                    .Ratings
                                                                    .Count(
                                                                        rating =>
                                                                            !string
                                                                                .IsNullOrWhiteSpace(
                                                                                    rating.Rating));

                                                            AddPacePdfDataCell(
                                                                table,
                                                                learner.Number
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                learner
                                                                    .LearnerName,
                                                                false);

                                                            AddPacePdfDataCell(
                                                                table,
                                                                learner.Sex);

                                                            AddPacePdfDataCell(
                                                                table,
                                                                CountPaceRating(
                                                                    learner,
                                                                    "A")
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                CountPaceRating(
                                                                    learner,
                                                                    "B")
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                CountPaceRating(
                                                                    learner,
                                                                    "C")
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                CountPaceRating(
                                                                    learner,
                                                                    "D")
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                CountPaceRating(
                                                                    learner,
                                                                    "E")
                                                                    .ToString());

                                                            AddPacePdfDataCell(
                                                                table,
                                                                $"{rated} / " +
                                                                $"{total}");

                                                            AddPacePdfDataCell(
                                                                table,
                                                                total > 0 &&
                                                                rated == total
                                                                    ? "Complete"
                                                                    : "Incomplete");
                                                        }
                                                    });

                                            content.Item()
                                                .PaddingTop(
                                                    10)
                                                .Text(
                                                    "PACE Rating Guide: " +
                                                    "A, B, C, D, and E are " +
                                                    "descriptive ratings based " +
                                                    "on sufficient and varied " +
                                                    "evidence of learning.")
                                                .Italic()
                                                .FontSize(
                                                    8)
                                                .FontColor(
                                                    Colors.Grey.Darken1);

                                            content.Item()
                                                .Element(
                                                    container =>
                                                        ComposeFormalPdfSignatures(
                                                            container));
                                        });

                                page.Footer()
                                    .AlignCenter()
                                    .Text(
                                        footer =>
                                        {
                                            footer.Span(
                                                "Page ");

                                            footer
                                                .CurrentPageNumber();

                                            footer.Span(
                                                " of ");

                                            footer.TotalPages();
                                        });
                            });
                    })
                    .GeneratePdf(
                        saveDialog.FileName);

                StatusMessage =
                    "Grade 1 PACE summary PDF exported.";

                _dialogService.ShowInformation(
                    "Your Grade 1 PACE summary PDF " +
                    "has been saved successfully.",
                    "Export PDF Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not export the " +
                    $"Grade 1 PACE summary PDF.\n\n" +
                    $"{exception.Message}",
                    "Export PACE PDF Error");
            }
        }

        private static void AddPacePdfHeaderCell(
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

        private static void AddPacePdfDataCell(
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

        private static string CreateSafePacePdfFileName(
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