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
    public partial class EClassRecordViewModel
    {
        [RelayCommand]
        private async Task
            ExportKindergartenSummaryPdfAsync()
        {
            if (!IsKindergartenRecord ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class first.",
                    "Export Kinder PDF");

                return;
            }

            if (KindergartenLearners.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Kindergarten class record first.",
                    "Export Kinder PDF");

                return;
            }

            try
            {
                SaveFileDialog saveDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Export Kindergarten Summary",

                        Filter =
                            "PDF Document (*.pdf)|*.pdf",

                        DefaultExt =
                            ".pdf",

                        AddExtension =
                            true,

                        FileName =
                            CreateSafeKindergartenPdfFileName(
                                $"Kindergarten Summary - " +
                                $"{SelectedClass.DisplayName} - " +
                                $"{SelectedTermText}.pdf")
                    };

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                IsBusy =
                    true;

                StatusMessage =
                    "Preparing the Kindergarten PDF...";

                IReadOnlyList<KindergartenCompetency>
                    competencies =
                        await _kindergartenRecordRepository
                            .GetCompetenciesAsync();

                IReadOnlyList<KindergartenLearnerRating>
                    savedRatings =
                        await _kindergartenRecordRepository
                            .GetRatingsAsync(
                                SelectedClass.Id,
                                SelectedTerm);

                List<KindergartenPrintSummaryRow>
                    summaryRows =
                        CreateKindergartenPrintSummaryRows(
                            competencies,
                            savedRatings);

                QuestPDF.Settings.License =
                    LicenseType.Community;

                string className =
                    SelectedClass.DisplayName;

                string termName =
                    SelectedTermText;

                string schoolYear =
                    SummarySchoolYear;

                int totalCompetencies =
                    competencies.Count;

                KindergartenPrintSummaryRow[] rows =
                    summaryRows.ToArray();

                Document.Create(
                    document =>
                    {
                        document.Page(
                            page =>
                            {
                                page.Size(
                                    PageSizes.A4.Landscape());

                                page.Margin(
                                    30);

                                page.DefaultTextStyle(
                                    style =>
                                        style.FontFamily("Arial")
                                            .FontSize(9));

                                page.Header()
                                    .Column(
                                        header =>
                                        {
                                            header.Item()
                                                .Element(
                                                    container =>
                                                        ComposeFormalPdfHeader(
                                                            container,
                                                            "KINDERGARTEN " +
                                                            "COMPETENCY SUMMARY"));

                                            header.Item()
                                                .PaddingTop(10)
                                                .Text(
                                                    $"Class: {className}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"Term: {termName}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"School Year: {schoolYear}")
                                                .SemiBold();

                                            header.Item()
                                                .Text(
                                                    $"Total Competencies: " +
                                                    $"{totalCompetencies}")
                                                .SemiBold();
                                        });

                                page.Content()
                                    .PaddingTop(14)
                                    .Column(
                                        content =>
                                        {
                                            content.Item()
                                                .Table(
                                                    table =>
                                                    {
                                                        table.ColumnsDefinition(
                                                            columns =>
                                                            {
                                                                columns.ConstantColumn(35);
                                                                columns.RelativeColumn(3);
                                                                columns.RelativeColumn(1);
                                                                columns.RelativeColumn(0.8f);
                                                                columns.RelativeColumn(0.8f);
                                                                columns.RelativeColumn(0.8f);
                                                                columns.RelativeColumn(1.4f);
                                                                columns.RelativeColumn(1.3f);
                                                            });

                                                        table.Header(
                                                            header =>
                                                            {
                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "#");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "Learner");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "Sex");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "BG");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "DV");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "CO");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "Rated / Total");

                                                                AddKinderPdfHeaderCell(
                                                                    header,
                                                                    "Status");
                                                            });

                                                        foreach (
                                                            KindergartenPrintSummaryRow row
                                                            in rows)
                                                        {
                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.Number.ToString());

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.LearnerName,
                                                                false);

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.Sex);

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.BeginningCount
                                                                    .ToString());

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.DevelopingCount
                                                                    .ToString());

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.ConsistentCount
                                                                    .ToString());

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                $"{row.RatedCount} / " +
                                                                $"{row.TotalCount}");

                                                            AddKinderPdfDataCell(
                                                                table,
                                                                row.Status);
                                                        }
                                                    });

                                            content.Item()
                                                .PaddingTop(10)
                                                .Text(
                                                    "Rating Guide: " +
                                                    "BG — Beginning, " +
                                                    "DV — Developing, " +
                                                    "CO — Consistent.")
                                                .Italic()
                                                .FontSize(8)
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
                                            footer.Span("Page ");
                                            footer.CurrentPageNumber();
                                            footer.Span(" of ");
                                            footer.TotalPages();
                                        });
                            });
                    })
                    .GeneratePdf(
                        saveDialog.FileName);

                StatusMessage =
                    "Kindergarten summary PDF exported.";

                _dialogService.ShowInformation(
                    "Your Kindergarten summary PDF " +
                    "has been saved successfully.",
                    "Export PDF Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not export the " +
                    $"Kindergarten summary PDF.\n\n" +
                    $"{exception.Message}",
                    "Export Kinder PDF Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private static void AddKinderPdfHeaderCell(
            TableCellDescriptor table,
            string text)
        {
            table.Cell()
                .Background("#DBEAFE")
                .Border(0.5f)
                .BorderColor("#64748B")
                .Padding(5)
                .AlignCenter()
                .AlignMiddle()
                .Text(text)
                .SemiBold();
        }

        private static void AddKinderPdfDataCell(
            TableDescriptor table,
            string text,
            bool centerText = true)
        {
            IContainer cell =
                table.Cell()
                    .Border(0.5f)
                    .BorderColor("#94A3B8")
                    .Padding(5)
                    .AlignMiddle();

            if (centerText)
            {
                cell =
                    cell.AlignCenter();
            }

            cell.Text(
                text);
        }

        private static string
            CreateSafeKindergartenPdfFileName(
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