using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TeachFlex.Models;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
namespace TeachFlex.Services
{
    public class SF9ExportService :
        ISF9ExportService
    {
        private const string GradeElevenTemplateFileName =
            "DepEd_SF9_G11_Academic.xlsx";

        private const string GradeElevenWorksheetName =
            "SF9 - GRADE 11 ACADEMIC";

        private const string GradeTwelveTemplateFileName =
            "DepEd_SF9_G12_Academic.xlsx";

        private const string GradeTwelveWorksheetName =
            "SF9 - GRADE 12 ACADEMIC";

        private const string GradeFourToTenTemplateFileName =
            "UPDATED G4-10  School Form 9 (SY 26-27).xlsx";

        private const string GradeFourToTenWorksheetName =
            "SF9 - GRADE 4-10";

        public string ExportOfficialSf9Pdf(
            SF9ExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "An SF9 output path is required.",
                    nameof(outputPath));
            }

            ValidateRequest(
                request);

            bool isGradeFourToTen =
                IsGradeFourToTen(
                    request.SchoolClass.GradeLevel);

            bool isGradeTwelve =
                IsGradeTwelve(
                    request.SchoolClass.GradeLevel);

            string templateFileName =
                isGradeFourToTen
                    ? GradeFourToTenTemplateFileName
                    : isGradeTwelve
                    ? GradeTwelveTemplateFileName
                    : GradeElevenTemplateFileName;

            string worksheetName =
                isGradeFourToTen
                    ? GradeFourToTenWorksheetName
                    : isGradeTwelve
                    ? GradeTwelveWorksheetName
                    : GradeElevenWorksheetName;

            string templatePath =
                ResolveTemplatePath(
                    templateFileName,
                    request.SchoolClass.GradeLevel);

            string completeOutputPath =
                Path.GetFullPath(
                    outputPath);

