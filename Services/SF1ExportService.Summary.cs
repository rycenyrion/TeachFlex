using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1ExportService
    {
        private static void WriteSummaryInformation(
    dynamic sheet,
    SF1ExportRequest request,
    Sf1SheetLayout layout)
        {
            DateTime beginningCutoff =
                GetFirstFridayOfJune(
                    request.SchoolYearStartYear);

            List<SF1LearnerRow> beginningLearners =
                request.Learners
                    .Where(
                        learner =>
                            IsBeginningOfSchoolYearLearner(
                                learner,
                                beginningCutoff))
                    .ToList();

            int beginningMale =
                CountBySex(
                    beginningLearners,
                    "Male");

            int beginningFemale =
                CountBySex(
                    beginningLearners,
                    "Female");

            int registeredMaleRow =
                layout.ShiftedRow(
                    27);

            int registeredFemaleRow =
                layout.ShiftedRow(
                    30);

            int registeredTotalRow =
                layout.ShiftedRow(
                    32);

            // BoSY totals only.

            SetCellValue(
                sheet,
                registeredMaleRow,
                24,
                beginningMale);

            SetCellValue(
                sheet,
                registeredFemaleRow,
                24,
                beginningFemale);

            SetCellValue(
                sheet,
                registeredTotalRow,
                24,
                beginningMale +
                beginningFemale);

            // EoSY remains blank until the School Year
            // is officially finalized.

            SetCellValue(
                sheet,
                registeredMaleRow,
                27,
                null);

            SetCellValue(
                sheet,
                registeredFemaleRow,
                27,
                null);

            SetCellValue(
                sheet,
                registeredTotalRow,
                27,
                null);

            WriteSignatoryNames(
                sheet,
                request,
                layout);
        }

        private static void WriteSignatoryNames(
    dynamic sheet,
    SF1ExportRequest request,
    Sf1SheetLayout layout)
        {
            int adviserNameRow =
                layout.ShiftedRow(
                    27);

            int schoolHeadNameRow =
    layout.ShiftedRow(
        27);

            // Prepared by: Adviser
            // The prescribed template uses a merged area
            // from the name space down to the signature line.

            SetCellValue(
                sheet,
                adviserNameRow,
                31,
                request.AdviserName);

            dynamic adviserNameCell =
                sheet.Cells[
                    adviserNameRow,
                    31];

            adviserNameCell.HorizontalAlignment =
                -4108;

            adviserNameCell.VerticalAlignment =
                -4107;

            adviserNameCell.Font.Bold =
                true;

            adviserNameCell.Font.Size =
                8;

            // Certified Correct: School Head
            // Row 28 is directly above its signature description.

            // Certified Correct: School Head

SetCellValue(
    sheet,
    schoolHeadNameRow,
    40,
    request.SchoolHeadName);

            dynamic schoolHeadNameCell =
                sheet.Cells[
                    schoolHeadNameRow,
                    40];

            schoolHeadNameCell.HorizontalAlignment =
                -4108;

            schoolHeadNameCell.VerticalAlignment =
                -4107;

            schoolHeadNameCell.Font.Bold =
                true;

            schoolHeadNameCell.Font.Size =
                8;

            schoolHeadNameCell.ShrinkToFit =
                true;
        }
        private static bool
    IsBeginningOfSchoolYearLearner(
        SF1LearnerRow learner,
        DateTime beginningCutoff)
        {
            bool isLateEnrollee =
                learner.EnrollmentType.Equals(
                    "Late Enrollee",
                    StringComparison.OrdinalIgnoreCase);

            bool isTransferredIn =
                learner.EnrollmentType.Equals(
                    "Transferred In",
                    StringComparison.OrdinalIgnoreCase);

            if (!isLateEnrollee &&
                !isTransferredIn)
            {
                return true;
            }

            return learner.EnrollmentDate.HasValue &&
                   learner.EnrollmentDate.Value.Date <=
                   beginningCutoff.Date;
        }
        private static int CountBySex(
            IEnumerable<SF1LearnerRow> learners,
            string sex)
        {
            return learners.Count(
                learner =>
                    learner.Sex.Equals(
                        sex,
                        StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasExitedBeforeEndOfYear(
            SF1LearnerRow learner)
        {
            if (string.IsNullOrWhiteSpace(
                    learner.ExitReason))
            {
                return false;
            }

            return !learner.ExitReason.Equals(
                "Completed",
                StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime GetFirstFridayOfJune(
            int year)
        {
            DateTime date =
                new DateTime(
                    year,
                    6,
                    1);

            while (date.DayOfWeek !=
                   DayOfWeek.Friday)
            {
                date =
                    date.AddDays(
                        1);
            }

            return date;
        }
    }
}