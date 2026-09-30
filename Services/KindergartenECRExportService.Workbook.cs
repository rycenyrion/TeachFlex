using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class KindergartenECRExportService
    {
        static partial void PopulateWorkbook(
            object workbook,
            KindergartenEcrExportRequest request)
        {
            WriteInputData(
                workbook,
                request);
           
           
            WriteTermRatings(
                workbook,
                request);

            WriteTeacherRemarks(
                workbook,
                request);
        }

        static partial void WriteTermRatings(
            object workbook,
            KindergartenEcrExportRequest request);

        static partial void WriteTeacherRemarks(
            object workbook,
            KindergartenEcrExportRequest request);

        private static void WriteInputData(
            object workbook,
            KindergartenEcrExportRequest request)
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
                    "F12",
                    request.CityMunicipality);

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
                    "Kindergarten");

                SetCellValue(
                    worksheet,
                    "F28",
                    request.SectionName);

                List<KindergartenEcrLearnerExportRow>
                    maleLearners =
                        request.Learners
                            .Where(
                                learner =>
                                    IsMaleKinderLearner(
                                        learner.Sex))
                            .OrderBy(
                                learner =>
                                    learner.LearnerName)
                            .Take(
                                50)
                            .ToList();

                List<KindergartenEcrLearnerExportRow>
                    femaleLearners =
                        request.Learners
                            .Where(
                                learner =>
                                    !IsMaleKinderLearner(
                                        learner.Sex))
                            .OrderBy(
                                learner =>
                                    learner.LearnerName)
                            .Take(
                                50)
                            .ToList();

                ClearLearnerInputRows(
                    worksheet);

                WriteLearnerInputGroup(
                    worksheet,
                    maleLearners,
                    "L",
                    "M",
                    "N");

                WriteLearnerInputGroup(
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

        private static void WriteLearnerInputGroup(
            object worksheet,
            IReadOnlyList<
                KindergartenEcrLearnerExportRow>
                    learners,
            string nameColumn,
            string lrnColumn,
            string birthDateColumn)
        {
            for (int index = 0;
                 index < learners.Count;
                 index++)
            {
                KindergartenEcrLearnerExportRow learner =
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
                else
                {
                    SetCellValue(
                        worksheet,
                        $"{birthDateColumn}{row}",
                        string.Empty);
                }
            }
        }

        private static bool IsMaleKinderLearner(
            string sex)
        {
            string normalized =
                sex?
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            return normalized == "male" ||
                   normalized == "m" ||
                   normalized == "lalaki";
        }
    }
}