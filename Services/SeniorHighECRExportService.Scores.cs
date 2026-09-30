using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SeniorHighECRExportService
    {
        static partial void WriteTermSheetData(
            dynamic worksheet,
            SeniorHighEcrExportRequest request,
            ECRTermExportData? termData)
        {
            List<ECRAssessmentExportColumn>
                writtenColumns =
                    GetAssessmentColumns(
                        termData,
                        IsWrittenAssessment,
                        10);

            List<ECRAssessmentExportColumn>
                performanceColumns =
                    GetAssessmentColumns(
                        termData,
                        IsPerformanceAssessment,
                        10);

            List<ECRAssessmentExportColumn>
                examinationColumns =
                    termData?
                        .AssessmentColumns
                        .Where(
                            column =>
                                !IsWrittenAssessment(
                                    column) &&
                                !IsPerformanceAssessment(
                                    column))
                        .OrderBy(
                            column =>
                                column.DisplayOrder)
                        .ToList()
                    ?? new List<
                        ECRAssessmentExportColumn>();

            ECRAssessmentExportColumn?
                summativeTestOne =
                    FindExamination(
                        examinationColumns,
                        "ST1",
                        "SUMMATIVETEST1");

            ECRAssessmentExportColumn?
                summativeTestTwo =
                    FindExamination(
                        examinationColumns,
                        "ST2",
                        "SUMMATIVETEST2");

            ECRAssessmentExportColumn?
                termExamination =
                    FindExamination(
                        examinationColumns,
                        "TE",
                        "TERMEXAM",
                        "TERMEXAMINATION");

            WriteHighestPossibleScores(
                worksheet,
                writtenColumns,
                performanceColumns,
                summativeTestOne,
                summativeTestTwo,
                termExamination);

            WriteLearnerScoreBlocks(
                worksheet,
                request,
                termData,
                writtenColumns,
                performanceColumns,
                summativeTestOne,
                summativeTestTwo,
                termExamination);
        }

        private static List<
            ECRAssessmentExportColumn>
                GetAssessmentColumns(
                    ECRTermExportData? termData,
                    Func<
                        ECRAssessmentExportColumn,
                        bool> predicate,
                    int maximumCount)
        {
            return termData?
                .AssessmentColumns
                .Where(
                    predicate)
                .OrderBy(
                    column =>
                        column.DisplayOrder)
                .Take(
                    maximumCount)
                .ToList()
            ?? new List<
                ECRAssessmentExportColumn>();
        }

        private static bool IsWrittenAssessment(
            ECRAssessmentExportColumn column)
        {
            string category =
                NormalizeAssessmentText(
                    column.Category);

            string name =
                NormalizeAssessmentText(
                    column.AssessmentName);

            return category.Contains(
                       "WRITTEN") ||
                   category.Contains(
                       "ORAL") ||
                   name.StartsWith(
                       "WW");
        }

        private static bool
            IsPerformanceAssessment(
                ECRAssessmentExportColumn column)
        {
            string category =
                NormalizeAssessmentText(
                    column.Category);

            string name =
                NormalizeAssessmentText(
                    column.AssessmentName);

            return category.Contains(
                       "PERFORMANCE") ||
                   category.Contains(
                       "PRODUCT") ||
                   name.StartsWith(
                       "PT");
        }

        private static
            ECRAssessmentExportColumn?
                FindExamination(
                    IReadOnlyList<
                        ECRAssessmentExportColumn>
                            columns,
                    params string[] acceptedNames)
        {
            foreach (
                ECRAssessmentExportColumn column
                in columns)
            {
                string normalizedName =
                    NormalizeAssessmentText(
                        column.AssessmentName);

                if (acceptedNames.Any(
                        accepted =>
                            normalizedName ==
                                accepted ||
                            normalizedName.Contains(
                                accepted)))
                {
                    return column;
                }
            }

            return null;
        }

        private static string
            NormalizeAssessmentText(
                string value)
        {
            StringBuilder result =
                new StringBuilder();

            foreach (char character
                     in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(
                        character))
                {
                    result.Append(
                        char.ToUpperInvariant(
                            character));
                }
            }

            return result.ToString();
        }

        private static void
            WriteHighestPossibleScores(
                dynamic worksheet,
                IReadOnlyList<
                    ECRAssessmentExportColumn>
                        writtenColumns,
                IReadOnlyList<
                    ECRAssessmentExportColumn>
                        performanceColumns,
                ECRAssessmentExportColumn?
                    summativeTestOne,
                ECRAssessmentExportColumn?
                    summativeTestTwo,
                ECRAssessmentExportColumn?
                    termExamination)
        {
            object?[,] writtenValues =
                new object[1, 10];

            object?[,] performanceValues =
                new object[1, 10];

            object?[,] examinationValues =
                new object[1, 3];

            for (int index = 0;
                 index < writtenColumns.Count;
                 index++)
            {
                writtenValues[0, index] =
                    Convert.ToDouble(
                        writtenColumns[index]
                            .HighestPossibleScore);
            }

            for (int index = 0;
                 index < performanceColumns.Count;
                 index++)
            {
                performanceValues[0, index] =
                    Convert.ToDouble(
                        performanceColumns[index]
                            .HighestPossibleScore);
            }

            examinationValues[0, 0] =
                GetHighestPossibleScore(
                    summativeTestOne);

            examinationValues[0, 1] =
                GetHighestPossibleScore(
                    summativeTestTwo);

            examinationValues[0, 2] =
                GetHighestPossibleScore(
                    termExamination);

            SetRangeValues(
                worksheet,
                "D14:M14",
                writtenValues);

            SetRangeValues(
                worksheet,
                "Q14:Z14",
                performanceValues);

            SetRangeValues(
                worksheet,
                "AD14:AF14",
                examinationValues);
        }

        private static object?
            GetHighestPossibleScore(
                ECRAssessmentExportColumn? column)
        {
            if (column == null ||
                column.HighestPossibleScore <= 0)
            {
                return null;
            }

            return Convert.ToDouble(
                column.HighestPossibleScore);
        }

        private static void WriteLearnerScoreBlocks(
            dynamic worksheet,
            SeniorHighEcrExportRequest request,
            ECRTermExportData? termData,
            IReadOnlyList<
                ECRAssessmentExportColumn>
                    writtenColumns,
            IReadOnlyList<
                ECRAssessmentExportColumn>
                    performanceColumns,
            ECRAssessmentExportColumn?
                summativeTestOne,
            ECRAssessmentExportColumn?
                summativeTestTwo,
            ECRAssessmentExportColumn?
                termExamination)
        {
            List<ECRFinalGradeExportRow>
                maleLearners =
                    request.FinalGrades
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
                    request.FinalGrades
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

            Dictionary<int, ECRLearnerExportRow>
                scoreLookup =
                    termData?
                        .Learners
                        .GroupBy(
                            learner =>
                                learner.LearnerId)
                        .ToDictionary(
                            group =>
                                group.Key,
                            group =>
                                group.First())
                    ?? new Dictionary<
                        int,
                        ECRLearnerExportRow>();

            WriteLearnerScoreBlock(
                worksheet,
                maleLearners,
                scoreLookup,
                17,
                writtenColumns,
                performanceColumns,
                summativeTestOne,
                summativeTestTwo,
                termExamination);

            WriteLearnerScoreBlock(
                worksheet,
                femaleLearners,
                scoreLookup,
                68,
                writtenColumns,
                performanceColumns,
                summativeTestOne,
                summativeTestTwo,
                termExamination);
        }

        private static void WriteLearnerScoreBlock(
            dynamic worksheet,
            IReadOnlyList<ECRFinalGradeExportRow>
                learners,
            IReadOnlyDictionary<
                int,
                ECRLearnerExportRow>
                    scoreLookup,
            int startingRow,
            IReadOnlyList<
                ECRAssessmentExportColumn>
                    writtenColumns,
            IReadOnlyList<
                ECRAssessmentExportColumn>
                    performanceColumns,
            ECRAssessmentExportColumn?
                summativeTestOne,
            ECRAssessmentExportColumn?
                summativeTestTwo,
            ECRAssessmentExportColumn?
                termExamination)
        {
            object?[,] writtenScores =
                new object[50, 10];

            object?[,] performanceScores =
                new object[50, 10];

            object?[,] examinationScores =
                new object[50, 3];

            for (int learnerIndex = 0;
                 learnerIndex < learners.Count;
                 learnerIndex++)
            {
                if (!scoreLookup.TryGetValue(
                        learners[learnerIndex]
                            .LearnerId,
                        out ECRLearnerExportRow?
                            learnerScores))
                {
                    continue;
                }

                for (int columnIndex = 0;
                     columnIndex <
                         writtenColumns.Count;
                     columnIndex++)
                {
                    writtenScores[
                        learnerIndex,
                        columnIndex] =
                            GetLearnerScore(
                                learnerScores,
                                writtenColumns[
                                    columnIndex]);
                }

                for (int columnIndex = 0;
                     columnIndex <
                         performanceColumns.Count;
                     columnIndex++)
                {
                    performanceScores[
                        learnerIndex,
                        columnIndex] =
                            GetLearnerScore(
                                learnerScores,
                                performanceColumns[
                                    columnIndex]);
                }

                examinationScores[
                    learnerIndex,
                    0] =
                        GetLearnerScore(
                            learnerScores,
                            summativeTestOne);

                examinationScores[
                    learnerIndex,
                    1] =
                        GetLearnerScore(
                            learnerScores,
                            summativeTestTwo);

                examinationScores[
                    learnerIndex,
                    2] =
                        GetLearnerScore(
                            learnerScores,
                            termExamination);
            }

            SetRangeValues(
                worksheet,
                $"D{startingRow}:M{startingRow + 49}",
                writtenScores);

            SetRangeValues(
                worksheet,
                $"Q{startingRow}:Z{startingRow + 49}",
                performanceScores);

            SetRangeValues(
                worksheet,
                $"AD{startingRow}:AF{startingRow + 49}",
                examinationScores);
        }

        private static object? GetLearnerScore(
            ECRLearnerExportRow learner,
            ECRAssessmentExportColumn? column)
        {
            if (column == null ||
                !learner.Scores.TryGetValue(
                    column.AssessmentItemId,
                    out decimal? score) ||
                !score.HasValue)
            {
                return null;
            }

            return Convert.ToDouble(
                score.Value);
        }

        private static void SetRangeValues(
            dynamic worksheet,
            string rangeAddress,
            object?[,] values)
        {
            dynamic? range =
                null;

            try
            {
                range =
                    worksheet.Range[
                        rangeAddress];

                range.Value2 =
                    values;
            }
            finally
            {
                if (range != null)
                {
                    ReleaseComObject(
                        range);
                }
            }
        }
    }
}