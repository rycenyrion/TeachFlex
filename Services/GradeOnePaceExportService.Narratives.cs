using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class GradeOnePaceExportService
    {
        static partial void WriteNarrativeSummaries(
            object workbook,
            GradeOnePaceExportRequest request)
        {
            ArgumentNullException.ThrowIfNull(workbook);
            ArgumentNullException.ThrowIfNull(request);

            List<GradeOnePaceLearnerExportRow> maleLearners =
                request.Learners
                    .Where(learner =>
                        IsMaleForNarrativeExport(
                            learner.Sex))
                    .OrderBy(learner =>
                        learner.LearnerName)
                    .ToList();

            List<GradeOnePaceLearnerExportRow> femaleLearners =
                request.Learners
                    .Where(learner =>
                        !IsMaleForNarrativeExport(
                            learner.Sex))
                    .OrderBy(learner =>
                        learner.LearnerName)
                    .ToList();

            Dictionary<(int LearnerId, int TermNumber),
                GradeOnePaceSummaryExportRow> summaries =
                request.Summaries
                    .GroupBy(summary =>
                        (
                            summary.LearnerId,
                            summary.TermNumber
                        ))
                    .ToDictionary(
                        group => group.Key,
                        group => group.Last());

            WriteTermNarrativeSummary(
                workbook,
                "TERM 1 SUMMARY",
                1,
                maleLearners,
                femaleLearners,
                summaries);

            WriteTermNarrativeSummary(
                workbook,
                "TERM 2 SUMMARY",
                2,
                maleLearners,
                femaleLearners,
                summaries);

            WriteTermNarrativeSummary(
                workbook,
                "TERM 3 SUMMARY",
                3,
                maleLearners,
                femaleLearners,
                summaries);
        }

        private static void WriteTermNarrativeSummary(
            object workbook,
            string sheetName,
            int termNumber,
            IReadOnlyList<GradeOnePaceLearnerExportRow>
                maleLearners,
            IReadOnlyList<GradeOnePaceLearnerExportRow>
                femaleLearners,
            IReadOnlyDictionary<
                (int LearnerId, int TermNumber),
                GradeOnePaceSummaryExportRow> summaries)
        {
            object? worksheet = null;

            try
            {
                worksheet = GetWorksheet(
                    workbook,
                    sheetName);

                ClearNarrativeArea(
                    worksheet);

                WriteLearnerNarratives(
                    worksheet,
                    maleLearners,
                    14,
                    termNumber,
                    summaries);

                WriteLearnerNarratives(
                    worksheet,
                    femaleLearners,
                    65,
                    termNumber,
                    summaries);
            }
            finally
            {
                ReleaseComObject(
                    worksheet);
            }
        }

        private static void WriteLearnerNarratives(
            object worksheet,
            IReadOnlyList<GradeOnePaceLearnerExportRow>
                learners,
            int startRow,
            int termNumber,
            IReadOnlyDictionary<
                (int LearnerId, int TermNumber),
                GradeOnePaceSummaryExportRow> summaries)
        {
            for (int index = 0;
                 index < learners.Count && index < 50;
                 index++)
            {
                GradeOnePaceLearnerExportRow learner =
                    learners[index];

                int row = startRow + index;

                if (!summaries.TryGetValue(
                        (
                            learner.LearnerId,
                            termNumber
                        ),
                        out GradeOnePaceSummaryExportRow?
                            summary))
                {
                    continue;
                }

                SetNarrativeCellValue(
                    worksheet,
                    row,
                    14,
                    summary.WhatLearnerCanDo);

                SetNarrativeCellValue(
                    worksheet,
                    row,
                    17,
                    summary.WhatLearnerNeedsToImprove);

                SetTeacherRemarksNote(
                    worksheet,
                    row,
                    17,
                    summary.TeacherRemarks);
            }
        }

        private static void ClearNarrativeArea(
            object worksheet)
        {
            for (int row = 14;
                 row <= 63;
                 row++)
            {
                ClearNarrativeRow(
                    worksheet,
                    row);
            }

            for (int row = 65;
                 row <= 114;
                 row++)
            {
                ClearNarrativeRow(
                    worksheet,
                    row);
            }
        }

        private static void ClearNarrativeRow(
            object worksheet,
            int row)
        {
            SetNarrativeCellValue(
                worksheet,
                row,
                14,
                string.Empty);

            SetNarrativeCellValue(
                worksheet,
                row,
                17,
                string.Empty);

            SetTeacherRemarksNote(
                worksheet,
                row,
                17,
                string.Empty);
        }

        private static bool IsMaleForNarrativeExport(
            string sex)
        {
            string normalized =
                (sex ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant();

            return normalized == "male" ||
                   normalized == "m" ||
                   normalized == "lalaki";
        }

        private static void SetNarrativeCellValue(
            object worksheet,
            int row,
            int column,
            object? value)
        {
            object? cells = null;
            object? cell = null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                cells =
                    selectedWorksheet.Cells;

                dynamic cellCollection =
                    cells;

                cell =
                    cellCollection.Item[
                        row,
                        column];

                dynamic selectedCell =
                    cell;

                selectedCell.Value2 =
                    value?.ToString()
                    ?? string.Empty;
            }
            finally
            {
                ReleaseComObject(
                    cell);

                ReleaseComObject(
                    cells);
            }
        }

        private static void SetTeacherRemarksNote(
            object worksheet,
            int row,
            int column,
            string teacherRemarks)
        {
            object? cells = null;
            object? cell = null;
            object? existingComment = null;
            object? newComment = null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                cells =
                    selectedWorksheet.Cells;

                dynamic cellCollection =
                    cells;

                cell =
                    cellCollection.Item[
                        row,
                        column];

                dynamic selectedCell =
                    cell;

                try
                {
                    existingComment =
                        selectedCell.Comment;

                    if (existingComment != null)
                    {
                        dynamic comment =
                            existingComment;

                        comment.Delete();
                    }
                }
                catch
                {
                    // The cell has no existing note.
                }

                if (!string.IsNullOrWhiteSpace(
                        teacherRemarks))
                {
                    newComment =
                        selectedCell.AddComment(
                            "Teacher Remarks:\n" +
                            teacherRemarks.Trim());

                    if (newComment != null)
                    {
                        dynamic comment =
                            newComment;

                        comment.Visible =
                            false;
                    }
                }
            }
            finally
            {
                ReleaseComObject(
                    newComment);

                ReleaseComObject(
                    existingComment);

                ReleaseComObject(
                    cell);

                ReleaseComObject(
                    cells);
            }
        }
    }
}
