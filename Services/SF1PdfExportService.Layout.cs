using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1PdfExportService
    {
        private static IDocument CreateSf1Document(
            SF1ExportRequest request)
        {
            return Document.Create(
                document =>
                {
                    document.Page(
                        page =>
                        {
                            page.Size(
                                PageSizes.Legal
                                    .Landscape());

                            page.Margin(
                                14);

                            page.DefaultTextStyle(
                                style =>
                                    style.FontSize(
                                        5));

                            page.Header()
                                .Element(
                                    container =>
                                        ComposeSf1Header(
                                            container,
                                            request));

                            page.Content()
                                .PaddingTop(
                                    6)
                                .Element(
                                    container =>
                                        ComposeSf1Content(
                                            container,
                                            request));

                            page.Footer()
                                .PaddingTop(
                                    4)
                                .Row(
                                    row =>
                                    {
                                        row.RelativeItem()
                                            .Text(
                                                "TeachFlex - One System, Every Classroom")
                                            .FontSize(
                                                5)
                                            .FontColor(
                                                Colors.Grey
                                                    .Darken1);

                                        row.RelativeItem()
    .DefaultTextStyle(
        style =>
            style.FontSize(
                    5)
                .FontColor(
                    Colors.Grey
                        .Darken1))
    .AlignRight()
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
                });
        }

        private static void ComposeSf1Header(
            IContainer container,
            SF1ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(
                        4);

                    column.Item()
                        .AlignCenter()
                        .Text(
                            "School Form 1 (SF 1) School Register")
                        .FontSize(
                            16)
                        .Bold();

                    column.Item()
                        .AlignCenter()
                        .Text(
                            "(This replaces Form 1, Master List & STS Form 2 - Family Background and Profile)")
                        .FontSize(
                            6)
                        .Italic();

                    column.Item()
                        .PaddingTop(
                            3)
                        .Table(
                            table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"School ID: {request.SchoolId}");

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"Division: {request.Division}");

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"District: {request.District}");

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"School Name: {request.SchoolName}");

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"School Year: {request.SchoolYear}");

                                table.Cell()
                                    .Element(
                                        InformationCellStyle)
                                    .Text(
                                        $"Grade & Section: " +
                                        $"{request.GradeLevel} - " +
                                        $"{request.SectionName}");
                            });
                });
        }

        private static void ComposeSf1Content(
            IContainer container,
            SF1ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Item()
                        .Table(
                            table =>
                            {
                                ConfigureLearnerColumns(
                                    table);

                                table.Header(
                                    header =>
                                    {
                                        AddHeaderCell(
                                            header,
                                            "LRN");

                                        AddHeaderCell(
                                            header,
                                            "NAME\n(Last Name, First Name, Middle Name)");

                                        AddHeaderCell(
                                            header,
                                            "Sex");

                                        AddHeaderCell(
                                            header,
                                            "Birth Date");

                                        AddHeaderCell(
                                            header,
                                            "Age");

                                        AddHeaderCell(
                                            header,
                                            "Mother Tongue");

                                        AddHeaderCell(
                                            header,
                                            "IP / Ethnic Group");

                                        AddHeaderCell(
                                            header,
                                            "Religion");

                                        AddHeaderCell(
                                            header,
                                            "House / Street / Sitio / Purok");

                                        AddHeaderCell(
                                            header,
                                            "Barangay");

                                        AddHeaderCell(
                                            header,
                                            "Municipality / City");

                                        AddHeaderCell(
                                            header,
                                            "Province");

                                        AddHeaderCell(
                                            header,
                                            "Father's Name");

                                        AddHeaderCell(
                                            header,
                                            "Mother's Maiden Name");

                                        AddHeaderCell(
                                            header,
                                            "Guardian Name");

                                        AddHeaderCell(
                                            header,
                                            "Relationship");

                                        AddHeaderCell(
                                            header,
                                            "Contact Number");

                                        AddHeaderCell(
                                            header,
                                            "Learning Modality");

                                        AddHeaderCell(
                                            header,
                                            "Remarks");
                                    });

                                List<SF1LearnerRow>
                                    maleLearners =
                                        request.Learners
                                            .Where(
                                                learner =>
                                                    learner.Sex.Equals(
                                                        "Male",
                                                        StringComparison.OrdinalIgnoreCase))
                                            .OrderBy(
                                                learner =>
                                                    learner.LearnerName)
                                            .ToList();

                                List<SF1LearnerRow>
                                    femaleLearners =
                                        request.Learners
                                            .Where(
                                                learner =>
                                                    learner.Sex.Equals(
                                                        "Female",
                                                        StringComparison.OrdinalIgnoreCase))
                                            .OrderBy(
                                                learner =>
                                                    learner.LearnerName)
                                            .ToList();

                                foreach (SF1LearnerRow learner
                                         in maleLearners)
                                {
                                    AddLearnerRow(
                                        table,
                                        learner);
                                }

                                AddTotalRow(
                                    table,
                                    "TOTAL MALE",
                                    maleLearners.Count);

                                foreach (SF1LearnerRow learner
                                         in femaleLearners)
                                {
                                    AddLearnerRow(
                                        table,
                                        learner);
                                }

                                AddTotalRow(
                                    table,
                                    "TOTAL FEMALE",
                                    femaleLearners.Count);

                                AddTotalRow(
                                    table,
                                    "COMBINED",
                                    maleLearners.Count +
                                    femaleLearners.Count);
                            });

                    column.Item()
                        .PaddingTop(
                            6)
                        .Element(
                            item =>
                                ComposeLegendAndSummary(
                                    item,
                                    request));

                    column.Item()
                        .PaddingTop(
                            10)
                        .Element(
                            item =>
                                ComposeSignatories(
                                    item,
                                    request));
                });
        }

        private static void ConfigureLearnerColumns(
            TableDescriptor table)
        {
            table.ColumnsDefinition(
                columns =>
                {
                    columns.RelativeColumn(
                        1.1f);

                    columns.RelativeColumn(
                        1.9f);

                    columns.RelativeColumn(
                        0.35f);

                    columns.RelativeColumn(
                        0.75f);

                    columns.RelativeColumn(
                        0.35f);

                    columns.RelativeColumn(
                        0.7f);

                    columns.RelativeColumn(
                        0.55f);

                    columns.RelativeColumn(
                        0.6f);

                    columns.RelativeColumn(
                        0.9f);

                    columns.RelativeColumn(
                        0.7f);

                    columns.RelativeColumn(
                        0.8f);

                    columns.RelativeColumn(
                        0.7f);

                    columns.RelativeColumn(
                        1.0f);

                    columns.RelativeColumn(
                        1.0f);

                    columns.RelativeColumn(
                        0.85f);

                    columns.RelativeColumn(
                        0.65f);

                    columns.RelativeColumn(
                        0.75f);

                    columns.RelativeColumn(
                        0.75f);

                    columns.RelativeColumn(
                        1.1f);
                });
        }

        private static void AddLearnerRow(
            TableDescriptor table,
            SF1LearnerRow learner)
        {
            AddBodyCell(
                table,
                learner.Lrn);

            AddBodyCell(
                table,
                learner.LearnerName);

            AddBodyCell(
                table,
                GetPdfSexCode(
                    learner.Sex));

            AddBodyCell(
                table,
                learner.BirthDate?
                    .ToString(
                        "MM/dd/yyyy")
                ?? string.Empty);

            AddBodyCell(
                table,
                learner.AgeAsOfFirstFriday?
                    .ToString()
                ?? string.Empty);

            AddBodyCell(
                table,
                learner.MotherTongue);

            AddBodyCell(
                table,
                learner.IndigenousPeopleEthnicGroup);

            AddBodyCell(
                table,
                learner.Religion);

            AddBodyCell(
                table,
                learner.HouseStreetSitioPurok);

            AddBodyCell(
                table,
                learner.Barangay);

            AddBodyCell(
                table,
                learner.MunicipalityCity);

            AddBodyCell(
                table,
                learner.Province);

            AddBodyCell(
                table,
                learner.FatherName);

            AddBodyCell(
                table,
                learner.MotherMaidenName);

            AddBodyCell(
                table,
                learner.GuardianName);

            AddBodyCell(
                table,
                learner.GuardianRelationship);

            AddBodyCell(
                table,
                learner.ContactNumber);

            AddBodyCell(
                table,
                learner.LearningModality);

            AddBodyCell(
                table,
                CreatePdfRemarks(
                    learner));
        }

        private static void AddTotalRow(
            TableDescriptor table,
            string label,
            int total)
        {
            table.Cell()
                .ColumnSpan(
                    2)
                .Element(
                    TotalCellStyle)
                .AlignRight()
                .Text(
                    $"{total}   <=== {label}")
                .Bold()
                .FontSize(
                    5);

            table.Cell()
                .ColumnSpan(
                    17)
                .Element(
                    TotalCellStyle)
                .Text(
                    string.Empty);
        }

        private static void ComposeLegendAndSummary(
            IContainer container,
            SF1ExportRequest request)
        {
            container.Row(
                row =>
                {
                    row.RelativeItem(
                            2.2f)
                        .Element(
                            item =>
                                ComposeRemarksLegend(
                                    item));

                    row.ConstantItem(
                        8);

                    row.RelativeItem()
                        .Element(
                            item =>
                                ComposeRegisteredSummary(
                                    item,
                                    request));
                });
        }

        private static void ComposeRemarksLegend(
            IContainer container)
        {
            container.Border(
                    0.6f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    4)
                .Column(
                    column =>
                    {
                        column.Item()
                            .AlignCenter()
                            .Text(
                                "List and Code of Indicators under REMARKS column")
                            .Bold()
                            .FontSize(
                                6);

                        column.Item()
                            .PaddingTop(
                                3)
                            .Text(
                                "T/O - Transferred Out     " +
                                "T/I - Transferred In     " +
                                "DRP - Dropped     " +
                                "LE - Late Enrollment")
                            .FontSize(
                                5);

                        column.Item()
                            .PaddingTop(
                                2)
                            .Text(
                                "CCT - CCT Recipient     " +
                                "B/A - Balik-Aral     " +
                                "SNED - Special Needs Education     " +
                                "ACL - Accelerated")
                            .FontSize(
                                5);
                    });
        }

        private static void ComposeRegisteredSummary(
            IContainer container,
            SF1ExportRequest request)
        {
            int maleBoSy =
                CountBeginningLearners(
                    request,
                    "Male");

            int femaleBoSy =
                CountBeginningLearners(
                    request,
                    "Female");

            container.Table(
                table =>
                {
                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn(
                                1.5f);

                            columns.RelativeColumn();

                            columns.RelativeColumn();
                        });

                    AddSummaryHeaderCell(
                        table,
                        "REGISTERED");

                    AddSummaryHeaderCell(
                        table,
                        "BoSY");

                    AddSummaryHeaderCell(
                        table,
                        "EoSY");

                    AddSummaryCell(
                        table,
                        "MALE",
                        true);

                    AddSummaryCell(
                        table,
                        maleBoSy.ToString());

                    AddSummaryCell(
                        table,
                        string.Empty);

                    AddSummaryCell(
                        table,
                        "FEMALE",
                        true);

                    AddSummaryCell(
                        table,
                        femaleBoSy.ToString());

                    AddSummaryCell(
                        table,
                        string.Empty);

                    AddSummaryCell(
                        table,
                        "TOTAL",
                        true);

                    AddSummaryCell(
                        table,
                        (maleBoSy +
                         femaleBoSy)
                            .ToString());

                    AddSummaryCell(
                        table,
                        string.Empty);
                });
        }

        private static void ComposeSignatories(
            IContainer container,
            SF1ExportRequest request)
        {
            container.Row(
                row =>
                {
                    row.RelativeItem();

                    row.RelativeItem()
                        .PaddingHorizontal(
                            10)
                        .Column(
                            column =>
                            {
                                column.Item()
                                    .Text(
                                        "Prepared by:")
                                    .Bold()
                                    .FontSize(
                                        5);

                                column.Item()
                                    .PaddingTop(
                                        14)
                                    .BorderBottom(
                                        0.7f)
                                    .AlignCenter()
                                    .Text(
                                        request.AdviserName)
                                    .Bold()
                                    .FontSize(
                                        6);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "Signature of Adviser over Printed Name")
                                    .FontSize(
                                        4.5f);
                            });

                    row.RelativeItem()
                        .PaddingHorizontal(
                            10)
                        .Column(
                            column =>
                            {
                                column.Item()
                                    .Text(
                                        "Certified Correct:")
                                    .Bold()
                                    .FontSize(
                                        5);

                                column.Item()
                                    .PaddingTop(
                                        14)
                                    .BorderBottom(
                                        0.7f)
                                    .AlignCenter()
                                    .Text(
                                        request.SchoolHeadName)
                                    .Bold()
                                    .FontSize(
                                        6);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "Signature of School Head over Printed Name")
                                    .FontSize(
                                        4.5f);
                            });
                });
        }

        private static int CountBeginningLearners(
            SF1ExportRequest request,
            string sex)
        {
            DateTime cutoff =
                GetPdfFirstFridayOfJune(
                    request.SchoolYearStartYear);

            return request.Learners.Count(
                learner =>
                    learner.Sex.Equals(
                        sex,
                        StringComparison.OrdinalIgnoreCase) &&
                    IsPdfBeginningLearner(
                        learner,
                        cutoff));
        }

        private static bool IsPdfBeginningLearner(
            SF1LearnerRow learner,
            DateTime cutoff)
        {
            bool lateEnrollee =
                learner.EnrollmentType.Equals(
                    "Late Enrollee",
                    StringComparison.OrdinalIgnoreCase);

            bool transferredIn =
                learner.EnrollmentType.Equals(
                    "Transferred In",
                    StringComparison.OrdinalIgnoreCase);

            if (!lateEnrollee &&
                !transferredIn)
            {
                return true;
            }

            return learner.EnrollmentDate.HasValue &&
                   learner.EnrollmentDate.Value.Date <=
                   cutoff.Date;
        }

        private static DateTime
            GetPdfFirstFridayOfJune(
                int year)
        {
            DateTime date =
                new DateTime(
                    year,
                    6,
                    1);

            while (date.DayOfWeek !=
                   DayOfWeek.Friday)
            {
                date =
                    date.AddDays(
                        1);
            }

            return date;
        }

        private static string GetPdfSexCode(
            string sex)
        {
            if (sex.Equals(
                    "Male",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "M";
            }

            if (sex.Equals(
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "F";
            }

            return sex;
        }

        private static string CreatePdfRemarks(
            SF1LearnerRow learner)
        {
            List<string> remarks =
                new();

            if (learner.EnrollmentType.Equals(
                    "Late Enrollee",
                    StringComparison.OrdinalIgnoreCase))
            {
                remarks.Add(
                    "LE");
            }

            if (learner.EnrollmentType.Equals(
                    "Transferred In",
                    StringComparison.OrdinalIgnoreCase))
            {
                remarks.Add(
                    string.IsNullOrWhiteSpace(
                        learner.PreviousSchoolName)
                        ? "T/I"
                        : $"T/I - {learner.PreviousSchoolName}");
            }

            if (learner.ExitReason.Equals(
                    "Transferred Out",
                    StringComparison.OrdinalIgnoreCase))
            {
                remarks.Add(
                    string.IsNullOrWhiteSpace(
                        learner.NextSchoolName)
                        ? "T/O"
                        : $"T/O - {learner.NextSchoolName}");
            }

            if (learner.ExitReason.Equals(
                    "Dropped Out",
                    StringComparison.OrdinalIgnoreCase))
            {
                remarks.Add(
                    "DRP");
            }

            if (learner.IsCctRecipient)
            {
                remarks.Add(
                    string.IsNullOrWhiteSpace(
                        learner.CctReferenceNumber)
                        ? "CCT"
                        : $"CCT - {learner.CctReferenceNumber}");
            }

            if (learner.IsBalikAral)
            {
                remarks.Add(
                    "B/A");
            }

            if (learner.IsSpecialNeedsEducation)
            {
                remarks.Add(
                    "SNED");
            }

            if (learner.IsAccelerated)
            {
                remarks.Add(
                    "ACL");
            }

            if (!string.IsNullOrWhiteSpace(
                    learner.AdditionalRemarks))
            {
                remarks.Add(
                    learner.AdditionalRemarks.Trim());
            }

            return string.Join(
                "; ",
                remarks);
        }

        private static void AddHeaderCell(
    TableCellDescriptor table,
    string text)
        {
            table.Cell()
                .Element(
                    HeaderCellStyle)
                .AlignCenter()
                .AlignMiddle()
                .Text(
                    text)
                .Bold()
                .FontSize(
                    4.2f);
        }

        private static void AddBodyCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Element(
                    BodyCellStyle)
                .AlignMiddle()
                .Text(
                    text ?? string.Empty)
                .FontSize(
                    4.1f);
        }

        private static void AddSummaryHeaderCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Element(
                    SummaryHeaderCellStyle)
                .AlignCenter()
                .Text(
                    text)
                .Bold()
                .FontSize(
                    5);
        }

        private static void AddSummaryCell(
            TableDescriptor table,
            string text,
            bool bold = false)
        {
            TextBlockDescriptor value =
                table.Cell()
                    .Element(
                        SummaryCellStyle)
                    .AlignCenter()
                    .Text(
                        text);

            if (bold)
            {
                value.Bold();
            }

            value.FontSize(
                5);
        }

        private static IContainer
            InformationCellStyle(
                IContainer container)
        {
            return container
                .Border(
                    0.5f)
                .BorderColor(
                    Colors.Grey.Darken1)
                .Padding(
                    4)
                .MinHeight(
                    19)
                .AlignMiddle();
        }

        private static IContainer HeaderCellStyle(
            IContainer container)
        {
            return container
                .Background(
                    Colors.Grey.Lighten3)
                .Border(
                    0.5f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    2)
                .MinHeight(
                    38);
        }

        private static IContainer BodyCellStyle(
            IContainer container)
        {
            return container
                .Border(
                    0.5f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    1.5f)
                .MinHeight(
                    18);
        }

        private static IContainer TotalCellStyle(
            IContainer container)
        {
            return container
                .Background(
                    Colors.Grey.Lighten4)
                .Border(
                    0.5f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    2)
                .MinHeight(
                    17);
        }

        private static IContainer
            SummaryHeaderCellStyle(
                IContainer container)
        {
            return container
                .Background(
                    Colors.Grey.Lighten3)
                .Border(
                    0.6f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    3);
        }

        private static IContainer SummaryCellStyle(
            IContainer container)
        {
            return container
                .Border(
                    0.6f)
                .BorderColor(
                    Colors.Black)
                .Padding(
                    4)
                .MinHeight(
                    18);
        }
    }
}