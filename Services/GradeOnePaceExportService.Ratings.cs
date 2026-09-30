using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class GradeOnePaceExportService
    {
        static partial void WritePaceRatings(
            object workbook,
            GradeOnePaceExportRequest request)
        {
            ArgumentNullException.ThrowIfNull(workbook);
            ArgumentNullException.ThrowIfNull(request);

            List<GradeOnePaceLearnerExportRow> maleLearners =
                request.Learners
                    .Where(learner => IsMaleForPaceRatings(learner.Sex))
                    .OrderBy(learner => learner.LearnerName)
                    .ToList();

            List<GradeOnePaceLearnerExportRow> femaleLearners =
                request.Learners
                    .Where(learner => !IsMaleForPaceRatings(learner.Sex))
                    .OrderBy(learner => learner.LearnerName)
                    .ToList();

            Dictionary<(int LearnerId, int CompetencyId), string> ratings =
                request.Ratings
                    .GroupBy(rating =>
                        (rating.LearnerId, rating.PaceCompetencyId))
                    .ToDictionary(
                        group => group.Key,
                        group => NormalizeRating(
                            group.Last().Rating));

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 1 READING & LITERACY",
                "reading",
                1,
                CreateReadingColumns(),
                15,
                66);

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 2 READING & LITERACY",
                "reading",
                2,
                CreateReadingColumns(),
                15,
                66);

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 3 READING & LITERACY",
                "reading",
                3,
                CreateReadingColumns(),
                15,
                66);

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 1 LANGUAGE",
                "language",
                1,
                CreateLanguageColumns(1),
                15,
                66);

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 2 LANGUAGE",
                "language",
                2,
                CreateLanguageColumns(2),
                15,
                66);

            WriteTermSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 3 LANGUAGE",
                "language",
                3,
                CreateLanguageColumns(3),
                15,
                66);

            WriteCombinedSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 1-3 MATHEMATICS",
                "math",
                CreateMathematicsColumns(),
                16,
                67,
                repairLearnerFormulas: true);

            WriteCombinedSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "TERM 1-3 GMRC",
                "gmrc",
                CreateGmrcColumns(),
                15,
                66,
                repairLearnerFormulas: false);

            WriteCombinedSheet(
                workbook,
                request,
                ratings,
                maleLearners,
                femaleLearners,
                "G1 PACE FORM MAKABANSA",
                "makabansa",
                CreateMakabansaColumns(),
                15,
                66,
                repairLearnerFormulas: false);
        }

        private static void WriteTermSheet(
            object workbook,
            GradeOnePaceExportRequest request,
            IReadOnlyDictionary<(int LearnerId, int CompetencyId), string> ratings,
            IReadOnlyList<GradeOnePaceLearnerExportRow> maleLearners,
            IReadOnlyList<GradeOnePaceLearnerExportRow> femaleLearners,
            string sheetName,
            string learningAreaKey,
            int termNumber,
            IReadOnlyDictionary<string, int[]> columns,
            int maleStartRow,
            int femaleStartRow)
        {
            object? worksheet = null;

            try
            {
                worksheet = GetWorksheet(workbook, sheetName);

                List<GradeOnePaceCompetencyExportRow> competencies =
                    request.Competencies
                        .Where(competency =>
                            competency.TermNumber == termNumber &&
                            IsLearningArea(
                                competency.LearningArea,
                                learningAreaKey))
                        .OrderBy(competency => competency.DisplayOrder)
                        .ToList();

                WriteLearnerRatings(
                    worksheet,
                    maleLearners,
                    maleStartRow,
                    competencies,
                    columns,
                    ratings);

                WriteLearnerRatings(
                    worksheet,
                    femaleLearners,
                    femaleStartRow,
                    competencies,
                    columns,
                    ratings);
            }
            finally
            {
                ReleaseComObject(worksheet);
            }
        }

        private static void WriteCombinedSheet(
            object workbook,
            GradeOnePaceExportRequest request,
            IReadOnlyDictionary<(int LearnerId, int CompetencyId), string> ratings,
            IReadOnlyList<GradeOnePaceLearnerExportRow> maleLearners,
            IReadOnlyList<GradeOnePaceLearnerExportRow> femaleLearners,
            string sheetName,
            string learningAreaKey,
            IReadOnlyDictionary<(int Term, string Code), int> columns,
            int maleStartRow,
            int femaleStartRow,
            bool repairLearnerFormulas)
        {
            object? worksheet = null;

            try
            {
                worksheet = GetWorksheet(workbook, sheetName);

                if (repairLearnerFormulas)
                {
                    RepairMathematicsLearnerFormulas(worksheet);
                }

                List<GradeOnePaceCompetencyExportRow> competencies =
                    request.Competencies
                        .Where(competency =>
                            IsLearningArea(
                                competency.LearningArea,
                                learningAreaKey))
                        .OrderBy(competency => competency.TermNumber)
                        .ThenBy(competency => competency.DisplayOrder)
                        .ToList();

                WriteLearnerRatings(
                    worksheet,
                    maleLearners,
                    maleStartRow,
                    competencies,
                    columns,
                    ratings);

                WriteLearnerRatings(
                    worksheet,
                    femaleLearners,
                    femaleStartRow,
                    competencies,
                    columns,
                    ratings);
            }
            finally
            {
                ReleaseComObject(worksheet);
            }
        }

        private static void WriteLearnerRatings(
            object worksheet,
            IReadOnlyList<GradeOnePaceLearnerExportRow> learners,
            int startRow,
            IReadOnlyList<GradeOnePaceCompetencyExportRow> competencies,
            IReadOnlyDictionary<string, int[]> columns,
            IReadOnlyDictionary<(int LearnerId, int CompetencyId), string> ratings)
        {
            for (int learnerIndex = 0;
                 learnerIndex < learners.Count;
                 learnerIndex++)
            {
                GradeOnePaceLearnerExportRow learner =
                    learners[learnerIndex];

                int row = startRow + learnerIndex;

                foreach (GradeOnePaceCompetencyExportRow competency
                         in competencies)
                {
                    string code = NormalizeCode(
                        competency.CompetencyCode);

                    if (!columns.TryGetValue(code, out int[]? targetColumns))
                    {
                        continue;
                    }

                    ratings.TryGetValue(
                        (learner.LearnerId, competency.PaceCompetencyId),
                        out string? rating);

                    foreach (int column in targetColumns)
                    {
                        SetPaceRatingCellValue(
                            worksheet,
                            row,
                            column,
                            rating ?? string.Empty);
                    }
                }
            }
        }

        private static void WriteLearnerRatings(
            object worksheet,
            IReadOnlyList<GradeOnePaceLearnerExportRow> learners,
            int startRow,
            IReadOnlyList<GradeOnePaceCompetencyExportRow> competencies,
            IReadOnlyDictionary<(int Term, string Code), int> columns,
            IReadOnlyDictionary<(int LearnerId, int CompetencyId), string> ratings)
        {
            for (int learnerIndex = 0;
                 learnerIndex < learners.Count;
                 learnerIndex++)
            {
                GradeOnePaceLearnerExportRow learner =
                    learners[learnerIndex];

                int row = startRow + learnerIndex;

                foreach (GradeOnePaceCompetencyExportRow competency
                         in competencies)
                {
                    var key = (
                        competency.TermNumber,
                        NormalizeCode(competency.CompetencyCode));

                    if (!columns.TryGetValue(key, out int column))
                    {
                        continue;
                    }

                    ratings.TryGetValue(
                        (learner.LearnerId, competency.PaceCompetencyId),
                        out string? rating);

                    SetPaceRatingCellValue(
                        worksheet,
                        row,
                        column,
                        rating ?? string.Empty);
                }
            }
        }

        private static Dictionary<string, int[]>
            CreateReadingColumns()
        {
            return CreateColumnMap(new[]
            {
                "1:K-L", "2:M-N", "3:O-P", "4:Q-R", "5:S-T",
                "6:U-W", "7:X-Z", "8:AA-AC", "9:AD-AF", "10:AG-AI",
                "11:AJ-AL", "12a:AM-AP", "12b:AQ-AT", "13:AU-AW",
                "14:AX-AZ", "15:BA-BD", "16:BE-BG", "17:BH-BJ",
                "18:BK-BM", "19:BN-BP", "20a:BQ-BS", "20b:BT-BV",
                "20c:BW-BY", "20d:BZ-CB", "20e:CC-CE",
                "20f:CF-CH", "20g:CI-CK", "21a:CL-CN",
                "21b:CO-CQ", "22:CR-CS", "22b:CT-CU",
                "22c:CV-CW", "23:CX-CY", "24a:CZ-DA",
                "24b:DB-DC", "24c:DD-DE", "25:DF-DG"
            });
        }

        private static Dictionary<string, int[]>
            CreateLanguageColumns(int termNumber)
        {
            if (termNumber == 3)
            {
                return CreateColumnMap(new[]
                {
                    "1a:K-L", "1b:M-N", "1c:O-P",
                    "2a:Q-R", "2b:S-T",
                    "3a:U-V", "3b:W-X", "3c:Y-Z", "3d:AA-AB",
                    "3e:AC-AD", "3f:AE-AF",
                    "4a:AG-AH", "4b:AI-AJ", "4c:AK-AL", "5:AM-AN",
                    "6a:AO-AP", "6b:AQ-AT", "6c:AU-AV",
                    "7a:AW-AX", "7b:AY-AZ", "7c:BA-BB",
                    "8a:BC-BD", "8b:BE-BF", "8c:BG-BH",
                    "9a:BI-BJ", "9b:BK-BL", "9c:BM-BN", "10:BO-BP",
                    "11:BQ-BR", "12a:BS-BT", "12b:BU-BV",
                    "13a:BW-BX", "13b:BY-BZ", "13c:CA-CB", "13d:CC-CD",
                    "14:CE", "15:CF-CG", "16a:CH-CI", "16b:CJ-CK",
                    "16c:CL-CM", "16d:CN-CO", "17:CP-CQ",
                    "18a:CR-CS", "18b:CT-CU", "18c:CV-CW",
                    "19:CX-CY", "20a:CZ-DA", "20b:DB-DC",
                    "20c:DD-DE", "20d:DF-DG", "20e:DH"
                });
            }

            return CreateColumnMap(new[]
            {
                "1a:K-L", "1b:M-N", "1c:O-P",
                "2a:Q-R", "2b:S-T",
                "3a:U-V", "3b:W-X", "3c:Y-Z", "3d:AA-AB",
                "3e:AC-AD", "3f:AE-AF",
                "4a:AG-AH", "4b:AI-AJ", "4c:AK-AL", "5:AM-AN",
                "6a:AO-AP", "6b:AQ-AR", "6c:AS-AT",
                "7a:AU-AV", "7b:AW-AX", "7c:AY-AZ",
                "8a:BA-BB", "8b:BC-BD", "8c:BE-BF",
                "9a:BG-BH", "9b:BI-BJ", "9c:BK-BL", "10:BM-BN",
                "11:BO-BP", "12a:BQ-BR", "12b:BS-BT",
                "13a:BU-BV", "13b:BW-BX", "13c:BY-BZ", "13d:CA-CB",
                "14:CC", "15:CD-CE",
                "16a:CF-CG", "16b:CH-CI", "16c:CJ-CK", "16d:CL-CM",
                "17:CN-CO", "18a:CP-CQ", "18b:CR-CS", "18c:CT-CU",
                "19:CV-CW", "20a:CX-CY", "20b:CZ-DA",
                "20c:DB-DC", "20d:DD-DE", "20e:DF"
            });
        }

        private static Dictionary<(int Term, string Code), int>
            CreateMathematicsColumns()
        {
            Dictionary<(int Term, string Code), int> result = new();

            AddTermColumns(result, 1, new[]
            {
                "1:K", "2:L", "3:M", "4:N", "5:O",
                "6:P", "7:Q", "8:R", "9a:S", "9b:T",
                "10:U", "11:V", "12:W", "13:X",
                "14:Y", "15:Z", "16:AA"
            });

            AddTermColumns(result, 2, new[]
            {
                "1:AB", "2:AC", "3a:AD", "4:AE", "5:AF",
                "6:AG", "7:AH", "8:AI", "8a:AI", "8b:AJ",
                "9:AK", "10:AL", "11:AM", "12:AN",
                "13:AO", "14a:AP", "14b:AQ", "15:AR",
                "16:AS", "17:AT", "18:AU", "19:AV"
            });

            AddTermColumns(result, 3, new[]
            {
                "1:AW", "2:AX", "3:AY", "4:AZ", "5:BA",
                "6:BB", "7: BC", "8:BD", "9:BE", "10:BF",
                "11:BG", "12:BH", "13:BI", "14:BJ"
            });

            return result;
        }

        private static Dictionary<(int Term, string Code), int>
            CreateGmrcColumns()
        {
            Dictionary<(int Term, string Code), int> result = new();

            AddTermColumns(result, 1, new[]
            {
                "1:K", "2:L", "3:M", "4:N",
                "5:O", "6:P", "7:Q"
            });

            AddTermColumns(result, 2, new[]
            {
                "1:R", "2:S", "3:T", "4:U", "5:V",
                "6:W", "7:X", "8:Y", "9:Z"
            });

            AddTermColumns(result, 3, new[]
            {
                "1:AA", "2:AB", "3:AC", "4:AD",
                "5:AE", "6:AF", "7:AG", "8:AH"
            });

            return result;
        }

        private static Dictionary<(int Term, string Code), int>
            CreateMakabansaColumns()
        {
            Dictionary<(int Term, string Code), int> result = new();

            AddTermColumns(result, 1, new[]
            {
                "1a:K", "1b:L", "1c:M", "2:N", "3:O"
            });

            AddTermColumns(result, 2, new[]
            {
                "1:P", "2:Q", "3:R"
            });

            AddTermColumns(result, 3, new[]
            {
                "1:S", "2:T", "3:U", "4:V",
                "5:W", "6:X", "7:Y"
            });

            return result;
        }

        private static Dictionary<string, int[]> CreateColumnMap(
            IEnumerable<string> definitions)
        {
            Dictionary<string, int[]> result =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (string definition in definitions)
            {
                string[] parts = definition.Split(':');

                string[] range = parts[1].Split('-');
                int firstColumn = ExcelColumnNumber(range[0]);
                int lastColumn = range.Length == 2
                    ? ExcelColumnNumber(range[1])
                    : firstColumn;

                result[NormalizeCode(parts[0])] =
                    Enumerable.Range(
                        firstColumn,
                        lastColumn - firstColumn + 1)
                    .ToArray();
            }

            return result;
        }

        private static void AddTermColumns(
            IDictionary<(int Term, string Code), int> destination,
            int termNumber,
            IEnumerable<string> definitions)
        {
            foreach (string definition in definitions)
            {
                string[] parts = definition.Split(':');

                destination[(termNumber, NormalizeCode(parts[0]))] =
                    ExcelColumnNumber(parts[1]);
            }
        }

        private static int ExcelColumnNumber(string columnName)
        {
            int result = 0;

            foreach (char character in columnName.Trim().ToUpperInvariant())
            {
                result = (result * 26) + (character - 'A' + 1);
            }

            return result;
        }

        private static bool IsLearningArea(
            string learningArea,
            string expectedKey)
        {
            string normalized = NormalizeText(learningArea);

            return expectedKey switch
            {
                "reading" =>
                    normalized.Contains("reading") ||
                    normalized.Contains("literacy"),

                "language" =>
                    normalized == "language" ||
                    normalized.Contains("language"),

                "math" =>
                    normalized.Contains("math"),

                "gmrc" =>
                    normalized == "gmrc" ||
                    normalized.Contains("good manners"),

                "makabansa" =>
                    normalized.Contains("makabansa"),

                _ => false
            };
        }

        private static bool IsMaleForPaceRatings(string sex)
        {
            string normalized = NormalizeText(sex);

            return normalized == "male" ||
                   normalized == "m" ||
                   normalized == "lalaki";
        }

        private static string NormalizeCode(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty)
                .ToLowerInvariant();
        }

        private static string NormalizeRating(string value)
        {
            string rating = (value ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            return rating is "A" or "B" or "C" or "D" or "E"
                ? rating
                : string.Empty;
        }

        private static string NormalizeText(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .ToLowerInvariant();
        }

        private static void RepairMathematicsLearnerFormulas(
            object worksheet)
        {
            for (int index = 0; index < 50; index++)
            {
                int maleRow = 16 + index;
                int femaleRow = 67 + index;
                int inputRow = 11 + index;

                SetLearnerFormula(
                    worksheet,
                    maleRow,
                    inputRow,
                    "L",
                    "M",
                    "N");

                SetLearnerFormula(
                    worksheet,
                    femaleRow,
                    inputRow,
                    "Q",
                    "R",
                    "S");
            }
        }

        private static void SetLearnerFormula(
            object worksheet,
            int outputRow,
            int inputRow,
            string nameColumn,
            string lrnColumn,
            string birthDateColumn)
        {
            SetCellFormula(
                worksheet,
                outputRow,
                3,
                $"=IF('INPUT DATA'!{nameColumn}{inputRow}=\"\",\"\",'INPUT DATA'!{nameColumn}{inputRow})");

            for (int column = 4; column <= 6; column++)
            {
                SetCellFormula(
                    worksheet,
                    outputRow,
                    column,
                    $"=IF('INPUT DATA'!{lrnColumn}{inputRow}=\"\",\"\",'INPUT DATA'!{lrnColumn}{inputRow})");
            }

            for (int column = 7; column <= 8; column++)
            {
                SetCellFormula(
                    worksheet,
                    outputRow,
                    column,
                    $"=IF('INPUT DATA'!{birthDateColumn}{inputRow}=\"\",\"\",'INPUT DATA'!{birthDateColumn}{inputRow})");
            }
        }

        private static void SetCellFormula(
            object worksheet,
            int row,
            int column,
            string formula)
        {
            object? cells = null;
            object? cell = null;

            try
            {
                dynamic selectedWorksheet = worksheet;
                cells = selectedWorksheet.Cells;

                dynamic cellCollection = cells;
                cell = cellCollection.Item[row, column];

                dynamic selectedCell = cell;
                selectedCell.Formula = formula;
            }
            finally
            {
                ReleaseComObject(cell);
                ReleaseComObject(cells);
            }
        }

        private static void SetPaceRatingCellValue(
            object worksheet,
            int row,
            int column,
            object? value)
        {
            object? cells = null;
            object? cell = null;

            try
            {
                dynamic selectedWorksheet = worksheet;
                cells = selectedWorksheet.Cells;

                dynamic cellCollection = cells;
                cell = cellCollection.Item[row, column];

                dynamic selectedCell = cell;
                selectedCell.Value2 = value?.ToString()
                    ?? string.Empty;
            }
            finally
            {
                ReleaseComObject(cell);
                ReleaseComObject(cells);
            }
        }
    }
}
