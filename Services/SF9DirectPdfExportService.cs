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
    public class SF9DirectPdfExportService :
        ISF9ExportService
    {
        private readonly SF9ExportService
            _templateExportService =
                new SF9ExportService();

        public string ExportOfficialSf9Pdf(
            SF9ExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException(
                    "An SF9 output path is required.",
                    nameof(outputPath));
            }

            int gradeNumber =
                GetGradeNumber(
                    request.SchoolClass.GradeLevel);

            // Keep the template export only for unsupported grades.
            if (gradeNumber < 1 ||
                gradeNumber > 12)
            {
                return _templateExportService
                    .ExportOfficialSf9Pdf(
                        request,
                        outputPath);
            }

            string completeOutputPath =
                Path.GetFullPath(outputPath);

            string? outputDirectory =
                Path.GetDirectoryName(
                    completeOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputDirectory))
            {
                Directory.CreateDirectory(
                    outputDirectory);
            }

            Document
                .Create(
                    document =>
                        ComposeDocument(
                            document,
                            request))
                .GeneratePdf(
                    completeOutputPath);

            return completeOutputPath;
        }

        private static void ComposeDocument(
            IDocumentContainer document,
            SF9ExportRequest request)
        {
            if (GetGradeNumber(
                    request.SchoolClass.GradeLevel) == 1)
            {
                ComposeGradeOneDocument(
                    document,
                    request);

                return;
            }

            document.Page(
                page =>
                {
                    page.Size(
                        PageSizes.A4.Landscape());

                    page.MarginVertical(
                        0.4f,
                        Unit.Inch);

                    page.MarginHorizontal(
                        0.4f,
                        Unit.Inch);

                    page.DefaultTextStyle(
                        style =>
                            style
                                .FontFamily("Arial")
                                .FontSize(8.8f)
                                .LineHeight(1.22f));

                    page.Content()
                        .Height(
                            189,
                            Unit.Millimetre)
                        .Row(
                            row =>
                            {
                                row.RelativeItem()
                                    .Border(1.2f)
                                    .Padding(3.5f)
                                    .Element(
                                        container =>
                                            ComposeLeftPanel(
                                                container,
                                                request));

                                row.ConstantItem(
                                    1,
                                    Unit.Centimetre);

                                row.RelativeItem()
                                    .Border(1.2f)
                                    .Padding(3.5f)
                                    .Element(
                                        container =>
                                            ComposeRightPanel(
                                                container,
                                                request));
                            });
                });
        }

        private static void ComposeGradeOneDocument(
            IDocumentContainer document,
            SF9ExportRequest request)
        {
            document.Page(
                page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0.3f, Unit.Inch);
                    page.DefaultTextStyle(
                        style =>
                            style
                                .FontFamily("Arial")
                                .FontSize(8.2f)
                                .LineHeight(1.14f));

                    page.Content()
                        .Border(1.1f)
                        .Padding(7)
                        .Column(
                            column =>
                            {
                                column.Spacing(5);

                                column.Item()
                                    .Element(
                                        value =>
                                            ComposeHeader(
                                                value,
                                                request));

                                column.Item()
                                    .Element(
                                        value =>
                                            ComposeGradeOneLearnerInformation(
                                                value,
                                                request));

                                column.Item()
                                    .PaddingVertical(2)
                                    .Element(
                                        ComposeGradeOneIntroduction);

                                column.Item()
                                    .Row(
                                        row =>
                                        {
                                            row.RelativeItem()
                                                .PaddingRight(6)
                                                .Element(
                                                    value =>
                                                        ComposeGradeOneTerms(
                                                            value,
                                                            request));

                                            row.RelativeItem()
                                                .PaddingLeft(6)
                                                .Element(
                                                    value =>
                                                        ComposeGradeOneRightPanel(
                                                            value,
                                                            request));
                                        });
                            });
                });
        }

        private static void ComposeGradeOneLearnerInformation(
            IContainer container,
            SF9ExportRequest request)
        {
            DateTime? birthDate =
                request.Learner.BirthDate;

            (int Years, int Months) beginningAge =
                CalculateAgeInYearsAndMonths(
                    birthDate,
                    new DateTime(
                        request.AcademicYear.StartYear,
                        6,
                        1));

            (int Years, int Months) endingAge =
                CalculateAgeInYearsAndMonths(
                    birthDate,
                    new DateTime(
                        request.AcademicYear.StartYear + 1,
                        4,
                        30));

            container.Column(
                column =>
                {
                    column.Spacing(2);

                    column.Item().Row(
                        row =>
                        {
                            GradeOneLabeledLine(
                                row,
                                "Name:",
                                request.Learner.OfficialName,
                                2.4f);

                            GradeOneLabeledLine(
                                row,
                                "LRN:",
                                request.Learner.Lrn,
                                1.4f);
                        });

                    column.Item().Row(
                        row =>
                        {
                            GradeOneLabeledLine(
                                row,
                                "Section:",
                                request.SchoolClass.SectionName,
                                1.25f);

                            GradeOneLabeledLine(
                                row,
                                "Teacher:",
                                ResolveAdviserName(request),
                                1.45f);

                            GradeOneLabeledLine(
                                row,
                                "Birthdate:",
                                birthDate?.ToString("MMMM d, yyyy")
                                ?? string.Empty,
                                1.3f);
                        });

                    column.Item().Row(
                        row =>
                        {
                            GradeOneLabeledLine(
                                row,
                                "Age at Beginning of SY:",
                                FormatAge(beginningAge),
                                1);

                            GradeOneLabeledLine(
                                row,
                                "Age at End of SY:",
                                FormatAge(endingAge),
                                1);
                        });
                });
        }

        private static void GradeOneLabeledLine(
            RowDescriptor row,
            string label,
            string value,
            float relativeSize)
        {
            row.RelativeItem(relativeSize)
                .PaddingRight(5)
                .Row(
                    field =>
                    {
                        field.AutoItem()
                            .PaddingRight(3)
                            .AlignBottom()
                            .Text(label)
                            .FontSize(8.4f);

                        field.RelativeItem()
                            .BorderBottom(0.55f)
                            .AlignCenter()
                            .AlignBottom()
                            .Text(value ?? string.Empty)
                            .FontSize(8.4f);
                    });
        }

        private static void ComposeGradeOneIntroduction(
            IContainer container)
        {
            container.Column(
                column =>
                {
                    column.Spacing(2);

                    column.Item()
                        .Text(
                            "This report provides a descriptive account of your child's learning progress for each term. It highlights what your child can already do, what they are currently developing, and how they can be further supported.")
                        .FontSize(7.8f);

                    column.Item()
                        .Text(
                            "This report is based on varied evidence of learning, including classroom activities, observations, learner outputs, and assessments. It is designed to give a clearer and more meaningful understanding of your child's development rather than relying on numerical or letter grades.")
                        .FontSize(7.8f);
                });
        }

        private static void ComposeGradeOneTerms(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(6);

                    for (int termNumber = 1;
                         termNumber <= 3;
                         termNumber++)
                    {
                        int capturedTerm = termNumber;
                        SF9SubjectGradeRow? summary =
                            request.SubjectGrades
                                .FirstOrDefault(
                                    grade =>
                                        grade.DisplayOrder ==
                                            capturedTerm &&
                                        grade.SubjectName.StartsWith(
                                            "Grade 1 Term",
                                            StringComparison.OrdinalIgnoreCase));

                        column.Item()
                            .Element(
                                value =>
                                    ComposeGradeOneTermBox(
                                        value,
                                        capturedTerm,
                                        summary?.LearningArea
                                            ?? string.Empty,
                                        summary?.SubjectCategory
                                            ?? string.Empty));
                    }
                });
        }

        private static void ComposeGradeOneTermBox(
            IContainer container,
            int termNumber,
            string canDo,
            string needsImprovement)
        {
            string[] termNames =
            {
                "TERM 1 (UNANG TERMINO)",
                "TERM 2 (IKALAWANG TERMINO)",
                "TERM 3 (IKATLONG TERMINO)"
            };

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(termNames[termNumber - 1])
                        .Bold()
                        .FontSize(9.2f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(1.25f);
                                    columns.RelativeColumn(2.55f);
                                });

                            GradeOneNarrativeLabel(
                                table,
                                "What Your Child Can Do\n(Mga Nagagawa)");

                            GradeOneNarrativeValue(
                                table,
                                canDo);

                            GradeOneNarrativeLabel(
                                table,
                                "What Your Child Is\nLearning To Improve\n(Dapat Linangin)");

                            GradeOneNarrativeValue(
                                table,
                                needsImprovement);
                        });

                    column.Item()
                        .PaddingTop(2)
                        .Row(
                            row =>
                            {
                                row.AutoItem()
                                    .PaddingRight(3)
                                    .Text("Parent's/Guardian's Signature:")
                                    .FontSize(7.4f);

                                row.RelativeItem()
                                    .BorderBottom(0.55f)
                                    .Text(string.Empty);
                            });
                });
        }

        private static void GradeOneNarrativeLabel(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Border(0.6f)
                .MinHeight(75)
                .Padding(4)
                .AlignCenter()
                .AlignMiddle()
                .Text(text)
                .Bold()
                .FontSize(7.8f);
        }

        private static void GradeOneNarrativeValue(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Border(0.6f)
                .MinHeight(75)
                .Padding(5)
                .AlignMiddle()
                .Text(text ?? string.Empty)
                .FontSize(7.5f)
                .LineHeight(1.1f);
        }

        private static void ComposeGradeOneRightPanel(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(6);

                    column.Item()
                        .Element(
                            value =>
                                ComposeGradeOneAttendance(
                                    value,
                                    request.AttendanceRows));

                    column.Item()
                        .Element(ComposeGradeOneParentNote);

                    column.Item()
                        .Element(ComposeGradeOneDescriptors);

                    column.Item()
                        .Element(
                            value =>
                                ComposeGradeOneTransfer(
                                    value,
                                    request));
                });
        }

        private static void ComposeGradeOneAttendance(
            IContainer container,
            IReadOnlyList<SF9AttendanceRow> attendanceRows)
        {
            string[] monthNames =
            {
                "June", "July", "August", "September",
                "October", "November", "December",
                "January", "February", "March", "April"
            };

            int[] monthNumbers =
            {
                6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4
            };

            Dictionary<int, SF9AttendanceRow> attendanceByMonth =
                attendanceRows
                    .GroupBy(item => item.MonthNumber)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First());

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text("ATTENDANCE RECORD")
                        .Bold()
                        .FontSize(9.2f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(0.8f);
                                    columns.RelativeColumn(1.7f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.15f);
                                });

                            GradeOneAttendanceCell(table, "Term", true);
                            GradeOneAttendanceCell(table, "Month", true);
                            GradeOneAttendanceCell(table, "Class\nDays", true);
                            GradeOneAttendanceCell(table, "Days\nPresent", true);
                            GradeOneAttendanceCell(table, "Times\nAbsent", true);

                            int totalSchoolDays = 0;
                            int totalPresent = 0;
                            int totalAbsent = 0;

                            for (int index = 0;
                                 index < monthNumbers.Length;
                                 index++)
                            {
                                int monthNumber = monthNumbers[index];
                                int termNumber =
                                    index <= 3
                                        ? 1
                                        : index <= 6
                                            ? 2
                                            : 3;

                                attendanceByMonth.TryGetValue(
                                    monthNumber,
                                    out SF9AttendanceRow? attendance);

                                int schoolDays =
                                    attendance?.SchoolDays ?? 0;
                                int present =
                                    attendance?.DaysPresent ?? 0;
                                int absent =
                                    attendance?.DaysAbsent ?? 0;

                                totalSchoolDays += schoolDays;
                                totalPresent += present;
                                totalAbsent += absent;

                                GradeOneAttendanceCell(
                                    table,
                                    termNumber.ToString(),
                                    index is 0 or 4 or 7);

                                GradeOneAttendanceCell(
                                    table,
                                    monthNames[index],
                                    false,
                                    alignLeft: true);

                                GradeOneAttendanceCell(
                                    table,
                                    schoolDays > 0
                                        ? schoolDays.ToString()
                                        : string.Empty,
                                    false);

                                GradeOneAttendanceCell(
                                    table,
                                    present > 0
                                        ? present.ToString()
                                        : string.Empty,
                                    false);

                                GradeOneAttendanceCell(
                                    table,
                                    absent > 0
                                        ? absent.ToString()
                                        : string.Empty,
                                    false);
                            }

                            GradeOneAttendanceCell(table, string.Empty, true);
                            GradeOneAttendanceCell(table, "TOTAL", true);
                            GradeOneAttendanceCell(table, totalSchoolDays > 0 ? totalSchoolDays.ToString() : string.Empty, true);
                            GradeOneAttendanceCell(table, totalPresent > 0 ? totalPresent.ToString() : string.Empty, true);
                            GradeOneAttendanceCell(table, totalAbsent > 0 ? totalAbsent.ToString() : string.Empty, true);
                        });
                });
        }

        private static void GradeOneAttendanceCell(
            TableDescriptor table,
            string text,
            bool bold,
            bool alignLeft = false)
        {
            IContainer cell =
                table.Cell()
                    .Border(0.5f)
                    .MinHeight(14f)
                    .PaddingHorizontal(2f)
                    .AlignMiddle();

            cell = alignLeft
                ? cell.AlignLeft()
                : cell.AlignCenter();

            TextBlockDescriptor value =
                cell.Text(text)
                    .FontSize(6.5f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeGradeOneParentNote(
            IContainer container)
        {
            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text("IMPORTANT NOTE TO PARENTS/GUARDIANS")
                        .Bold()
                        .FontSize(8.6f);

                    column.Item()
                        .PaddingTop(2)
                        .Text(
                            "A detailed record of your child's progress across specific learning competencies is attached in the succeeding pages. This includes the monitoring of skills in Reading and Literacy, Language, Mathematics, Good Manners and Right Conduct, and Makabansa.")
                        .FontSize(7.1f);
                });
        }

        private static void ComposeGradeOneDescriptors(
            IContainer container)
        {
            (string Grade, string Descriptor, string Description)[] rows =
            {
                (
                    "A",
                    "Advancing\n(Namumukod-tangi)",
                    "Consistently demonstrates advanced skills, understanding, and values beyond expectations; applies learning independently, confidently, and with initiative across tasks and situations."),
                (
                    "B",
                    "Benchmarking\n(Naipamamalas)",
                    "Demonstrates expected skills, understanding, and values at grade level with consistency; performs tasks accurately and independently in most situations."),
                (
                    "C",
                    "Connecting\n(Natutungo)",
                    "Shows developing skills, understanding, and values; able to apply learning in familiar tasks with minimal guidance and support."),
                (
                    "D",
                    "Developing\n(Nagpapaunlad)",
                    "Demonstrates emerging skills, understanding, and values; requires regular guidance, practice, and support to improve performance."),
                (
                    "E",
                    "Emerging\n(Nagsisimula)",
                    "Beginning to demonstrate basic skills, understanding, and values; requires close supervision, structured support, and targeted intervention.")
            };

            container.Column(
                column =>
                {
                    column.Item()
                        .Text("Performance levels used in monitoring:")
                        .FontSize(7.1f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(0.65f);
                                    columns.RelativeColumn(1.15f);
                                    columns.RelativeColumn(2.8f);
                                });

                            GradeOneDescriptorCell(table, "Letter\nGrade", true);
                            GradeOneDescriptorCell(table, "Descriptor", true);
                            GradeOneDescriptorCell(table, "Description", true);

                            foreach ((string grade,
                                      string descriptor,
                                      string description) in rows)
                            {
                                GradeOneDescriptorCell(table, grade, true);
                                GradeOneDescriptorCell(table, descriptor, true);
                                GradeOneDescriptorCell(table, description, false);
                            }
                        });
                });
        }

        private static void GradeOneDescriptorCell(
            TableDescriptor table,
            string text,
            bool bold)
        {
            TextBlockDescriptor value =
                table.Cell()
                    .Border(0.5f)
                    .MinHeight(34)
                    .Padding(2.5f)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text)
                    .FontSize(6.2f)
                    .LineHeight(1.08f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeGradeOneTransfer(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(3);

                    column.Item()
                        .AlignCenter()
                        .Text("CERTIFICATE OF TRANSFER")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item()
                        .AlignCenter()
                        .Text(
                            "This is to certify that the above-named learner has satisfactorily completed the requirements for the grade level indicated.")
                        .FontSize(7f);

                    column.Item()
                        .Text("Admitted to Grade: ____________________")
                        .FontSize(6.9f);

                    column.Item()
                        .Text("Eligible for Admission to Grade: ____________________")
                        .FontSize(6.9f);

                    column.Item().Row(
                        row =>
                        {
                            row.RelativeItem()
                                .Element(
                                    value =>
                                        ComposeSignature(
                                            value,
                                            ResolveAdviserName(request),
                                            "Adviser"));

                            row.RelativeItem()
                                .Element(
                                    value =>
                                        ComposeSignature(
                                            value,
                                            ResolveSchoolHeadName(request),
                                            "School Head"));
                        });

                    column.Item()
                        .PaddingTop(2)
                        .AlignCenter()
                        .Text("CANCELLATION OF ELIGIBILITY TO TRANSFER")
                        .Bold()
                        .FontSize(8f);

                    column.Item()
                        .Text("Admitted in: ____________    Date: ____________")
                        .FontSize(6.8f);
                });
        }

        private static (int Years, int Months)
            CalculateAgeInYearsAndMonths(
                DateTime? birthDate,
                DateTime referenceDate)
        {
            if (!birthDate.HasValue ||
                birthDate.Value.Date > referenceDate.Date)
            {
                return (0, 0);
            }

            DateTime birth = birthDate.Value.Date;
            int months =
                (referenceDate.Year - birth.Year) * 12 +
                referenceDate.Month - birth.Month;

            if (referenceDate.Day < birth.Day)
            {
                months--;
            }

            return (months / 12, months % 12);
        }

        private static string FormatAge(
            (int Years, int Months) age)
        {
            return age.Years == 0 &&
                   age.Months == 0
                ? string.Empty
                : $"{age.Years} Years, {age.Months} Months";
        }

        private static void ComposeLeftPanel(
            IContainer container,
            SF9ExportRequest request)
        {
            int gradeNumber =
                GetGradeNumber(
                    request.SchoolClass.GradeLevel);

            bool isSeniorHigh =
                gradeNumber is 11 or 12;

            container.Column(
                column =>
                {
                    column.Spacing(
                        isSeniorHigh
                            ? 1
                            : 6);

                    column.Item()
                        .Element(
                            value =>
                                ComposeHeader(
                                    value,
                                    request));

                    column.Item()
                        .PaddingTop(
                            isSeniorHigh
                                ? 2
                                : 4)
                        .Element(
                            value =>
                                ComposeLearnerInformation(
                                    value,
                                    request));

                    column.Item()
                        .PaddingTop(
                            isSeniorHigh
                                ? 2
                                : 5)
                        .PaddingBottom(
                            isSeniorHigh
                                ? 1
                                : 3)
                        .Element(
                            value =>
                                ComposeParentMessage(
                                    value,
                                    request));

                    column.Item()
                        .PaddingVertical(
                            isSeniorHigh
                                ? 2
                                : 4)
                        .Element(
                            value =>
                                ComposePrimarySignatories(
                                    value,
                                    request));

                    column.Item()
                        .PaddingTop(
                            isSeniorHigh
                                ? 1
                                : 3)
                        .Element(
                            value =>
                            {
                                if (gradeNumber == 11)
                                {
                                    ComposeSeniorHighGradeTable(
                                        value,
                                        request);
                                }
                                else if (gradeNumber == 12)
                                {
                                    bool isTechPro =
                                        FormatShsTrackType(
                                            request.SchoolClass.TrackStrand)
                                        .Equals(
                                            "TechPro",
                                            StringComparison.OrdinalIgnoreCase);

                                    if (isTechPro)
                                    {
                                        bool hasAcademicCrossSubjects =
                                            request.SubjectGrades.Any(
                                                IsExplicitAcademicElectiveSubject);

                                        if (hasAcademicCrossSubjects)
                                        {
                                            ComposeGradeTwelveTechProCrossTable(
                                                value,
                                                request.SubjectGrades);
                                        }
                                        else
                                        {
                                            ComposeGradeTwelveTechProTable(
                                                value,
                                                request.SubjectGrades);
                                        }
                                    }
                                    else
                                    {
                                        bool hasTechProCrossSubjects =
                                            request.SubjectGrades.Any(
                                                grade =>
                                                    IsTechProElectiveSubject(grade) ||
                                                    IsWorkImmersionSubject(grade));

                                        if (hasTechProCrossSubjects)
                                        {
                                            ComposeGradeTwelveAcademicCrossTable(
                                                value,
                                                request.SubjectGrades);
                                        }
                                        else
                                        {
                                            ComposeGradeTwelveAcademicTable(
                                                value,
                                                request.SubjectGrades);
                                        }
                                    }
                                }
                                else
                                {
                                    ComposeGradeTable(
                                        value,
                                        request.SubjectGrades,
                                        gradeNumber);
                                }
                            });

                    column.Item()
                        .PaddingTop(
                            isSeniorHigh
                                ? 1
                                : gradeNumber == 2
                                    ? 6
                                    : gradeNumber == 3
                                        ? 5
                                    : 4)
                        .Element(
                            value =>
                                ComposePerformanceDescriptors(
                                    value,
                                    isSeniorHigh,
                                    gradeNumber == 2,
                                    gradeNumber == 3));
                });
        }

        private static void ComposeHeader(
            IContainer container,
            SF9ExportRequest request)
        {
            bool isGradeOne =
                GetGradeNumber(
                    request.SchoolClass.GradeLevel) == 1;

            container
                .PaddingHorizontal(
                    isGradeOne
                        ? 30
                        : 0)
                .Row(
                row =>
                {
                    row.ConstantItem(64)
                        .Height(64)
                        .Element(
                            value =>
                                ComposeLogo(
                                    value,
                                    request.School
                                        .DepEdLogoPath));

                    row.RelativeItem()
                        .PaddingHorizontal(2)
                        .AlignCenter()
                        .Column(
                            column =>
                            {
                                column.Spacing(1.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "Republic of the Philippines")
                                    .FontSize(9.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        "Department of Education")
                                    .FontSize(9.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        FormatRegion(
                                            request.School.Region))
                                    .FontSize(8.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        FormatDivision(
                                            request.School.Division))
                                    .Bold()
                                    .FontSize(8.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        request.School.District)
                                    .FontSize(8.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        request.School.SchoolName
                                            .ToUpperInvariant())
                                    .Bold()
                                    .FontSize(9.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        request.School.SchoolAddress)
                                    .FontSize(8f);

                                column.Item()
                                    .PaddingTop(2)
                                    .AlignCenter()
                                    .Text(
                                        "LEARNER'S PERFORMANCE REPORT")
                                    .Bold()
                                    .FontSize(10.5f);

                                column.Item()
                                    .AlignCenter()
                                    .Text(
                                        $"School Year " +
                                        $"{request.AcademicYear.DisplayName}")
                                    .FontSize(8.5f);
                            });

                    row.ConstantItem(64)
                        .Height(64)
                        .Element(
                            value =>
                                ComposeLogo(
                                    value,
                                    request.School
                                        .SchoolLogoPath));
                });
        }

        private static void ComposeLogo(
            IContainer container,
            string path)
        {
            if (!string.IsNullOrWhiteSpace(path) &&
                File.Exists(path))
            {
                container.Image(path)
                    .FitArea();

                return;
            }

            container.AlignCenter()
                .AlignMiddle()
                .Text(string.Empty);
        }

        private static void ComposeLearnerInformation(
            IContainer container,
            SF9ExportRequest request)
        {
            string learnerName =
                request.Learner.OfficialName
                    .Trim()
                    .ToUpperInvariant();

            container.Column(
                column =>
                {
                    column.Item().Row(
                        row =>
                        {
                            ComposeLabeledLine(
                                row,
                                "Name:",
                                learnerName,
                                3);

                            ComposeLabeledLine(
                                row,
                                "Age:",
                                request.Learner.Age?
                                    .ToString()
                                    ?? string.Empty,
                                1);

                            ComposeLabeledLine(
                                row,
                                "Sex:",
                                FormatSex(
                                    request.Learner.Sex),
                                1);
                        });

                    column.Item().Row(
                        row =>
                        {
                            ComposeLabeledLine(
                                row,
                                "LRN:",
                                request.Learner.Lrn,
                                2.4f);

                            ComposeLabeledLine(
                                row,
                                "Grade:",
                                FormatGradeLevel(
                                    request.SchoolClass
                                        .GradeLevel),
                                0.9f);

                            ComposeLabeledLine(
                                row,
                                "Section:",
                                request.SchoolClass
                                    .SectionName,
                                1.7f);
                        });

                    if (!string.IsNullOrWhiteSpace(
                            request.SchoolClass.TrackStrand) &&
                        !request.SchoolClass.TrackStrand.Equals(
                            "Not Applicable",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        column.Item().Row(
                            row =>
                                ComposeLabeledLine(
                                    row,
                                    "Track (SHS only):",
                                    FormatShsTrackType(
                                        request.SchoolClass
                                            .TrackStrand),
                                    1,
                                    alignLeft: true));
                    }
                });
        }

        private static void ComposeLabeledLine(
            RowDescriptor row,
            string label,
            string value,
            float relativeSize,
            bool alignLeft = false)
        {
            row.RelativeItem(relativeSize)
                .PaddingRight(4)
                .Row(
                    field =>
                    {
                        field.AutoItem()
                            .MinHeight(15)
                            .PaddingRight(3)
                            .AlignBottom()
                            .Text(label)
                            .FontSize(10f);

                        IContainer valueContainer = field.RelativeItem()
                            .MinHeight(15)
                            .BorderBottom(0.7f)
                            .PaddingHorizontal(2);

                        valueContainer = alignLeft
                            ? valueContainer.AlignLeft()
                            : valueContainer.AlignCenter();

                        valueContainer
                            .AlignBottom()
                            .Text(value ?? string.Empty)
                            .FontSize(10f);
                    });
        }

        private static void ComposeParentMessage(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(2.5f);

                    column.Item().Text("Dear Parents,");

                    column.Item()
                        .PaddingHorizontal(10)
                        .AlignCenter()
                        .Text(
                            "This Performance Report presents your child's " +
                            "progress and achievement in the different " +
                            "learning areas.")
                        .FontSize(8f)
                        .LineHeight(1.32f);

                    column.Item()
                        .PaddingHorizontal(10)
                        .AlignCenter()
                        .Text(
                            "The school welcomes you to reach out should " +
                            "you wish to know more about your child's " +
                            "learning and performance.")
                        .FontSize(8f)
                        .LineHeight(1.32f);
                });
        }

        private static void ComposePrimarySignatories(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Row(
                row =>
                {
                    row.RelativeItem()
                        .PaddingHorizontal(8)
                        .Element(
                            value =>
                                ComposeSignature(
                                    value,
                                    ResolveSchoolHeadName(
                                        request),
                                    "School Head"));

                    row.RelativeItem()
                        .PaddingHorizontal(8)
                        .Element(
                            value =>
                                ComposeSignature(
                                    value,
                                    ResolveAdviserName(
                                        request),
                                    "Adviser"));
                });
        }

        private static void ComposeGradeTable(
            IContainer container,
            IReadOnlyList<SF9SubjectGradeRow> grades,
            int gradeNumber)
        {
            List<SF9SubjectGradeRow> orderedGrades =
                grades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .ToList();

            SF9SubjectGradeRow? musicAndArts =
                orderedGrades.FirstOrDefault(
                    IsMusicAndArtsSubject);

            SF9SubjectGradeRow? peAndHealth =
                orderedGrades.FirstOrDefault(
                    IsPhysicalEducationAndHealthSubject);

            List<(string Label, string[] Names)>
                prescribedDefinitions =
                    gradeNumber == 2
                        ? new()
                        {
                            ("Filipino", new[] { "Filipino" }),
                            ("English", new[] { "English" }),
                            ("Mathematics", new[] { "Mathematics" }),
                            (
                                "GMRC / Values Education",
                                new[]
                                {
                                    "GMRC",
                                    "Values Education",
                                    "GMRC / Values Education"
                                }),
                            ("Makabansa", new[] { "Makabansa" })
                        }
                        : gradeNumber == 3
                            ? new()
                            {
                                ("Filipino", new[] { "Filipino" }),
                                ("English", new[] { "English" }),
                                ("Mathematics", new[] { "Mathematics" }),
                                ("Science", new[] { "Science" }),
                                (
                                    "GMRC / Values Education",
                                    new[]
                                    {
                                        "GMRC",
                                        "Values Education",
                                        "GMRC / Values Education"
                                    }),
                                ("Makabansa", new[] { "Makabansa" })
                            }
                            : new()
                            {
                                ("Filipino", new[] { "Filipino" }),
                                ("English", new[] { "English" }),
                                ("Mathematics", new[] { "Mathematics" }),
                                ("Science", new[] { "Science" }),
                                (
                                    "Araling Panlipunan (AP)",
                                    new[]
                                    {
                                        "Araling Panlipunan",
                                        "Araling Panlipunan (AP)"
                                    }),
                                (
                                    "GMRC / Values Education",
                                    new[]
                                    {
                                        "GMRC",
                                        "Values Education",
                                        "GMRC / Values Education"
                                    }),
                                ("EPP/TLE", new[] { "EPP", "TLE", "EPP/TLE" }),
                                ("MAPEH", new[] { "MAPEH" })
                            };

            List<SF9SubjectGradeRow> prescribedGrades =
                prescribedDefinitions
                    .Select(
                        definition =>
                            CopyGradeWithName(
                                FindSubjectByExactName(
                                    orderedGrades,
                                    definition.Names),
                                definition.Label))
                    .ToList();

            List<SF9SubjectGradeRow> displayGrades =
                prescribedGrades
                    .ToList();

            if (gradeNumber >= 4)
            {
                displayGrades.Add(
                    musicAndArts ??
                    new SF9SubjectGradeRow
                    {
                        SubjectName =
                            "Music and Arts"
                    });

                displayGrades.Add(
                    peAndHealth ??
                    new SF9SubjectGradeRow
                    {
                        SubjectName =
                            "Physical Education and Health"
                    });
            }

            List<SF9SubjectGradeRow> additionalGrades =
                gradeNumber is >= 7 and <= 10
                    ? orderedGrades
                        .Where(IsAdditionalSf9Subject)
                        .Where(
                            grade =>
                                !IsMapehComponentSubject(grade))
                        .ToList()
                    : new List<SF9SubjectGradeRow>();

            displayGrades.AddRange(additionalGrades);

            int additionalCount = additionalGrades.Count;
            bool compactRows = additionalCount > 0;
            bool extraCompactRows = additionalCount > 3;
            bool expandedRows = gradeNumber == 2;
            bool moderatelyExpandedRows = gradeNumber == 3;

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(
                            "LEARNING PROGRESS AND ACHIEVEMENT")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(4.6f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn(2.2f);
                                });

                            table.Header(
                                header =>
                                {
                                    HeaderCell(header, "Learning Areas", compactRows, expandedRows, moderatelyExpandedRows);
                                    HeaderCell(header, "T1", compactRows, expandedRows, moderatelyExpandedRows);
                                    HeaderCell(header, "T2", compactRows, expandedRows, moderatelyExpandedRows);
                                    HeaderCell(header, "T3", compactRows, expandedRows, moderatelyExpandedRows);
                                    HeaderCell(
                                        header,
                                        gradeNumber == 3
                                            ? "Ave."
                                            : "Final\nGrade",
                                        compactRows,
                                        expandedRows,
                                        moderatelyExpandedRows);
                                    HeaderCell(header, "Remarks", compactRows, expandedRows, moderatelyExpandedRows);
                                });

                            foreach (SF9SubjectGradeRow grade
                                     in displayGrades)
                            {
                                bool isMapehComponent =
                                    IsMapehComponentSubject(
                                        grade);

                                BodyCell(
                                    table,
                                    grade.SubjectName,
                                    false,
                                    false,
                                    isMapehComponent,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);

                                BodyCell(
                                    table,
                                    FormatGrade(
                                        grade.TermOneGrade),
                                    true,
                                    false,
                                    false,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);

                                BodyCell(
                                    table,
                                    FormatGrade(
                                        grade.TermTwoGrade),
                                    true,
                                    false,
                                    false,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);

                                BodyCell(
                                    table,
                                    FormatGrade(
                                        grade.TermThreeGrade),
                                    true,
                                    false,
                                    false,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);

                                BodyCell(
                                    table,
                                    FormatGrade(
                                        grade.FinalGrade),
                                    true,
                                    grade.FinalGrade.HasValue,
                                    false,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);

                                BodyCell(
                                    table,
                                    FormatRemarks(
                                        grade.FinalGrade),
                                    true,
                                    false,
                                    false,
                                    compactRows,
                                    extraCompactRows,
                                    expandedRows,
                                    moderatelyExpandedRows);
                            }

                            int? generalAverage =
                                CalculateGeneralAverage(
                                    prescribedGrades
                                        .Concat(
                                            additionalGrades.Where(
                                                IsAdditionalGeneralAverageSubject))
                                        .ToList());

                            table.Cell()
                                .ColumnSpan(4)
                                .Element(TableCell)
                                .AlignRight()
                                .PaddingRight(4)
                                .Text("General Average")
                                .Bold()
                                .Italic();

                            BodyCell(
                                table,
                                FormatGrade(generalAverage),
                                true,
                                generalAverage.HasValue,
                                false,
                                compactRows,
                                extraCompactRows,
                                expandedRows,
                                moderatelyExpandedRows);

                            BodyCell(
                                table,
                                FormatRemarks(generalAverage),
                                true,
                                false,
                                false,
                                compactRows,
                                extraCompactRows,
                                expandedRows,
                                moderatelyExpandedRows);
                        });
                });
        }

        private static void HeaderCell(
            TableCellDescriptor header,
            string text,
            bool compact = false,
            bool expanded = false,
            bool moderatelyExpanded = false)
        {
            header.Cell()
                .Element(TableCell)
                .MinHeight(
                    expanded
                        ? 23
                        : moderatelyExpanded
                            ? 22
                        : compact
                            ? 17
                            : 20)
                .AlignCenter()
                .AlignMiddle()
                .Text(text)
                .Bold()
                .FontSize(
                    expanded
                        ? 8.6f
                        : moderatelyExpanded
                            ? 8.2f
                        : compact
                            ? 7.1f
                            : 7.6f);
        }

        private static void ComposeSeniorHighGradeTable(
            IContainer container,
            SF9ExportRequest request)
        {
            List<SF9SubjectGradeRow> orderedGrades =
                request.SubjectGrades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .ToList();

            bool isTechPro =
                FormatShsTrackType(
                    request.SchoolClass.TrackStrand)
                .Equals(
                    "TechPro",
                    StringComparison.OrdinalIgnoreCase);

            if (!isTechPro &&
                orderedGrades.Count(IsGradeElevenAcademicElectiveSubject) > 3)
            {
                throw new InvalidOperationException(
                    "Grade 11 Academic SF9 has three elective slots. " +
                    "Review the assigned elective subjects before export.");
            }

            SF9SubjectGradeRow? combinedCommunication =
                orderedGrades.FirstOrDefault(
                    IsCombinedCommunicationSubject);

            SF9SubjectGradeRow? englishCommunication =
                orderedGrades.FirstOrDefault(
                    IsEnglishCommunicationSubject);

            SF9SubjectGradeRow? filipinoCommunication =
                orderedGrades.FirstOrDefault(
                    IsFilipinoCommunicationSubject);

            SF9SubjectGradeRow communication =
                combinedCommunication ??
                CreateCombinedCommunicationGrade(
                    englishCommunication,
                    filipinoCommunication);

            List<SF9SubjectGradeRow> prescribedCoreGrades =
                new()
                {
                    communication,
                    FindSubject(
                        orderedGrades,
                        "general mathematics",
                        "general math"),
                    FindSubject(
                        orderedGrades,
                        "general science"),
                    FindSubject(
                        orderedGrades,
                        "life and career skills",
                        "life and career"),
                    FindSubject(
                        orderedGrades,
                        "pag-aaral ng kasaysayan",
                        "pag aaral ng kasaysayan",
                        "kasaysayan at lipunang pilipino")
                };

            List<SF9SubjectGradeRow> electives;

            if (isTechPro)
            {
                SF9SubjectGradeRow sourceElective =
                    orderedGrades.FirstOrDefault(
                        IsTechProElectiveSubject)
                    ?? orderedGrades.FirstOrDefault(
                        grade =>
                            !IsPrescribedSeniorHighCoreSubject(
                                grade) &&
                            !IsAcademicElectiveSubject(grade))
                    ?? new SF9SubjectGradeRow();

                electives =
                    new List<SF9SubjectGradeRow>
                    {
                        CopyGradeWithUnits(
                            sourceElective,
                            12m)
                    };
            }
            else
            {
                List<SF9SubjectGradeRow> availableElectives =
                    orderedGrades
                        .Where(IsGradeElevenAcademicElectiveSubject)
                        .ToList();
                electives = new List<SF9SubjectGradeRow>();
                for (int term = 1; term <= 3; term++)
                {
                    int capturedTerm = term;
                    SF9SubjectGradeRow? elective =
                        availableElectives.FirstOrDefault(grade =>
                            ContainsAnyText(grade.SubjectName,
                                $"Academic Elective {capturedTerm}"))
                        ?? availableElectives.FirstOrDefault(grade =>
                            capturedTerm switch
                            {
                                1 => grade.TermOneGrade.HasValue,
                                2 => grade.TermTwoGrade.HasValue,
                                _ => grade.TermThreeGrade.HasValue
                            })
                        ?? availableElectives.FirstOrDefault(grade =>
                            !grade.TermOneGrade.HasValue &&
                            !grade.TermTwoGrade.HasValue &&
                            !grade.TermThreeGrade.HasValue);

                    if (elective != null)
                        availableElectives.Remove(elective);
                    electives.Add(elective ?? new SF9SubjectGradeRow
                    {
                        Units = 3m
                    });
                }
                if (availableElectives.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Some Grade 11 electives cannot be assigned to " +
                        "their term slots. Check each elective's term grades.");
                }
            }

            List<SF9SubjectGradeRow> requiredGrades =
                prescribedCoreGrades
                    .Concat(electives)
                    .ToList();

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(
                            "LEARNING PROGRESS AND ACHIEVEMENT")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(4.7f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.8f);
                                });

                            table.Header(
                                header =>
                                {
                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Learning Areas",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "TERM",
                                        columnSpan: 3);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Units",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Final\nGrade",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Remarks",
                                        rowSpan: 2);

                                    SeniorHighHeaderCell(header, "T1");
                                    SeniorHighHeaderCell(header, "T2");
                                    SeniorHighHeaderCell(header, "T3");
                                });

                            SeniorHighCategoryRow(
                                table,
                                "Core Subjects");

                            WriteSeniorHighGradeRow(
                                table,
                                "Effective Communication\n" +
                                "/ Mabisang Komunikasyon",
                                communication);

                            WriteSeniorHighGradeRow(
                                table,
                                "Effective Communication",
                                englishCommunication,
                                termsOnly: true,
                                indented: true);

                            WriteSeniorHighGradeRow(
                                table,
                                "Mabisang Komunikasyon",
                                filipinoCommunication,
                                termsOnly: true,
                                indented: true);

                            WriteSeniorHighGradeRow(
                                table,
                                "General Mathematics",
                                prescribedCoreGrades[1]);

                            WriteSeniorHighGradeRow(
                                table,
                                "General Science",
                                prescribedCoreGrades[2]);

                            WriteSeniorHighGradeRow(
                                table,
                                "Life and Career Skills",
                                prescribedCoreGrades[3]);

                            WriteSeniorHighGradeRow(
                                table,
                                "Pag-Aaral ng Kasaysayan at " +
                                "Lipunang Pilipino",
                                prescribedCoreGrades[4]);

                            SeniorHighCategoryRow(
                                table,
                                "Elective Subjects");

                            if (isTechPro)
                            {
                                WriteSeniorHighGradeRow(
                                    table,
                                    "TechPro Elective 1",
                                    electives[0]);
                            }
                            else
                            {
                                for (int index = 0;
                                     index < 3;
                                     index++)
                                {
                                    WriteSeniorHighGradeRow(
                                        table,
                                        $"Academic Elective {index + 1}",
                                        electives[index],
                                        activeTerm: index + 1);
                                }
                            }

                            int? generalAverage =
                                requiredGrades.All(grade =>
                                    grade.FinalGrade.HasValue &&
                                    grade.Units > 0)
                                    ? CalculateWeightedGeneralAverage(
                                        requiredGrades)
                                    : null;

                            decimal totalUnits =
                                prescribedCoreGrades
                                    .Concat(electives)
                                    .Where(grade => grade.Units > 0)
                                    .Sum(grade => grade.Units);

                            table.Cell()
                                .ColumnSpan(4)
                                .Element(TableCell)
                                .AlignRight()
                                .PaddingRight(4)
                                .Text("General Average")
                                .Bold()
                                .Italic()
                                .FontSize(7f);

                            SeniorHighBodyCell(
                                table,
                                FormatUnits(totalUnits),
                                true,
                                true);

                            SeniorHighBodyCell(
                                table,
                                FormatGrade(generalAverage),
                                true,
                                generalAverage.HasValue);

                            SeniorHighBodyCell(
                                table,
                                FormatRemarks(generalAverage),
                                true);
                        });
                });
        }

        private static void ComposeGradeTwelveAcademicTable(
            IContainer container,
            IReadOnlyList<SF9SubjectGradeRow> grades)
        {
            List<SF9SubjectGradeRow> electives =
                grades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .Where(IsGradeTwelveAcademicElectiveSubject)
                    .Take(12)
                    .Select(
                        grade =>
                            CopyGradeWithUnits(
                                grade,
                                3m))
                    .ToList();

            while (electives.Count < 12)
            {
                electives.Add(
                    new SF9SubjectGradeRow
                    {
                        Units = 3m
                    });
            }

            List<SF9SubjectGradeRow> averageGrades =
                electives
                    .Where(
                        grade =>
                            grade.FinalGrade.HasValue &&
                            grade.Units > 0)
                    .ToList();

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(
                            "LEARNING PROGRESS AND ACHIEVEMENT")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(4.7f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.8f);
                                });

                            table.Header(
                                header =>
                                {
                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Learning Areas",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "TERM",
                                        columnSpan: 3);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Units",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Final\nGrade",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Remarks",
                                        rowSpan: 2);

                                    SeniorHighHeaderCell(header, "T1");
                                    SeniorHighHeaderCell(header, "T2");
                                    SeniorHighHeaderCell(header, "T3");
                                });

                            SeniorHighCategoryRow(
                                table,
                                "Elective Subjects");

                            for (int index = 0;
                                 index < 12;
                                 index++)
                            {
                                int activeTerm =
                                    index < 4
                                        ? 1
                                        : index < 8
                                            ? 2
                                            : 3;

                                WriteSeniorHighGradeRow(
                                    table,
                                    $"Academic Elective {index + 4}",
                                    electives[index],
                                    activeTerm: activeTerm,
                                    compactRow: true);
                            }

                            int? generalAverage =
                                electives.All(grade =>
                                    !string.IsNullOrWhiteSpace(grade.SubjectName) &&
                                    grade.FinalGrade.HasValue)
                                    ? CalculateWeightedGeneralAverage(averageGrades)
                                    : null;

                            decimal totalUnits =
                                electives.Sum(
                                    grade => grade.Units);

                            table.Cell()
                                .ColumnSpan(4)
                                .Element(TableCell)
                                .AlignRight()
                                .PaddingRight(4)
                                .Text("General Average")
                                .Bold()
                                .Italic()
                                .FontSize(7f);

                            SeniorHighBodyCell(
                                table,
                                FormatUnits(totalUnits),
                                true,
                                true,
                                compactRow: true);

                            SeniorHighBodyCell(
                                table,
                                FormatGrade(generalAverage),
                                true,
                                generalAverage.HasValue,
                                compactRow: true);

                            SeniorHighBodyCell(
                                table,
                                FormatRemarks(generalAverage),
                                true,
                                compactRow: true);
                        });
                });
        }

        private static void ComposeGradeTwelveTechProTable(
            IContainer container,
            IReadOnlyList<SF9SubjectGradeRow> grades)
        {
            List<SF9SubjectGradeRow> orderedGrades =
                grades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .ToList();

            List<SF9SubjectGradeRow> techProCandidates =
                orderedGrades
                    .Where(
                        grade =>
                            IsTechProElectiveSubject(grade) &&
                            !ContainsAnyText(
                                grade.SubjectName,
                                "work immersion"))
                    .ToList();

            SF9SubjectGradeRow electiveTwo =
                FindSubject(
                    orderedGrades,
                    "techpro elective 2",
                    "tech pro elective 2",
                    "tech-pro elective 2");

            if (string.IsNullOrWhiteSpace(
                    electiveTwo.SubjectName))
            {
                electiveTwo =
                    techProCandidates.ElementAtOrDefault(0)
                    ?? new SF9SubjectGradeRow();
            }

            SF9SubjectGradeRow electiveThree =
                FindSubject(
                    orderedGrades,
                    "techpro elective 3",
                    "tech pro elective 3",
                    "tech-pro elective 3");

            if (string.IsNullOrWhiteSpace(
                    electiveThree.SubjectName))
            {
                electiveThree =
                    techProCandidates.ElementAtOrDefault(1)
                    ?? new SF9SubjectGradeRow();
            }

            SF9SubjectGradeRow workImmersion =
                FindSubject(
                    orderedGrades,
                    "work immersion for techpro",
                    "work immersion for tech pro",
                    "work immersion for tech-pro",
                    "work immersion");

            if (string.IsNullOrWhiteSpace(
                    workImmersion.SubjectName))
            {
                workImmersion =
                    new SF9SubjectGradeRow();
            }

            List<SF9SubjectGradeRow> electives =
                new()
                {
                    CopyGradeWithUnits(electiveTwo, 12m),
                    CopyGradeWithUnits(electiveThree, 12m),
                    CopyGradeWithUnits(workImmersion, 12m)
                };

            List<SF9SubjectGradeRow> averageGrades =
                electives
                    .Where(
                        grade =>
                            grade.FinalGrade.HasValue &&
                            grade.Units > 0)
                    .ToList();

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(
                            "LEARNING PROGRESS AND ACHIEVEMENT")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(4.7f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.8f);
                                });

                            table.Header(
                                header =>
                                {
                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Learning Areas",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "TERM",
                                        columnSpan: 3);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Units",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Final\nGrade",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Remarks",
                                        rowSpan: 2);

                                    SeniorHighHeaderCell(header, "T1");
                                    SeniorHighHeaderCell(header, "T2");
                                    SeniorHighHeaderCell(header, "T3");
                                });

                            SeniorHighCategoryRow(
                                table,
                                "Elective Subjects");

                            WriteSeniorHighGradeRow(
                                table,
                                "TechPro Elective 2",
                                electives[0],
                                activeTerm: 1);

                            WriteSeniorHighGradeRow(
                                table,
                                "TechPro Elective 3",
                                electives[1],
                                activeTerm: 2);

                            WriteSeniorHighGradeRow(
                                table,
                                "Work Immersion for TechPro Track",
                                electives[2],
                                activeTerm: 3);

                            int? generalAverage =
                                electives.All(grade =>
                                    !string.IsNullOrWhiteSpace(grade.SubjectName) &&
                                    grade.FinalGrade.HasValue)
                                    ? CalculateWeightedGeneralAverage(averageGrades)
                                    : null;

                            decimal totalUnits =
                                electives.Sum(
                                    grade => grade.Units);

                            table.Cell()
                                .ColumnSpan(4)
                                .Element(TableCell)
                                .AlignRight()
                                .PaddingRight(4)
                                .Text("General Average")
                                .Bold()
                                .Italic()
                                .FontSize(7f);

                            SeniorHighBodyCell(
                                table,
                                FormatUnits(totalUnits),
                                true,
                                true);

                            SeniorHighBodyCell(
                                table,
                                FormatGrade(generalAverage),
                                true,
                                generalAverage.HasValue);

                            SeniorHighBodyCell(
                                table,
                                FormatRemarks(generalAverage),
                                true);
                        });
                });
        }

        private static void ComposeGradeTwelveAcademicCrossTable(
            IContainer container,
            IReadOnlyList<SF9SubjectGradeRow> grades)
        {
            List<SF9SubjectGradeRow> orderedGrades =
                grades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .ToList();

            List<SF9SubjectGradeRow> academicElectives =
                orderedGrades
                    .Where(IsExplicitAcademicElectiveSubject)
                    .Take(8)
                    .Select(
                        grade =>
                            CopyGradeWithUnits(
                                grade,
                                3m))
                    .ToList();

            while (academicElectives.Count < 8)
            {
                academicElectives.Add(
                    new SF9SubjectGradeRow
                    {
                        Units = 3m
                    });
            }

            SF9SubjectGradeRow techProElective =
                FindSubject(
                    orderedGrades,
                    "techpro elective 1",
                    "tech pro elective 1",
                    "tech-pro elective 1");

            if (string.IsNullOrWhiteSpace(
                    techProElective.SubjectName))
            {
                techProElective =
                    orderedGrades.FirstOrDefault(
                        grade =>
                            IsTechProElectiveSubject(grade) &&
                            !IsWorkImmersionSubject(grade))
                    ?? new SF9SubjectGradeRow();
            }

            List<(string Label,
                  SF9SubjectGradeRow Grade,
                  int ActiveTerm)> rows =
                new();

            for (int index = 0;
                 index < 8;
                 index++)
            {
                rows.Add(
                    ($"Academic Elective {index + 4}",
                     academicElectives[index],
                     index < 4
                         ? 1
                         : 2));
            }

            rows.Add(
                ("TechPro Elective 1",
                 CopyGradeWithUnits(
                     techProElective,
                     12m),
                 3));

            ComposeGradeTwelveCrossTable(
                container,
                rows,
                compactRows: true);
        }

        private static void ComposeGradeTwelveTechProCrossTable(
            IContainer container,
            IReadOnlyList<SF9SubjectGradeRow> grades)
        {
            List<SF9SubjectGradeRow> orderedGrades =
                grades
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName)
                    .ToList();

            SF9SubjectGradeRow techProElective =
                FindSubject(
                    orderedGrades,
                    "techpro elective 2",
                    "tech pro elective 2",
                    "tech-pro elective 2");

            if (string.IsNullOrWhiteSpace(
                    techProElective.SubjectName))
            {
                techProElective =
                    orderedGrades.FirstOrDefault(
                        grade =>
                            IsTechProElectiveSubject(grade) &&
                            !IsWorkImmersionSubject(grade))
                    ?? new SF9SubjectGradeRow();
            }

            List<SF9SubjectGradeRow> academicElectives =
                orderedGrades
                    .Where(IsExplicitAcademicElectiveSubject)
                    .Take(4)
                    .Select(
                        grade =>
                            CopyGradeWithUnits(
                                grade,
                                3m))
                    .ToList();

            while (academicElectives.Count < 4)
            {
                academicElectives.Add(
                    new SF9SubjectGradeRow
                    {
                        Units = 3m
                    });
            }

            SF9SubjectGradeRow workImmersion =
                orderedGrades.FirstOrDefault(
                    IsWorkImmersionSubject)
                ?? new SF9SubjectGradeRow();

            List<(string Label,
                  SF9SubjectGradeRow Grade,
                  int ActiveTerm)> rows =
                new()
                {
                    ("TechPro Elective 2",
                     CopyGradeWithUnits(
                         techProElective,
                         12m),
                     1)
                };

            for (int index = 0;
                 index < 4;
                 index++)
            {
                rows.Add(
                    ($"Academic Elective {index + 1}",
                     academicElectives[index],
                     2));
            }

            rows.Add(
                ("Work Immersion for TechPro Track",
                 CopyGradeWithUnits(
                     workImmersion,
                     12m),
                 3));

            ComposeGradeTwelveCrossTable(
                container,
                rows,
                compactRows: false);
        }

        private static void ComposeGradeTwelveCrossTable(
            IContainer container,
            IReadOnlyList<(string Label,
                           SF9SubjectGradeRow Grade,
                           int ActiveTerm)> rows,
            bool compactRows)
        {
            List<SF9SubjectGradeRow> averageGrades =
                rows
                    .Select(row => row.Grade)
                    .Where(
                        grade =>
                            grade.FinalGrade.HasValue &&
                            grade.Units > 0)
                    .ToList();

            container.Column(
                column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(
                            "LEARNING PROGRESS AND ACHIEVEMENT")
                        .Bold()
                        .FontSize(8.8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn(4.7f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(0.95f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.8f);
                                });

                            table.Header(
                                header =>
                                {
                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Learning Areas",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "TERM",
                                        columnSpan: 3);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Units",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Final\nGrade",
                                        rowSpan: 2);

                                    SeniorHighSpanningHeaderCell(
                                        header,
                                        "Remarks",
                                        rowSpan: 2);

                                    SeniorHighHeaderCell(header, "T1");
                                    SeniorHighHeaderCell(header, "T2");
                                    SeniorHighHeaderCell(header, "T3");
                                });

                            SeniorHighCategoryRow(
                                table,
                                "Elective Subjects");

                            foreach ((string label,
                                      SF9SubjectGradeRow grade,
                                      int activeTerm) in rows)
                            {
                                WriteSeniorHighGradeRow(
                                    table,
                                    label,
                                    grade,
                                    activeTerm: activeTerm,
                                    compactRow: compactRows);
                            }

                            int? generalAverage =
                                rows.All(row =>
                                    !string.IsNullOrWhiteSpace(row.Grade.SubjectName) &&
                                    row.Grade.FinalGrade.HasValue)
                                    ? CalculateWeightedGeneralAverage(averageGrades)
                                    : null;

                            decimal totalUnits =
                                rows.Sum(
                                    row => row.Grade.Units);

                            table.Cell()
                                .ColumnSpan(4)
                                .Element(TableCell)
                                .AlignRight()
                                .PaddingRight(4)
                                .Text("General Average")
                                .Bold()
                                .Italic()
                                .FontSize(7f);

                            SeniorHighBodyCell(
                                table,
                                FormatUnits(totalUnits),
                                true,
                                true,
                                compactRow: compactRows);

                            SeniorHighBodyCell(
                                table,
                                FormatGrade(generalAverage),
                                true,
                                generalAverage.HasValue,
                                compactRow: compactRows);

                            SeniorHighBodyCell(
                                table,
                                FormatRemarks(generalAverage),
                                true,
                                compactRow: compactRows);
                        });
                });
        }

        private static void SeniorHighCategoryRow(
            TableDescriptor table,
            string label)
        {
            table.Cell()
                .ColumnSpan(7)
                .Element(TableCell)
                .MinHeight(12)
                .AlignMiddle()
                .PaddingLeft(2)
                .Text(label)
                .Bold()
                .Italic()
                .FontSize(7f);
        }

        private static void WriteSeniorHighGradeRow(
            TableDescriptor table,
            string label,
            SF9SubjectGradeRow? grade,
            bool termsOnly = false,
            bool indented = false,
            int? activeTerm = null,
            bool compactRow = false)
        {
            SeniorHighLearningAreaCell(
                table,
                label,
                indented,
                compactRow);

            SeniorHighTermCell(
                table,
                activeTerm.HasValue &&
                activeTerm.Value != 1
                    ? string.Empty
                    : FormatGrade(grade?.TermOneGrade),
                activeTerm.HasValue &&
                activeTerm.Value != 1,
                compactRow);

            SeniorHighTermCell(
                table,
                activeTerm.HasValue &&
                activeTerm.Value != 2
                    ? string.Empty
                    : FormatGrade(grade?.TermTwoGrade),
                activeTerm.HasValue &&
                activeTerm.Value != 2,
                compactRow);

            SeniorHighTermCell(
                table,
                activeTerm.HasValue &&
                activeTerm.Value != 3
                    ? string.Empty
                    : FormatGrade(grade?.TermThreeGrade),
                activeTerm.HasValue &&
                activeTerm.Value != 3,
                compactRow);

            SeniorHighBodyCell(
                table,
                termsOnly
                    ? string.Empty
                    : FormatUnits(grade?.Units ?? 0),
                true,
                compactRow: compactRow);

            SeniorHighBodyCell(
                table,
                termsOnly
                    ? string.Empty
                    : FormatGrade(grade?.FinalGrade),
                true,
                !termsOnly &&
                grade?.FinalGrade.HasValue == true,
                compactRow);

            SeniorHighBodyCell(
                table,
                termsOnly
                    ? string.Empty
                    : FormatRemarks(grade?.FinalGrade),
                true,
                compactRow: compactRow);
        }

        private static void SeniorHighTermCell(
            TableDescriptor table,
            string text,
            bool unavailable,
            bool compactRow = false)
        {
            IContainer cell =
                table.Cell()
                    .Element(TableCell)
                    .MinHeight(
                        compactRow
                            ? 12
                            : 13)
                    .PaddingVertical(
                        compactRow
                            ? 0.4f
                            : 0.7f);

            if (unavailable)
            {
                cell = cell.Background(
                    Colors.Grey.Medium);
            }

            cell = cell
                .AlignCenter()
                .AlignMiddle();

            cell.Text(text)
                .FontSize(
                    compactRow
                        ? 6.6f
                        : 6.8f)
                .LineHeight(1.08f);
        }

        private static void SeniorHighLearningAreaCell(
            TableDescriptor table,
            string text,
            bool indented,
            bool compactRow = false)
        {
            TextBlockDescriptor value =
                table.Cell()
                    .Element(TableCell)
                    .MinHeight(
                        compactRow
                            ? 12
                            : 13)
                    .PaddingVertical(
                        compactRow
                            ? 0.4f
                            : 0.7f)
                    .PaddingLeft(
                        indented
                            ? 12
                            : 1.5f)
                    .PaddingRight(1.5f)
                    .AlignMiddle()
                    .Text(text)
                    .FontSize(
                        compactRow
                            ? 6.6f
                            : 6.8f)
                    .LineHeight(1.08f);

            if (indented)
            {
                value.Italic();
            }
        }

        private static void SeniorHighHeaderCell(
            TableCellDescriptor header,
            string text)
        {
            header.Cell()
                .Element(TableCell)
                .MinHeight(13)
                .AlignCenter()
                .AlignMiddle()
                .Text(text)
                .Bold()
                .FontSize(6.8f)
                .LineHeight(1.05f);
        }

        private static void SeniorHighSpanningHeaderCell(
            TableCellDescriptor header,
            string text,
            uint rowSpan = 1,
            uint columnSpan = 1)
        {
            header.Cell()
                .RowSpan(rowSpan)
                .ColumnSpan(columnSpan)
                .Element(TableCell)
                .MinHeight(13)
                .AlignCenter()
                .AlignMiddle()
                .Text(text)
                .Bold()
                .FontSize(6.8f)
                .LineHeight(1.05f);
        }

        private static void SeniorHighBodyCell(
            TableDescriptor table,
            string text,
            bool centered,
            bool bold = false,
            bool compactRow = false)
        {
            IContainer cell =
                table.Cell()
                    .Element(TableCell)
                    .MinHeight(
                        compactRow
                            ? 12
                            : 13)
                    .PaddingVertical(
                        compactRow
                            ? 0.4f
                            : 0.7f)
                    .AlignMiddle();

            if (centered)
            {
                cell = cell.AlignCenter();
            }
            else
            {
                cell = cell.PaddingHorizontal(1.5f);
            }

            TextBlockDescriptor value =
                cell.Text(text)
                    .FontSize(
                        compactRow
                            ? 6.6f
                            : 6.8f)
                    .LineHeight(1.08f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void BodyCell(
            TableDescriptor table,
            string text,
            bool centered,
            bool bold = false,
            bool italic = false,
            bool compact = false,
            bool extraCompact = false,
            bool expanded = false,
            bool moderatelyExpanded = false)
        {
            IContainer cell =
                table.Cell()
                    .Element(TableCell)
                    .MinHeight(
                        expanded
                            ? 18
                            : moderatelyExpanded
                                ? 16
                            : extraCompact
                            ? 10.5f
                            : compact
                                ? 11.5f
                                : 13)
                    .AlignMiddle();

            if (centered)
            {
                cell = cell.AlignCenter();
            }
            else
            {
                cell = cell.PaddingLeft(2);
            }

            TextBlockDescriptor value =
                cell.Text(text)
                    .FontSize(
                        expanded
                            ? 9f
                            : moderatelyExpanded
                                ? 8.4f
                            : extraCompact
                            ? 6.6f
                            : compact
                                ? 7.1f
                                : 7.6f);

            if (bold)
            {
                value.Bold();
            }

            if (italic)
            {
                value.Italic();
            }
        }

        private static IContainer TableCell(
            IContainer container)
        {
            return container
                .Border(0.55f)
                .BorderColor(Colors.Black);
        }

        private static void ComposePerformanceDescriptors(
            IContainer container,
            bool compact = false,
            bool expanded = false,
            bool moderatelyExpanded = false)
        {
            container.Column(
                column =>
                {
                    column.Item()
                        .Text("PERFORMANCE DESCRIPTORS")
                        .Bold()
                        .FontSize(
                            expanded
                                ? 10f
                                : moderatelyExpanded
                                    ? 9.4f
                                : compact
                                ? 9f
                                : 8f);

                    column.Item().Table(
                        table =>
                        {
                            table.ColumnsDefinition(
                                columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                            DescriptorCell(table, "Grading Scale", true, compact, expanded, moderatelyExpanded);
                            DescriptorCell(table, "Descriptors", true, compact, expanded, moderatelyExpanded);
                            DescriptorCell(table, "Remarks", true, compact, expanded, moderatelyExpanded);

                            string[,] rows =
                            {
                                { "90-100", "Advancing", "Passed" },
                                { "80-89", "Benchmarking", "Passed" },
                                { "75-79", "Connecting", "Passed" },
                                { "65-74", "Developing", "Failed" },
                                { "0-64", "Emerging", "Failed" }
                            };

                            for (int row = 0;
                                 row < rows.GetLength(0);
                                 row++)
                            {
                                DescriptorCell(table, rows[row, 0], false, compact, expanded, moderatelyExpanded);
                                DescriptorCell(table, rows[row, 1], false, compact, expanded, moderatelyExpanded);
                                DescriptorCell(table, rows[row, 2], false, compact, expanded, moderatelyExpanded);
                            }
                        });
                });
        }

        private static void DescriptorCell(
            TableDescriptor table,
            string text,
            bool bold,
            bool compact,
            bool expanded,
            bool moderatelyExpanded)
        {
            TextBlockDescriptor value =
                table.Cell()
                    .MinHeight(
                        expanded
                            ? 12
                            : moderatelyExpanded
                                ? 10
                            : 0)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text)
                    .FontSize(
                        expanded
                            ? 9f
                            : moderatelyExpanded
                                ? 8.5f
                            : compact
                            ? 9f
                            : 6.8f)
                    .LineHeight(
                        expanded
                            ? 1.18f
                            : moderatelyExpanded
                                ? 1.12f
                            : compact
                            ? 1.02f
                            : 1.15f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeRightPanel(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(9);

                    column.Item()
                        .PaddingBottom(3)
                        .Element(
                            value =>
                                ComposeAttendance(
                                    value,
                                    request.AttendanceRows));

                    column.Item()
                        .PaddingTop(3)
                        .Element(
                            value =>
                                ComposeTeacherComments(
                                    value,
                                    request));

                    column.Item()
                        .PaddingTop(4)
                        .PaddingBottom(2)
                        .Element(ComposeParentSignatures);

                    column.Item()
                        .PaddingTop(4)
                        .Element(
                            value =>
                                ComposeTransferCertificate(
                                    value,
                                    request));

                    column.Item()
                        .PaddingTop(5)
                        .Element(
                            value =>
                                ComposeCancellation(
                                    value,
                                    request));
                });
        }

        private static void ComposeAttendance(
            IContainer container,
            IReadOnlyList<SF9AttendanceRow> attendanceRows)
        {
            string[] monthNames =
            {
                "Jun", "Jul", "Aug", "Sep", "Oct", "Nov",
                "Dec", "Jan", "Feb", "Mar", "Apr"
            };

            int[] monthNumbers =
            {
                6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4
            };

            Dictionary<int, SF9AttendanceRow> attendanceByMonth =
                attendanceRows
                    .GroupBy(item => item.MonthNumber)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First());

            container.Table(
                table =>
                {
                    table.ColumnsDefinition(
                        columns =>
                        {
                            columns.RelativeColumn(2.2f);

                            for (int index = 0;
                                 index < 11;
                                 index++)
                            {
                                columns.RelativeColumn();
                            }

                            columns.RelativeColumn(1.5f);
                        });

                    AttendanceCell(table, "Month", true);

                    foreach (string month in monthNames)
                    {
                        AttendanceCell(table, month, true);
                    }

                    AttendanceCell(table, "Total", true);

                    WriteAttendanceRow(
                        table,
                        "No. of Class\nDays",
                        monthNumbers,
                        attendanceByMonth,
                        item => item.SchoolDays);

                    WriteAttendanceRow(
                        table,
                        "No. of Days\nPresent",
                        monthNumbers,
                        attendanceByMonth,
                        item => item.DaysPresent);

                    WriteAttendanceRow(
                        table,
                        "No. of Days\nAbsent",
                        monthNumbers,
                        attendanceByMonth,
                        item => item.DaysAbsent);
                });
        }

        private static void WriteAttendanceRow(
            TableDescriptor table,
            string label,
            IReadOnlyList<int> monthNumbers,
            IReadOnlyDictionary<int, SF9AttendanceRow>
                attendanceByMonth,
            Func<SF9AttendanceRow, int> selector)
        {
            AttendanceCell(table, label, true);

            int total = 0;
            bool hasValue = false;

            foreach (int monthNumber in monthNumbers)
            {
                string value = string.Empty;

                if (attendanceByMonth.TryGetValue(
                        monthNumber,
                        out SF9AttendanceRow? attendance))
                {
                    int number = selector(attendance);

                    if (number > 0)
                    {
                        value = number.ToString();
                        total += number;
                        hasValue = true;
                    }
                }

                AttendanceCell(table, value, false);
            }

            AttendanceCell(
                table,
                hasValue
                    ? total.ToString()
                    : string.Empty,
                true);
        }

        private static void AttendanceCell(
            TableDescriptor table,
            string text,
            bool bold)
        {
            TextBlockDescriptor value =
                table.Cell()
                    .Border(0.55f)
                    .MinHeight(21)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(text)
                    .FontSize(7f);

            if (bold)
            {
                value.Bold();
            }
        }

        private static void ComposeTeacherComments(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(2);

                    column.Item()
                        .PaddingBottom(3)
                        .AlignCenter()
                        .Text("TEACHER'S COMMENTS / REMARKS")
                        .Bold()
                        .FontSize(8.5f);

                    for (int term = 1;
                         term <= 3;
                         term++)
                    {
                        int capturedTerm = term;
                        string teacherRemark =
                            SF9TeacherRemarksRegistry.GetRemark(
                                request,
                                capturedTerm);

                        column.Item()
                            .Border(0.65f)
                            .Height(56)
                            .Padding(4)
                            .Column(
                                termColumn =>
                                {
                                    termColumn.Item()
                                        .Text($"Term {capturedTerm}")
                                        .Bold()
                                        .FontSize(8f);

                                    termColumn.Item()
                                        .PaddingTop(3)
                                        .Text(teacherRemark)
                                        .FontSize(8.5f)
                                        .LineHeight(1.12f);
                                });
                    }
                });
        }

        private static void ComposeParentSignatures(
            IContainer container)
        {
            container.Column(
                column =>
                {
                    column.Spacing(3);

                    column.Item()
                        .PaddingBottom(3)
                        .AlignCenter()
                        .Text("PARENT/S GUARDIAN'S SIGNATURE")
                        .Bold()
                        .FontSize(8f);

                    for (int term = 1;
                         term <= 3;
                         term++)
                    {
                        int capturedTerm = term;

                        column.Item()
                            .PaddingHorizontal(55)
                            .Row(
                                row =>
                                {
                                    row.AutoItem()
                                        .PaddingRight(3)
                                        .Text($"Term {capturedTerm}")
                                        .Bold()
                                        .FontSize(7.2f);

                                    row.RelativeItem()
                                        .BorderBottom(0.6f)
                                        .Text(string.Empty);
                                });
                    }
                });
        }

        private static void ComposeTransferCertificate(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(5);

                    column.Item()
                        .AlignCenter()
                        .Text("CERTIFICATE OF TRANSFER")
                        .Bold()
                        .FontSize(8f);

                    column.Item()
                        .Text(
                            "This is to certify that the above-named " +
                            "learner has satisfactorily completed the " +
                            "requirements for the grade level indicated.")
                        .FontSize(7.5f)
                        .LineHeight(1.32f);

                    column.Item()
                        .PaddingTop(3)
                        .Text("Admitted to Grade: __________________")
                        .FontSize(7.5f);

                    column.Item()
                        .Text(
                            "Eligible for Admission to Grade: " +
                            "__________________")
                        .FontSize(7.5f);

                    column.Item().Row(
                        row =>
                        {
                            row.RelativeItem()
                                .PaddingRight(16)
                                .Element(
                                    value =>
                                        ComposeSignature(
                                            value,
                                            ResolveSchoolHeadName(
                                                request),
                                            "School Head"));

                            row.RelativeItem()
                                .PaddingLeft(16)
                                .Element(
                                    value =>
                                        ComposeSignature(
                                            value,
                                            ResolveAdviserName(
                                                request),
                                            "Adviser"));
                        });
                });
        }

        private static void ComposeCancellation(
            IContainer container,
            SF9ExportRequest request)
        {
            container.Column(
                column =>
                {
                    column.Spacing(5);

                    column.Item()
                        .AlignCenter()
                        .Text(
                            "CANCELLATION OF ELIGIBILITY TO TRANSFER")
                        .Bold()
                        .FontSize(8f);

                    column.Item().Row(
                        row =>
                        {
                            row.RelativeItem()
                                .Text("Admitted in: ____________________")
                                .FontSize(7.5f);

                            row.RelativeItem()
                                .Text("Date: ____________________")
                                .FontSize(7.5f);
                        });

                    column.Item()
                        .Width(150)
                        .Element(
                            value =>
                                ComposeSignature(
                                    value,
                                    ResolveSchoolHeadName(
                                        request),
                                    "School Head"));
                });
        }

        private static void ComposeSignature(
            IContainer container,
            string name,
            string role)
        {
            container.Column(
                column =>
                {
                    column.Item()
                        .PaddingTop(2)
                        .BorderBottom(0.65f)
                        .AlignCenter()
                        .PaddingHorizontal(2)
                        .Text(
                            string.IsNullOrWhiteSpace(name)
                                ? " "
                                : name.Trim().ToUpperInvariant())
                        .Bold()
                        .FontSize(7.8f);

                    column.Item()
                        .PaddingTop(2)
                        .AlignCenter()
                        .Text(role)
                        .FontSize(7.2f);
                });
        }

        private static int? CalculateGeneralAverage(
            IEnumerable<SF9SubjectGradeRow> grades)
        {
            int[] values =
                grades
                    .Where(
                        grade =>
                            grade.FinalGrade.HasValue &&
                            !IsMapehComponentSubject(grade))
                    .Select(grade => grade.FinalGrade!.Value)
                    .ToArray();

            if (values.Length == 0)
            {
                return null;
            }

            return (int)Math.Round(
                values.Average(),
                MidpointRounding.AwayFromZero);
        }

        private static string FormatGrade(
            int? grade)
        {
            return grade?.ToString() ?? string.Empty;
        }

        private static string FormatUnits(
            decimal units)
        {
            return units > 0
                ? units.ToString("0.##")
                : string.Empty;
        }

        private static SF9SubjectGradeRow FindSubject(
            IEnumerable<SF9SubjectGradeRow> grades,
            params string[] expectedValues)
        {
            return grades.FirstOrDefault(
                       grade =>
                           ContainsAnyText(
                               grade.SubjectName,
                               expectedValues) ||
                           ContainsAnyText(
                               grade.LearningArea,
                               expectedValues))
                   ?? new SF9SubjectGradeRow
                   {
                       Units = 6m
                   };
        }

        private static SF9SubjectGradeRow FindSubjectByExactName(
            IEnumerable<SF9SubjectGradeRow> grades,
            params string[] expectedValues)
        {
            string[] normalizedExpectedValues =
                expectedValues
                    .Select(NormalizeSubjectName)
                    .ToArray();

            return grades.FirstOrDefault(
                       grade =>
                           normalizedExpectedValues.Contains(
                               NormalizeSubjectName(
                                   grade.SubjectName)))
                   ?? new SF9SubjectGradeRow();
        }

        private static SF9SubjectGradeRow CopyGradeWithName(
            SF9SubjectGradeRow source,
            string subjectName)
        {
            return new SF9SubjectGradeRow
            {
                SubjectId = source.SubjectId,
                SubjectCode = source.SubjectCode,
                SubjectName = subjectName,
                LearningArea = source.LearningArea,
                SubjectCategory = source.SubjectCategory,
                DisplayOrder = source.DisplayOrder,
                TermOneGrade = source.TermOneGrade,
                TermTwoGrade = source.TermTwoGrade,
                TermThreeGrade = source.TermThreeGrade,
                Units = source.Units,
                FinalGrade = source.FinalGrade
            };
        }

        private static bool IsCombinedCommunicationSubject(
            SF9SubjectGradeRow grade)
        {
            string name =
                NormalizeSubjectName(
                    grade.SubjectName);

            return name.Contains(
                       "effective communication",
                       StringComparison.Ordinal) &&
                   name.Contains(
                       "mabisang komunikasyon",
                       StringComparison.Ordinal);
        }

        private static bool IsEnglishCommunicationSubject(
            SF9SubjectGradeRow grade)
        {
            string name =
                NormalizeSubjectName(
                    grade.SubjectName);

            return name.Contains(
                       "effective communication",
                       StringComparison.Ordinal) &&
                   !name.Contains(
                       "mabisang komunikasyon",
                       StringComparison.Ordinal);
        }

        private static bool IsFilipinoCommunicationSubject(
            SF9SubjectGradeRow grade)
        {
            return ContainsAnyText(
                grade.SubjectName,
                "mabisang komunikasyon");
        }

        private static bool IsAcademicElectiveSubject(
            SF9SubjectGradeRow grade)
        {
            return ContainsAnyText(
                       grade.SubjectCategory,
                       "elective") ||
                   ContainsAnyText(
                       grade.LearningArea,
                       "elective") ||
                   ContainsAnyText(
                       grade.SubjectName,
                       "academic elective",
                       "elective");
        }

        private static bool IsGradeElevenAcademicElectiveSubject(
            SF9SubjectGradeRow grade)
        {
            return IsAcademicElectiveSubject(grade) ||
                   (!IsPrescribedSeniorHighCoreSubject(grade) &&
                    ContainsAnyText(grade.SubjectCategory, "academic"));
        }

        private static bool IsGradeTwelveAcademicElectiveSubject(
            SF9SubjectGradeRow grade)
        {
            bool isAcademic =
                IsAcademicElectiveSubject(grade) ||
                ContainsAnyText(
                    grade.SubjectCategory,
                    "academic") ||
                ContainsAnyText(
                    grade.LearningArea,
                    "academic elective");

            return isAcademic &&
                   !IsPrescribedSeniorHighCoreSubject(grade);
        }

        private static bool IsExplicitAcademicElectiveSubject(
            SF9SubjectGradeRow grade)
        {
            return ContainsAnyText(
                       grade.SubjectName,
                       "academic elective") ||
                   ContainsAnyText(
                       grade.LearningArea,
                       "academic elective") ||
                   (ContainsAnyText(
                        grade.SubjectCategory,
                        "academic") &&
                    !IsPrescribedSeniorHighCoreSubject(grade) &&
                    !IsWorkImmersionSubject(grade));
        }

        private static bool IsTechProElectiveSubject(
            SF9SubjectGradeRow grade)
        {
            return ContainsAnyText(
                       grade.SubjectCategory,
                       "techpro",
                       "tech pro",
                       "tech-pro") ||
                   ContainsAnyText(
                       grade.LearningArea,
                       "techpro elective",
                       "tech pro elective",
                       "tech-pro elective") ||
                   ContainsAnyText(
                       grade.SubjectName,
                       "techpro elective",
                       "tech pro elective",
                       "tech-pro elective");
        }

        private static bool IsWorkImmersionSubject(
            SF9SubjectGradeRow grade)
        {
            return ContainsAnyText(
                       grade.SubjectName,
                       "work immersion") ||
                   ContainsAnyText(
                       grade.LearningArea,
                       "work immersion");
        }

        private static bool IsPrescribedSeniorHighCoreSubject(
            SF9SubjectGradeRow grade)
        {
            return IsCombinedCommunicationSubject(grade) ||
                   IsEnglishCommunicationSubject(grade) ||
                   IsFilipinoCommunicationSubject(grade) ||
                   ContainsAnyText(
                       grade.SubjectName,
                       "general mathematics",
                       "general math",
                       "general science",
                       "life and career skills",
                       "life and career",
                       "pag-aaral ng kasaysayan",
                       "pag aaral ng kasaysayan",
                       "kasaysayan at lipunang pilipino");
        }

        private static SF9SubjectGradeRow CopyGradeWithUnits(
            SF9SubjectGradeRow source,
            decimal units)
        {
            return new SF9SubjectGradeRow
            {
                SubjectName = source.SubjectName,
                LearningArea = source.LearningArea,
                SubjectCategory = source.SubjectCategory,
                DisplayOrder = source.DisplayOrder,
                TermOneGrade = source.TermOneGrade,
                TermTwoGrade = source.TermTwoGrade,
                TermThreeGrade = source.TermThreeGrade,
                Units = units,
                FinalGrade = source.FinalGrade
            };
        }

        private static SF9SubjectGradeRow
            CreateCombinedCommunicationGrade(
                SF9SubjectGradeRow? english,
                SF9SubjectGradeRow? filipino)
        {
            int? termOne =
                AveragePairedGrades(
                    english?.TermOneGrade,
                    filipino?.TermOneGrade);

            int? termTwo =
                AveragePairedGrades(
                    english?.TermTwoGrade,
                    filipino?.TermTwoGrade);

            int? termThree =
                AveragePairedGrades(
                    english?.TermThreeGrade,
                    filipino?.TermThreeGrade);

            return new SF9SubjectGradeRow
            {
                SubjectName =
                    "Effective Communication / " +
                    "Mabisang Komunikasyon",

                TermOneGrade = termOne,
                TermTwoGrade = termTwo,
                TermThreeGrade = termThree,
                Units = 6m,
                FinalGrade =
                    termOne.HasValue &&
                    termTwo.HasValue &&
                    termThree.HasValue
                        ? AverageAvailableGrades(
                            termOne, termTwo, termThree)
                        : null
            };
        }

        private static int? AveragePairedGrades(
            int? english,
            int? filipino)
        {
            return english.HasValue && filipino.HasValue
                ? AverageAvailableGrades(english, filipino)
                : null;
        }

        private static int? AverageAvailableGrades(
            params int?[] grades)
        {
            int[] values =
                grades
                    .Where(grade => grade.HasValue)
                    .Select(grade => grade!.Value)
                    .ToArray();

            return values.Length == 0
                ? null
                : (int)Math.Round(
                    values.Average(),
                    MidpointRounding.AwayFromZero);
        }

        private static int? CalculateWeightedGeneralAverage(
            IEnumerable<SF9SubjectGradeRow> grades)
        {
            SF9SubjectGradeRow[] completed =
                grades
                    .Where(
                        grade =>
                            grade.FinalGrade.HasValue &&
                            grade.Units > 0)
                    .ToArray();

            decimal totalUnits =
                completed.Sum(grade => grade.Units);

            if (totalUnits <= 0)
            {
                return null;
            }

            decimal weightedGrades =
                completed.Sum(
                    grade =>
                        grade.FinalGrade!.Value *
                        grade.Units);

            return (int)Math.Round(
                weightedGrades / totalUnits,
                MidpointRounding.AwayFromZero);
        }

        private static bool ContainsAnyText(
            string? value,
            params string[] expectedValues)
        {
            string normalized =
                NormalizeSubjectName(value);

            return expectedValues.Any(
                expected =>
                    normalized.Contains(
                        NormalizeSubjectName(expected),
                        StringComparison.Ordinal));
        }

        private static string FormatSeniorHighCategory(
            SF9SubjectGradeRow grade)
        {
            string category =
                grade.SubjectCategory?.Trim()
                ?? string.Empty;

            string combinedValue =
                $"{category} {grade.LearningArea} " +
                grade.SubjectName;

            if (combinedValue.Contains(
                    "elective",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Elective Subjects";
            }

            if (combinedValue.Contains(
                    "specialized",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Specialized Subjects";
            }

            if (combinedValue.Contains(
                    "applied",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Applied Subjects";
            }

            if (string.IsNullOrWhiteSpace(category) ||
                combinedValue.Contains(
                    "core",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Core Subjects";
            }

            return category.EndsWith(
                    "Subjects",
                    StringComparison.OrdinalIgnoreCase)
                ? category
                : $"{category} Subjects";
        }

        private static string FormatRemarks(
            int? grade)
        {
            if (!grade.HasValue)
            {
                return string.Empty;
            }

            return grade.Value >= 75
                ? "Passed"
                : "Failed";
        }

        private static bool IsAdditionalSf9Subject(
            SF9SubjectGradeRow grade)
        {
            string category =
                grade.SubjectCategory?.Trim()
                ?? string.Empty;

            return category.Equals(
                       "Additional - SF9 Only",
                       StringComparison.OrdinalIgnoreCase) ||
                   category.Equals(
                       "Additional - SF9 and General Average",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdditionalGeneralAverageSubject(
            SF9SubjectGradeRow grade)
        {
            return string.Equals(
                grade.SubjectCategory?.Trim(),
                "Additional - SF9 and General Average",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMapehMainSubject(
            SF9SubjectGradeRow grade)
        {
            string subjectName =
                NormalizeSubjectName(
                    grade.SubjectName);

            return subjectName == "mapeh" ||
                   subjectName.StartsWith(
                       "mapeh ",
                       StringComparison.Ordinal);
        }

        private static bool IsMusicAndArtsSubject(
            SF9SubjectGradeRow grade)
        {
            string subjectName =
                NormalizeSubjectName(
                    grade.SubjectName);

            return subjectName.Contains(
                       "music and arts",
                       StringComparison.Ordinal) ||
                   subjectName.Contains(
                       "music & arts",
                       StringComparison.Ordinal);
        }

        private static bool
            IsPhysicalEducationAndHealthSubject(
                SF9SubjectGradeRow grade)
        {
            string subjectName =
                NormalizeSubjectName(
                    grade.SubjectName);

            return subjectName.Contains(
                       "physical education and health",
                       StringComparison.Ordinal) ||
                   subjectName.Contains(
                       "pe and health",
                       StringComparison.Ordinal) ||
                   subjectName.Contains(
                       "p.e. and health",
                       StringComparison.Ordinal) ||
                   subjectName.Contains(
                       "pe & health",
                       StringComparison.Ordinal);
        }

        private static bool IsMapehComponentSubject(
            SF9SubjectGradeRow grade)
        {
            return IsMusicAndArtsSubject(grade) ||
                   IsPhysicalEducationAndHealthSubject(grade);
        }

        private static string NormalizeSubjectName(
            string? subjectName)
        {
            return subjectName?
                .Trim()
                .ToLowerInvariant()
                ?? string.Empty;
        }

        private static string ResolveAdviserName(
            SF9ExportRequest request)
        {
            return request.AdviserName?.Trim()
                ?? string.Empty;
        }

        private static string ResolveSchoolHeadName(
            SF9ExportRequest request)
        {
            return string.IsNullOrWhiteSpace(
                    request.SchoolHeadName)
                ? request.School.SchoolHead?.Trim()
                    ?? string.Empty
                : request.SchoolHeadName.Trim();
        }

        private static string FormatRegion(
            string region)
        {
            string value = region?.Trim()
                ?? string.Empty;

            if (value.StartsWith(
                    "REGION ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : $"REGION {value}";
        }

        private static string FormatDivision(
            string division)
        {
            string value = division?.Trim()
                ?? string.Empty;

            const string prefix =
                "SCHOOLS DIVISION OF ";

            if (value.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                value = value[prefix.Length..];
            }

            return string.IsNullOrWhiteSpace(value)
                ? "SCHOOLS DIVISION"
                : $"SCHOOLS DIVISION OF {value.ToUpperInvariant()}";
        }

        private static string FormatSex(
            string sex)
        {
            if (string.Equals(
                    sex,
                    "M",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    sex,
                    "Male",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Male";
            }

            if (string.Equals(
                    sex,
                    "F",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    sex,
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Female";
            }

            return sex?.Trim() ?? string.Empty;
        }

        private static string FormatShsTrackType(
            string? trackStrand)
        {
            string value =
                trackStrand?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals(
                    "Not Applicable",
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string[] academicTracks =
            {
                "Arts, Social Sciences, and Humanities",
                "Business and Entrepreneurship",
                "Science, Technology, Engineering, and Mathematics",
                "Sports, Health, and Wellness"
            };

            if (value.Equals(
                    "Academic",
                    StringComparison.OrdinalIgnoreCase) ||
                value.Equals(
                    "Academics",
                    StringComparison.OrdinalIgnoreCase) ||
                value.Equals(
                    "All Academic",
                    StringComparison.OrdinalIgnoreCase) ||
                academicTracks.Any(
                    track =>
                        value.Equals(
                            track,
                            StringComparison.OrdinalIgnoreCase)))
            {
                return "Academics";
            }

            return "TechPro";
        }

        private static int GetGradeNumber(
            string gradeLevel)
        {
            string value = gradeLevel?
                .Replace(
                    "Grade",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                .Trim()
                ?? string.Empty;

            return int.TryParse(
                value,
                out int gradeNumber)
                    ? gradeNumber
                    : 0;
        }

        private static string FormatGradeLevel(
            string gradeLevel)
        {
            int gradeNumber =
                GetGradeNumber(
                    gradeLevel);

            return gradeNumber > 0
                ? gradeNumber.ToString()
                : gradeLevel?.Trim()
                    ?? string.Empty;
        }
    }
}
