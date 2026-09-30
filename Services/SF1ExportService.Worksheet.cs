using System;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1ExportService
    {
        private const int OriginalMaleStartRow =
            7;

        private const int OriginalMaleCapacity =
            7;

        private const int OriginalFemaleCapacity =
            8;

        partial void PopulateOfficialSf1(
            object worksheet,
            SF1ExportRequest request)
        {
            dynamic sheet =
                worksheet;

            int maleCount =
                request.Learners.Count(
                    learner =>
                        learner.Sex.Equals(
                            "Male",
                            StringComparison.OrdinalIgnoreCase));

            int femaleCount =
                request.Learners.Count(
                    learner =>
                        learner.Sex.Equals(
                            "Female",
                            StringComparison.OrdinalIgnoreCase));

            Sf1SheetLayout layout =
                PrepareDynamicLearnerRows(
                    sheet,
                    maleCount,
                    femaleCount);

            WriteHeaderInformation(
                sheet,
                request);

            WriteLearnerInformation(
                sheet,
                request,
                layout);

            WriteSummaryInformation(
                sheet,
                request,
                layout);

            ConfigurePrintLayout(
                sheet,
                layout);
        }

        private static Sf1SheetLayout
            PrepareDynamicLearnerRows(
                dynamic sheet,
                int maleCount,
                int femaleCount)
        {
            int maleTotalRow =
                AdjustLearnerSection(
                    sheet,
                    OriginalMaleStartRow,
                    OriginalMaleCapacity,
                    maleCount);

            int femaleStartRow =
                maleTotalRow + 1;

            int femaleTotalRow =
                AdjustLearnerSection(
                    sheet,
                    femaleStartRow,
                    OriginalFemaleCapacity,
                    femaleCount);

            int combinedTotalRow =
                femaleTotalRow + 1;

            int legendStartRow =
                combinedTotalRow + 1;

            int rowShift =
                legendStartRow - 25;

            return new Sf1SheetLayout
            {
                MaleStartRow =
                    OriginalMaleStartRow,

                MaleTotalRow =
                    maleTotalRow,

                FemaleStartRow =
                    femaleStartRow,

                FemaleTotalRow =
                    femaleTotalRow,

                CombinedTotalRow =
                    combinedTotalRow,

                RowShift =
                    rowShift,

                LastPrintRow =
                    36 + rowShift
            };
        }

        private static int AdjustLearnerSection(
            dynamic sheet,
            int startRow,
            int originalCapacity,
            int requiredRows)
        {
            int originalTotalRow =
                startRow +
                originalCapacity;

            if (requiredRows < originalCapacity)
            {
                int firstUnusedRow =
                    startRow +
                    requiredRows;

                int lastUnusedRow =
                    originalTotalRow -
                    1;

                if (firstUnusedRow <=
                    lastUnusedRow)
                {
                    dynamic unusedRows =
                        sheet.Rows[
                            $"{firstUnusedRow}:" +
                            $"{lastUnusedRow}"];

                    unusedRows.Delete();
                }

                return startRow +
                       requiredRows;
            }

            if (requiredRows > originalCapacity)
            {
                int additionalRows =
                    requiredRows -
                    originalCapacity;

                int insertionRow =
                    originalTotalRow;

                for (int index = 0;
                     index < additionalRows;
                     index++)
                {
                    dynamic row =
                        sheet.Rows[
                            insertionRow];

                    // Shift down and inherit the formatting
                    // from the learner row immediately above.
                    row.Insert(
                        -4121,
                        0);

                    insertionRow++;
                }
            }

            return startRow +
                   requiredRows;
        }

        private static void WriteHeaderInformation(
            dynamic sheet,
            SF1ExportRequest request)
        {
            SetCellValue(
                sheet,
                3,
                6,
                request.SchoolId);

            SetCellValue(
                sheet,
                3,
                20,
                request.Division);

            SetCellValue(
                sheet,
                3,
                39,
                request.District);

            SetCellValue(
                sheet,
                4,
                6,
                request.SchoolName);

            SetCellValue(
                sheet,
                4,
                20,
                request.SchoolYear);

            SetCellValue(
                sheet,
                4,
                31,
                request.GradeLevel);

            SetCellValue(
                sheet,
                4,
                39,
                request.SectionName);
        }

        private static void ConfigurePrintLayout(
            dynamic sheet,
            Sf1SheetLayout layout)
        {
            dynamic pageSetup =
                sheet.PageSetup;

            pageSetup.Orientation =
                2;

            pageSetup.PaperSize =
                5;

            pageSetup.Zoom =
                false;

            pageSetup.FitToPagesWide =
                1;

            pageSetup.FitToPagesTall =
                false;

            pageSetup.CenterHorizontally =
                true;

            pageSetup.PrintArea =
                $"$A$1:$AT${layout.LastPrintRow}";
        }

        private static void SetCellValue(
            dynamic sheet,
            int row,
            int column,
            object? value)
        {
            dynamic cell =
                sheet.Cells[
                    row,
                    column];

            cell.Value2 =
                value;
        }

        private sealed class Sf1SheetLayout
        {
            public int MaleStartRow
            {
                get;
                init;
            }

            public int MaleTotalRow
            {
                get;
                init;
            }

            public int FemaleStartRow
            {
                get;
                init;
            }

            public int FemaleTotalRow
            {
                get;
                init;
            }

            public int CombinedTotalRow
            {
                get;
                init;
            }

            public int RowShift
            {
                get;
                init;
            }

            public int LastPrintRow
            {
                get;
                init;
            }

            public int ShiftedRow(
                int originalRow)
            {
                return originalRow +
                       RowShift;
            }
        }
    }
}