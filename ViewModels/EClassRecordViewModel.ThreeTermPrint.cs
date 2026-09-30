using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        [CommunityToolkit.Mvvm.Input.RelayCommand]
        private void PrintThreeTermSummary()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Print Summary");

                return;
            }

            if (!HasThreeTermSummary)
            {
                _dialogService.ShowWarning(
                    "Click Prepare Summary first.",
                    "Print Summary");

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
                    CreateThreeTermPrintDocument(
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
                    10;

                IDocumentPaginatorSource
                    paginatorSource =
                        document;

                printDialog.PrintDocument(
                    paginatorSource
                        .DocumentPaginator,
                    $"Three-Term Grade Summary - " +
                    $"{SelectedClass.DisplayName}");

                StatusMessage =
                    "Three-term grade summary sent to printer.";
                _dialogService.ShowInformation(
    "Your three-term grade summary was " +
    "successfully sent to the selected printer.",
    "Print Summary Successful");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not print the " +
                    $"three-term summary.\n\n" +
                    $"{exception.Message}",
                    "Print Summary Error");
            }
        }

        private FlowDocument
    CreateThreeTermPrintDocument(
        double availableTableWidth)
        {
            FlowDocument document =
                new FlowDocument();

            AddFormalPrintHeader(
     document,
     "THREE-TERM GRADE SUMMARY",
     availableTableWidth);

            document.Blocks.Add(
                CreatePrintInformationParagraph(
                    "Class",
                    SelectedClass?.DisplayName
                        ?? string.Empty));

            document.Blocks.Add(
                CreatePrintInformationParagraph(
                    "Subjects",
                    "All assigned numerical subjects"));

            document.Blocks.Add(
    CreatePrintInformationParagraph(
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

            foreach (TermSubjectSummarySection section in TermSubjectSections)
            {
                document.Blocks.Add(new Paragraph(new Bold(new Run(section.Title)))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 12, 0, 5)
                });
                Table table = new Table { CellSpacing = 0 };
                int columnCount = section.Subjects.Count + 5;
                for (int index = 0; index < columnCount; index++)
                    table.Columns.Add(new TableColumn
                    {
                        Width = new GridLength(availableTableWidth / columnCount)
                    });
                TableRowGroup group = new TableRowGroup();
                group.Rows.Add(CreateThreeTermPrintRow(true,
                    new[] { "No.", "Learner", "Sex" }
                        .Concat(section.Subjects)
                        .Concat(new[] { "Average", "Status" }).ToArray()));
                foreach (TermSubjectSummaryRow row in section.Rows)
                    group.Rows.Add(CreateThreeTermPrintRow(false,
                        new[] { row.Number.ToString(), row.LearnerName, row.Sex }
                            .Concat(row.Grades.Select(cell => cell.GradeText))
                            .Concat(new[] { row.AverageText, row.Status }).ToArray()));
                table.RowGroups.Add(group);
                document.Blocks.Add(table);
            }
            AddFormalPrintSignatures(
    document);
            return document;
        }

        private static Paragraph
            CreatePrintInformationParagraph(
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
            CreateThreeTermPrintRow(
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
                                4),

                        TextAlignment =
                            TextAlignment.Center
                    };

                if (isHeader)
                {
                    paragraph.FontWeight =
                        FontWeights.Bold;
                }

                TableCell cell =
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
                    };

                row.Cells.Add(
                    cell);
            }

            return row;
        }

        private static string FormatPrintGrade(
            double? grade)
        {
            return grade.HasValue
                ? grade.Value.ToString(
                    "0")
                : "—";
        }
    }
}
