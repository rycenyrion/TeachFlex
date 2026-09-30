namespace TeachFlex.Services
{
    public partial class
        KindergartenECRExportService
    {
        private static void WriteLearnerLinks(
            dynamic workbook)
        {
            string[] termSheetNames =
            {
                "TERM 1 SUMMARY",
                "TERM 2 SUMMARY",
                "TERM 3 SUMMARY"
            };

            foreach (string sheetName
                     in termSheetNames)
            {
                dynamic worksheet =
                    GetWorksheet(
                        workbook,
                        sheetName);

                WriteSummaryLearnerLinks(
                    worksheet,
                    summaryStartRow: 16,
                    inputLrnColumn: "M",
                    inputBirthDateColumn: "N");

                WriteSummaryLearnerLinks(
                    worksheet,
                    summaryStartRow: 66,
                    inputLrnColumn: "R",
                    inputBirthDateColumn: "S");

                ReleaseComObject(
                    worksheet);
            }
        }

        private static void WriteSummaryLearnerLinks(
            dynamic worksheet,
            int summaryStartRow,
            string inputLrnColumn,
            string inputBirthDateColumn)
        {
            const int availableLearnerRows =
                49;

            for (int index = 0;
                 index < availableLearnerRows;
                 index++)
            {
                int summaryRow =
                    summaryStartRow + index;

                int inputRow =
                    11 + index;

                SetSummaryFormula(
                    worksheet,
                    $"F{summaryRow}",
                    $"=IF('INPUT DATA'!" +
                    $"{inputLrnColumn}{inputRow}=\"\",\"'," +
                    $"'INPUT DATA'!" +
                    $"{inputLrnColumn}{inputRow})");

                SetSummaryFormula(
                    worksheet,
                    $"H{summaryRow}",
                    $"=IF('INPUT DATA'!" +
                    $"{inputBirthDateColumn}{inputRow}=\"\",\"'," +
                    $"'INPUT DATA'!" +
                    $"{inputBirthDateColumn}{inputRow})");
            }
        }

        private static void SetSummaryFormula(
            dynamic worksheet,
            string cellAddress,
            string formula)
        {
            dynamic? cell =
                null;

            dynamic? targetRange =
                null;

            try
            {
                cell =
                    worksheet.Range[
                        cellAddress];

                bool isMerged =
                    cell.MergeCells;

                targetRange =
                    isMerged
                        ? cell.MergeArea
                        : cell;

                targetRange.Formula =
                    formula;
            }
            finally
            {
                if (targetRange != null &&
                    !ReferenceEquals(
                        targetRange,
                        cell))
                {
                    ReleaseComObject(
                        targetRange);
                }

                if (cell != null)
                {
                    ReleaseComObject(
                        cell);
                }
            }
        }
    }
}