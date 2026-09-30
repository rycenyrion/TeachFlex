using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public class KindergartenProgressReportPdfService :
        IKindergartenProgressReportPdfService
    {
        public string ExportToPdf(
            KindergartenProgressReportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.BaseRequest == null)
            {
                throw new ArgumentException(
                    "Kindergarten learner and school information are required.",
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException(
                    "An output PDF path is required.",
                    nameof(outputPath));
            }

            string fullPath = Path.GetFullPath(outputPath);
            string? directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(
                    document =>
                    {
                        document.Page(
                            page => ComposeFirstPage(page, request));

                        document.Page(
                            page => ComposeSecondPage(page, request));
                    })
                .GeneratePdf(fullPath);

            return fullPath;
        }

        private static void ComposeFirstPage(
            PageDescriptor page,
            KindergartenProgressReportRequest request)
        {
            page.Size(PageSizes.A4);
            page.Margin(0.24f, Unit.Inch);
            page.DefaultTextStyle(
                style => style.FontFamily("Arial").FontSize(7f));

            page.Content()
                .Border(1f)
                .Padding(8)
                .Column(
                    column =>
                    {
                        column.Spacing(5);
                        column.Item().Element(
                            value => ComposeHeader(value, request));
                        column.Item().Element(
                            value => ComposeLearnerInformation(value, request));
                        column.Item().Text(
                                "This progress report presents the learner's " +
                                "development across the Kindergarten curriculum. " +
                                "Ratings are based on classroom observations, " +
                                "learner outputs, and performance across three terms.")
                            .FontSize(9f)
                            .LineHeight(1.12f);
                        column.Item().Element(
                            value => ComposeCompetencyTables(value, request));
                    });
        }

        private static void ComposeSecondPage(
            PageDescriptor page,
            KindergartenProgressReportRequest request)
        {
            page.Size(PageSizes.A4);
            page.Margin(0.24f, Unit.Inch);
            page.DefaultTextStyle(
                style => style.FontFamily("Arial").FontSize(7f));

            page.Content()
                .Border(1f)
                .Padding(9)
                .Row(
                    row =>
                    {
                        row.RelativeItem()
                            .PaddingRight(7)
                            .Element(
                                value => ComposeComments(value, request));

                        row.RelativeItem()
                            .PaddingLeft(7)
                            .Column(
                                column =>
                                {
                                    column.Spacing(13);
                                    column.Item().Element(
                                        value => ComposeAttendance(value, request));
                                    column.Item().Element(ComposeParentNote);
                                    column.Item().Element(ComposeRatingGuide);
                                    column.Item().Element(
                                        value => ComposeTransferCertificate(
                                            value,
                                            request));
                                });
                    });
        }

        private static void ComposeHeader(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            SF9ExportRequest data = request.BaseRequest;

            container.Row(
                row =>
                {
                    row.RelativeItem(0.35f);

                    row.ConstantItem(64)
                        .Height(64)
                        .Element(
                            value => ComposeLogo(
                                value,
                                data.School.DepEdLogoPath));

                    row.RelativeItem(2.8f)
                        .AlignCenter()
                        .Column(
                            column =>
                            {
                                column.Spacing(0.45f);
                                HeaderLine(column, "Republic of the Philippines", 9.2f);
                                HeaderLine(column, "Department of Education", 9.2f);
                                HeaderLine(column, FormatRegion(data.School.Region), 8.5f);
                                HeaderLine(column, FormatDivision(data.School.Division), 8.7f, true);
                                HeaderLine(column, data.School.District, 8.4f);
                                HeaderLine(
                                    column,
                                    data.School.SchoolName.ToUpperInvariant(),
                                    9.8f,
                                    true);
                                HeaderLine(
                                    column,
                                    "KINDERGARTEN PROGRESS REPORT CARD",
                                    12.2f,
                                    true);
                                HeaderLine(
                                    column,
                                    $"School Year {data.AcademicYear.DisplayName}",
                                    8.8f);
                            });

                    row.ConstantItem(64)
                        .Height(64)
                        .Element(
                            value => ComposeLogo(
                                value,
                                data.School.SchoolLogoPath));

                    row.RelativeItem(0.35f);
                });
        }

        private static void HeaderLine(
            ColumnDescriptor column,
            string text,
            float size,
            bool bold = false)
        {
            TextBlockDescriptor line = column.Item()
                .AlignCenter()
                .Text(text ?? string.Empty)
                .FontSize(size);

            if (bold)
            {
                line.Bold();
            }
        }

        private static void ComposeLearnerInformation(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            SF9ExportRequest data = request.BaseRequest;
            (int Years, int Months) age = CalculateAge(
                data.Learner.BirthDate,
                new DateTime(data.AcademicYear.StartYear, 6, 1));

            container.Column(
                column =>
                {
                    column.Spacing(2);
                    column.Item().Row(
                        row =>
                        {
                            LabeledLine(row, "Name:", data.Learner.OfficialName, 2.4f);
                            LabeledLine(row, "LRN:", data.Learner.Lrn, 1.6f);
                        });
                    column.Item().Row(
                        row =>
                        {
                            LabeledLine(row, "Section:", data.SchoolClass.SectionName, 1.6f);
                            LabeledLine(row, "Teacher:", data.AdviserName, 2.1f);
                            LabeledLine(row, "Sex:", data.Learner.Sex, 1f);
                        });
                    column.Item().Row(
                        row =>
                        {
                            LabeledLine(
                                row,
                                "Age at Beginning of SY:",
                                age.Years > 0
                                    ? $"{age.Years} Years, {age.Months} Months"
                                    : string.Empty,
                                2f);
                            LabeledLine(
                                row,
                                "Birthdate:",
                                data.Learner.BirthDate?.ToString("MMMM d, yyyy")
                                    ?? string.Empty,
                                1.7f);
                        });
                });
        }

        private static void LabeledLine(
            RowDescriptor row,
            string label,
            string value,
            float width)
        {
            row.RelativeItem(width)
                .PaddingRight(5)
                .Row(
                    inner =>
                    {
                        inner.AutoItem().Text(label).FontSize(8.2f);
                        inner.RelativeItem()
                            .PaddingLeft(3)
                            .BorderBottom(0.55f)
                            .AlignCenter()
                            .Text(value ?? string.Empty)
                            .FontSize(8.2f);
                    });
        }

        private static void ComposeCompetencyTables(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            List<KindergartenCompetency> ordered = request.Competencies
                .OrderBy(item => item.DisplayOrder)
                .ToList();

            int splitIndex = FindBalancedSplitIndex(ordered);
            List<KindergartenCompetency> left = ordered.Take(splitIndex).ToList();
            List<KindergartenCompetency> right = ordered.Skip(splitIndex).ToList();

            Dictionary<(int CompetencyId, int Term), string> ratings =
                request.Ratings
                    .GroupBy(item => (item.KindergartenCompetencyId, item.TermNumber))
                    .ToDictionary(
                        group => group.Key,
                        group => group.First().Rating ?? string.Empty);

            container.Row(
                row =>
                {
                    row.RelativeItem().PaddingRight(3).Element(
                        value => ComposeCompetencyTable(value, left, ratings));
                    row.RelativeItem().PaddingLeft(3).Element(
                        value => ComposeCompetencyTable(value, right, ratings));
                });
        }

        private static int FindBalancedSplitIndex(
            IReadOnlyList<KindergartenCompetency> competencies)
        {
            if (competencies.Count <= 1)
            {
                return competencies.Count;
            }

            int bestIndex = Math.Max(1, competencies.Count / 2);
            double smallestDifference = double.MaxValue;

            for (int index = 1; index < competencies.Count; index++)
            {
                double leftHeight = EstimateColumnHeight(
                    competencies.Take(index));
                double rightHeight = EstimateColumnHeight(
                    competencies.Skip(index));
                double difference = Math.Abs(leftHeight - rightHeight);

                if (difference < smallestDifference)
                {
                    smallestDifference = difference;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private static double EstimateColumnHeight(
            IEnumerable<KindergartenCompetency> competencies)
        {
            double height = 1;
            string lastArea = string.Empty;
            string lastSubDomain = string.Empty;

            foreach (KindergartenCompetency competency in competencies)
            {
                if (!string.Equals(
                        lastArea,
                        competency.DevelopmentArea,
                        StringComparison.Ordinal))
                {
                    height += 1.45;
                    lastArea = competency.DevelopmentArea;
                    lastSubDomain = string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(competency.SubDomain) &&
                    !string.Equals(
                        lastSubDomain,
                        competency.SubDomain,
                        StringComparison.Ordinal))
                {
                    height += 1.25;
                    lastSubDomain = competency.SubDomain;
                }

                int descriptionLength = competency.Description?.Length ?? 0;
                height += Math.Max(1, Math.Ceiling(descriptionLength / 48d));
            }

            return height;
        }

        private static void ComposeCompetencyTable(
            IContainer container,
            IReadOnlyList<KindergartenCompetency> competencies,
            IReadOnlyDictionary<(int CompetencyId, int Term), string> ratings)
        {
            container.Table(
                table =>
                {
                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn(5.2f);
                            columns.RelativeColumn(0.7f);
                            columns.RelativeColumn(0.7f);
                            columns.RelativeColumn(0.7f);
                        });

                    table.Header(
                        header =>
                        {
                            KinderCell(header, "Competency", true);
                            KinderCell(header, "T1", true);
                            KinderCell(header, "T2", true);
                            KinderCell(header, "T3", true);
                        });

                    string lastArea = string.Empty;
                    string lastSubDomain = string.Empty;

                    foreach (KindergartenCompetency competency in competencies)
                    {
                        if (!string.Equals(
                                lastArea,
                                competency.DevelopmentArea,
                                StringComparison.Ordinal))
                        {
                            table.Cell().ColumnSpan(4)
                                .Background("#E5E7EB")
                                .Border(0.45f)
                                .Padding(2.3f)
                                .Text(competency.DevelopmentArea)
                                .Bold()
                                .FontSize(7.8f);
                            lastArea = competency.DevelopmentArea;
                            lastSubDomain = string.Empty;
                        }

                        if (!string.IsNullOrWhiteSpace(competency.SubDomain) &&
                            !string.Equals(
                                lastSubDomain,
                                competency.SubDomain,
                                StringComparison.Ordinal))
                        {
                            table.Cell().ColumnSpan(4)
                                .Background("#F3F4F6")
                                .Border(0.45f)
                                .Padding(2.3f)
                                .Text(competency.SubDomain)
                                .Bold()
                                .Italic()
                                .FontSize(7.5f);
                            lastSubDomain = competency.SubDomain;
                        }

                        KinderCell(table, competency.Description, false, true);

                        for (int term = 1; term <= 3; term++)
                        {
                            ratings.TryGetValue(
                                (competency.Id, term),
                                out string? rating);
                            KinderCell(table, rating ?? string.Empty, false);
                        }
                    }
                });
        }

        private static void KinderCell(
            TableCellDescriptor header,
            string text,
            bool bold,
            bool alignLeft = false)
        {
            IContainer cell = header.Cell()
                .Border(0.45f)
                .Padding(2.55f)
                .AlignMiddle();

            if (!alignLeft)
            {
                cell = cell.AlignCenter();
            }

            TextBlockDescriptor value = cell.Text(text ?? string.Empty)
                .FontSize(7.6f)
                .LineHeight(1.1f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void KinderCell(
            TableDescriptor table,
            string text,
            bool bold,
            bool alignLeft = false)
        {
            IContainer cell = table.Cell()
                .Border(0.45f)
                .Padding(2.55f)
                .AlignMiddle();

            if (!alignLeft)
            {
                cell = cell.AlignCenter();
            }

            TextBlockDescriptor value = cell.Text(text ?? string.Empty)
                .FontSize(7.6f)
                .LineHeight(1.1f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeComments(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Item().AlignCenter()
                        .Text("TEACHER'S COMMENTS / REMARKS")
                        .Bold().FontSize(9.2f);
                    column.Spacing(9);

                    for (int term = 1; term <= 3; term++)
                    {
                        KindergartenTermRemark? remark = request.Remarks
                            .FirstOrDefault(item => item.TermNumber == term);

                        string narrative = BuildNarrative(remark);
                        int capturedTerm = term;

                        column.Item()
                            .Height(224)
                            .Border(0.65f)
                            .Padding(6)
                            .Column(
                                box =>
                                {
                                    box.Item().AlignCenter()
                                        .Text($"TERM {capturedTerm}")
                                        .Bold().FontSize(8.6f);
                                    box.Item().PaddingTop(5)
                                        .Text(narrative)
                                        .FontSize(8.5f)
                                        .LineHeight(1.15f);
                                });

                        column.Item()
                            .Row(
                                row =>
                                {
                                    row.AutoItem()
                                        .Text("Parent's/Guardian's Signature:")
                                        .FontSize(8.2f);
                                    row.RelativeItem().PaddingLeft(3)
                                        .BorderBottom(0.5f).Text(string.Empty);
                                });
                    }
                });
        }

        private static string BuildNarrative(KindergartenTermRemark? remark)
        {
            if (remark == null)
            {
                return string.Empty;
            }

            return string.Join(
                Environment.NewLine + Environment.NewLine,
                new[]
                {
                    remark.TeacherComment,
                    remark.LearnerStrengths,
                    remark.SuggestedInterventions
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static void ComposeAttendance(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            int[] months = { 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4 };
            string[] names =
            {
                "June", "July", "August", "September", "October",
                "November", "December", "January", "February", "March", "April"
            };

            Dictionary<int, SF9AttendanceRow> attendance =
                request.BaseRequest.AttendanceRows
                    .GroupBy(item => item.MonthNumber)
                    .ToDictionary(group => group.Key, group => group.First());

            container.Column(
                column =>
                {
                    column.Item().AlignCenter().Text("ATTENDANCE RECORD")
                        .Bold().FontSize(10.2f);
                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(0.7f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1f);
                                    columns.RelativeColumn(1f);
                                    columns.RelativeColumn(1f);
                                });

                            foreach (string heading in new[]
                                     {
                                         "Term", "Month", "Class\nDays",
                                         "Days\nPresent", "Times\nAbsent"
                                     })
                            {
                                AttendanceCell(table, heading, true);
                            }

                            int totalDays = 0;
                            int totalPresent = 0;
                            int totalAbsent = 0;

                            for (int index = 0; index < months.Length; index++)
                            {
                                attendance.TryGetValue(
                                    months[index],
                                    out SF9AttendanceRow? item);

                                int schoolDays = item?.SchoolDays ?? 0;
                                int present = item?.DaysPresent ?? 0;
                                int absent = item?.DaysAbsent ?? 0;
                                int term = index <= 3 ? 1 : index <= 6 ? 2 : 3;

                                totalDays += schoolDays;
                                totalPresent += present;
                                totalAbsent += absent;

                                AttendanceCell(
                                    table,
                                    term.ToString(),
                                    false,
                                    false,
                                    8.6f);
                                AttendanceCell(
                                    table,
                                    names[index],
                                    false,
                                    true,
                                    8.6f);
                                AttendanceCell(table, NumberOrBlank(schoolDays), false);
                                AttendanceCell(table, NumberOrBlank(present), false);
                                AttendanceCell(table, NumberOrBlank(absent), false);
                            }

                            AttendanceCell(table, string.Empty, true);
                            AttendanceCell(table, "TOTAL", true);
                            AttendanceCell(table, NumberOrBlank(totalDays), true);
                            AttendanceCell(table, NumberOrBlank(totalPresent), true);
                            AttendanceCell(table, NumberOrBlank(totalAbsent), true);
                        });
                });
        }

        private static void AttendanceCell(
            TableDescriptor table,
            string text,
            bool bold,
            bool alignLeft = false,
            float fontSize = 7.8f)
        {
            IContainer cell = table.Cell().Border(0.45f)
                .Padding(3.9f).AlignMiddle();

            if (!alignLeft)
            {
                cell = cell.AlignCenter();
            }

            TextBlockDescriptor value = cell.Text(text).FontSize(fontSize);
            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeParentNote(IContainer container)
        {
            container.Column(
                column =>
                {
                    column.Item().AlignCenter()
                        .Text("IMPORTANT NOTE TO PARENTS/GUARDIANS")
                        .Bold().FontSize(9.4f);
                    column.Item().PaddingTop(5)
                        .Text(
                            "The ratings reflect the learner's current level of " +
                            "development. Continued guidance at home and regular " +
                            "communication with the teacher support the child's progress.")
                        .FontSize(8.3f).LineHeight(1.17f);
                });
        }

        private static void ComposeRatingGuide(IContainer container)
        {
            (string Rating, string Meaning)[] rows =
            {
                ("Consistent (CO)", "Always demonstrates the expected competency."),
                ("Developing (DV)", "Demonstrates the competency with guidance or inconsistently."),
                ("Beginning (BG)", "Rarely demonstrates the competency and needs close guidance.")
            };

            container.Column(
                column =>
                {
                    column.Item().AlignCenter()
                        .Text("RATING DESCRIPTORS")
                        .Bold().FontSize(9.5f);
                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn(2.7f);
                                });

                            AttendanceCell(table, "Rating", true);
                            AttendanceCell(table, "Indicators", true);

                            foreach ((string rating, string meaning) in rows)
                            {
                                AttendanceCell(table, rating, true);
                                AttendanceCell(table, meaning, false, true);
                            }
                        });
                });
        }

        private static void ComposeTransferCertificate(
            IContainer container,
            KindergartenProgressReportRequest request)
        {
            SF9ExportRequest data = request.BaseRequest;

            container.Column(
                column =>
                {
                    column.Item().AlignCenter()
                        .Text("CERTIFICATE OF TRANSFER")
                        .Bold().FontSize(10.2f);
                    column.Item().PaddingTop(5).AlignCenter()
                        .Text("This is to certify that")
                        .FontSize(8.8f);
                    column.Item().PaddingTop(7)
                        .BorderBottom(0.55f)
                        .AlignCenter()
                        .Text(data.Learner.OfficialName)
                        .FontSize(9.1f);
                    column.Item().PaddingTop(4).AlignCenter()
                        .Text(
                            "has developed the general competencies based on " +
                            "the Kindergarten Curriculum Guide.")
                        .FontSize(8.5f)
                        .LineHeight(1.12f);
                    column.Item().PaddingTop(34).Row(
                        row =>
                        {
                            Signature(row, data.AdviserName, "Adviser");
                            row.ConstantItem(12);
                            Signature(row, data.SchoolHeadName, "School Head");
                        });
                });
        }

        private static void Signature(
            RowDescriptor row,
            string name,
            string title)
        {
            row.RelativeItem().Column(
                column =>
                {
                    column.Item().BorderBottom(0.55f)
                        .AlignCenter().Text(name ?? string.Empty)
                        .Bold().FontSize(8.6f);
                    column.Item().AlignCenter().Text(title).FontSize(7.8f);
                });
        }

        private static void ComposeLogo(IContainer container, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                container.Image(path).FitArea();
            }
        }

        private static (int Years, int Months) CalculateAge(
            DateTime? birthDate,
            DateTime referenceDate)
        {
            if (!birthDate.HasValue || birthDate.Value.Date > referenceDate.Date)
            {
                return (0, 0);
            }

            DateTime birth = birthDate.Value.Date;
            int months = (referenceDate.Year - birth.Year) * 12 +
                         referenceDate.Month - birth.Month;

            if (referenceDate.Day < birth.Day)
            {
                months--;
            }

            return (months / 12, months % 12);
        }

        private static string NumberOrBlank(int value) =>
            value > 0 ? value.ToString() : string.Empty;

        private static string FormatRegion(string region) =>
            string.IsNullOrWhiteSpace(region)
                ? string.Empty
                : region.StartsWith("Region", StringComparison.OrdinalIgnoreCase)
                    ? region
                    : $"Region {region}";

        private static string FormatDivision(string division) =>
            string.IsNullOrWhiteSpace(division)
                ? string.Empty
                : division.StartsWith("Schools Division", StringComparison.OrdinalIgnoreCase)
                    ? division.ToUpperInvariant()
                    : $"SCHOOLS DIVISION OF {division.ToUpperInvariant()}";
    }
}
