using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        [RelayCommand]
        private async Task ImportSf1LearnersAsync()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select the destination class first.",
                    "Import Learners from SF1");

                return;
            }

            OpenFileDialog openFileDialog =
                new OpenFileDialog
                {
                    Title =
                        "Select Official DepEd SF1 File",

                    Filter =
                        "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|" +
                        "Excel 97-2003 Workbook (*.xls)|*.xls|" +
                        "Excel Workbook (*.xlsx)|*.xlsx",

                    Multiselect =
                        false,

                    CheckFileExists =
                        true
                };

            bool? fileSelected =
                openFileDialog.ShowDialog();

            if (fileSelected != true)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    "Reading the selected SF1 file...";

                Sf1ImportReadResult importResult =
                    ReadOfficialSf1File(
                        openFileDialog.FileName);

                if (importResult.Learners.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "No valid learner records were found.\n\n" +
                        "Make sure the selected file uses the " +
                        "official DepEd SF1 format and contains " +
                        "valid 12-digit LRNs.",
                        "No SF1 Learners Found");

                    StatusMessage =
                        "No valid learners were found in the SF1 file.";

                    return;
                }

                IReadOnlyList<Learner>
                    existingLearners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                HashSet<string> existingLrns =
                    existingLearners
                        .Select(
                            learner =>
                                NormalizeImportedLrn(
                                    learner.Lrn))
                        .Where(
                            lrn =>
                                !string.IsNullOrWhiteSpace(
                                    lrn))
                        .ToHashSet(
                            StringComparer.Ordinal);

                List<Learner> newLearners =
                    importResult.Learners
                        .Where(
                            learner =>
                                !existingLrns.Contains(
                                    learner.Lrn))
                        .ToList();

                int duplicateCount =
                    importResult.Learners.Count -
                    newLearners.Count;

                if (newLearners.Count == 0)
                {
                    _dialogService.ShowInformation(
                        "All valid learners in the selected " +
                        "SF1 file already exist in the " +
                        "selected class.\n\n" +
                        $"Duplicate learners skipped: " +
                        $"{duplicateCount}",
                        "SF1 Import Complete");

                    StatusMessage =
                        "No new learners were imported.";

                    return;
                }

                string confirmationMessage =
                    $"Destination class:\n" +
                    $"{SelectedClass.DisplayName}\n\n" +
                    $"New learners ready to import: " +
                    $"{newLearners.Count}\n" +
                    $"Duplicate LRNs to skip: " +
                    $"{duplicateCount}\n" +
                    $"Invalid rows to skip: " +
                    $"{importResult.InvalidRowCount}\n\n" +
                    $"Continue importing these learners?";

                bool confirmed =
                    _dialogService.Confirm(
                        confirmationMessage,
                        "Confirm SF1 Learner Import");

                if (!confirmed)
                {
                    StatusMessage =
                        "SF1 learner import was cancelled.";

                    return;
                }

                StatusMessage =
                    $"Importing {newLearners.Count} " +
                    $"learner(s)...";

                int importedCount =
                    0;

                foreach (Learner learner
                         in newLearners)
                {
                    learner.SchoolClassId =
                        SelectedClass.Id;

                    await _learnerRepository
                        .SaveAsync(
                            learner);

                    importedCount++;
                }

                await LoadLearnersAsync();

                ClearForm();

                StatusMessage =
                    $"{importedCount} learner(s) imported " +
                    $"successfully from SF1.";

                _dialogService.ShowInformation(
                    $"SF1 learner import completed.\n\n" +
                    $"Imported: {importedCount}\n" +
                    $"Duplicate LRNs skipped: " +
                    $"{duplicateCount}\n" +
                    $"Invalid rows skipped: " +
                    $"{importResult.InvalidRowCount}",
                    "SF1 Import Complete");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The SF1 learner import could not be completed.";

                _dialogService.ShowError(
                    $"TeachFlex could not import learners " +
                    $"from the selected SF1 file.\n\n" +
                    $"{errorMessage}",
                    "SF1 Import Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private static Sf1ImportReadResult
            ReadOfficialSf1File(
                string filePath)
        {
            Type? excelType =
                Type.GetTypeFromProgID(
                    "Excel.Application");

            if (excelType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Excel is not installed. " +
                    "TeachFlex needs Microsoft Excel to " +
                    "read the official SF1 file.");
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

            object? usedRange =
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

                // Open the source workbook as read-only.
                workbook =
                    workbookCollection.Open(
                        filePath,
                        0,
                        true);

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
                        "The first SF1 worksheet could not be opened.");
                }

                dynamic selectedWorksheet =
                    worksheet;

                usedRange =
                    selectedWorksheet.UsedRange;

                dynamic selectedRange =
                    usedRange;

                int firstUsedRow =
                    Convert.ToInt32(
                        selectedRange.Row);

                int usedRowCount =
                    Convert.ToInt32(
                        selectedRange.Rows.Count);

                int lastUsedRow =
                    firstUsedRow +
                    usedRowCount -
                    1;

                Sf1ImportReadResult result =
                    new Sf1ImportReadResult();

                HashSet<string> fileLrns =
                    new HashSet<string>(
                        StringComparer.Ordinal);

                // Official SF1 learner records begin
                // below the two-row column heading.
                for (int row = 7;
                     row <= lastUsedRow;
                     row++)
                {
                    object? rawLrn =
                        GetSf1CellValue(
                            worksheet,
                            row,
                            1);

                    string normalizedLrn =
                        NormalizeImportedLrn(
                            rawLrn);

                    // Blank and total rows are ignored.
                    if (string.IsNullOrWhiteSpace(
                            normalizedLrn))
                    {
                        continue;
                    }

                    if (normalizedLrn.Length != 12 ||
                        !normalizedLrn.All(
                            char.IsDigit))
                    {
                        result.InvalidRowCount++;

                        continue;
                    }

                    // Duplicate rows inside the same
                    // uploaded workbook are skipped.
                    if (!fileLrns.Add(
                            normalizedLrn))
                    {
                        result.InvalidRowCount++;

                        continue;
                    }

                    string learnerName =
                        GetSf1CellText(
                            worksheet,
                            row,
                            3);

                    string rawSex =
                        GetSf1CellText(
                            worksheet,
                            row,
                            7);

                    string sex =
                        NormalizeImportedSex(
                            rawSex);

                    if (string.IsNullOrWhiteSpace(
                            learnerName) ||
                        string.IsNullOrWhiteSpace(
                            sex))
                    {
                        result.InvalidRowCount++;

                        continue;
                    }

                    if (!TryParseLearnerName(
                            learnerName,
                            out string lastName,
                            out string firstName,
                            out string middleName))
                    {
                        result.InvalidRowCount++;

                        continue;
                    }

                    DateTime? birthDate =
                        ParseImportedDate(
                            GetSf1CellValue(
                                worksheet,
                                row,
                                8));

                    string houseAddress =
                        GetSf1CellText(
                            worksheet,
                            row,
                            16);

                    string barangay =
                        GetSf1CellText(
                            worksheet,
                            row,
                            18);

                    string municipalityCity =
                        GetSf1CellText(
                            worksheet,
                            row,
                            21);

                    string province =
                        GetSf1CellText(
                            worksheet,
                            row,
                            23);

                    string fatherName =
                        GetSf1CellText(
                            worksheet,
                            row,
                            28);

                    string motherName =
                        GetSf1CellText(
                            worksheet,
                            row,
                            32);

                    string guardianName =
                        GetSf1CellText(
                            worksheet,
                            row,
                            37);

                    string completeAddress =
                        JoinImportedValues(
                            houseAddress,
                            barangay,
                            municipalityCity,
                            province);

                    string parentGuardianName =
                        FirstNonBlank(
                            guardianName,
                            motherName,
                            fatherName);

                    Learner learner =
                        new Learner
                        {
                            Lrn =
                                normalizedLrn,

                            LastName =
                                lastName,

                            FirstName =
                                firstName,

                            MiddleName =
                                middleName,

                            Suffix =
                                string.Empty,

                            Sex =
                                sex,

                            BirthDate =
                                birthDate,

                            Address =
                                completeAddress,

                            ParentGuardianName =
                                parentGuardianName,

                            ParentGuardianContactNumber =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    42),

                            Status =
                                "Active",

                            // A blank enrollment date means
                            // the learner was already enrolled
                            // before the imported report period.
                            EnrollmentDate =
                                null,

                            EnrollmentType =
                                "Regular",

                            MotherTongue =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    12),

                            IndigenousPeopleEthnicGroup =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    14),

                            Religion =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    15),

                            HouseStreetSitioPurok =
                                houseAddress,

                            Barangay =
                                barangay,

                            MunicipalityCity =
                                municipalityCity,

                            Province =
                                province,

                            FatherName =
                                fatherName,

                            MotherMaidenName =
                                motherName,

                            GuardianName =
                                guardianName,

                            GuardianRelationship =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    41),

                            LearningModality =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    44),

                            Sf1Remarks =
                                GetSf1CellText(
                                    worksheet,
                                    row,
                                    45)
                        };

                    result.Learners.Add(
                        learner);
                }

                openedWorkbook.Close(
                    false);

                ReleaseSf1ComObject(
                    usedRange);

                usedRange =
                    null;

                ReleaseSf1ComObject(
                    worksheet);

                worksheet =
                    null;

                ReleaseSf1ComObject(
                    worksheets);

                worksheets =
                    null;

                ReleaseSf1ComObject(
                    workbook);

                workbook =
                    null;

                dynamic runningExcel =
                    excelApplication;

                runningExcel.Quit();

                ReleaseSf1ComObject(
                    workbooks);

                workbooks =
                    null;

                ReleaseSf1ComObject(
                    excelApplication);

                excelApplication =
                    null;

                return result;
            }
            catch
            {
                TryCloseSf1ImportWorkbook(
                    workbook);

                TryQuitSf1ImportExcel(
                    excelApplication);

                throw;
            }
            finally
            {
                ReleaseSf1ComObject(
                    usedRange);

                ReleaseSf1ComObject(
                    worksheet);

                ReleaseSf1ComObject(
                    worksheets);

                ReleaseSf1ComObject(
                    workbook);

                ReleaseSf1ComObject(
                    workbooks);

                ReleaseSf1ComObject(
                    excelApplication);

                GC.Collect();

                GC.WaitForPendingFinalizers();

                GC.Collect();

                GC.WaitForPendingFinalizers();
            }
        }

        private static object? GetSf1CellValue(
            object worksheet,
            int row,
            int column)
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

                return selectedCell.Value2;
            }
            finally
            {
                ReleaseSf1ComObject(
                    cell);

                ReleaseSf1ComObject(
                    cells);
            }
        }

        private static string GetSf1CellText(
            object worksheet,
            int row,
            int column)
        {
            object? value =
                GetSf1CellValue(
                    worksheet,
                    row,
                    column);

            return value?
                .ToString()?
                .Trim()
                ?? string.Empty;
        }

        private static string NormalizeImportedLrn(
            object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            string rawValue;

            if (value is double doubleValue)
            {
                rawValue =
                    Math.Round(
                            doubleValue)
                        .ToString(
                            "0",
                            CultureInfo.InvariantCulture);
            }
            else if (value is decimal decimalValue)
            {
                rawValue =
                    decimalValue.ToString(
                        "0",
                        CultureInfo.InvariantCulture);
            }
            else
            {
                rawValue =
                    value.ToString()
                    ?? string.Empty;
            }

            return new string(
                rawValue
                    .Where(
                        char.IsDigit)
                    .ToArray());
        }

        private static string NormalizeImportedSex(
            string value)
        {
            string normalizedValue =
                value.Trim();

            if (normalizedValue.Equals(
                    "M",
                    StringComparison.OrdinalIgnoreCase) ||
                normalizedValue.Equals(
                    "Male",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Male";
            }

            if (normalizedValue.Equals(
                    "F",
                    StringComparison.OrdinalIgnoreCase) ||
                normalizedValue.Equals(
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Female";
            }

            return string.Empty;
        }

        private static DateTime? ParseImportedDate(
            object? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is DateTime dateValue)
            {
                return dateValue.Date;
            }

            if (value is double numericDate)
            {
                try
                {
                    return DateTime
                        .FromOADate(
                            numericDate)
                        .Date;
                }
                catch
                {
                    return null;
                }
            }

            string text =
                value.ToString()
                ?? string.Empty;

            if (DateTime.TryParse(
                    text,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out DateTime currentCultureDate))
            {
                return currentCultureDate.Date;
            }

            if (DateTime.TryParse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime invariantDate))
            {
                return invariantDate.Date;
            }

            return null;
        }

        private static string JoinImportedValues(
            params string[] values)
        {
            return string.Join(
                ", ",
                values
                    .Where(
                        value =>
                            !string.IsNullOrWhiteSpace(
                                value))
                    .Select(
                        value =>
                            value.Trim()));
        }

        private static string FirstNonBlank(
            params string[] values)
        {
            return values
                .FirstOrDefault(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value))?
                .Trim()
                ?? string.Empty;
        }

        private static void TryCloseSf1ImportWorkbook(
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
                // Preserve the original import error.
            }
        }

        private static void TryQuitSf1ImportExcel(
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
                // Preserve the original import error.
            }
        }

        private static void ReleaseSf1ComObject(
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

        private sealed class Sf1ImportReadResult
        {
            public List<Learner> Learners
            {
                get;
            } = new List<Learner>();

            public int InvalidRowCount
            {
                get;
                set;
            }
        }
    }
}