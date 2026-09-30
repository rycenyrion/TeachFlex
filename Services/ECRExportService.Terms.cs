using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class ECRExportService
    {
        private static void WriteRegularTermSheets(
            object worksheets,
            ECRExportRequest request)
        {
            if (request.AssessmentType ==
                SubjectAssessmentType
                    .GmrcValuesEducation)
            {
                return;
            }

            foreach (ECRTermExportData termData
                     in request.Terms)
            {
                object? worksheet =
                    null;

                try
                {
                    worksheet =
                        GetTermWorksheet(
                            worksheets,
                            request,
                            termData);

                    if (worksheet == null)
                    {
                        continue;
                    }

                    WriteRegularTermData(
                        worksheet,
                        termData);
                }
                finally
                {
                    ReleaseECRComObject(
                        worksheet);
                }
            }
        }

        private static object? GetTermWorksheet(
            object worksheets,
            ECRExportRequest request,
            ECRTermExportData termData)
        {
            dynamic worksheetCollection =
                worksheets;

            bool usesTwoComponentSheets =
                request.UsesComponentRecords &&
                (
                    request.AssessmentType ==
                        SubjectAssessmentType
                            .MapehComponents ||
                    request.AssessmentType ==
                        SubjectAssessmentType
                            .EppTleComponents
                );

            if (!usesTwoComponentSheets)
            {
                int worksheetIndex =
                    termData.TermNumber + 1;

                return worksheetCollection.Item[
                    worksheetIndex];
            }

            int componentIndex =
                IsSecondComponent(
                    request,
                    termData.ComponentName)
                    ? 1
                    : 0;

            int componentWorksheetIndex =
                2 +
                (
                    termData.TermNumber - 1
                ) * 2 +
                componentIndex;

            object worksheet =
                worksheetCollection.Item[
                    componentWorksheetIndex];

            RenameComponentWorksheet(
                worksheet,
                termData.TermNumber,
                termData.ComponentName);

            return worksheet;
        }

        private static bool IsSecondComponent(
            ECRExportRequest request,
            string componentName)
        {
            return componentName.Equals(
                request.ComponentTwoName,
                StringComparison.OrdinalIgnoreCase);
        }

        private static void RenameComponentWorksheet(
            object worksheet,
            int termNumber,
            string componentName)
        {
            string safeComponentName =
                MakeSafeWorksheetName(
                    componentName);

            string worksheetName =
                $"TERM {termNumber} " +
                $"{safeComponentName}";

            if (worksheetName.Length > 31)
            {
                worksheetName =
                    worksheetName.Substring(
                        0,
                        31);
            }

            dynamic selectedWorksheet =
                worksheet;

            selectedWorksheet.Name =
                worksheetName;
        }

        private static string MakeSafeWorksheetName(
            string value)
        {
            string safeValue =
                string.IsNullOrWhiteSpace(
                    value)
                    ? "Component"
                    : value.Trim();

            char[] invalidCharacters =
            {
                ':',
                '\\',
                '/',
                '?',
                '*',
                '[',
                ']'
            };

            foreach (char invalidCharacter
                     in invalidCharacters)
            {
                safeValue =
                    safeValue.Replace(
                        invalidCharacter,
                        '-');
            }

            return safeValue;
        }

        private static void WriteRegularTermData(
            object worksheet,
            ECRTermExportData termData)
        {
            int[] writtenWorkColumns =
            {
                6,
                7,
                8,
                9,
                10
            };

            int[] performanceTaskColumns =
            {
                14,
                15,
                16
            };

            ClearTermInputCells(
                worksheet,
                writtenWorkColumns,
                performanceTaskColumns);

            List<ECRAssessmentExportColumn>
                writtenWorkItems =
                    termData.AssessmentColumns
                        .Where(
                            item =>
                                IsCategory(
                                    item,
                                    "Written Work") &&
                                item.HighestPossibleScore >
                                    0)
                        .OrderBy(
                            item =>
                                item.DisplayOrder)
                        .ToList();

            List<ECRAssessmentExportColumn>
                performanceTaskItems =
                    termData.AssessmentColumns
                        .Where(
                            item =>
                                IsCategory(
                                    item,
                                    "Performance Task") &&
                                item.HighestPossibleScore >
                                    0)
                        .OrderBy(
                            item =>
                                item.DisplayOrder)
                        .ToList();

            List<List<ECRAssessmentExportColumn>>
                writtenWorkGroups =
                    CreateSlotGroups(
                        writtenWorkItems,
                        writtenWorkColumns.Length);

            List<List<ECRAssessmentExportColumn>>
                performanceTaskGroups =
                    CreateSlotGroups(
                        performanceTaskItems,
                        performanceTaskColumns.Length);

            WriteHighestPossibleScores(
                worksheet,
                writtenWorkColumns,
                writtenWorkGroups);

            WriteHighestPossibleScores(
                worksheet,
                performanceTaskColumns,
                performanceTaskGroups);

            ECRAssessmentExportColumn?
                summativeTestOne =
                    FindAssessment(
                        termData,
                        "Summative Test 1");

            ECRAssessmentExportColumn?
                summativeTestTwo =
                    FindAssessment(
                        termData,
                        "Summative Test 2");

            ECRAssessmentExportColumn?
                termExamination =
                    FindAssessment(
                        termData,
                        "Term Examination");

            WriteExamHighestPossibleScore(
                worksheet,
                20,
                summativeTestOne);

            WriteExamHighestPossibleScore(
                worksheet,
                21,
                summativeTestTwo);

            WriteExamHighestPossibleScore(
                worksheet,
                22,
                termExamination);

            List<ECRLearnerExportRow>
                maleLearners =
                    termData.Learners
                        .Where(
                            learner =>
                                IsMale(
                                    learner.Sex))
                        .OrderBy(
                            learner =>
                                learner.LearnerName,
                            StringComparer
                                .OrdinalIgnoreCase)
                        .ToList();

            List<ECRLearnerExportRow>
                femaleLearners =
                    termData.Learners
                        .Where(
                            learner =>
                                !IsMale(
                                    learner.Sex))
                        .OrderBy(
                            learner =>
                                learner.LearnerName,
                            StringComparer
                                .OrdinalIgnoreCase)
                        .ToList();

            WriteLearnerTermScores(
                worksheet,
                maleLearners,
                18,
                writtenWorkColumns,
                writtenWorkGroups,
                performanceTaskColumns,
                performanceTaskGroups,
                summativeTestOne,
                summativeTestTwo,
                termExamination);

            WriteLearnerTermScores(
                worksheet,
                femaleLearners,
                69,
                writtenWorkColumns,
                writtenWorkGroups,
                performanceTaskColumns,
                performanceTaskGroups,
                summativeTestOne,
                summativeTestTwo,
                termExamination);
        }

        private static void ClearTermInputCells(
            object worksheet,
            IReadOnlyList<int>
                writtenWorkColumns,
            IReadOnlyList<int>
                performanceTaskColumns)
        {
            foreach (int column
                     in writtenWorkColumns)
            {
                SetECRCellValue(
                    worksheet,
                    15,
                    column,
                    null);

                ClearLearnerScoreColumn(
                    worksheet,
                    column);
            }

            foreach (int column
                     in performanceTaskColumns)
            {
                SetECRCellValue(
                    worksheet,
                    15,
                    column,
                    null);

                ClearLearnerScoreColumn(
                    worksheet,
                    column);
            }

            for (int column = 20;
                 column <= 22;
                 column++)
            {
                SetECRCellValue(
                    worksheet,
                    15,
                    column,
                    null);

                ClearLearnerScoreColumn(
                    worksheet,
                    column);
            }
        }

        private static void ClearLearnerScoreColumn(
            object worksheet,
            int column)
        {
            ClearECRRange(
                worksheet,
                $"{GetExcelColumnName(column)}18:" +
                $"{GetExcelColumnName(column)}67");

            ClearECRRange(
                worksheet,
                $"{GetExcelColumnName(column)}69:" +
                $"{GetExcelColumnName(column)}118");
        }

        private static List<
            List<ECRAssessmentExportColumn>>
                CreateSlotGroups(
                    IReadOnlyList<
                        ECRAssessmentExportColumn>
                            assessmentItems,
                    int numberOfSlots)
        {
            List<
                List<ECRAssessmentExportColumn>>
                    groups =
                        new List<
                            List<
                                ECRAssessmentExportColumn>>();

            for (int index = 0;
                 index < numberOfSlots;
                 index++)
            {
                groups.Add(
                    new List<
                        ECRAssessmentExportColumn>());
            }

            for (int index = 0;
                 index < assessmentItems.Count;
                 index++)
            {
                int groupIndex =
                    index %
                    numberOfSlots;

                groups[groupIndex].Add(
                    assessmentItems[index]);
            }

            return groups;
        }

        private static void WriteHighestPossibleScores(
            object worksheet,
            IReadOnlyList<int> columns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    groups)
        {
            for (int index = 0;
                 index < columns.Count;
                 index++)
            {
                decimal highestPossibleScore =
                    groups[index].Sum(
                        assessment =>
                            assessment
                                .HighestPossibleScore);

                SetECRCellValue(
                    worksheet,
                    15,
                    columns[index],
                    highestPossibleScore > 0
                        ? highestPossibleScore
                        : null);
            }
        }

        private static void WriteExamHighestPossibleScore(
            object worksheet,
            int column,
            ECRAssessmentExportColumn?
                assessmentItem)
        {
            SetECRCellValue(
                worksheet,
                15,
                column,
                assessmentItem != null &&
                assessmentItem.HighestPossibleScore > 0
                    ? assessmentItem
                        .HighestPossibleScore
                    : null);
        }

        private static void WriteLearnerTermScores(
            object worksheet,
            IReadOnlyList<ECRLearnerExportRow>
                learners,
            int firstRow,
            IReadOnlyList<int>
                writtenWorkColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    writtenWorkGroups,
            IReadOnlyList<int>
                performanceTaskColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    performanceTaskGroups,
            ECRAssessmentExportColumn?
                summativeTestOne,
            ECRAssessmentExportColumn?
                summativeTestTwo,
            ECRAssessmentExportColumn?
                termExamination)
        {
            for (int learnerIndex = 0;
                 learnerIndex < learners.Count;
                 learnerIndex++)
            {
                ECRLearnerExportRow learner =
                    learners[learnerIndex];

                int row =
                    firstRow +
                    learnerIndex;

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    writtenWorkColumns,
                    writtenWorkGroups);

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    performanceTaskColumns,
                    performanceTaskGroups);

                WriteIndividualScore(
                    worksheet,
                    row,
                    20,
                    learner,
                    summativeTestOne);

                WriteIndividualScore(
                    worksheet,
                    row,
                    21,
                    learner,
                    summativeTestTwo);

                WriteIndividualScore(
                    worksheet,
                    row,
                    22,
                    learner,
                    termExamination);
            }
        }

        private static void WriteGroupedScores(
            object worksheet,
            int row,
            ECRLearnerExportRow learner,
            IReadOnlyList<int> columns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    groups)
        {
            for (int index = 0;
                 index < columns.Count;
                 index++)
            {
                List<ECRAssessmentExportColumn>
                    group =
                        groups[index];

                List<decimal>
                    encodedScores =
                        group
                            .Where(
                                assessment =>
                                    learner.Scores
                                        .TryGetValue(
                                            assessment
                                                .AssessmentItemId,
                                            out decimal?
                                                score) &&
                                    score.HasValue)
                            .Select(
                                assessment =>
                                    learner.Scores[
                                        assessment
                                            .AssessmentItemId]!
                                        .Value)
                            .ToList();

                object? value =
                    encodedScores.Count > 0
                        ? encodedScores.Sum()
                        : null;

                SetECRCellValue(
                    worksheet,
                    row,
                    columns[index],
                    value);
            }
        }

        private static void WriteIndividualScore(
            object worksheet,
            int row,
            int column,
            ECRLearnerExportRow learner,
            ECRAssessmentExportColumn?
                assessmentItem)
        {
            if (assessmentItem == null ||
                !learner.Scores.TryGetValue(
                    assessmentItem.AssessmentItemId,
                    out decimal? score) ||
                !score.HasValue)
            {
                SetECRCellValue(
                    worksheet,
                    row,
                    column,
                    null);

                return;
            }

            SetECRCellValue(
                worksheet,
                row,
                column,
                score.Value);
        }

        private static ECRAssessmentExportColumn?
            FindAssessment(
                ECRTermExportData termData,
                string category)
        {
            return termData.AssessmentColumns
                .Where(
                    item =>
                        IsCategory(
                            item,
                            category) &&
                        item.HighestPossibleScore >
                            0)
                .OrderBy(
                    item =>
                        item.DisplayOrder)
                .FirstOrDefault();
        }

        private static bool IsCategory(
            ECRAssessmentExportColumn item,
            string category)
        {
            return item.Category.Equals(
                category,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetExcelColumnName(
            int columnNumber)
        {
            string columnName =
                string.Empty;

            int number =
                columnNumber;

            while (number > 0)
            {
                number--;

                columnName =
                    (char)(
                        'A' +
                        number % 26) +
                    columnName;

                number /=
                    26;
            }

            return columnName;
        }
    }
}