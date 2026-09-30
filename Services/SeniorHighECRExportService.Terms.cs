using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SeniorHighECRExportService
    {
        static partial void WriteTermSheets(
            dynamic workbook,
            SeniorHighEcrExportRequest request)
        {
            WriteTermSheet(
                workbook,
                request,
                1,
                "Term 1");

            WriteTermSheet(
                workbook,
                request,
                2,
                "Term 2");

            WriteTermSheet(
                workbook,
                request,
                3,
                "Term 3");
        }

        private static void WriteTermSheet(
            dynamic workbook,
            SeniorHighEcrExportRequest request,
            int termNumber,
            string worksheetName)
        {
            dynamic worksheet =
                GetWorksheet(
                    workbook,
                    worksheetName);

            try
            {
                ECRTermExportData? termData =
                    request.Terms
                        .Where(
                            term =>
                                term.TermNumber ==
                                    termNumber)
                        .OrderBy(
                            term =>
                                term.ComponentName)
                        .FirstOrDefault();

                WriteTermSheetData(
                    worksheet,
                    request,
                    termData);
            }
            finally
            {
                ReleaseComObject(
                    worksheet);
            }
        }

        static partial void WriteTermSheetData(
            dynamic worksheet,
            SeniorHighEcrExportRequest request,
            ECRTermExportData? termData);
    }
}