using System;
using System.IO;
using System.Runtime.InteropServices;
using TeachFlex.Models;
using DrawingColor = System.Drawing.Color;

namespace TeachFlex.Services
{
    public class GradesExportService :
        IGradesExportService
    {
        public string ExportConsolidatedGrades(
            GradesExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "An output path is required.",
                    nameof(outputPath));
            }

            string fullOutputPath =
                Path.GetFullPath(
                    outputPath);

            string? outputFolder =
                Path.GetDirectoryName(
                    fullOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputFolder))
            {
                Directory.CreateDirectory(
                    outputFolder);
            }

            Type? excelType =
                Type.GetTypeFromProgID(
                    "Excel.Application");

            if (excelType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Excel is not installed. " +
                    "TeachFlex needs Microsoft Excel to " +
                    "create the editable Grades workbook.");
            }

            object? excelApplication =
                null;

            object? workbooks =
                null;

            object? workbook =
                null;

            object? worksheets =
                null;

            object? worksheet =
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
                    workbookCollection.Add();

                dynamic openedWorkbook =
                    workbook;

                worksheets =
                    openedWorkbook.Worksheets;

                dynamic worksheetCollection =
                    worksheets;

                worksheet =
                    worksheetCollection.Item[1];

                dynamic sheet =
                    worksheet;

                sheet.Name =
                    "Consolidated Grades";

                PopulateWorksheet(
                    excel,
                    sheet,
                    request);

                if (File.Exists(
                        fullOutputPath))
                {
                    File.Delete(
                        fullOutputPath);
                }

                openedWorkbook.SaveAs(
                    fullOutputPath,
                    51);

                openedWorkbook.Close(
                    true);

                ReleaseComObject(
                    worksheet);

                worksheet =
                    null;

                ReleaseComObject(
                    worksheets);

                worksheets =
                    null;

                ReleaseComObject(
                    workbook);

                workbook =
                    null;

                excel.Quit();

                ReleaseComObject(
                    workbooks);

                workbooks =
                    null;

                ReleaseComObject(
                    excelApplication);

                excelApplication =
                    null;

                return fullOutputPath;
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
                        dynamic excel =
                            excelApplication;

                        excel.Quit();
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
                ReleaseComObject(
                    worksheet);

                ReleaseComObject(
                    worksheets);

                ReleaseComObject(
                    workbook);

                ReleaseComObject(
                    workbooks);

                ReleaseComObject(
                    excelApplication);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void PopulateWorksheet(
            dynamic excel,
            dynamic sheet,
            GradesExportRequest request)
        {
            const int titleRow =
                1;

            const int informationRow =
                3;

            const int headerRow =
                6;

            int firstSubjectColumn =
                5;

            int generalAverageColumn =
                firstSubjectColumn +
                request.Subjects.Count;

            int remarksColumn =
                generalAverageColumn + 1;

            int lastColumn =
                remarksColumn;

            dynamic titleRange =
                sheet.Range[
                    sheet.Cells[
                        titleRow,
                        1],
                    sheet.Cells[
                        titleRow,
                        lastColumn]];

            titleRange.Merge();

            titleRange.Value2 =
                "TEACHFLEX CLASS CONSOLIDATED GRADES";

            titleRange.Font.Bold =
                true;

            titleRange.Font.Size =
                16;

            titleRange.Font.Color =
                ToOleColor(
                    255,
                    255,
                    255);

            titleRange.Interior.Color =
                ToOleColor(
                    30,
                    64,
                    175);

            titleRange.HorizontalAlignment =
                -4108;

            titleRange.VerticalAlignment =
                -4108;

            titleRange.RowHeight =
                30;

            sheet.Cells[
                informationRow,
                1] =
                    "School:";

            sheet.Cells[
                informationRow,
                2] =
                    request.SchoolName;

            sheet.Cells[
                informationRow,
                4] =
                    "School ID:";

            sheet.Cells[
                informationRow,
                5] =
                    request.SchoolId;

            sheet.Cells[
                informationRow + 1,
                1] =
                    "Class:";

            sheet.Cells[
                informationRow + 1,
                2] =
                    $"{request.GradeLevel} - " +
                    $"{request.SectionName}";

            sheet.Cells[
                informationRow + 1,
                4] =
                    "School Year:";

            sheet.Cells[
                informationRow + 1,
                5] =
                    request.SchoolYear;

            sheet.Cells[
                informationRow + 2,
                1] =
                    "Class Adviser:";

            sheet.Cells[
                informationRow + 2,
                2] =
                    request.AdviserName;

            sheet.Cells[
                informationRow + 2,
                4] =
                    "Date Prepared:";

            sheet.Cells[
                informationRow + 2,
                5] =
                    request.DatePrepared
                        .ToString(
                            "MMMM dd, yyyy");

            sheet.Cells[
                headerRow,
                1] =
                    "No.";

            sheet.Cells[
                headerRow,
                2] =
                    "LRN";

            sheet.Cells[
                headerRow,
                3] =
                    "Learner Name";

            sheet.Cells[
                headerRow,
                4] =
                    "Sex";

            for (int subjectIndex = 0;
                 subjectIndex <
                    request.Subjects.Count;
                 subjectIndex++)
            {
                sheet.Cells[
                    headerRow,
                    firstSubjectColumn +
                        subjectIndex] =
                            request.Subjects[
                                subjectIndex]
                                .SubjectName;
            }

            sheet.Cells[
                headerRow,
                generalAverageColumn] =
                    "General Average";

            sheet.Cells[
                headerRow,
                remarksColumn] =
                    "Remarks";

            dynamic headerRange =
                sheet.Range[
                    sheet.Cells[
                        headerRow,
                        1],
                    sheet.Cells[
                        headerRow,
                        lastColumn]];

            headerRange.Font.Bold =
                true;

            headerRange.Font.Color =
                ToOleColor(
                    255,
                    255,
                    255);

            headerRange.Interior.Color =
                ToOleColor(
                    37,
                    99,
                    235);

            headerRange.HorizontalAlignment =
                -4108;

            headerRange.VerticalAlignment =
                -4108;

            headerRange.WrapText =
                true;

            headerRange.RowHeight =
                34;

            int currentRow =
                headerRow + 1;

            foreach (
                GradesExportLearner learner
                in request.Learners)
            {
                sheet.Cells[
                    currentRow,
                    1] =
                        learner.Number;

                sheet.Cells[
                    currentRow,
                    2] =
                        learner.Lrn;

                sheet.Cells[
                    currentRow,
                    3] =
                        learner.LearnerName;

                sheet.Cells[
                    currentRow,
                    4] =
                        learner.Sex;

                for (int subjectIndex = 0;
                     subjectIndex <
                        request.Subjects.Count;
                     subjectIndex++)
                {
                    GradesExportSubject subject =
                        request.Subjects[
                            subjectIndex];

                    learner.SubjectFinalGrades
                        .TryGetValue(
                            subject.SubjectId,
                            out int? finalGrade);

                    sheet.Cells[
                        currentRow,
                        firstSubjectColumn +
                            subjectIndex] =
                                finalGrade.HasValue
    ? (object)finalGrade.Value
    : "—";
                }

                sheet.Cells[
                    currentRow,
                    generalAverageColumn] =
                        learner.GeneralAverage
    .HasValue
        ? (object)learner
            .GeneralAverage
            .Value
        : "—";

                sheet.Cells[
                    currentRow,
                    remarksColumn] =
                        learner.Remarks;

                currentRow++;
            }

            int lastDataRow =
                Math.Max(
                    headerRow + 1,
                    currentRow - 1);

            dynamic dataRange =
                sheet.Range[
                    sheet.Cells[
                        headerRow,
                        1],
                    sheet.Cells[
                        lastDataRow,
                        lastColumn]];

            dataRange.Borders.LineStyle =
                1;

            dataRange.Borders.Color =
                ToOleColor(
                    203,
                    213,
                    225);

            dataRange.VerticalAlignment =
                -4108;

            dynamic bodyRange =
                sheet.Range[
                    sheet.Cells[
                        headerRow + 1,
                        1],
                    sheet.Cells[
                        lastDataRow,
                        lastColumn]];

            bodyRange.RowHeight =
                22;

            dynamic numberRange =
                sheet.Range[
                    sheet.Cells[
                        headerRow + 1,
                        1],
                    sheet.Cells[
                        lastDataRow,
                        1]];

            numberRange.HorizontalAlignment =
                -4108;

            dynamic gradeRange =
                sheet.Range[
                    sheet.Cells[
                        headerRow + 1,
                        firstSubjectColumn],
                    sheet.Cells[
                        lastDataRow,
                        generalAverageColumn]];

            gradeRange.HorizontalAlignment =
                -4108;

            dynamic allColumns =
                sheet.Range[
                    sheet.Cells[
                        1,
                        1],
                    sheet.Cells[
                        lastDataRow,
                        lastColumn]]
                    .EntireColumn;

            allColumns.AutoFit();

            sheet.Columns[1]
                .ColumnWidth =
                    6;

            sheet.Columns[2]
                .ColumnWidth =
                    16;

            sheet.Columns[3]
                .ColumnWidth =
                    28;

            sheet.Columns[4]
                .ColumnWidth =
                    10;

            for (int column = 5;
                 column <= lastColumn;
                 column++)
            {
                if (sheet.Columns[column]
                        .ColumnWidth > 20)
                {
                    sheet.Columns[column]
                        .ColumnWidth =
                            20;
                }
            }

            dynamic pageSetup =
                sheet.PageSetup;

            pageSetup.Orientation =
                2;

            pageSetup.Zoom =
                false;

            pageSetup.FitToPagesWide =
                1;

            pageSetup.FitToPagesTall =
                false;

            pageSetup.LeftMargin =
                excel.InchesToPoints(
                    0.25);

            pageSetup.RightMargin =
                excel.InchesToPoints(
                    0.25);

            pageSetup.TopMargin =
                excel.InchesToPoints(
                    0.40);

            pageSetup.BottomMargin =
                excel.InchesToPoints(
                    0.40);

            pageSetup.PrintTitleRows =
                $"${headerRow}:${headerRow}";

            pageSetup.CenterHeader =
                "&BTeachFlex — One System, Every Classroom";

            pageSetup.CenterFooter =
                "Page &P of &N";

            sheet.Activate();

            excel.ActiveWindow.SplitRow =
                headerRow;

            excel.ActiveWindow.FreezePanes =
                true;
        }

        private static int ToOleColor(
            int red,
            int green,
            int blue)
        {
            return System.Drawing
                .ColorTranslator
                .ToOle(
                    DrawingColor.FromArgb(
                        red,
                        green,
                        blue));
        }

        private static void ReleaseComObject(
            object? comObject)
        {
            if (comObject == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(
                        comObject))
                {
                    Marshal.FinalReleaseComObject(
                        comObject);
                }
            }
            catch
            {
                // Cleanup only.
            }
        }
    }
}