using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class GradeOnePaceExportService
    {
        static partial void PopulateOfficialWorkbook(
            object workbook,
            GradeOnePaceExportRequest request)
        {
            WriteInputData(
                workbook,
                request);

            WritePaceRatings(
                workbook,
                request);

            WriteNarrativeSummaries(
                workbook,
                request);
        }

        static partial void WritePaceRatings(
            object workbook,
            GradeOnePaceExportRequest request);

        static partial void WriteNarrativeSummaries(
            object workbook,
            GradeOnePaceExportRequest request);

        private static void WriteInputData(
            object workbook,
            GradeOnePaceExportRequest request)
        {
            object? worksheet =
                null;

            try
            {
                worksheet =
                    GetWorksheet(
                        workbook,
                        "INPUT DATA");

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
                    request.District);

                SetCellValue(
                    worksheet,
                    "F15",
                    request.SchoolId);

                SetCellValue(
                    worksheet,
                    "F16",
                    request.SchoolName);

                SetCellValue(
                    worksheet,
                    "F18",
                    request.SchoolYear);

                SetCellValue(
                    worksheet,
                    "F19",
                    request.SchoolHeadName);

                SetCellValue(
                    worksheet,
                    "F26",
                    request.AdviserName);

                SetCellValue(
                    worksheet,
                    "F27",
                    request.GradeLevel);

                SetCellValue(
                    worksheet,
                    "F28",
                    request.SectionName);

                List<GradeOnePaceLearnerExportRow>
                    maleLearners =
                        request.Learners
                            .Where(
                                learner =>
                                    IsMale(
                                        learner.Sex))
                            .OrderBy(
                                learner =>
                                    learner.LearnerName)
                            .Take(
                                50)
                            .ToList();

                List<GradeOnePaceLearnerExportRow>
                    femaleLearners =
                        request.Learners
                            .Where(
                                learner =>
                                    !IsMale(
                                        learner.Sex))
                            .OrderBy(
                                learner =>
                                    learner.LearnerName)
                            .Take(
                                50)
                            .ToList();

                ClearLearnerInputRows(
                    worksheet);

                WriteLearnerGroup(
                    worksheet,
                    maleLearners,
                    "L",
                    "M",
                    "N");

                WriteLearnerGroup(
                    worksheet,
                    femaleLearners,
                    "Q",
                    "R",
                    "S");
            }
            finally
            {
                ReleaseComObject(
                    worksheet);
            }
        }

        private static void ClearLearnerInputRows(
            object worksheet)
        {
            for (int row = 11;
                 row <= 60;
                 row++)
            {
                SetCellValue(
                    worksheet,
                    $"L{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"M{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"N{row}",
                    string.Empty);

                SetCellValue(
                    worksheet,
                    $"Q{row}",
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
        }

        private static void WriteLearnerGroup(
            object worksheet,
            IReadOnlyList<
                GradeOnePaceLearnerExportRow>
                    learners,
            string nameColumn,
            string lrnColumn,
            string birthDateColumn)
        {
            for (int index = 0;
                 index < learners.Count;
                 index++)
            {
                GradeOnePaceLearnerExportRow learner =
                    learners[index];

                int row =
                    11 + index;

                SetCellValue(
                    worksheet,
                    $"{nameColumn}{row}",
                    learner.LearnerName);

                SetCellValue(
                    worksheet,
                    $"{lrnColumn}{row}",
                    string.IsNullOrWhiteSpace(
                        learner.Lrn)
                            ? string.Empty
                            : $"'{learner.Lrn.Trim()}");

                if (learner.BirthDate.HasValue)
                {
                    SetCellValue(
                        worksheet,
                        $"{birthDateColumn}{row}",
                        learner.BirthDate
                            .Value
                            .Date
                            .ToOADate());

                    SetCellNumberFormat(
                        worksheet,
                        $"{birthDateColumn}{row}",
                        "mm/dd/yyyy");
                }
            }
        }

        private static void SetCellNumberFormat(
            object worksheet,
            string cellAddress,
            string numberFormat)
        {
            object? range =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                range =
                    selectedWorksheet.Range[
                        cellAddress];

                dynamic selectedRange =
                    range;

                selectedRange.NumberFormat =
                    numberFormat;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static bool IsMale(
            string sex)
        {
            string normalizedSex =
                sex?
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            return normalizedSex == "male" ||
                   normalizedSex == "m";
        }
    }
}