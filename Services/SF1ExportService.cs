using System;
using System.IO;
using System.Runtime.InteropServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1ExportService :
        ISF1ExportService
    {
        public string PrepareOfficialSF1(
            SF1ExportRequest request,
            string outputPath)
        {
            return GenerateOfficialSf1(
                request,
                outputPath,
                false);
        }

        public string ExportOfficialSF1Pdf(
            SF1ExportRequest request,
            string outputPath)
        {
            return GenerateOfficialSf1(
                request,
                outputPath,
                true);
        }

        private string GenerateOfficialSf1(
            SF1ExportRequest request,
            string outputPath,
            bool exportAsPdf)
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

            if (request.Learners.Count > 50)
            {
                throw new InvalidOperationException(
                    "The SF1 export supports a maximum " +
                    "of 50 learners.");
            }

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "DepEd_SF1_Template.xls");

            if (!File.Exists(
                    templatePath))
            {
                throw new FileNotFoundException(
                    "The official DepEd SF1 template " +
                    "was not found.",
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
                    "prepare the prescribed SF1 form.");
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

            string workingWorkbookPath =
                exportAsPdf
                    ? CreateTemporaryWorkbookPath()
                    : fullOutputPath;

            string? temporaryFolder =
                exportAsPdf
                    ? Path.GetDirectoryName(
                        workingWorkbookPath)
                    : null;

            if (!string.IsNullOrWhiteSpace(
                    temporaryFolder))
            {
                Directory.CreateDirectory(
                    temporaryFolder);
            }

            if (File.Exists(
                    workingWorkbookPath))
            {
                File.Delete(
                    workingWorkbookPath);
            }

            File.Copy(
                templatePath,
                workingWorkbookPath);

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

                excel.EnableEvents =
                    false;

                workbooks =
                    excel.Workbooks;

                dynamic workbookCollection =
                    workbooks;

                workbook =
                    workbookCollection.Open(
                        workingWorkbookPath);

                dynamic openedWorkbook =
                    workbook;

                worksheets =
                    openedWorkbook.Worksheets;

                dynamic worksheetCollection =
                    worksheets;

                worksheet =
                    worksheetCollection.Item(
                        1);

                if (worksheet == null)
                {
                    throw new InvalidOperationException(
                        "The prescribed SF1 worksheet " +
                        "could not be opened.");
                }

                PopulateOfficialSf1(
                    worksheet,
                    request);

                if (exportAsPdf)
                {
                    openedWorkbook.ExportAsFixedFormat(
                        Type: 0,
                        Filename: fullOutputPath,
                        Quality: 0,
                        IncludeDocProperties: true,
                        IgnorePrintAreas: false,
                        OpenAfterPublish: false);

                    openedWorkbook.Close(
                        false);
                }
                else
                {
                    openedWorkbook.Save();

                    openedWorkbook.Close(
                        true);
                }

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
                TryCloseWorkbook(
                    workbook);

                TryQuitExcel(
                    excelApplication);

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

                if (exportAsPdf)
                {
                    TryDeleteTemporaryWorkbook(
                        workingWorkbookPath);
                }
            }
        }

        partial void PopulateOfficialSf1(
            object worksheet,
            SF1ExportRequest request);

        private static string
            CreateTemporaryWorkbookPath()
        {
            string temporaryFolder =
                Path.Combine(
                    Path.GetTempPath(),
                    "TeachFlex",
                    "SF1");

            string temporaryFileName =
                $"sf1-{Guid.NewGuid():N}.xls";

            return Path.Combine(
                temporaryFolder,
                temporaryFileName);
        }

        private static void
            TryDeleteTemporaryWorkbook(
                string workbookPath)
        {
            try
            {
                if (File.Exists(
                        workbookPath))
                {
                    File.Delete(
                        workbookPath);
                }
            }
            catch
            {
                // Temporary cleanup must not hide
                // a successfully created PDF.
            }
        }

        private static void TryCloseWorkbook(
            object? workbook)
        {
            if (workbook == null)
            {
                return;
            }

            try
            {
                dynamic openedWorkbook =
                    workbook;

                openedWorkbook.Close(
                    false);
            }
            catch
            {
                // Preserve the original export error.
            }
        }

        private static void TryQuitExcel(
            object? excelApplication)
        {
            if (excelApplication == null)
            {
                return;
            }

            try
            {
                dynamic excel =
                    excelApplication;

                excel.Quit();
            }
            catch
            {
                // Preserve the original export error.
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
                // Excel may have already released it.
            }
        }
    }
}