using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private void ComposeFormalPdfHeader(
            IContainer container,
            string reportTitle)
        {
            container.Column(
                header =>
                {
                    header.Item()
                        .AlignCenter()
                        .Width(
                            520)
                        .Table(
                         table =>
        {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(
                                            95);

                                        columns.RelativeColumn();

                                        columns.ConstantColumn(
                                            95);
                                    });

                                table.Cell()
                                    .Element(
                                        logoContainer =>
                                            ComposePdfLogo(
                                                logoContainer,
                                                SummaryDepEdLogoPath));

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
                                                    10);

                                            center.Item()
                                                .AlignCenter()
                                                .Text(
                                                    "Department of Education")
                                                .FontSize(
                                                    14)
                                                .SemiBold();

                                            if (!string.IsNullOrWhiteSpace(
                                                    SummaryRegion))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreatePdfRegionLine())
                                                    .FontSize(
                                                        9);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    SummaryDivision))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreatePdfDivisionLine())
                                                    .FontSize(
                                                        9);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    SummaryDistrict))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        CreatePdfDistrictLine())
                                                    .FontSize(
                                                        9);
                                            }

                                            center.Item()
                                                .PaddingTop(
                                                    2)
                                                .AlignCenter()
                                                .Text(
                                                    SummarySchoolName)
                                                .FontSize(
                                                    14)
                                                .Bold();

                                            if (!string.IsNullOrWhiteSpace(
                                                    SummarySchoolAddress))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        SummarySchoolAddress)
                                                    .FontSize(
                                                        9);
                                            }

                                            if (!string.IsNullOrWhiteSpace(
                                                    SummarySchoolId))
                                            {
                                                center.Item()
                                                    .AlignCenter()
                                                    .Text(
                                                        $"School ID: " +
                                                        $"{SummarySchoolId}")
                                                    .FontSize(
                                                        9);
                                            }
                                        });

                                table.Cell()
                                    .Element(
                                        logoContainer =>
                                            ComposePdfLogo(
                                                logoContainer,
                                                SummarySchoolLogoPath));
                            });

                    header.Item()
                        .PaddingTop(
                            8)
                        .AlignCenter()
                        .Text(
                            reportTitle)
                        .FontSize(
                            16)
                        .Bold();
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
                container
                    .Height(
                        80);

                return;
            }

            container
                .Height(
                    80)
                .Padding(
                    5)
                .AlignCenter()
                .AlignMiddle()
                .Image(
                    logoPath)
                .FitArea();
        }

        private void ComposeFormalPdfSignatures(
            IContainer container)
        {
            container
                .PaddingTop(
                    30)
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
        SummaryAdviserName
            .ToUpperInvariant())
    .Bold()
    .FontSize(
        9);

                                    signature.Item()
                                        .PaddingTop(
                                            3)
                                        .AlignCenter()
                                        .Text(
                                            "Class Adviser")
                                        .FontSize(
                                            8);
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
         SummarySchoolHeadName
             .ToUpperInvariant())
     .Bold()
     .FontSize(
         9);

                                    signature.Item()
                                        .PaddingTop(
                                            3)
                                        .AlignCenter()
                                        .Text(
                                            "School Head")
                                        .FontSize(
                                            8);
                                });
                    });
        }

        private string CreatePdfRegionLine()
        {
            return SummaryRegion.StartsWith(
                "REGION",
                StringComparison.OrdinalIgnoreCase)
                ? SummaryRegion
                : $"REGION {SummaryRegion}";
        }

        private string CreatePdfDivisionLine()
        {
            string division =
                SummaryDivision
                    .Replace(
                        "SCHOOLS DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "DIVISION OF ",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase);

            return $"SCHOOLS DIVISION OF {division}";
        }

        private string CreatePdfDistrictLine()
        {
            return SummaryDistrict.EndsWith(
                "DISTRICT",
                StringComparison.OrdinalIgnoreCase)
                ? SummaryDistrict
                : $"{SummaryDistrict} DISTRICT";
        }
    }
}