using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        [RelayCommand]
        private async Task
            PrintKindergartenSummaryAsync()
        {
            if (!IsKindergartenRecord ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class first.",
                    "Print Kinder Summary");

                return;
            }

            if (KindergartenLearners.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Kindergarten class record first.",
                    "Print Kinder Summary");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Preparing the Kindergarten summary...";

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

                PrintDialog printDialog =
                    new PrintDialog();

                if (printDialog.ShowDialog() != true)
                {
                    StatusMessage =
                        "Kindergarten summary printing cancelled.";

                    return;
                }

                double availableTableWidth =
                    Math.Max(
                        500,
                        printDialog.PrintableAreaWidth - 64);

                FlowDocument document =
                    CreateKindergartenPrintDocument(
                        availableTableWidth,
                        competencies.Count,
                        summaryRows);

                document.PageWidth =
                    printDialog.PrintableAreaWidth;

                document.PageHeight =
                    printDialog.PrintableAreaHeight;

                document.PagePadding =
                    new Thickness(
                        32);

                document.ColumnWidth =
                    double.PositiveInfinity;

                document.FontFamily =
                    new FontFamily(
                        "Arial");

                document.FontSize =
                    9;

                IDocumentPaginatorSource
                    paginatorSource =
                        document;

                printDialog.PrintDocument(
                    paginatorSource
                        .DocumentPaginator,
                    $"Kindergarten Summary - " +
                    $"{SelectedClass.DisplayName}");

                StatusMessage =
                    "Kindergarten summary sent to printer.";

                _dialogService.ShowInformation(
                    "Your Kindergarten summary was successfully " +
                    "sent to the selected printer.",
                    "Print Summary Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not print the Kindergarten " +
                    $"summary.\n\n{exception.Message}",
                    "Print Kinder Summary Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private List<KindergartenPrintSummaryRow>
            CreateKindergartenPrintSummaryRows(
                IReadOnlyList<KindergartenCompetency>
                    competencies,
                IReadOnlyList<KindergartenLearnerRating>
                    savedRatings)
        {
            List<KindergartenPrintSummaryRow>
                rows =
                    new List<
                        KindergartenPrintSummaryRow>();

            int number =
                1;

            foreach (Learner learner
                     in KindergartenLearners)
            {
                List<KindergartenLearnerRating>
                    learnerRatings =
                        savedRatings
                            .Where(
                                rating =>
                                    rating.LearnerId ==
                                        learner.Id)
                            .GroupBy(
                                rating =>
                                    rating
                                        .KindergartenCompetencyId)
                            .Select(
                                group =>
                                    group
                                        .OrderByDescending(
                                            rating =>
                                                rating.UpdatedAtUtc)
                                        .First())
                            .ToList();

                int ratedCount =
                    learnerRatings.Count(
                        rating =>
                            !string.IsNullOrWhiteSpace(
                                rating.Rating));

                rows.Add(
                    new KindergartenPrintSummaryRow
                    {
                        Number =
                            number++,

                        LearnerName =
                            learner.OfficialName,

                        Sex =
                            learner.Sex,

                        BeginningCount =
                            CountKindergartenRating(
                                learnerRatings,
                                "BG"),

                        DevelopingCount =
                            CountKindergartenRating(
                                learnerRatings,
                                "DV"),

                        ConsistentCount =
                            CountKindergartenRating(
                                learnerRatings,
                                "CO"),

                        RatedCount =
                            ratedCount,

                        TotalCount =
                            competencies.Count,

                        Status =
                            competencies.Count > 0 &&
                            ratedCount ==
                                competencies.Count
                                ? "Complete"
                                : "Incomplete"
                    });
            }

            return rows;
        }

        private FlowDocument
            CreateKindergartenPrintDocument(
                double availableTableWidth,
                int totalCompetencies,
                IReadOnlyList<
                    KindergartenPrintSummaryRow>
                        summaryRows)
        {
            FlowDocument document =
                new FlowDocument();

            AddFormalPrintHeader(
    document,
    "KINDERGARTEN COMPETENCY SUMMARY",
    availableTableWidth);

            document.Blocks.Add(
                CreateKindergartenInformationParagraph(
                    "Class",
                    SelectedClass?.DisplayName
                        ?? string.Empty));

            document.Blocks.Add(
                CreateKindergartenInformationParagraph(
                    "Term",
                    SelectedTermText));

            document.Blocks.Add(
     CreateKindergartenInformationParagraph(
         "School Year",
         SummarySchoolYear));

            document.Blocks.Add(
                CreateKindergartenInformationParagraph(
                    "Total Competencies",
                    totalCompetencies.ToString()));

            document.Blocks.Add(
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            8)
                });

            Table table =
                new Table
                {
                    CellSpacing =
                        0
                };

            double[] columnRatios =
            {
                0.06,
                0.31,
                0.10,
                0.09,
                0.09,
                0.09,
                0.14,
                0.12
            };

            foreach (double ratio
                     in columnRatios)
            {
                table.Columns.Add(
                    new TableColumn
                    {
                        Width =
                            new GridLength(
                                availableTableWidth *
                                ratio)
                    });
            }

            TableRowGroup tableRows =
                new TableRowGroup();

            tableRows.Rows.Add(
                CreateKindergartenPrintRow(
                    true,
                    "#",
                    "Learner",
                    "Sex",
                    "BG",
                    "DV",
                    "CO",
                    "Rated / Total",
                    "Status"));

            foreach (
                KindergartenPrintSummaryRow row
                in summaryRows)
            {
                tableRows.Rows.Add(
                    CreateKindergartenPrintRow(
                        false,
                        row.Number.ToString(),
                        row.LearnerName,
                        row.Sex,
                        row.BeginningCount.ToString(),
                        row.DevelopingCount.ToString(),
                        row.ConsistentCount.ToString(),
                        $"{row.RatedCount} / " +
                        $"{row.TotalCount}",
                        row.Status));
            }

            table.RowGroups.Add(
                tableRows);

            document.Blocks.Add(
                table);

            document.Blocks.Add(
                new Paragraph(
                    new Run(
                        "Rating Guide: BG — Beginning, " +
                        "DV — Developing, CO — Consistent."))
                {
                    FontSize =
                        8,

                    FontStyle =
                        FontStyles.Italic,

                    Foreground =
                        Brushes.DimGray,

                    Margin =
                        new Thickness(
                            0,
                            10,
                            0,
                            0)
                });
            AddFormalPrintSignatures(
    document);
            return document;
        }

        private static int CountKindergartenRating(
            IEnumerable<KindergartenLearnerRating>
                ratings,
            string ratingValue)
        {
            return ratings.Count(
                rating =>
                    string.Equals(
                        rating.Rating?.Trim(),
                        ratingValue,
                        StringComparison.OrdinalIgnoreCase));
        }

        private static Paragraph
            CreateKindergartenInformationParagraph(
                string label,
                string value)
        {
            Paragraph paragraph =
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            0,
                            1,
                            0,
                            1)
                };

            paragraph.Inlines.Add(
                new Bold(
                    new Run(
                        $"{label}: ")));

            paragraph.Inlines.Add(
                new Run(
                    value));

            return paragraph;
        }

        private static TableRow
            CreateKindergartenPrintRow(
                bool isHeader,
                params string[] values)
        {
            TableRow row =
                new TableRow
                {
                    Background =
                        isHeader
                            ? new SolidColorBrush(
                                Color.FromRgb(
                                    219,
                                    234,
                                    254))
                            : Brushes.White
                };

            foreach (string value
                     in values)
            {
                Paragraph paragraph =
                    new Paragraph(
                        new Run(
                            value))
                    {
                        Margin =
                            new Thickness(
                                3),

                        TextAlignment =
                            TextAlignment.Center
                    };

                if (isHeader)
                {
                    paragraph.FontWeight =
                        FontWeights.Bold;
                }

                row.Cells.Add(
                    new TableCell(
                        paragraph)
                    {
                        BorderBrush =
                            Brushes.Gray,

                        BorderThickness =
                            new Thickness(
                                0.5),

                        Padding =
                            new Thickness(
                                2)
                    });
            }

            return row;
        }

        private sealed class
            KindergartenPrintSummaryRow
        {
            public int Number
            {
                get;
                set;
            }

            public string LearnerName
            {
                get;
                set;
            } = string.Empty;

            public string Sex
            {
                get;
                set;
            } = string.Empty;

            public int BeginningCount
            {
                get;
                set;
            }

            public int DevelopingCount
            {
                get;
                set;
            }

            public int ConsistentCount
            {
                get;
                set;
            }

            public int RatedCount
            {
                get;
                set;
            }

            public int TotalCount
            {
                get;
                set;
            }

            public string Status
            {
                get;
                set;
            } = string.Empty;
        }
    }
}