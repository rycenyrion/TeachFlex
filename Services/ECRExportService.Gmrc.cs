using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class ECRExportService
    {
        private static void WriteDomainTermSheets(
            object worksheets,
            ECRExportRequest request)
        {
            if (request.AssessmentType !=
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
                    dynamic worksheetCollection =
                        worksheets;

                    int worksheetIndex =
                        termData.TermNumber + 1;

                    worksheet =
                        worksheetCollection.Item[
                            worksheetIndex];

                    WriteDomainTermData(
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

        private static void WriteDomainTermData(
            object worksheet,
            ECRTermExportData termData)
        {
            int[] writtenCognitiveColumns =
            {
                6,
                7,
                8,
                9,
                10
            };

            int[] writtenAffectiveColumns =
            {
                14,
                15,
                16,
                17,
                18
            };

            int[] performanceCognitiveColumns =
            {
                22,
                23,
                24
            };

            int[] performanceAffectiveColumns =
            {
                28,
                29,
                30
            };

            int[] performanceBehavioralColumns =
            {
                34,
                35,
                36
            };

            int[] examinationColumns =
            {
                40,
                41,
                42
            };

            List<ECRAssessmentExportColumn>
                writtenCognitiveItems =
                    GetDomainItems(
                        termData,
                        "Written Work",
                        "Cognitive");

            List<ECRAssessmentExportColumn>
                writtenAffectiveItems =
                    GetDomainItems(
                        termData,
                        "Written Work",
                        "Affective");

            List<ECRAssessmentExportColumn>
                performanceCognitiveItems =
                    GetDomainItems(
                        termData,
                        "Performance Task",
                        "Cognitive");

            List<ECRAssessmentExportColumn>
                performanceAffectiveItems =
                    GetDomainItems(
                        termData,
                        "Performance Task",
                        "Affective");

            List<ECRAssessmentExportColumn>
                performanceBehavioralItems =
                    GetDomainItems(
                        termData,
                        "Performance Task",
                        "Behavioral");

            List<List<ECRAssessmentExportColumn>>
                writtenCognitiveGroups =
                    CreateSlotGroups(
                        writtenCognitiveItems,
                        writtenCognitiveColumns.Length);

            List<List<ECRAssessmentExportColumn>>
                writtenAffectiveGroups =
                    CreateSlotGroups(
                        writtenAffectiveItems,
                        writtenAffectiveColumns.Length);

            List<List<ECRAssessmentExportColumn>>
                performanceCognitiveGroups =
                    CreateSlotGroups(
                        performanceCognitiveItems,
                        performanceCognitiveColumns.Length);

            List<List<ECRAssessmentExportColumn>>
                performanceAffectiveGroups =
                    CreateSlotGroups(
                        performanceAffectiveItems,
                        performanceAffectiveColumns.Length);

            List<List<ECRAssessmentExportColumn>>
                performanceBehavioralGroups =
                    CreateSlotGroups(
                        performanceBehavioralItems,
                        performanceBehavioralColumns.Length);

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

            ClearDomainTermInputs(
                worksheet,
                writtenCognitiveColumns,
                writtenAffectiveColumns,
                performanceCognitiveColumns,
                performanceAffectiveColumns,
                performanceBehavioralColumns,
                examinationColumns);

            WriteDomainHighestPossibleScores(
                worksheet,
                writtenCognitiveColumns,
                writtenCognitiveGroups);

            WriteDomainHighestPossibleScores(
                worksheet,
                writtenAffectiveColumns,
                writtenAffectiveGroups);

            WriteDomainHighestPossibleScores(
                worksheet,
                performanceCognitiveColumns,
                performanceCognitiveGroups);

            WriteDomainHighestPossibleScores(
                worksheet,
                performanceAffectiveColumns,
                performanceAffectiveGroups);

            WriteDomainHighestPossibleScores(
                worksheet,
                performanceBehavioralColumns,
                performanceBehavioralGroups);

            WriteDomainExamHps(
                worksheet,
                examinationColumns[0],
                summativeTestOne);

            WriteDomainExamHps(
                worksheet,
                examinationColumns[1],
                summativeTestTwo);

            WriteDomainExamHps(
                worksheet,
                examinationColumns[2],
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

            WriteDomainLearnerScores(
                worksheet,
                maleLearners,
                19,
                writtenCognitiveColumns,
                writtenCognitiveGroups,
                writtenAffectiveColumns,
                writtenAffectiveGroups,
                performanceCognitiveColumns,
                performanceCognitiveGroups,
                performanceAffectiveColumns,
                performanceAffectiveGroups,
                performanceBehavioralColumns,
                performanceBehavioralGroups,
                summativeTestOne,
                summativeTestTwo,
                termExamination);

            WriteDomainLearnerScores(
                worksheet,
                femaleLearners,
                70,
                writtenCognitiveColumns,
                writtenCognitiveGroups,
                writtenAffectiveColumns,
                writtenAffectiveGroups,
                performanceCognitiveColumns,
                performanceCognitiveGroups,
                performanceAffectiveColumns,
                performanceAffectiveGroups,
                performanceBehavioralColumns,
                performanceBehavioralGroups,
                summativeTestOne,
                summativeTestTwo,
                termExamination);
        }

        private static List<ECRAssessmentExportColumn>
            GetDomainItems(
                ECRTermExportData termData,
                string category,
                string domain)
        {
            return termData.AssessmentColumns
                .Where(
                    item =>
                        IsCategory(
                            item,
                            category) &&
                        item.AssessmentDomain.Equals(
                            domain,
                            StringComparison
                                .OrdinalIgnoreCase) &&
                        item.HighestPossibleScore >
                            0)
                .OrderBy(
                    item =>
                        item.DisplayOrder)
                .ToList();
        }

        private static void ClearDomainTermInputs(
            object worksheet,
            params int[][] columnGroups)
        {
            foreach (int[] columns
                     in columnGroups)
            {
                foreach (int column
                         in columns)
                {
                    SetECRCellValue(
                        worksheet,
                        16,
                        column,
                        null);

                    string columnName =
                        GetExcelColumnName(
                            column);

                    ClearECRRange(
                        worksheet,
                        $"{columnName}19:" +
                        $"{columnName}68");

                    ClearECRRange(
                        worksheet,
                        $"{columnName}70:" +
                        $"{columnName}119");
                }
            }
        }

        private static void
            WriteDomainHighestPossibleScores(
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
                    16,
                    columns[index],
                    highestPossibleScore > 0
                        ? highestPossibleScore
                        : null);
            }
        }

        private static void WriteDomainExamHps(
            object worksheet,
            int column,
            ECRAssessmentExportColumn?
                assessmentItem)
        {
            SetECRCellValue(
                worksheet,
                16,
                column,
                assessmentItem != null &&
                assessmentItem.HighestPossibleScore > 0
                    ? assessmentItem
                        .HighestPossibleScore
                    : null);
        }

        private static void WriteDomainLearnerScores(
            object worksheet,
            IReadOnlyList<ECRLearnerExportRow>
                learners,
            int firstRow,
            IReadOnlyList<int>
                writtenCognitiveColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    writtenCognitiveGroups,
            IReadOnlyList<int>
                writtenAffectiveColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    writtenAffectiveGroups,
            IReadOnlyList<int>
                performanceCognitiveColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    performanceCognitiveGroups,
            IReadOnlyList<int>
                performanceAffectiveColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    performanceAffectiveGroups,
            IReadOnlyList<int>
                performanceBehavioralColumns,
            IReadOnlyList<
                List<ECRAssessmentExportColumn>>
                    performanceBehavioralGroups,
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
                    writtenCognitiveColumns,
                    writtenCognitiveGroups);

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    writtenAffectiveColumns,
                    writtenAffectiveGroups);

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    performanceCognitiveColumns,
                    performanceCognitiveGroups);

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    performanceAffectiveColumns,
                    performanceAffectiveGroups);

                WriteGroupedScores(
                    worksheet,
                    row,
                    learner,
                    performanceBehavioralColumns,
                    performanceBehavioralGroups);

                WriteIndividualScore(
                    worksheet,
                    row,
                    40,
                    learner,
                    summativeTestOne);

                WriteIndividualScore(
                    worksheet,
                    row,
                    41,
                    learner,
                    summativeTestTwo);

                WriteIndividualScore(
                    worksheet,
                    row,
                    42,
                    learner,
                    termExamination);
            }
        }
    }
}