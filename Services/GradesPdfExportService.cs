using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public class GradesPdfExportService :
        IGradesPdfExportService
    {
        public string ExportConsolidatedGrades(
            GradesExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (request.Subjects.Count == 0)
            {
                throw new InvalidOperationException(
                    "No subjects are available for PDF export.");
            }

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "An output path is required.",
                    nameof(outputPath));
            }

            string fullOutputPath =
                Path.GetFullPath(
                    outputPath);

            string? outputFolder =
                Path.GetDirectoryName(
                    fullOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputFolder))
            {
                Directory.CreateDirectory(
                    outputFolder);
            }

            QuestPDF.Settings.License =
                LicenseType.Community;

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
                                20);

                            page.DefaultTextStyle(
                                style =>
                                    style.FontFamily(
                                            "Arial")
                                        .FontSize(
                                            7));

                            page.Header()
                                .Element(
                                    container =>
                                        ComposeHeader(
                                            container,
                                            request));

                            page.Content()
                                .PaddingTop(
                                    12)
                                .Column(
                                    content =>
                                    {
                                        content.Item()
                                            .Element(
                                                container =>
                                                    ComposeTable(
                                                        container,
                                                        request));

                                        content.Item()
                                            .PaddingTop(
                                                10)
                                            .Text(
                                                "Note: A dash indicates an incomplete grade. " +
                                                "The General Average is computed only when all " +
                                                "listed subjects have complete final grades.")
                                            .FontSize(
                                                6.5f)
                                            .FontColor(
                                                "#64748B");
                                        content.Item()
    .Element(
        container =>
            ComposeSignatures(
                container,
                request));
                                    });

                            page.Footer()
                                .Row(
                                    footer =>
                                    {
                                        footer.RelativeItem()
                                            .AlignLeft()
                                            .Text(
                                                "TeachFlex - One System, Every Classroom")
                                            .FontSize(
                                                6.5f)
                                            .FontColor(
                                                "#64748B");

                                        footer.RelativeItem()
    .AlignRight()
    .DefaultTextStyle(
        style =>
            style.FontSize(
                    6.5f)
                .FontColor(
                    "#64748B"))
    .Text(
        text =>
        {
            text.Span(
                "Page ");

            text.CurrentPageNumber();

            text.Span(
                " of ");

            text.TotalPages();
        });
                                    });
                        });
                })
                .GeneratePdf(
                    fullOutputPath);

            return fullOutputPath;
        }

        private static void ComposeHeader(
    IContainer container,
    GradesExportRequest request)
        {
            container.Column(
                header =>
                {
                    header.Item()
                        .AlignCenter()
                        .Width(
                            650)
                        .Table(
                            table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(
                                            85);

                                        columns.RelativeColumn();

                                        columns.ConstantColumn(
                                            85);
                                    });

                                table.Cell()
                                    .Element(
                                        logoContainer =>
                                            ComposePdfLogo(
                                                logoContainer,
                                                request.DepEdLogoPath));

                                table.Cell()
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Column(
                                        center =>
                                        {
                                            center.Item()
                                                .AlignCenter()
                                                .Text(
                                                    "Republic of the Philippines")
                                                .FontSize(
                                                    9);

                                            center.Item()
                                                .AlignCenter()
                                                .Text(
                                                    "Department of Education")
                                                .FontSize(
                                                    13)
                                                .SemiBold();

                                            if (!string.IsNullOrWhiteSpace(
                                                    request.Region))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreateRegionLine(
                                                            request.Region))
                                                    .FontSize(
                                                        8);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    request.Division))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreateDivisionLine(
                                                            request.Division))
                                                    .FontSize(
                                                        8);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    request.District))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreateDistrictLine(
                                                            request.District))
                                                    .FontSize(
                                                        8);
                                            }

                                            center.Item()
                                                .PaddingTop(
                                                    2)
                                                .AlignCenter()
                                                .Text(
                                                    request.SchoolName)
                                                .FontSize(
                                                    13)
                                                .Bold();

                                            if (!string.IsNullOrWhiteSpace(
                                                    request.SchoolAddress))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        request.SchoolAddress)
                                                    .FontSize(
                                                        8);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    request.SchoolId))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        $"School ID: " +
                                                        $"{request.SchoolId}")
                                                    .FontSize(
                                                        8);
                                            }
                                        });

                                table.Cell()
                                    .Element(
                                        logoContainer =>
                                            ComposePdfLogo(
                                                logoContainer,
                                                request.SchoolLogoPath));
                            });

                    header.Item()
                        .PaddingTop(
                            7)
                        .AlignCenter()
                        .Text(
                            "CLASS CONSOLIDATED GRADES")
                        .FontSize(
                            14)
                        .Bold();

                    header.Item()
                        .PaddingTop(
                            7)
                        .BorderTop(
                            0.8f)
                        .BorderColor(
                            "#64748B")
                        .PaddingTop(
                            6)
                        .Table(
                            table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(
                                            62);

                                        columns.RelativeColumn();

                                        columns.ConstantColumn(
                                            68);

                                        columns.RelativeColumn();
                                    });

                                AddInformationLabel(
                                    table,
                                    "Class:");

                                AddInformationValue(
                                    table,
                                    $"{request.GradeLevel} - " +
                                    $"{request.SectionName}");

                                AddInformationLabel(
                                    table,
                                    "School Year:");

                                AddInformationValue(
                                    table,
                                    request.SchoolYear);

                                AddInformationLabel(
                                    table,
                                    "Adviser:");

                                AddInformationValue(
                                    table,
                                    string.IsNullOrWhiteSpace(
                                        request.AdviserName)
                                            ? "Not provided"
                                            : request.AdviserName);

                                AddInformationLabel(
                                    table,
                                    "Prepared:");

                                AddInformationValue(
                                    table,
                                    request.DatePrepared
                                        .ToString(
                                            "MMMM dd, yyyy"));
                            });
                });
        }

        private static void ComposePdfLogo(
            IContainer container,
            string logoPath)
        {
            if (string.IsNullOrWhiteSpace(
                    logoPath) ||
                !File.Exists(
                    logoPath))
            {
                container.Height(
                    68);

                return;
            }

            container
                .Height(
                    68)
                .Padding(
                    4)
                .AlignCenter()
                .AlignMiddle()
                .Image(
                    logoPath)
                .FitArea();
        }

        private static string CreateRegionLine(
            string region)
        {
            string cleanRegion =
                region.Trim();

            return cleanRegion.StartsWith(
                "REGION",
                StringComparison.OrdinalIgnoreCase)
                    ? cleanRegion
                    : $"REGION {cleanRegion}";
        }

        private static string CreateDivisionLine(
            string division)
        {
            string cleanDivision =
                division
                    .Trim()
                    .Replace(
                        "SCHOOLS DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase);

            return $"SCHOOLS DIVISION OF {cleanDivision}";
        }

        private static string CreateDistrictLine(
            string district)
        {
            string cleanDistrict =
                district.Trim();

            return cleanDistrict.EndsWith(
                "DISTRICT",
                StringComparison.OrdinalIgnoreCase)
                    ? cleanDistrict
                    : $"{cleanDistrict} DISTRICT";
        }

        private static void ComposeTable(
            IContainer container,
            GradesExportRequest request)
        {
            container.Table(
                table =>
                {
                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.ConstantColumn(
                                24);

                            columns.RelativeColumn(
                                1.25f);

                            columns.RelativeColumn(
                                2.1f);

                            columns.RelativeColumn(
                                0.65f);

                            foreach (
                                GradesExportSubject subject
                                in request.Subjects)
                            {
                                columns.RelativeColumn(
                                    0.85f);
                            }

                            columns.RelativeColumn(
                                0.9f);

                            columns.RelativeColumn(
                                0.9f);
                        });

                    table.Header(
                        header =>
                        {
                            AddHeaderCell(
                                header,
                                "No.");

                            AddHeaderCell(
                                header,
                                "LRN");

                            AddHeaderCell(
                                header,
                                "Learner Name");

                            AddHeaderCell(
                                header,
                                "Sex");

                            foreach (
                                GradesExportSubject subject
                                in request.Subjects)
                            {
                                AddHeaderCell(
                                    header,
                                    subject.SubjectName);
                            }

                            AddHeaderCell(
                                header,
                                "General Average");

                            AddHeaderCell(
                                header,
                                "Remarks");
                        });

                    foreach (
                        GradesExportLearner learner
                        in request.Learners)
                    {
                        AddDataCell(
                            table,
                            learner.Number.ToString());

                        AddDataCell(
                            table,
                            learner.Lrn);

                        AddDataCell(
                            table,
                            learner.LearnerName,
                            false);

                        AddDataCell(
                            table,
                            learner.Sex);

                        foreach (
                            GradesExportSubject subject
                            in request.Subjects)
                        {
                            learner.SubjectFinalGrades
                                .TryGetValue(
                                    subject.SubjectId,
                                    out int? finalGrade);

                            AddDataCell(
                                table,
                                finalGrade?.ToString()
                                ?? "-");
                        }

                        AddDataCell(
                            table,
                            learner.GeneralAverage?
                                .ToString()
                            ?? "-");

                        AddDataCell(
                            table,
                            learner.Remarks);
                    }
                });
        }
        private static void ComposeSignatures(
    IContainer container,
    GradesExportRequest request)
        {
            container
                .PaddingTop(
                    24)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .PaddingHorizontal(
                                35)
                            .AlignCenter()
                            .Column(
                                signature =>
                                {
                                    signature.Item()
                                        .BorderBottom(
                                            1)
                                        .BorderColor(
                                            "#000000")
                                        .PaddingBottom(
                                            2)
                                        .AlignCenter()
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                request.AdviserName)
                                                    ? " "
                                                    : request.AdviserName
                                                        .ToUpperInvariant())
                                        .Bold()
                                        .FontSize(
                                            8);

                                    signature.Item()
                                        .PaddingTop(
                                            3)
                                        .AlignCenter()
                                        .Text(
                                            "Class Adviser")
                                        .FontSize(
                                            7);
                                });

                        row.RelativeItem()
                            .PaddingHorizontal(
                                35)
                            .AlignCenter()
                            .Column(
                                signature =>
                                {
                                    signature.Item()
                                        .BorderBottom(
                                            1)
                                        .BorderColor(
                                            "#000000")
                                        .PaddingBottom(
                                            2)
                                        .AlignCenter()
                                        .Text(
                                            string.IsNullOrWhiteSpace(
                                                request.SchoolHeadName)
                                                    ? " "
                                                    : request.SchoolHeadName
                                                        .ToUpperInvariant())
                                        .Bold()
                                        .FontSize(
                                            8);

                                    signature.Item()
                                        .PaddingTop(
                                            3)
                                        .AlignCenter()
                                        .Text(
                                            "School Head")
                                        .FontSize(
                                            7);
                                });
                    });
        }

        private static void AddInformationLabel(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .PaddingVertical(
                    2)
                .Text(
                    text)
                .SemiBold()
                .FontColor(
                    "#475569");
        }

        private static void AddInformationValue(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .PaddingVertical(
                    2)
                .Text(
                    text)
                .FontColor(
                    "#0F172A");
        }

        private static void AddHeaderCell(
            TableCellDescriptor header,
            string text)
        {
            header.Cell()
                .Background(
                    "#DBEAFE")
                .Border(
                    0.5f)
                .BorderColor(
                    "#64748B")
                .Padding(
                    4)
                .MinHeight(
                    28)
                .AlignCenter()
                .AlignMiddle()
                .Text(
                    text)
                .SemiBold()
                .FontSize(
                    6.5f);
        }

        private static void AddDataCell(
            TableDescriptor table,
            string text,
            bool centerText = true)
        {
            IContainer cell =
                table.Cell()
                    .Border(
                        0.5f)
                    .BorderColor(
                        "#CBD5E1")
                    .Padding(
                        4)
                    .MinHeight(
                        22)
                    .AlignMiddle();

            if (centerText)
            {
                cell =
                    cell.AlignCenter();
            }

            cell.Text(
                    text)
                .FontSize(
                    6.5f);
        }
    }
}