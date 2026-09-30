using System;
using System.IO;
using System.Runtime.InteropServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SeniorHighECRExportService :
        ISeniorHighECRExportService
    {
        public string PrepareOfficialSeniorHighECR(
            SeniorHighEcrExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "Select a valid output file.",
                    nameof(outputPath));
            }

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "ECR",
                    "ECR_SSHS.xlsx");

            if (!File.Exists(
                    templatePath))
            {
                throw new FileNotFoundException(
                    "The official SSHS E-Class Record template was not found.",
                    templatePath);
            }

            string fullOutputPath =
                Path.GetFullPath(
                    outputPath);

            string? outputDirectory =
                Path.GetDirectoryName(
                    fullOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputDirectory))
            {
                Directory.CreateDirectory(
                    outputDirectory);
            }

            File.Copy(
                templatePath,
                fullOutputPath,
                true);

            Type? excelType =
                Type.GetTypeFromProgID(
                    "Excel.Application");

            if (excelType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Excel is required to prepare the official SSHS E-Class Record.");
            }

            dynamic? excelApplication =
                null;

            dynamic? workbook =
                null;

            try
            {
                excelApplication =
    Activator.CreateInstance(
        excelType)
    ?? throw new InvalidOperationException(
        "Microsoft Excel could not be started.");

                excelApplication.Visible =
                    false;

                excelApplication.DisplayAlerts =
                    false;

                excelApplication.AskToUpdateLinks =
                    false;

                workbook =
                    excelApplication.Workbooks.Open(
                        fullOutputPath,
                        UpdateLinks: 0,
                        ReadOnly: false);

                PopulateWorkbook(
                    workbook,
                    request);

                workbook.Save();

                return fullOutputPath;
            }
            finally
            {
                if (workbook != null)
                {
                    try
                    {
                        workbook.Close(
                            SaveChanges: false);
                    }
                    catch
                    {
                    }

                    ReleaseComObject(
                        workbook);
                }

                if (excelApplication != null)
                {
                    try
                    {
                        excelApplication.Quit();
                    }
                    catch
                    {
                    }

                    ReleaseComObject(
                        excelApplication);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static dynamic GetWorksheet(
            dynamic workbook,
            string worksheetName)
        {
            return workbook.Worksheets[
                worksheetName];
        }

        private static void SetCellValue(
            dynamic worksheet,
            string cellAddress,
            object? value)
        {
            dynamic? cell =
                null;

            try
            {
                cell =
                    worksheet.Range[
                        cellAddress];

                cell.Value2 =
                    value;
            }
            finally
            {
                if (cell != null)
                {
                    ReleaseComObject(
                        cell);
                }
            }
        }

        private static void SetCellNumberFormat(
            dynamic worksheet,
            string cellAddress,
            string numberFormat)
        {
            dynamic? cell =
                null;

            try
            {
                cell =
                    worksheet.Range[
                        cellAddress];

                cell.NumberFormat =
                    numberFormat;
            }
            finally
            {
                if (cell != null)
                {
                    ReleaseComObject(
                        cell);
                }
            }
        }

        private static void ReleaseComObject(
            object? comObject)
        {
            if (comObject != null &&
                Marshal.IsComObject(
                    comObject))
            {
                Marshal.FinalReleaseComObject(
                    comObject);
            }
        }

        static partial void PopulateWorkbook(
            dynamic workbook,
            SeniorHighEcrExportRequest request);
    }
}