            string? outputDirectory =
                Path.GetDirectoryName(
                    completeOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputDirectory))
            {
                Directory.CreateDirectory(
                    outputDirectory);
            }

            Excel.Application? application =
                null;

            Excel.Workbook? workbook =
                null;

            Excel.Worksheet? worksheet =
                null;

            try
            {
                application =
                    new Excel.Application
                    {
                        Visible =
                            false,

                        DisplayAlerts =
                            false,

                        ScreenUpdating =
                            false,

                        EnableEvents =
                            false
                    };

                workbook =
                    application.Workbooks.Open(
                        templatePath,
                        ReadOnly: true);

                worksheet =
                    workbook.Worksheets[
                        worksheetName]
                        as Excel.Worksheet;

                if (worksheet == null)
                {
                    throw new InvalidOperationException(
                        $"The worksheet '{worksheetName}' " +
                        "was not found in the official SF9 template.");
                }

                PopulateSchoolInformation(
                    worksheet,
                    request,
                    isGradeTwelve);

                PopulateLearnerInformation(
                    worksheet,
                    request,
                    isGradeTwelve);

                PopulateAttendance(
                    worksheet,
                    request.AttendanceRows,
                    isGradeTwelve);

                PopulateSubjectGrades(
                    worksheet,
                    request.SubjectGrades,
                    isGradeTwelve,
                    isGradeFourToTen);

                PopulateSignatories(
                    worksheet,
                    request,
                    isGradeTwelve);

                ConfigurePdfPage(
                    worksheet,
                    isGradeTwelve,
                    isGradeFourToTen);

                worksheet.Activate();

                worksheet.ExportAsFixedFormat(
                    Excel.XlFixedFormatType
                        .xlTypePDF,
                    completeOutputPath,
                    Excel.XlFixedFormatQuality
                        .xlQualityStandard,
                    IncludeDocProperties: true,
                    IgnorePrintAreas: false,
                    OpenAfterPublish: false);

                return completeOutputPath;
            }
            finally
            {
                if (workbook != null)
                {
                    try
                    {
                        workbook.Close(
                            SaveChanges: false);
                    }
                    catch
                    {
                        // Excel cleanup continues.
                    }
                }

                if (application != null)
                {
                    try
                    {
                        application.Quit();
                    }
                    catch
                    {
                        // Excel cleanup continues.
                    }
                }

                ReleaseComObject(
                    worksheet);

                ReleaseComObject(
                    workbook);

                ReleaseComObject(
                    application);

                GC.Collect();

                GC.WaitForPendingFinalizers();

                GC.Collect();

                GC.WaitForPendingFinalizers();
            }
        }

        private static void ValidateRequest(
            SF9ExportRequest request)
        {
            if (request.School == null)
            {
                throw new InvalidOperationException(
                    "School information is required.");
            }

            if (request.AcademicYear == null)
            {
                throw new InvalidOperationException(
                    "The current school year is required.");
            }

            if (request.SchoolClass == null)
            {
                throw new InvalidOperationException(
                    "A Grade 4 to Grade 12 class is required.");
            }

            if (request.Learner == null)
            {
                throw new InvalidOperationException(
                    "A learner must be selected.");
            }

            if (!IsGradeFourToTen(
                    request.SchoolClass.GradeLevel) &&
                !IsGradeEleven(
                    request.SchoolClass.GradeLevel) &&
                !IsGradeTwelve(
                    request.SchoolClass.GradeLevel))
            {
                throw new NotSupportedException(
                    "The current SF9 implementation supports " +
                    "Grades 4 to 10 and Grade 11 to 12 Academic classes only.");
            }
        }

        private static bool IsGradeFourToTen(
            string gradeLevel)
        {
            string normalized =
                NormalizeText(
                    gradeLevel)
                    .Replace(
                        "grade",
                        string.Empty)
                    .Replace(
                        "g",
                        string.Empty)
                    .Trim();

            return int.TryParse(
                       normalized,
                       out int gradeNumber) &&
                   gradeNumber >= 4 &&
                   gradeNumber <= 10;
        }

        private static bool IsGradeEleven(
            string gradeLevel)
        {
            string normalized =
                NormalizeText(
                    gradeLevel);

            return normalized == "11" ||
                   normalized == "grade 11" ||
                   normalized == "g11";
        }

        private static bool IsGradeTwelve(
            string gradeLevel)
        {
            string normalized =
                NormalizeText(
                    gradeLevel);

            return normalized == "12" ||
                   normalized == "grade 12" ||
                   normalized == "g12";
        }

        private static string ResolveTemplatePath(
            string templateFileName,
            string gradeLevel)
        {
            string outputTemplatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    templateFileName);

            if (File.Exists(
                    outputTemplatePath))
            {
                return outputTemplatePath;
            }

            string projectTemplatePath =
                Path.Combine(
                    Environment.CurrentDirectory,
                    "Templates",
                    templateFileName);

            if (File.Exists(
                    projectTemplatePath))
            {
                return projectTemplatePath;
            }

            throw new FileNotFoundException(
                $"The official {gradeLevel} Academic SF9 " +
                $"template was not found. Expected file: " +
                $"Templates\\{templateFileName}");
        }

        private static void PopulateSchoolInformation(
            Excel.Worksheet worksheet,
            SF9ExportRequest request,
            bool isGradeTwelve)
        {
            School school =
                request.School;

            int rowOffset =
                isGradeTwelve
                    ? -2
                    : 0;

            SetCellValue(
                worksheet,
                $"B{7 + rowOffset}",
                CreateLabeledValue(
                    "REGION",
                    NormalizeRegionName(
                        school.Region)));

            string divisionName =
                school.Division?.Trim() ??
                string.Empty;

            const string divisionPrefix =
                "Schools Division of ";

            if (divisionName.StartsWith(
                    divisionPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                divisionName =
                    divisionName[
                        divisionPrefix.Length..];
            }

            SetCellValue(
                worksheet,
                $"B{8 + rowOffset}",
                CreateLabeledValue(
                    "SCHOOLS DIVISION OF",
                    divisionName));

            ConfigureDivisionHeader(
                worksheet,
                isGradeTwelve);

            SetCellValue(
                worksheet,
                $"B{9 + rowOffset}",
                CreateLabeledValue(
                    "District",
                    school.District));

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "B8"
                    : "B10",
                school.SchoolName);

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "B9"
                    : "B11",
                school.SchoolAddress);

            ConfigureSchoolHeaderOrder(
                worksheet,
                isGradeTwelve);

            SetCellValue(
                worksheet,
                $"B{15 + rowOffset}",
                $"School Year " +
                $"{request.AcademicYear.DisplayName}");

            InsertSchoolLogo(
                worksheet,
                school.SchoolLogoPath);

        }

        private static void ConfigureSchoolHeaderOrder(
            Excel.Worksheet worksheet,
            bool isGradeTwelve)
        {
            Excel.Range? schoolNameCell =
                null;

            Excel.Range? schoolAddressCell =
                null;

            Excel.Range? reportTitleCell =
                null;

            try
            {
                int rowOffset =
                    isGradeTwelve
                        ? -2
                        : 0;

                schoolNameCell =
                    worksheet.Range[
                        isGradeTwelve
                            ? "B8"
                            : "B10"];

                schoolNameCell.Font.Bold =
                    true;

                schoolNameCell.Font.Size =
                    12;

                schoolNameCell.ShrinkToFit =
                    true;

                schoolAddressCell =
                    worksheet.Range[
                        isGradeTwelve
                            ? "B9"
                            : "B11"];

                schoolAddressCell.Font.Bold =
                    false;

                schoolAddressCell.Font.Size =
                    10;

                schoolAddressCell.ShrinkToFit =
                    true;

                reportTitleCell =
                    worksheet.Range[$"B{14 + rowOffset}"];

                reportTitleCell.Font.Bold =
                    true;

                reportTitleCell.Font.Size =
                    14;

                reportTitleCell.ShrinkToFit =
                    true;

            }
            finally
            {
                ReleaseComObject(
                    reportTitleCell);

                ReleaseComObject(
                    schoolAddressCell);

                ReleaseComObject(
                    schoolNameCell);
            }
        }

        private static void ConfigureDivisionHeader(
            Excel.Worksheet worksheet,
            bool isGradeTwelve)
        {
            Excel.Range? divisionCell =
                null;

            try
            {
                int row =
                    isGradeTwelve
                        ? 6
                        : 8;

                divisionCell =
                    worksheet.Range[$"B{row}"];

                divisionCell.WrapText =
                    false;

                divisionCell.ShrinkToFit =
                    true;

                divisionCell.Font.Bold =
                    true;

                divisionCell.Font.Size =
                    11;
            }
            finally
            {
                ReleaseComObject(
                    divisionCell);
            }
        }

        private static void PopulateLearnerInformation(
            Excel.Worksheet worksheet,
            SF9ExportRequest request,
            bool isGradeTwelve)
        {
            Learner learner =
                request.Learner;

            int rowOffset =
                isGradeTwelve
                    ? -2
                    : 0;

            SetCellValue(
                worksheet,
                $"C{17 + rowOffset}",
                string.IsNullOrWhiteSpace(
                    learner.OfficialName)
                        ? string.Empty
                        : learner.OfficialName
                            .Trim()
                            .ToUpperInvariant());

            SetTextCellValue(
                worksheet,
                $"C{18 + rowOffset}",
                learner.Lrn);

            SetOptionalNumber(
                worksheet,
                $"I{17 + rowOffset}",
                learner.Age);

            SetCellValue(
                worksheet,
                $"K{17 + rowOffset}",
                FormatSex(
                    learner.Sex));

            SetCellValue(
                worksheet,
                $"I{18 + rowOffset}",
                FormatGradeLevelForHeader(
                    request.SchoolClass.GradeLevel));

            SetCellValue(
                worksheet,
                $"K{18 + rowOffset}",
                request.SchoolClass.SectionName);

            SetCellValue(
                worksheet,
                $"E{19 + rowOffset}",
                FormatTrackStrand(
                    request.SchoolClass.TrackStrand));

            ConfigureLearnerHeaderFields(
                worksheet,
                isGradeTwelve);
        }

        private static void ConfigureLearnerHeaderFields(
            Excel.Worksheet worksheet,
            bool isGradeTwelve)
        {
            int rowOffset =
                isGradeTwelve
                    ? -2
                    : 0;

            ConfigureHeaderValueCell(
                worksheet,
                $"C{17 + rowOffset}",
                11);

            ConfigureHeaderValueCell(
                worksheet,
                $"C{18 + rowOffset}",
                11);

            ConfigureHeaderValueCell(
                worksheet,
                $"K{17 + rowOffset}",
                9);

            ConfigureHeaderValueCell(
                worksheet,
                $"I{18 + rowOffset}",
                10);

            ConfigureHeaderValueCell(
                worksheet,
                $"K{18 + rowOffset}",
                10);

            ConfigureHeaderValueCell(
                worksheet,
                $"E{19 + rowOffset}",
                8);
        }

        private static void ConfigureHeaderValueCell(
            Excel.Worksheet worksheet,
            string address,
            double fontSize)
        {
            Excel.Range? range =
                null;

            try
            {
                range =
                    worksheet.Range[address];

                range.WrapText =
                    false;

                range.ShrinkToFit =
                    true;

                range.Font.Size =
                    fontSize;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static void PopulateAttendance(
            Excel.Worksheet worksheet,
            IReadOnlyList<SF9AttendanceRow>
                attendanceRows,
            bool isGradeTwelve)
        {
            int schoolDaysRow =
                isGradeTwelve
                    ? 3
                    : 5;

            int presentRow =
                isGradeTwelve
                    ? 5
                    : 7;

            int absentRow =
                isGradeTwelve
                    ? 7
                    : 9;

            string[] monthColumns =
            {
                "R", "S", "T", "U", "V", "W",
                "X", "Y", "Z", "AA", "AB"
            };

            string[] schoolDayCells =
                monthColumns
                    .Select(
                        column =>
                            $"{column}{schoolDaysRow}")
                    .ToArray();

            string[] presentCells =
                monthColumns
                    .Select(
                        column =>
                            $"{column}{presentRow}")
                    .ToArray();

            string[] absentCells =
                monthColumns
                    .Select(
                        column =>
                            $"{column}{absentRow}")
                    .ToArray();

            ConfigureAttendanceLabelCell(
                worksheet,
                $"Q{schoolDaysRow}");

            ConfigureAttendanceLabelCell(
                worksheet,
                $"Q{presentRow}");

            ConfigureAttendanceLabelCell(
                worksheet,
                $"Q{absentRow}");

            for (int index = 0;
                 index < schoolDayCells.Length;
                 index++)
            {
                SetCellValue(
                    worksheet,
                    schoolDayCells[index],
                    string.Empty);

                SetCellValue(
                    worksheet,
                    presentCells[index],
                    string.Empty);

                SetCellValue(
                    worksheet,
                    absentCells[index],
                    string.Empty);
            }

            foreach (SF9AttendanceRow attendance
                     in attendanceRows)
            {
                int columnIndex =
                    GetAttendanceColumnIndex(
                        attendance.MonthNumber);

                if (columnIndex < 0 ||
                    columnIndex >=
                    schoolDayCells.Length)
                {
                    continue;
                }

                SetPositiveNumber(
                    worksheet,
                    schoolDayCells[columnIndex],
                    attendance.SchoolDays);

                SetPositiveNumber(
                    worksheet,
                    presentCells[columnIndex],
                    attendance.DaysPresent);

                SetPositiveNumber(
                    worksheet,
                    absentCells[columnIndex],
                    attendance.DaysAbsent);
            }

            int totalSchoolDays =
                attendanceRows.Sum(
                    row =>
                        row.SchoolDays);

            int totalDaysPresent =
                attendanceRows.Sum(
                    row =>
                        row.DaysPresent);

            int totalDaysAbsent =
                attendanceRows.Sum(
                    row =>
                        row.DaysAbsent);

            SetPositiveNumber(
                worksheet,
                $"AC{schoolDaysRow}",
                totalSchoolDays);

            SetPositiveNumber(
                worksheet,
                $"AC{presentRow}",
                totalDaysPresent);

            SetPositiveNumber(
                worksheet,
                $"AC{absentRow}",
                totalDaysAbsent);
        }

        private static void ConfigureAttendanceLabelCell(
            Excel.Worksheet worksheet,
            string address)
        {
            Excel.Range? range =
                null;

            try
            {
                range =
                    worksheet.Range[address];

                range.WrapText =
                    true;

                range.ShrinkToFit =
                    true;

                range.Font.Size =
                    9;

                range.VerticalAlignment =
                    Excel.XlVAlign.xlVAlignCenter;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static int GetAttendanceColumnIndex(
            int monthNumber)
        {
            return monthNumber switch
            {
                6 => 0,
                7 => 1,
                8 => 2,
                9 => 3,
                10 => 4,
                11 => 5,
                12 => 6,
                1 => 7,
                2 => 8,
                3 => 9,
                4 => 10,
                _ => -1
            };
        }

        private static void PopulateSubjectGrades(
            Excel.Worksheet worksheet,
            IReadOnlyList<SF9SubjectGradeRow>
                subjectGrades,
            bool isGradeTwelve,
            bool isGradeFourToTen)
        {
            if (isGradeFourToTen)
            {
                PopulateGradeFourToTenSubjectGrades(
                    worksheet,
                    subjectGrades);

                return;
            }

            if (isGradeTwelve)
            {
                PopulateGradeTwelveSubjectGrades(
                    worksheet,
                    subjectGrades);

                return;
            }

            ClearGradeCells(
                worksheet);

            SF9SubjectGradeRow? combinedCommunication =
                subjectGrades.FirstOrDefault(
                    IsCombinedCommunication);

            SF9SubjectGradeRow? englishCommunication =
                subjectGrades.FirstOrDefault(
                    IsEnglishCommunication);

            SF9SubjectGradeRow? filipinoCommunication =
                subjectGrades.FirstOrDefault(
                    IsFilipinoCommunication);

            if (combinedCommunication != null)
            {
                WriteGradeRow(
                    worksheet,
                    33,
                    combinedCommunication);
            }
            else
            {
                if (englishCommunication != null)
                {
                    WriteTermCellsOnly(
                        worksheet,
                        34,
                        englishCommunication);
                }

                if (filipinoCommunication != null)
                {
                    WriteTermCellsOnly(
                        worksheet,
                        35,
                        filipinoCommunication);
                }

                WriteCombinedCommunicationRow(
                    worksheet,
                    englishCommunication,
                    filipinoCommunication);
            }

            WriteMatchedSubject(
                worksheet,
                subjectGrades,
                36,
                grade =>
                    ContainsAny(
                        grade.SubjectName,
                        "general mathematics",
                        "general math"));

            WriteMatchedSubject(
                worksheet,
                subjectGrades,
                37,
                grade =>
                    ContainsAny(
                        grade.SubjectName,
                        "general science"));

            WriteMatchedSubject(
                worksheet,
                subjectGrades,
                38,
                grade =>
                    ContainsAny(
                        grade.SubjectName,
                        "life and career skills",
                        "life and career"));

            WriteMatchedSubject(
                worksheet,
                subjectGrades,
                39,
                grade =>
                    ContainsAny(
                        grade.SubjectName,
                        "pag-aaral ng kasaysayan",
                        "pag aaral ng kasaysayan",
                        "kasaysayan at lipunang pilipino"));

            List<SF9SubjectGradeRow>
                electiveSubjects =
                    subjectGrades
                        .Where(
                            IsAcademicElective)
                        .OrderBy(
                            grade =>
                                grade.DisplayOrder)
                        .ThenBy(
                            grade =>
                                grade.SubjectName)
                        .Take(3)
                        .ToList();

            int[] electiveRows =
            {
                41,
                42,
                43
            };

            for (int index = 0;
                 index < electiveSubjects.Count;
                 index++)
            {
                WriteGradeRow(
                    worksheet,
                    electiveRows[index],
                    electiveSubjects[index]);
            }

            PopulateGeneralAverage(
                worksheet,
                subjectGrades,
                44);
        }

        private static void
            PopulateGradeFourToTenSubjectGrades(
                Excel.Worksheet worksheet,
                IReadOnlyList<SF9SubjectGradeRow>
                    subjectGrades)
        {
            for (int row = 32;
                 row <= 42;
                 row++)
            {
                foreach (string column
                         in new[]
                         {
                             "F", "G", "H", "I", "J"
                         })
                {
                    SetCellValue(
                        worksheet,
                        $"{column}{row}",
                        string.Empty);
                }
            }

            List<SF9SubjectGradeRow> mainSubjects =
                new();

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                32,
                grade =>
                    MatchesSubject(
                        grade,
                        "filipino"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                33,
                grade =>
                    MatchesSubject(
                        grade,
                        "english"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                34,
                grade =>
                    MatchesSubject(
                        grade,
                        "mathematics",
                        "math"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                35,
                grade =>
                    MatchesSubject(
                        grade,
                        "science"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                36,
                grade =>
                    IsAralingPanlipunanSubject(
                        grade));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                37,
                grade =>
                    MatchesSubject(
                        grade,
                        "gmrc",
                        "values education",
                        "values"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                38,
                grade =>
                    MatchesSubject(
                        grade,
                        "epp",
                        "tle",
                        "edukasyong pantahanan",
                        "technology and livelihood"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                mainSubjects,
                39,
                grade =>
                    IsMapehMainSubject(
                        grade));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                null,
                40,
                grade =>
                    MatchesSubject(
                        grade,
                        "music and arts",
                        "music & arts"));

            WriteGradeFourToTenMatchedSubject(
                worksheet,
                subjectGrades,
                null,
                41,
                grade =>
                    MatchesSubject(
                        grade,
                        "physical education and health",
                        "pe and health",
                        "p.e. and health"));

            List<int> completedFinalGrades =
                mainSubjects
                    .Where(
                        grade =>
                            GetCompletedFinalGrade(
                                grade)
                                .HasValue)
                    .Select(
                        grade =>
                            GetCompletedFinalGrade(
                                grade)!.Value)
                    .ToList();

            if (completedFinalGrades.Count == 0)
            {
                return;
            }

            int generalAverage =
                (int)Math.Round(
                    completedFinalGrades.Average(),
                    0,
                    MidpointRounding.AwayFromZero);

            SetCellValue(
                worksheet,
                "I42",
                generalAverage);

            SetCellValue(
                worksheet,
                "J42",
                GetRemarks(
                    generalAverage));
        }

        private static void
            WriteGradeFourToTenMatchedSubject(
                Excel.Worksheet worksheet,
                IReadOnlyList<SF9SubjectGradeRow>
                    subjectGrades,
                ICollection<SF9SubjectGradeRow>?
                    completedMainSubjects,
                int row,
                Func<SF9SubjectGradeRow, bool>
                    predicate)
        {
            SF9SubjectGradeRow? grade =
                subjectGrades.FirstOrDefault(
                    predicate);

            if (grade == null)
            {
                return;
            }

            SetOptionalNumber(
                worksheet,
                $"F{row}",
                grade.TermOneGrade);

            SetOptionalNumber(
                worksheet,
                $"G{row}",
                grade.TermTwoGrade);

            SetOptionalNumber(
                worksheet,
                $"H{row}",
                grade.TermThreeGrade);

            int? finalGrade =
                GetCompletedFinalGrade(
                    grade);

            SetOptionalNumber(
                worksheet,
                $"I{row}",
                finalGrade);

            SetCellValue(
                worksheet,
                $"J{row}",
                GetRemarks(
                    finalGrade));

            completedMainSubjects?.Add(
                grade);
        }

        private static int? GetCompletedFinalGrade(
            SF9SubjectGradeRow grade)
        {
            if (grade.FinalGrade.HasValue)
            {
                return grade.FinalGrade.Value;
            }

            if (!grade.TermOneGrade.HasValue ||
                !grade.TermTwoGrade.HasValue ||
                !grade.TermThreeGrade.HasValue)
            {
                return null;
            }

            return AverageAvailableGrades(
                grade.TermOneGrade,
                grade.TermTwoGrade,
                grade.TermThreeGrade);
        }

        private static bool MatchesSubject(
            SF9SubjectGradeRow grade,
            params string[] expectedValues)
        {
            return ContainsAny(
                       grade.SubjectName,
                       expectedValues) ||
                   ContainsAny(
                       grade.LearningArea,
                       expectedValues);
        }

        private static bool IsMapehMainSubject(
            SF9SubjectGradeRow grade)
        {
            string subjectName =
                NormalizeText(
                    grade.SubjectName);

            return subjectName == "mapeh" ||
                   subjectName.StartsWith(
                       "mapeh ");
        }

        private static bool
            IsAralingPanlipunanSubject(
                SF9SubjectGradeRow grade)
        {
            string subjectName =
                NormalizeText(
                    grade.SubjectName);

            return subjectName == "ap" ||
                   subjectName.StartsWith(
                       "ap ") ||
                   subjectName.Contains(
                       "araling panlipunan");
        }

        private static void PopulateGradeTwelveSubjectGrades(
            Excel.Worksheet worksheet,
            IReadOnlyList<SF9SubjectGradeRow>
                subjectGrades)
        {
            for (int row = 31;
                 row <= 42;
                 row++)
            {
                SetCellValue(
                    worksheet,
                    $"B{row}",
                    string.Empty);

                foreach (string column
                         in new[]
                         {
                             "F", "G", "H", "I",
                             "J", "K", "L"
                         })
                {
                    SetCellValue(
                        worksheet,
                        $"{column}{row}",
                        string.Empty);
                }
            }

            foreach (string column
                     in new[]
                     {
                         "I", "J", "K", "L"
                     })
            {
                SetCellValue(
                    worksheet,
                    $"{column}43",
                    string.Empty);
            }

            List<SF9SubjectGradeRow> orderedSubjects =
                subjectGrades
                    .OrderBy(
                        grade =>
                            grade.DisplayOrder)
                    .ThenBy(
                        grade =>
                            grade.SubjectName)
                    .Take(12)
                    .ToList();

            for (int index = 0;
                 index < orderedSubjects.Count;
                 index++)
            {
                int row =
                    31 + index;

                SF9SubjectGradeRow grade =
                    orderedSubjects[index];

                SetCellValue(
                    worksheet,
                    $"B{row}",
                    grade.SubjectName);

                WriteGradeRow(
                    worksheet,
                    row,
                    grade);
            }

            PopulateGeneralAverage(
                worksheet,
                subjectGrades,
                43);
        }

        private static void ClearGradeCells(
            Excel.Worksheet worksheet)
        {
            int[] rows =
            {
                33,
                34,
                35,
                36,
                37,
                38,
                39,
                41,
                42,
                43
            };

            foreach (int row
                     in rows)
            {
                SetCellValue(
                    worksheet,
                    $"F{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"G{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"H{row}",
                    string.Empty);
            }

            int[] finalGradeRows =
            {
                33,
                36,
                37,
                38,
                39,
                41,
                42,
                43
            };

            foreach (int row
                     in finalGradeRows)
            {
                SetCellValue(
                    worksheet,
                    $"I{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"J{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"K{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"L{row}",
                    string.Empty);
            }

            SetCellValue(
                worksheet,
                "I44",
                string.Empty);

            SetCellValue(
                worksheet,
                "J44",
                string.Empty);

            SetCellValue(
                worksheet,
                "K44",
                string.Empty);

            SetCellValue(
                worksheet,
                "L44",
                string.Empty);
        }

        private static void WriteMatchedSubject(
            Excel.Worksheet worksheet,
            IReadOnlyList<SF9SubjectGradeRow>
                subjectGrades,
            int row,
            Func<SF9SubjectGradeRow, bool>
                predicate)
        {
            SF9SubjectGradeRow? grade =
                subjectGrades.FirstOrDefault(
                    predicate);

            if (grade == null)
            {
                return;
            }

            WriteGradeRow(
                worksheet,
                row,
                grade);
        }

        private static void WriteGradeRow(
            Excel.Worksheet worksheet,
            int row,
            SF9SubjectGradeRow grade)
        {
            SetOptionalNumber(
                worksheet,
                $"F{row}",
                grade.TermOneGrade);

            SetOptionalNumber(
                worksheet,
                $"G{row}",
                grade.TermTwoGrade);

            SetOptionalNumber(
                worksheet,
                $"H{row}",
                grade.TermThreeGrade);

            SetOptionalDecimal(
                worksheet,
                $"I{row}",
                grade.Units);

            SetOptionalNumber(
                worksheet,
                $"J{row}",
                grade.FinalGrade);

            SetCellValue(
                worksheet,
                $"K{row}",
                grade.Remarks);

            if (grade.FinalGrade.HasValue &&
                grade.Units > 0)
            {
                SetCellValue(
                    worksheet,
                    $"L{row}",
                    Convert.ToDouble(
                        grade.FinalGrade.Value *
                        grade.Units));
            }
        }

        private static void WriteTermCellsOnly(
            Excel.Worksheet worksheet,
            int row,
            SF9SubjectGradeRow grade)
        {
            SetOptionalNumber(
                worksheet,
                $"F{row}",
                grade.TermOneGrade);

            SetOptionalNumber(
                worksheet,
                $"G{row}",
                grade.TermTwoGrade);

            SetOptionalNumber(
                worksheet,
                $"H{row}",
                grade.TermThreeGrade);
        }

        private static void
            WriteCombinedCommunicationRow(
                Excel.Worksheet worksheet,
                SF9SubjectGradeRow?
                    englishCommunication,
                SF9SubjectGradeRow?
                    filipinoCommunication)
        {
            int? termOne =
                AverageAvailableGrades(
                    englishCommunication?
                        .TermOneGrade,
                    filipinoCommunication?
                        .TermOneGrade);

            int? termTwo =
                AverageAvailableGrades(
                    englishCommunication?
                        .TermTwoGrade,
                    filipinoCommunication?
                        .TermTwoGrade);

            int? termThree =
                AverageAvailableGrades(
                    englishCommunication?
                        .TermThreeGrade,
                    filipinoCommunication?
                        .TermThreeGrade);

            int? finalGrade =
                AverageAvailableGrades(
                    termOne,
                    termTwo,
                    termThree);

            SetOptionalNumber(
                worksheet,
                "F33",
                termOne);

            SetOptionalNumber(
                worksheet,
                "G33",
                termTwo);

            SetOptionalNumber(
                worksheet,
                "H33",
                termThree);

            if (!termOne.HasValue &&
                !termTwo.HasValue &&
                !termThree.HasValue)
            {
                return;
            }

            const decimal units =
                6m;

            SetCellValue(
                worksheet,
                "I33",
                Convert.ToDouble(
                    units));

            SetOptionalNumber(
                worksheet,
                "J33",
                finalGrade);

            SetCellValue(
                worksheet,
                "K33",
                GetRemarks(
                    finalGrade));

            if (finalGrade.HasValue)
            {
                SetCellValue(
                    worksheet,
                    "L33",
                    Convert.ToDouble(
                        finalGrade.Value *
                        units));
            }
        }

        private static int?
            AverageAvailableGrades(
                params int?[] grades)
        {
            List<int> availableGrades =
                grades
                    .Where(
                        grade =>
                            grade.HasValue)
                    .Select(
                        grade =>
                            grade!.Value)
                    .ToList();

            if (availableGrades.Count == 0)
            {
                return null;
            }

            return (int)Math.Round(
                availableGrades.Average(),
                0,
                MidpointRounding.AwayFromZero);
        }

        private static void PopulateGeneralAverage(
            Excel.Worksheet worksheet,
            IReadOnlyList<SF9SubjectGradeRow>
                subjectGrades,
            int generalAverageRow)
        {
            List<SF9SubjectGradeRow>
                completedSubjects =
                    subjectGrades
                        .Where(
                            grade =>
                                grade.FinalGrade.HasValue &&
                                grade.Units > 0)
                        .ToList();

            if (completedSubjects.Count == 0)
            {
                return;
            }

            decimal totalUnits =
                completedSubjects.Sum(
                    grade =>
                        grade.Units);

            decimal totalWeightedGrades =
                completedSubjects.Sum(
                    grade =>
                        grade.FinalGrade!.Value *
                        grade.Units);

            if (totalUnits <= 0)
            {
                return;
            }

            int generalAverage =
                (int)Math.Round(
                    totalWeightedGrades /
                    totalUnits,
                    0,
                    MidpointRounding.AwayFromZero);

            SetCellValue(
                worksheet,
                $"I{generalAverageRow}",
                Convert.ToDouble(
                    totalUnits));

            SetCellValue(
                worksheet,
                $"J{generalAverageRow}",
                generalAverage);

            SetCellValue(
                worksheet,
                $"K{generalAverageRow}",
                GetRemarks(
                    generalAverage));

            SetCellValue(
                worksheet,
                $"L{generalAverageRow}",
                Convert.ToDouble(
                    totalWeightedGrades));
        }

        private static bool
            IsCombinedCommunication(
                SF9SubjectGradeRow grade)
        {
            string subject =
                NormalizeText(
                    grade.SubjectName);

            return subject.Contains(
                       "effective communication") &&
                   subject.Contains(
                       "mabisang komunikasyon");
        }

        private static bool
            IsEnglishCommunication(
                SF9SubjectGradeRow grade)
        {
            string subject =
                NormalizeText(
                    grade.SubjectName);

            return subject.Contains(
                       "effective communication") &&
                   !subject.Contains(
                       "mabisang komunikasyon");
        }

        private static bool
            IsFilipinoCommunication(
                SF9SubjectGradeRow grade)
        {
            return ContainsAny(
                grade.SubjectName,
                "mabisang komunikasyon");
        }

        private static bool IsAcademicElective(
            SF9SubjectGradeRow grade)
        {
            return ContainsAny(
                       grade.SubjectCategory,
                       "elective") ||
                   ContainsAny(
                       grade.LearningArea,
                       "elective") ||
                   ContainsAny(
                       grade.SubjectName,
                       "academic elective",
                       "elective");
        }

        private static bool ContainsAny(
            string value,
            params string[] expectedValues)
        {
            string normalizedValue =
                NormalizeText(
                    value);

            return expectedValues.Any(
                expected =>
                    normalizedValue.Contains(
                        NormalizeText(
                            expected)));
        }

        private static string NormalizeText(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? string.Empty
                : value
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "  ",
                        " ");
        }

        private static string GetRemarks(
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

        private static void PopulateSignatories(
            Excel.Worksheet worksheet,
            SF9ExportRequest request,
            bool isGradeTwelve)
        {
            string adviserName =
                string.IsNullOrWhiteSpace(
                    request.AdviserName)
                    ? request.SchoolClass
                        .Adviser?.ToString()
                        ?? string.Empty
                    : request.AdviserName;

            string schoolHeadName =
                string.IsNullOrWhiteSpace(
                    request.SchoolHeadName)
                    ? request.School.SchoolHead
                    : request.SchoolHeadName;

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "B25"
                    : "B27",
                schoolHeadName);

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "H25"
                    : "H27",
                adviserName);

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "X37"
                    : "X40",
                adviserName);

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "Q39"
                    : "Q41",
                schoolHeadName);

            SetCellValue(
                worksheet,
                isGradeTwelve
                    ? "Q49"
                    : "Q49",
                schoolHeadName);

            if (isGradeTwelve)
            {
                ConfigureGradeTwelvePrimarySignatories(
                    worksheet);
            }
        }

        private static void
            ConfigureGradeTwelvePrimarySignatories(
                Excel.Worksheet worksheet)
        {
            Excel.Range? signatoryRow =
                null;

            try
            {
                signatoryRow =
                    worksheet.Range["25:25"];

                signatoryRow.RowHeight =
                    18;

                ConfigureHeaderValueCell(
                    worksheet,
                    "B25",
                    10);

                ConfigureHeaderValueCell(
                    worksheet,
                    "H25",
                    10);
            }
            finally
            {
                ReleaseComObject(
                    signatoryRow);
            }
        }
        private static void InsertSchoolLogo(
            Excel.Worksheet worksheet,
            string schoolLogoPath)
        {
            if (string.IsNullOrWhiteSpace(
                    schoolLogoPath) ||
                !File.Exists(
                    schoolLogoPath))
            {
                return;
            }

            Excel.Shapes? shapes =
                null;

            Excel.Shape? placeholder =
                null;

            Excel.Shape? insertedLogo =
                null;

            try
            {
                shapes =
                    worksheet.Shapes;

                placeholder =
                    FindSchoolLogoPlaceholder(
                        shapes);
                if (placeholder == null)
                {
                    return;
                }

                float placeholderLeft =
                    placeholder.Left;

                float placeholderTop =
                    placeholder.Top;

                float placeholderWidth =
                    placeholder.Width;

                float placeholderHeight =
                    placeholder.Height;

                insertedLogo =
                    shapes.AddPicture(
                        schoolLogoPath,
                        Office.MsoTriState.msoFalse,
                        Office.MsoTriState.msoTrue,
                        placeholderLeft,
                        placeholderTop,
                        -1,
                        -1);

                insertedLogo.LockAspectRatio =
                    Office.MsoTriState.msoFalse;

                float originalWidth =
                    insertedLogo.Width;

                float originalHeight =
                    insertedLogo.Height;

                float widthScale =
                    placeholderWidth /
                    originalWidth;

                float heightScale =
                    placeholderHeight /
                    originalHeight;

                float selectedScale =
                    Math.Min(
                        widthScale,
                        heightScale);

                insertedLogo.Width =
                    originalWidth *
                    selectedScale;

                insertedLogo.Height =
                    originalHeight *
                    selectedScale;

                insertedLogo.LockAspectRatio =
                    Office.MsoTriState.msoTrue;

                insertedLogo.Left =
                    placeholderLeft +
                    (
                        placeholderWidth -
                        insertedLogo.Width
                    ) / 2f;

                insertedLogo.Top =
                    placeholderTop +
                    (
                        placeholderHeight -
                        insertedLogo.Height
                    ) / 2f;

                insertedLogo.Name =
                    "TeachFlexSchoolLogo";

                placeholder.Delete();
            }
            finally
            {
                ReleaseComObject(
                    insertedLogo);

                ReleaseComObject(
                    placeholder);

                ReleaseComObject(
                    shapes);
            }
        }

        private static Excel.Shape?
    FindSchoolLogoPlaceholder(
        Excel.Shapes shapes)
        {
            for (int index = 1;
                 index <= shapes.Count;
                 index++)
            {
                Excel.Shape? shape =
                    null;

                try
                {
                    shape =
                        shapes.Item(
                            index);

                    if (shape.Name.Equals(
                            "Shape 3",
                            StringComparison.OrdinalIgnoreCase) ||
                        shape.Name.Equals(
                            "Shape 4",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return shape;
                    }
                }
                catch
                {
                    ReleaseComObject(
                        shape);

                    continue;
                }

                ReleaseComObject(
                    shape);
            }

            return null;
        }

        private static void ConfigurePdfPage(
            Excel.Worksheet worksheet,
            bool isGradeTwelve,
            bool isGradeFourToTen)
        {
            Excel.PageSetup? pageSetup =
                null;

            try
            {
                if (isGradeFourToTen)
                {
                    ExpandGradeFourToTenPageHeight(
                        worksheet);
                }

                pageSetup =
                    worksheet.PageSetup;

                pageSetup.PrintArea =
                    isGradeFourToTen
                        ? "$A$2:$AD$52"
                        : isGradeTwelve
                        ? "$A$1:$AD$52"
                        : "$A$2:$AD$53";

                pageSetup.Orientation =
                    Excel.XlPageOrientation
                        .xlLandscape;

                pageSetup.PaperSize =
                    Excel.XlPaperSize
                        .xlPaperA4;

                pageSetup.Zoom =
                    false;

                pageSetup.FitToPagesWide =
                    1;

                pageSetup.FitToPagesTall =
                    1;

                pageSetup.CenterHorizontally =
                    true;

                pageSetup.CenterVertically =
                    false;
            }
            finally
            {
                ReleaseComObject(
                    pageSetup);
            }
        }

        private static void
            ExpandGradeFourToTenPageHeight(
                Excel.Worksheet worksheet)
        {
            const double heightMultiplier =
                1.10;

            for (int rowNumber = 2;
                 rowNumber <= 52;
                 rowNumber++)
            {
                Excel.Range? row =
                    null;

                try
                {
                    row =
                        worksheet.Range[
                            $"{rowNumber}:{rowNumber}"];

                    if (row == null)
                    {
                        continue;
                    }

                    double currentHeight =
                        Convert.ToDouble(
                            row.RowHeight);

                    if (currentHeight > 0)
                    {
                        row.RowHeight =
                            currentHeight *
                            heightMultiplier;
                    }
                }
                finally
                {
                    ReleaseComObject(
                        row);
                }
            }
        }

        private static string CreateLabeledValue(
            string label,
            string value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? label
                : $"{label} {value.Trim()}";
        }

        private static string NormalizeRegionName(
            string region)
        {
            if (string.IsNullOrWhiteSpace(
                    region))
            {
                return string.Empty;
            }

            string normalizedRegion =
                region.Trim();

            const string regionPrefix =
                "Region ";

            if (normalizedRegion.StartsWith(
                    regionPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalizedRegion =
                    normalizedRegion[
                        regionPrefix.Length..]
                        .Trim();
            }

            return normalizedRegion;
        }

        private static string FormatSex(
            string sex)
        {
            if (sex.Equals(
                    "Male",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Male";
            }

            if (sex.Equals(
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Female";
            }

            return sex;
        }

        private static string FormatTrackStrand(
            string trackStrand)
        {
            if (string.IsNullOrWhiteSpace(
                    trackStrand) ||
                trackStrand.Equals(
                    "Not Applicable",
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return trackStrand.Trim();
        }

        private static string FormatGradeLevelForHeader(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return string.Empty;
            }

            string value =
                gradeLevel.Trim();

            const string gradePrefix =
                "Grade ";

            return value.StartsWith(
                    gradePrefix,
                    StringComparison.OrdinalIgnoreCase)
                ? value[gradePrefix.Length..]
                : value;
        }

        private static void SetOptionalNumber(
            Excel.Worksheet worksheet,
            string address,
            int? value)
        {
            SetCellValue(
                worksheet,
                address,
                value.HasValue
                    ? value.Value
                    : string.Empty);
        }

        private static void SetPositiveNumber(
            Excel.Worksheet worksheet,
            string address,
            int value)
        {
            SetCellValue(
                worksheet,
                address,
                value > 0
                    ? value
                    : string.Empty);
        }

        private static void SetOptionalDecimal(
            Excel.Worksheet worksheet,
            string address,
            decimal value)
        {
            SetCellValue(
                worksheet,
                address,
                value > 0
                    ? Convert.ToDouble(
                        value)
                    : string.Empty);
        }

        private static void SetCellValue(
            Excel.Worksheet worksheet,
            string address,
            object? value)
        {
            Excel.Range? range =
                null;

            try
            {
                range =
                    worksheet.Range[
                        address];

                range.Value2 =
                    value ?? string.Empty;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static void SetTextCellValue(
            Excel.Worksheet worksheet,
            string address,
            object? value)
        {
            Excel.Range? range =
                null;

            try
            {
                range =
                    worksheet.Range[address];

                range.NumberFormat =
                    "@";

                range.Value2 =
                    Convert.ToString(value) ??
                    string.Empty;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static void ReleaseComObject(
            object? comObject)
        {
            if (comObject == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(
                        comObject))
                {
                    Marshal.FinalReleaseComObject(
                        comObject);
                }
            }
            catch
            {
                // No further cleanup is required.
            }
        }
    }
}
