using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class ECRExportService
    {
        private static void PopulateOfficialWorkbook(
            ECRExportRequest request,
            string outputPath)
        {
            Type? excelType =
                Type.GetTypeFromProgID(
                    "Excel.Application");

            if (excelType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Excel is not installed. " +
                    "TeachFlex needs Microsoft Excel to " +
                    "prepare the official E-Class Record.");
            }

            object? excelApplication =
                null;

            object? workbooks =
                null;

            object? workbook =
                null;

            object? worksheets =
                null;

            object? inputWorksheet =
                null;

            try
            {
                excelApplication =
                    Activator.CreateInstance(
                        excelType);

                if (excelApplication == null)
                {
                    throw new InvalidOperationException(
                        "Microsoft Excel could not be started.");
                }

                dynamic excel =
                    excelApplication;

                excel.Visible =
                    false;

                excel.DisplayAlerts =
                    false;

                excel.ScreenUpdating =
                    false;

                workbooks =
                    excel.Workbooks;

                dynamic workbookCollection =
                    workbooks;

                workbook =
                    workbookCollection.Open(
                        outputPath);

                dynamic openedWorkbook =
                    workbook;

                worksheets =
                    openedWorkbook.Worksheets;

                dynamic worksheetCollection =
                    worksheets;

                inputWorksheet =
                    worksheetCollection.Item[
                        "INPUT DATA"];

                if (inputWorksheet == null)
                {
                    throw new InvalidOperationException(
                        "The INPUT DATA worksheet could " +
                        "not be opened.");
                }

                WriteInputData(
                    inputWorksheet,
                    request);

                WriteRegularTermSheets(
                    worksheets,
                    request);

                WriteDomainTermSheets(
                    worksheets,
                    request);

                openedWorkbook.Save();

                openedWorkbook.Close(
                    true);

                ReleaseECRComObject(
                    inputWorksheet);

                inputWorksheet =
                    null;

                ReleaseECRComObject(
                    worksheets);

                worksheets =
                    null;

                ReleaseECRComObject(
                    workbook);

                workbook =
                    null;

                dynamic runningExcel =
                    excelApplication;

                runningExcel.Quit();

                ReleaseECRComObject(
                    workbooks);

                workbooks =
                    null;

                ReleaseECRComObject(
                    excelApplication);

                excelApplication =
                    null;
            }
            catch
            {
                try
                {
                    if (workbook != null)
                    {
                        dynamic openedWorkbook =
                            workbook;

                        openedWorkbook.Close(
                            false);
                    }
                }
                catch
                {
                    // Cleanup only.
                }

                try
                {
                    if (excelApplication != null)
                    {
                        dynamic runningExcel =
                            excelApplication;

                        runningExcel.Quit();
                    }
                }
                catch
                {
                    // Cleanup only.
                }

                throw;
            }
            finally
            {
                ReleaseECRComObject(
                    inputWorksheet);

                ReleaseECRComObject(
                    worksheets);

                ReleaseECRComObject(
                    workbook);

                ReleaseECRComObject(
                    workbooks);

                ReleaseECRComObject(
                    excelApplication);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void WriteInputData(
            object inputWorksheet,
            ECRExportRequest request)
        {
            SetECRCellValue(
                inputWorksheet,
                10,
                5,
                request.Region);

            SetECRCellValue(
                inputWorksheet,
                11,
                5,
                request.Division);

            SetECRCellValue(
                inputWorksheet,
                13,
                5,
                request.SchoolId);

            SetECRCellValue(
                inputWorksheet,
                14,
                5,
                request.SchoolName);

            SetECRCellValue(
                inputWorksheet,
                15,
                5,
                request.SchoolYear);

            SetECRCellValue(
                inputWorksheet,
                16,
                5,
                request.SchoolHeadName);

            SetECRCellValue(
                inputWorksheet,
                23,
                5,
                request.AdviserName);

            SetECRCellValue(
                inputWorksheet,
                24,
                5,
                request.SubjectName);

            SetECRCellValue(
                inputWorksheet,
                25,
                5,
                request.GradeLevel);

            SetECRCellValue(
                inputWorksheet,
                26,
                5,
                request.SectionName);

            ClearECRRange(
                inputWorksheet,
                "K11:K60");

            ClearECRRange(
                inputWorksheet,
                "N11:N60");

            List<ECRLearnerExportRow>
                learners =
                    request.Terms
                        .SelectMany(
                            term =>
                                term.Learners)
                        .GroupBy(
                            learner =>
                                learner.LearnerId)
                        .Select(
                            group =>
                                group.First())
                        .ToList();

            if (learners.Count == 0)
            {
                learners =
                    request.FinalGrades
                        .Select(
                            learner =>
                                new ECRLearnerExportRow
                                {
                                    LearnerId =
                                        learner.LearnerId,

                                    Lrn =
                                        learner.Lrn,

                                    LearnerName =
                                        learner.LearnerName,

                                    Sex =
                                        learner.Sex
                                })
                        .ToList();
            }

            List<ECRLearnerExportRow>
                maleLearners =
                    learners
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
                    learners
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

            if (maleLearners.Count > 50 ||
                femaleLearners.Count > 50)
            {
                throw new InvalidOperationException(
                    "The official ECR template supports " +
                    "up to 50 male and 50 female learners.");
            }

            for (int index = 0;
                 index < maleLearners.Count;
                 index++)
            {
                SetECRCellValue(
                    inputWorksheet,
                    11 + index,
                    11,
                    maleLearners[index]
                        .LearnerName);
            }

            for (int index = 0;
                 index < femaleLearners.Count;
                 index++)
            {
                SetECRCellValue(
                    inputWorksheet,
                    11 + index,
                    14,
                    femaleLearners[index]
                        .LearnerName);
            }
        }

        private static bool IsMale(
            string sex)
        {
            string normalizedSex =
                sex?.Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            return normalizedSex == "male" ||
                   normalizedSex == "m";
        }

        private static void SetECRCellValue(
            object worksheet,
            int row,
            int column,
            object? value)
        {
            object? cells =
                null;

            object? cell =
                null;

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
                    value;
            }
            finally
            {
                ReleaseECRComObject(
                    cell);

                ReleaseECRComObject(
                    cells);
            }
        }

        private static void ClearECRRange(
            object worksheet,
            string address)
        {
            object? selectedRange =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                selectedRange =
                    selectedWorksheet.Range[
                        address];

                dynamic range =
                    selectedRange;

                range.ClearContents();
            }
            finally
            {
                ReleaseECRComObject(
                    selectedRange);
            }
        }

        private static void ReleaseECRComObject(
            object? value)
        {
            if (value == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(
                        value))
                {
                    Marshal.FinalReleaseComObject(
                        value);
                }
            }
            catch
            {
                // Cleanup only.
            }
        }
    }
}
