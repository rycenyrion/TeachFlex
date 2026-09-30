using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class KindergartenECRExportService
    {
        static partial void WriteTeacherRemarks(
            object workbook,
            KindergartenEcrExportRequest request)
        {
            object? remarksWorksheet =
                null;

            object? sf9Worksheet =
                null;

            try
            {
                remarksWorksheet =
                    GetOrCreateRemarksWorksheet(
                        workbook);

                sf9Worksheet =
                    GetWorksheet(
                        workbook,
                        "SF9 - KINDER");

                List<KindergartenEcrLearnerExportRow>
                    orderedLearners =
                        request.Learners
                            .OrderBy(
                                learner =>
                                    IsMaleKinderLearner(
                                        learner.Sex)
                                        ? 0
                                        : 1)
                            .ThenBy(
                                learner =>
                                    learner.LearnerName)
                            .Take(
                                100)
                            .ToList();

                Dictionary<
                    (int LearnerId, int TermNumber),
                    KindergartenEcrRemarkExportRow>
                        remarkLookup =
                            request.Remarks
                                .GroupBy(
                                    remark =>
                                        (
                                            remark.LearnerId,
                                            remark.TermNumber
                                        ))
                                .ToDictionary(
                                    group =>
                                        group.Key,
                                    group =>
                                        group.Last());

                object?[,] values =
                    new object[
                        orderedLearners.Count + 1,
                        4];

                values[0, 0] =
                    "Learner Name";

                values[0, 1] =
                    "Term 1";

                values[0, 2] =
                    "Term 2";

                values[0, 3] =
                    "Term 3";

                for (int learnerIndex = 0;
                     learnerIndex <
                        orderedLearners.Count;
                     learnerIndex++)
                {
                    KindergartenEcrLearnerExportRow
                        learner =
                            orderedLearners[
                                learnerIndex];

                    values[
                        learnerIndex + 1,
                        0] =
                            learner.LearnerName;

                    for (int termNumber = 1;
                         termNumber <= 3;
                         termNumber++)
                    {
                        remarkLookup.TryGetValue(
                            (
                                learner.LearnerId,
                                termNumber
                            ),
                            out
                            KindergartenEcrRemarkExportRow?
                                remark);

                        values[
                            learnerIndex + 1,
                            termNumber] =
                                CreateCombinedRemark(
                                    remark);
                    }
                }

                string lastCell =
                    $"D{orderedLearners.Count + 1}";

                SetRangeValues(
                    remarksWorksheet,
                    $"A1:{lastCell}",
                    values);

                dynamic hiddenRemarksSheet =
                    remarksWorksheet;

                hiddenRemarksSheet.Visible =
                    2;

                SetCellFormula(
                    sf9Worksheet,
                    "D81",
                    CreateRemarkLookupFormula(
                        2));

                SetCellFormula(
                    sf9Worksheet,
                    "D99",
                    CreateRemarkLookupFormula(
                        3));

                SetCellFormula(
                    sf9Worksheet,
                    "D117",
                    CreateRemarkLookupFormula(
                        4));
            }
            finally
            {
                ReleaseComObject(
                    sf9Worksheet);

                ReleaseComObject(
                    remarksWorksheet);
            }
        }

        private static object
            GetOrCreateRemarksWorksheet(
                object workbook)
        {
            object? worksheets =
                null;

            object? worksheet =
                null;

            try
            {
                dynamic openedWorkbook =
                    workbook;

                worksheets =
                    openedWorkbook.Worksheets;

                dynamic worksheetCollection =
                    worksheets;

                try
                {
                    worksheet =
                        worksheetCollection.Item[
                            "_KINDER_REMARKS"];
                }
                catch
                {
                    worksheet =
                        worksheetCollection.Add();

                    dynamic newWorksheet =
                        worksheet;

                    newWorksheet.Name =
                        "_KINDER_REMARKS";
                }

                if (worksheet == null)
                {
                    throw new InvalidOperationException(
                        "The hidden Kinder remarks worksheet " +
                        "could not be prepared.");
                }

                return worksheet;
            }
            finally
            {
                ReleaseComObject(
                    worksheets);
            }
        }

        private static string
            CreateCombinedRemark(
                KindergartenEcrRemarkExportRow?
                    remark)
        {
            if (remark == null)
            {
                return string.Empty;
            }

            List<string> sections =
                new List<string>();

            if (!string.IsNullOrWhiteSpace(
                    remark.TeacherComment))
            {
                sections.Add(
                    remark.TeacherComment.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                    remark.LearnerStrengths))
            {
                sections.Add(
                    "Strengths:\n" +
                    remark.LearnerStrengths.Trim());
            }

            if (!string.IsNullOrWhiteSpace(
                    remark.SuggestedInterventions))
            {
                sections.Add(
                    "Suggested Interventions:\n" +
                    remark
                        .SuggestedInterventions
                        .Trim());
            }

            return string.Join(
                "\n\n",
                sections);
        }

        private static string
            CreateRemarkLookupFormula(
                int resultColumn)
        {
            return
                "=IFERROR(VLOOKUP(" +
                "$E$18," +
                "'_KINDER_REMARKS'!" +
                "$A$2:$D$101," +
                resultColumn +
                ",FALSE()),\"\")";
        }

        private static void SetCellFormula(
            object worksheet,
            string cellAddress,
            string formula)
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

                selectedRange.Formula =
                    formula;

                selectedRange.WrapText =
                    true;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }
    }
}