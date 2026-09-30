using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        [RelayCommand]
        private void PrintPaceSummary()
        {
            if (SelectedClass == null ||
                SelectedSubject == null)
            {
                _dialogService.ShowWarning(
                    "Select a Grade 1 class and " +
                    "learning area first.",
                    "Print PACE Summary");

                return;
            }

            if (PaceLearners.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Grade 1 PACE record first.",
                    "Print PACE Summary");

                return;
            }

            try
            {
                PrintDialog printDialog =
                    new PrintDialog();

                if (printDialog.ShowDialog() != true)
                {
                    return;
                }

                double availableTableWidth =
                    Math.Max(
                        500,
                        printDialog.PrintableAreaWidth - 64);

                FlowDocument document =
                    CreatePacePrintDocument(
                        availableTableWidth);

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
                    $"Grade 1 PACE Summary - " +
                    $"{SelectedClass.DisplayName}");

                StatusMessage =
                    "Grade 1 PACE summary sent to printer.";

                _dialogService.ShowInformation(
                    "Your Grade 1 PACE summary was " +
                    "successfully sent to the selected printer.",
                    "Print Summary Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not print the Grade 1 " +
                    $"PACE summary.\n\n" +
                    $"{exception.Message}",
                    "Print PACE Summary Error");
            }
        }

        private FlowDocument CreatePacePrintDocument(
            double availableTableWidth)
        {
            FlowDocument document =
                new FlowDocument();

            AddFormalPrintHeader(
    document,
    "GRADE 1 PACE SUMMARY",
    availableTableWidth);

            document.Blocks.Add(
                CreatePaceInformationParagraph(
                    "Class",
                    SelectedClass?.DisplayName
                        ?? string.Empty));

            document.Blocks.Add(
                CreatePaceInformationParagraph(
                    "Learning Area",
                    SelectedSubject?.SubjectName
                        ?? string.Empty));

            document.Blocks.Add(
                CreatePaceInformationParagraph(
                    "Term",
                    SelectedTermText));

            document.Blocks.Add(
     CreatePaceInformationParagraph(
         "School Year",
         SummarySchoolYear));

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
                0.05,
                0.27,
                0.08,
                0.065,
                0.065,
                0.065,
                0.065,
                0.065,
                0.16,
                0.14
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

            TableRowGroup rows =
                new TableRowGroup();

            rows.Rows.Add(
                CreatePacePrintRow(
                    true,
                    "#",
                    "Learner",
                    "Sex",
                    "A",
                    "B",
                    "C",
                    "D",
                    "E",
                    "Rated / Total",
                    "Status"));

            foreach (PaceLearnerRow learner
                     in PaceLearners)
            {
                int totalCompetencies =
                    learner.Ratings.Count;

                int ratedCompetencies =
                    learner.Ratings.Count(
                        rating =>
                            !string.IsNullOrWhiteSpace(
                                rating.Rating));

                rows.Rows.Add(
                    CreatePacePrintRow(
                        false,
                        learner.Number.ToString(),
                        learner.LearnerName,
                        learner.Sex,
                        CountPaceRating(
                            learner,
                            "A").ToString(),
                        CountPaceRating(
                            learner,
                            "B").ToString(),
                        CountPaceRating(
                            learner,
                            "C").ToString(),
                        CountPaceRating(
                            learner,
                            "D").ToString(),
                        CountPaceRating(
                            learner,
                            "E").ToString(),
                        $"{ratedCompetencies} / " +
                        $"{totalCompetencies}",
                        totalCompetencies > 0 &&
                        ratedCompetencies ==
                            totalCompetencies
                            ? "Complete"
                            : "Incomplete"));
            }

            table.RowGroups.Add(
                rows);

            document.Blocks.Add(
                table);

            document.Blocks.Add(
                new Paragraph(
                    new Run(
                        "PACE Rating Guide: " +
                        "A, B, C, D, and E are descriptive " +
                        "ratings based on sufficient and " +
                        "varied evidence of learning."))
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

        private static int CountPaceRating(
            PaceLearnerRow learner,
            string ratingValue)
        {
            return learner.Ratings.Count(
                rating =>
                    string.Equals(
                        rating.Rating?.Trim(),
                        ratingValue,
                        StringComparison.OrdinalIgnoreCase));
        }

        private static Paragraph
            CreatePaceInformationParagraph(
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

        private static TableRow CreatePacePrintRow(
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
    }
}