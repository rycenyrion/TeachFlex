using System;
using System.IO;
using System.Runtime.InteropServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class KindergartenECRExportService :
        IKindergartenECRExportService
    {
        public string PrepareOfficialKindergartenECR(
            KindergartenEcrExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "An output file path is required.",
                    nameof(outputPath));
            }

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "ECR",
                    "ECR_Kindergarten.xlsx");

            if (!File.Exists(
                    templatePath))
            {
                throw new FileNotFoundException(
                    "The official Kindergarten E-Class " +
                    "Record template was not found.",
                    templatePath);
            }

            Type? excelType =
                Type.GetTypeFromProgID(
                    "Excel.Application");

            if (excelType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Excel is not installed. " +
                    "TeachFlex needs Microsoft Excel to " +
                    "prepare the official Kindergarten ECR.");
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

            if (File.Exists(
                    fullOutputPath))
            {
                File.Delete(
                    fullOutputPath);
            }

            File.Copy(
                templatePath,
                fullOutputPath);

            object? excelApplication =
                null;

            object? workbooks =
                null;

            object? workbook =
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
                        fullOutputPath);

                if (workbook == null)
                {
                    throw new InvalidOperationException(
                        "The Kindergarten ECR workbook " +
                        "could not be opened.");
                }

                PopulateWorkbook(
                    workbook,
                    request);

                dynamic openedWorkbook =
                    workbook;

                openedWorkbook.Save();

                openedWorkbook.Close(
                    true);

                ReleaseComObject(
                    workbook);

                workbook =
                    null;

                dynamic runningExcel =
                    excelApplication;

                runningExcel.Quit();

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

        static partial void PopulateWorkbook(
            object workbook,
            KindergartenEcrExportRequest request);

        private static object GetWorksheet(
            object workbook,
            string worksheetName)
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

                worksheet =
                    worksheetCollection.Item[
                        worksheetName];

                if (worksheet == null)
                {
                    throw new InvalidOperationException(
                        $"The worksheet '{worksheetName}' " +
                        "was not found in the Kinder template.");
                }

                return worksheet;
            }
            finally
            {
                ReleaseComObject(
                    worksheets);
            }
        }

        private static void SetCellValue(
            object worksheet,
            string cellAddress,
            object? value)
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

                selectedRange.Value2 =
                    value ?? string.Empty;
            }
            finally
            {
                ReleaseComObject(
                    range);
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

        private static void ReleaseComObject(
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