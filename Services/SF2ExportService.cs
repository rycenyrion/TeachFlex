using System;
using System.IO;
using System.Runtime.InteropServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF2ExportService :
        ISF2ExportService
    {
        public string PrepareOfficialSF2(
            SF2ExportRequest request,
            string outputPath)
        {
            return GenerateOfficialSf2(
                request,
                outputPath,
                false);
        }

        public string ExportOfficialSF2Pdf(
            SF2ExportRequest request,
            string outputPath)
        {
            return GenerateOfficialSf2(
                request,
                outputPath,
                true);
        }

        private string GenerateOfficialSf2(
            SF2ExportRequest request,
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

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "DepEd_SF2_Template.xls");

            if (!File.Exists(
                    templatePath))
            {
                throw new FileNotFoundException(
                    "The official DepEd SF2 template " +
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
                    "prepare the prescribed SF2 form.");
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
                    ? CreateTemporarySf2WorkbookPath()
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
                        "The prescribed SF2 worksheet " +
                        "could not be opened.");
                }

                // Row 3: Header information.

                SetCellValue(
                    worksheet,
                    3,
                    6,
                    request.SchoolId);

                SetCellValue(
                    worksheet,
                    3,
                    13,
                    request.SchoolYear);

                SetCellValue(
                    worksheet,
                    3,
                    27,
                    $"'{request.ReportMonth:MMMM yyyy}");

                // Row 4: School and class information.

                SetCellValue(
                    worksheet,
                    4,
                    6,
                    request.SchoolName);

                SetCellValue(
                    worksheet,
                    4,
                    27,
                    request.GradeLevel);

                SetCellValue(
                    worksheet,
                    4,
                    39,
                    request.SectionName);

                WriteAttendanceData(
                    worksheet,
                    request);

                ConfigureSf2PrintLayout(
                    worksheet);

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
                TryCloseSf2Workbook(
                    workbook);

                TryQuitSf2Excel(
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
                    TryDeleteTemporarySf2Workbook(
                        workingWorkbookPath);
                }
            }
        }

        private static string
            CreateTemporarySf2WorkbookPath()
        {
            string temporaryFolder =
                Path.Combine(
                    Path.GetTempPath(),
                    "TeachFlex",
                    "SF2");

            string temporaryFileName =
                $"sf2-{Guid.NewGuid():N}.xls";

            return Path.Combine(
                temporaryFolder,
                temporaryFileName);
        }

        private static void
            TryDeleteTemporarySf2Workbook(
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

        private static void TryCloseSf2Workbook(
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

        private static void TryQuitSf2Excel(
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

        private static void ConfigureSf2PrintLayout(
    object worksheet)
        {
            object? pageSetup =
                null;

            object? cells =
                null;

            object? lastContentCell =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                cells =
                    selectedWorksheet.Cells;

                dynamic cellCollection =
                    cells;

                // Find the last row containing actual
                // text, values, or formulas.
                lastContentCell =
                    cellCollection.Find(
                        What:
                            "*",
                        After:
                            Type.Missing,
                        LookIn:
                            -4163,
                        LookAt:
                            2,
                        SearchOrder:
                            1,
                        SearchDirection:
                            2,
                        MatchCase:
                            false);

                int lastContentRow =
                    58;

                if (lastContentCell != null)
                {
                    dynamic lastCell =
                        lastContentCell;

                    lastContentRow =
                        Convert.ToInt32(
                            lastCell.Row);

                    // Keep one additional row so the
                    // bottom border is not cropped.
                    lastContentRow++;
                }

                pageSetup =
                    selectedWorksheet.PageSetup;

                dynamic setup =
                    pageSetup;

                // Dynamic print area based on the
                // remaining learner rows and footer.
                setup.PrintArea =
                    $"$A$1:$AU${lastContentRow}";

                // A4 landscape.
                setup.Orientation =
                    2;

                setup.PaperSize =
                    9;

                // Keep the complete dynamic SF2 form
                // on one A4 landscape page.
                setup.Zoom =
                    false;

                setup.FitToPagesWide =
                    2;

                setup.FitToPagesTall =
                    2;

                setup.CenterHorizontally =
                    true;

                setup.CenterVertically =
                    false;

                setup.LeftMargin =
                    9;

                setup.RightMargin =
                    9;

                setup.TopMargin =
                    13;

                setup.BottomMargin =
                    13;

                setup.HeaderMargin =
                    0;

                setup.FooterMargin =
                    0;

                setup.PrintGridlines =
                    false;

                setup.PrintHeadings =
                    false;

                setup.BlackAndWhite =
                    false;

                setup.Draft =
                    false;
            }
            finally
            {
                ReleaseComObject(
                    lastContentCell);

                ReleaseComObject(
                    cells);

                ReleaseComObject(
                    pageSetup);
            }
        }

        private static void SetCellValue(
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