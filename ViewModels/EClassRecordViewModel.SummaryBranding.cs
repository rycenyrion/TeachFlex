using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private string SummarySchoolName =>
            string.IsNullOrWhiteSpace(
                _currentSchool?.SchoolName)
                ? "School Name"
                : _currentSchool.SchoolName.Trim();

        private string SummarySchoolId =>
            _currentSchool?.SchoolId?.Trim()
            ?? string.Empty;

        private string SummarySchoolAddress =>
            _currentSchool?.SchoolAddress?.Trim()
            ?? string.Empty;
        private string SummaryRegion =>
    _currentSchool?.Region?.Trim()
        .ToUpperInvariant()
    ?? string.Empty;

        private string SummaryDivision =>
            _currentSchool?.Division?.Trim()
                .ToUpperInvariant()
            ?? string.Empty;

        private string SummaryDistrict =>
            _currentSchool?.District?.Trim()
                .ToUpperInvariant()
            ?? string.Empty;

        private string SummaryAdviserName =>
            string.IsNullOrWhiteSpace(
                SelectedClass?.Adviser?.FullName)
                ? "____________________________"
                : SelectedClass.Adviser.FullName.Trim();

        private string SummarySchoolHeadName =>
            string.IsNullOrWhiteSpace(
                _currentSchool?.SchoolHead)
                ? "____________________________"
                : _currentSchool.SchoolHead.Trim();

        private string SummarySchoolLogoPath =>
     GetExistingLogoPath(
         _currentSchool?.SchoolLogoPath);

        private string SummaryDepEdLogoPath =>
            GetExistingLogoPath(
                _currentSchool?.DepEdLogoPath);

        private static string GetExistingLogoPath(
            string? logoPath)
        {
            string normalizedPath =
                logoPath?.Trim()
                ?? string.Empty;

            return !string.IsNullOrWhiteSpace(
                       normalizedPath) &&
                   File.Exists(
                       normalizedPath)
                ? normalizedPath
                : string.Empty;
        }
        

        private string SummarySchoolYear =>
            _currentAcademicYear == null
                ? string.Empty
                : $"{_currentAcademicYear.StartYear}-" +
                  $"{_currentAcademicYear.EndYear}";

        private void AddFormalPrintHeader(
    FlowDocument document,
    string reportTitle,
    double availableWidth)
        {
            double headerWidth =
    Math.Min(
        700,
        availableWidth * 0.94);

            double headerSideMargin =
                Math.Max(
                    0,
                    (availableWidth - headerWidth) / 2);
            Table headerTable =
                new Table
                {
                    CellSpacing =
                        0,

                    Margin =
    new Thickness(
        headerSideMargin,
        0,
        headerSideMargin,
        20)
                };
            double sideColumnWidth =
                Math.Min(
                    110,
                    headerWidth * 0.16);

            double centerColumnWidth =
                headerWidth -
                (sideColumnWidth * 2);

            headerTable.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            sideColumnWidth)
                });

            headerTable.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            centerColumnWidth)
                });

            headerTable.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            sideColumnWidth)
                });

            TableRowGroup rowGroup =
                new TableRowGroup();

            TableRow row =
                new TableRow();

            TableCell logoCell =
                new TableCell
                {
                    Padding =
                        new Thickness(
                            4),

                    TextAlignment =
                        TextAlignment.Center
                };

            Image? depEdLogo =
     CreatePrintSummaryLogo(
         SummaryDepEdLogoPath);

            if (depEdLogo != null)
            {
                BlockUIContainer imageContainer =
                   new BlockUIContainer(
    depEdLogo)
                   {
                        TextAlignment =
                            TextAlignment.Center
                    };

                logoCell.Blocks.Add(
                    imageContainer);
            }
            else
            {
                logoCell.Blocks.Add(
                    new Paragraph());
            }

            row.Cells.Add(
                logoCell);

            string regionLine =
    string.IsNullOrWhiteSpace(
        SummaryRegion)
        ? string.Empty
        : SummaryRegion.StartsWith(
            "REGION",
            StringComparison.OrdinalIgnoreCase)
            ? SummaryRegion
            : $"REGION {SummaryRegion}";

            string divisionValue =
                SummaryDivision
                    .Replace(
                        "SCHOOLS DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase);

            string divisionLine =
                string.IsNullOrWhiteSpace(
                    divisionValue)
                    ? string.Empty
                    : $"SCHOOLS DIVISION OF {divisionValue}";

            string districtLine =
                string.IsNullOrWhiteSpace(
                    SummaryDistrict)
                    ? string.Empty
                    : SummaryDistrict.EndsWith(
                        "DISTRICT",
                        StringComparison.OrdinalIgnoreCase)
                        ? SummaryDistrict
                        : $"{SummaryDistrict} DISTRICT";

            Paragraph centerHeader =
                new Paragraph
                {
                    TextAlignment =
                        TextAlignment.Center,

                    Margin =
                        new Thickness(
                            0)
                };

            centerHeader.Inlines.Add(
                new Run(
                    "Republic of the Philippines")
                {
                    FontSize =
                        11
                });

            centerHeader.Inlines.Add(
                new LineBreak());

            centerHeader.Inlines.Add(
                new Run(
                    "Department of Education")
                {
                    FontSize =
                        15,

                    FontWeight =
                        FontWeights.SemiBold
                });

            if (!string.IsNullOrWhiteSpace(
                    regionLine))
            {
                centerHeader.Inlines.Add(
                    new LineBreak());

                centerHeader.Inlines.Add(
                    new Run(
                        regionLine)
                    {
                        FontSize =
                            10
                    });
            }

            if (!string.IsNullOrWhiteSpace(
                    divisionLine))
            {
                centerHeader.Inlines.Add(
                    new LineBreak());

                centerHeader.Inlines.Add(
                    new Run(
                        divisionLine)
                    {
                        FontSize =
                            10
                    });
            }

            if (!string.IsNullOrWhiteSpace(
                    districtLine))
            {
                centerHeader.Inlines.Add(
                    new LineBreak());

                centerHeader.Inlines.Add(
                    new Run(
                        districtLine)
                    {
                        FontSize =
                            10
                    });
            }

            centerHeader.Inlines.Add(
                new LineBreak());

            centerHeader.Inlines.Add(
                new Run(
                    SummarySchoolName)
                {
                    FontSize =
                        15,

                    FontWeight =
                        FontWeights.Bold
                });

            if (!string.IsNullOrWhiteSpace(
                    SummarySchoolAddress))
            {
                centerHeader.Inlines.Add(
                    new LineBreak());

                centerHeader.Inlines.Add(
                    new Run(
                        SummarySchoolAddress)
                    {
                        FontSize =
                            9
                    });
            }

            if (!string.IsNullOrWhiteSpace(
                    SummarySchoolId))
            {
                centerHeader.Inlines.Add(
                    new LineBreak());

                centerHeader.Inlines.Add(
                    new Run(
                        $"School ID: {SummarySchoolId}")
                    {
                        FontSize =
                            9
                    });
            }

            centerHeader.Inlines.Add(
                new LineBreak());

            centerHeader.Inlines.Add(
                new LineBreak());

            centerHeader.Inlines.Add(
                new Run(
                    reportTitle)
                {
                    FontSize =
                        16,

                    FontWeight =
                        FontWeights.Bold
                });

         
            row.Cells.Add(
                new TableCell(
                    centerHeader)
                {
                    Padding =
                        new Thickness(
                            4)
                });

            TableCell schoolLogoCell =
    new TableCell
    {
        Padding =
            new Thickness(
                4),

        TextAlignment =
            TextAlignment.Center
    };

            Image? schoolLogo =
                CreatePrintSummaryLogo(
                    SummarySchoolLogoPath);

            if (schoolLogo != null)
            {
                schoolLogoCell.Blocks.Add(
                    new BlockUIContainer(
                        schoolLogo)
                    {
                        TextAlignment =
                            TextAlignment.Center
                    });
            }
            else
            {
                schoolLogoCell.Blocks.Add(
                    new Paragraph());
            }

            row.Cells.Add(
                schoolLogoCell);

            rowGroup.Rows.Add(
                row);

            headerTable.RowGroups.Add(
                rowGroup);

            document.Blocks.Add(
                headerTable);
        }

        private static Image? CreatePrintSummaryLogo(
    string logoPath)
        {
            if (string.IsNullOrWhiteSpace(
        logoPath))
            {
                return null;
            }

            try
            {
                BitmapImage bitmap =
                    new BitmapImage();

                bitmap.BeginInit();

                bitmap.CacheOption =
                    BitmapCacheOption.OnLoad;

                bitmap.UriSource =
     new Uri(
         logoPath,
         UriKind.Absolute);

                bitmap.EndInit();
                bitmap.Freeze();

                return new Image
                {
                    Source =
                        bitmap,

                    Width =
    90,

                    Height =
    90,


                    Stretch =
                        Stretch.Uniform
                };
            }
            catch
            {
                return null;
            }
        }

        private void AddFormalPrintSignatures(
            FlowDocument document)
        {
            Table signatureTable =
                new Table
                {
                    CellSpacing =
                        0,

                    Margin =
                        new Thickness(
                            0,
                            36,
                            0,
                            0)
                };

            signatureTable.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            signatureTable.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });

            TableRowGroup group =
                new TableRowGroup();

            TableRow names =
                new TableRow();

            names.Cells.Add(
                CreateSignatureCell(
                    SummaryAdviserName,
                    "Class Adviser"));

            names.Cells.Add(
                CreateSignatureCell(
                    SummarySchoolHeadName,
                    "School Head"));

            group.Rows.Add(
                names);

            signatureTable.RowGroups.Add(
                group);

            document.Blocks.Add(
                signatureTable);
        }

        private static TableCell
            CreateSignatureCell(
                string name,
                string position)
        {
            Paragraph paragraph =
                new Paragraph
                {
                    TextAlignment =
                        TextAlignment.Center,

                    Margin =
                        new Thickness(
                            24,
                            0,
                            24,
                            0)
                };

            paragraph.Inlines.Add(
                new Run(
                    name.ToUpperInvariant())
                {
                    FontWeight =
                        FontWeights.Bold
                });

            paragraph.Inlines.Add(
                new LineBreak());

            paragraph.Inlines.Add(
                new Run(
                    "____________________________"));

            paragraph.Inlines.Add(
                new LineBreak());

            paragraph.Inlines.Add(
                new Run(
                    position)
                {
                    FontSize =
                        9
                });

            return new TableCell(
                paragraph)
            {
                Padding =
                    new Thickness(
                        8)
            };
        }
    }
}