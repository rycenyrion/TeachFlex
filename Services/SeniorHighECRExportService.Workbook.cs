using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SeniorHighECRExportService
    {
        static partial void PopulateWorkbook(
            dynamic workbook,
            SeniorHighEcrExportRequest request)
        {
            WriteInputData(
                workbook,
                request);

            WriteTermSheets(
                workbook,
                request);

            WriteFinalGrades(
                workbook,
                request);
        }

        private static void WriteInputData(
            dynamic workbook,
            SeniorHighEcrExportRequest request)
        {
            dynamic worksheet =
                GetWorksheet(
                    workbook,
                    "INPUT DATA");

            try
            {
                SetCellValue(
                    worksheet,
                    "F10",
                    request.Region);

                SetCellValue(
                    worksheet,
                    "F11",
                    request.Division);

                SetCellValue(
                    worksheet,
                    "F13",
                    request.SchoolId);

                SetCellValue(
                    worksheet,
                    "F14",
                    request.SchoolName);

                SetCellValue(
                    worksheet,
                    "F16",
                    GetSchoolYearStart(
                        request.SchoolYear));

                SetCellValue(
                    worksheet,
                    "F22",
                    request.AdviserName);

                SetCellValue(
                    worksheet,
                    "F24",
                    GetGradeLevelNumber(
                        request.GradeLevel));

                SetCellValue(
                    worksheet,
                    "F25",
                    request.SectionName);

                SetCellValue(
                    worksheet,
                    "F28",
                    request.SubjectCategory);

                SetCellValue(
                    worksheet,
                    "F29",
                    request.SubjectCluster);

                SetCellValue(
                    worksheet,
                    "F30",
                    request.SubjectName);
                SetCellValue(
                    worksheet,
                    "H33",
                    GetStartingTermName(
                    request.StartingTermNumber));

                WriteLearners(
                    worksheet,
                    request.FinalGrades);
            }
            finally
            {
                ReleaseComObject(
                    worksheet);
            }
        }

        private static string GetStartingTermName(
            int termNumber)
        {
            return termNumber switch
            {
                2 => "SECOND TERM",
                3 => "THIRD TERM",
                _ => "FIRST TERM"
            };
        }

        private static void WriteLearners(
                    dynamic worksheet,
            IReadOnlyList<ECRFinalGradeExportRow>
                learners)
        {
            for (int row = 11;
                 row <= 60;
                 row++)
            {
                SetCellValue(
                    worksheet,
                    $"N{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"O{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"R{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"S{row}",
                    string.Empty);
            }

            List<ECRFinalGradeExportRow>
                maleLearners =
                    learners
                        .Where(
                            learner =>
                                string.Equals(
                                    learner.Sex,
                                    "Male",
                                    StringComparison
                                        .OrdinalIgnoreCase))
                        .Take(
                            50)
                        .ToList();

            List<ECRFinalGradeExportRow>
                femaleLearners =
                    learners
                        .Where(
                            learner =>
                                string.Equals(
                                    learner.Sex,
                                    "Female",
                                    StringComparison
                                        .OrdinalIgnoreCase))
                        .Take(
                            50)
                        .ToList();

            for (int index = 0;
                 index < maleLearners.Count;
                 index++)
            {
                int row =
                    11 + index;

                SetCellNumberFormat(
                    worksheet,
                    $"N{row}",
                    "@");

                SetCellValue(
                    worksheet,
                    $"N{row}",
                    maleLearners[index].Lrn);

                SetCellValue(
                    worksheet,
                    $"O{row}",
                    maleLearners[index]
                        .LearnerName);
            }

            for (int index = 0;
                 index < femaleLearners.Count;
                 index++)
            {
                int row =
                    11 + index;

                SetCellNumberFormat(
                    worksheet,
                    $"R{row}",
                    "@");

                SetCellValue(
                    worksheet,
                    $"R{row}",
                    femaleLearners[index].Lrn);

                SetCellValue(
                    worksheet,
                    $"S{row}",
                    femaleLearners[index]
                        .LearnerName);
            }
        }

        private static int GetSchoolYearStart(
            string schoolYear)
        {
            if (string.IsNullOrWhiteSpace(
                    schoolYear))
            {
                return DateTime.Today.Year;
            }

            string firstPart =
                schoolYear
                    .Replace(
                        "–",
                        "-")
                    .Split(
                        '-',
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .FirstOrDefault()
                ?? string.Empty;

            return int.TryParse(
                    firstPart.Trim(),
                    out int startYear)
                ? startYear
                : DateTime.Today.Year;
        }

        private static int GetGradeLevelNumber(
            string gradeLevel)
        {
            return gradeLevel.Contains(
                "12",
                StringComparison.OrdinalIgnoreCase)
                ? 12
                : 11;
        }

        static partial void WriteTermSheets(
            dynamic workbook,
            SeniorHighEcrExportRequest request);

        static partial void WriteFinalGrades(
            dynamic workbook,
            SeniorHighEcrExportRequest request);
    }
}