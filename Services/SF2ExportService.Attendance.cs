using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF2ExportService
    {
        private const int OriginalMaleSlots =
            7;

        private const int OriginalFemaleSlots =
            8;

        private static readonly int[]
            AttendanceColumns =
            {
                6, 8, 9, 10, 11,
                12, 14, 15, 16, 17,
                18, 20, 21, 22, 24,
                26, 28, 29, 30, 31,
                32, 33, 35, 36, 37
            };

        private static void WriteAttendanceData(
            object worksheet,
            SF2ExportRequest request)
        {
            List<SF2LearnerRow> maleLearners =
                request.Learners
                    .Where(
                        learner =>
                            string.Equals(
                                learner.Sex,
                                "Male",
                                StringComparison
                                    .OrdinalIgnoreCase))
                    .ToList();

            List<SF2LearnerRow> femaleLearners =
                request.Learners
                    .Where(
                        learner =>
                            string.Equals(
                                learner.Sex,
                                "Female",
                                StringComparison
                                    .OrdinalIgnoreCase))
                    .ToList();

            List<int> recordedDays =
                request.Learners
                    .SelectMany(
                        learner =>
                            learner.DailyStatuses.Keys)
                    .Distinct()
                    .Where(
                        day =>
                            day >= 1 &&
                            day <= DateTime.DaysInMonth(
                                request.ReportMonth.Year,
                                request.ReportMonth.Month))
                    .OrderBy(
                        day =>
                            day)
                    .ToList();

            if (recordedDays.Count >
                AttendanceColumns.Length)
            {
                throw new InvalidOperationException(
                    "The selected month contains more " +
                    "attendance dates than the official " +
                    "SF2 template can display.");
            }

            SF2RowLayout layout =
                ExpandLearnerRows(
                    worksheet,
                    maleLearners.Count,
                    femaleLearners.Count);

            WriteDateHeaders(
                worksheet,
                request.ReportMonth,
                recordedDays);

            WriteLearnerGroup(
                worksheet,
                maleLearners,
                layout.MaleFirstRow,
                layout.MaleSlots,
                recordedDays);

            WriteLearnerGroup(
                worksheet,
                femaleLearners,
                layout.FemaleFirstRow,
                layout.FemaleSlots,
                recordedDays);

            WriteTotalRows(
                worksheet,
                maleLearners,
                femaleLearners,
                recordedDays,
                layout);

            WriteSummaryAndSignatures(
                worksheet,
                request,
                layout.RowOffset);
        }

        private static SF2RowLayout
    ExpandLearnerRows(
        object worksheet,
        int maleCount,
        int femaleCount)
        {
            int maleRowDifference =
                maleCount -
                OriginalMaleSlots;

            int femaleRowDifference =
                femaleCount -
                OriginalFemaleSlots;

            // Male learner rows originally occupy
            // rows 8 to 14.
            if (maleRowDifference > 0)
            {
                for (int index = 0;
                     index < maleRowDifference;
                     index++)
                {
                    CopyAndInsertRow(
                        worksheet,
                        sourceRow:
                            14 + index,
                        insertAtRow:
                            15 + index);
                }
            }
            else if (maleRowDifference < 0)
            {
                int rowsToDelete =
                    Math.Abs(
                        maleRowDifference);

                int deleteAtRow =
                    8 +
                    maleCount;

                for (int index = 0;
                     index < rowsToDelete;
                     index++)
                {
                    DeleteEntireRow(
                        worksheet,
                        deleteAtRow);
                }
            }

            int maleFirstRow =
                8;

            int maleTotalRow =
                maleFirstRow +
                maleCount;

            int femaleFirstRow =
                maleTotalRow +
                1;

            int femaleTotalRowBeforeResize =
                femaleFirstRow +
                OriginalFemaleSlots;

            // Female learner rows originally contain
            // eight available rows.
            if (femaleRowDifference > 0)
            {
                for (int index = 0;
                     index < femaleRowDifference;
                     index++)
                {
                    CopyAndInsertRow(
                        worksheet,
                        sourceRow:
                            femaleTotalRowBeforeResize -
                            1 +
                            index,
                        insertAtRow:
                            femaleTotalRowBeforeResize +
                            index);
                }
            }
            else if (femaleRowDifference < 0)
            {
                int rowsToDelete =
                    Math.Abs(
                        femaleRowDifference);

                int deleteAtRow =
                    femaleFirstRow +
                    femaleCount;

                for (int index = 0;
                     index < rowsToDelete;
                     index++)
                {
                    DeleteEntireRow(
                        worksheet,
                        deleteAtRow);
                }
            }

            int femaleTotalRow =
                femaleFirstRow +
                femaleCount;

            return new SF2RowLayout
            {
                MaleFirstRow =
                    maleFirstRow,

                MaleSlots =
                    maleCount,

                MaleTotalRow =
                    maleTotalRow,

                FemaleFirstRow =
                    femaleFirstRow,

                FemaleSlots =
                    femaleCount,

                FemaleTotalRow =
                    femaleTotalRow,

                CombinedTotalRow =
                    femaleTotalRow +
                    1,

                RowOffset =
                    maleRowDifference +
                    femaleRowDifference
            };
        }

        private static void CopyAndInsertRow(
            object worksheet,
            int sourceRow,
            int insertAtRow)
        {
            object? rows =
                null;

            object? sourceRange =
                null;

            object? targetRange =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                rows =
                    selectedWorksheet.Rows;

                dynamic rowCollection =
                    rows;

                sourceRange =
                    rowCollection.Item[
                        sourceRow];

                targetRange =
                    rowCollection.Item[
                        insertAtRow];

                dynamic source =
                    sourceRange;

                dynamic target =
                    targetRange;

                source.Copy();

                // -4121 means shift existing rows down.
                target.Insert(
                    -4121,
                    0);
            }
            finally
            {
                ReleaseComObject(
                    targetRange);

                ReleaseComObject(
                    sourceRange);

                ReleaseComObject(
                    rows);
            }
        }

        private static void DeleteEntireRow(
            object worksheet,
            int rowNumber)
        {
            object? rows =
                null;

            object? selectedRow =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                rows =
                    selectedWorksheet.Rows;

                dynamic rowCollection =
                    rows;

                selectedRow =
                    rowCollection.Item[
                        rowNumber];

                dynamic row =
                    selectedRow;

                row.Delete();
            }
            finally
            {
                ReleaseComObject(
                    selectedRow);

                ReleaseComObject(
                    rows);
            }
        }

        private static void WriteDateHeaders(
    object worksheet,
    DateTime reportMonth,
    IReadOnlyList<int> recordedDays)
        {
            for (int index = 0;
                 index < AttendanceColumns.Length;
                 index++)
            {
                int column =
                    AttendanceColumns[index];

                if (index >= recordedDays.Count)
                {
                    SetCellValue(
                        worksheet,
                        6,
                        column,
                        string.Empty);

                    SetCellValue(
                        worksheet,
                        7,
                        column,
                        string.Empty);

                    continue;
                }

                int dayNumber =
                    recordedDays[index];

                DateTime attendanceDate =
                    new DateTime(
                        reportMonth.Year,
                        reportMonth.Month,
                        dayNumber);

                SetCellValue(
                    worksheet,
                    6,
                    column,
                    dayNumber);

                SetCellValue(
                    worksheet,
                    7,
                    column,
                    GetWeekdayCode(
                        attendanceDate.DayOfWeek));
            }
        }

        private static void WriteLearnerGroup(
            object worksheet,
            IReadOnlyList<SF2LearnerRow> learners,
            int firstRow,
            int availableRows,
            IReadOnlyList<int> recordedDays)
        {
            for (int rowIndex = 0;
                 rowIndex < availableRows;
                 rowIndex++)
            {
                int worksheetRow =
                    firstRow +
                    rowIndex;

                if (rowIndex >= learners.Count)
                {
                    ClearLearnerRow(
                        worksheet,
                        worksheetRow);

                    continue;
                }

                SF2LearnerRow learner =
                    learners[rowIndex];

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    1,
                    rowIndex + 1);

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    3,
                    learner.LearnerName);

                int presentTotal =
                    0;

                int absentTotal =
                    0;

                for (int dateIndex = 0;
                     dateIndex <
                        AttendanceColumns.Length;
                     dateIndex++)
                {
                    int column =
                        AttendanceColumns[
                            dateIndex];

                    if (dateIndex >=
                        recordedDays.Count)
                    {
                        SetCellValue(
                            worksheet,
                            worksheetRow,
                            column,
                            string.Empty);

                        continue;
                    }

                    int dayNumber =
                        recordedDays[
                            dateIndex];

                    learner.DailyStatuses
                        .TryGetValue(
                            dayNumber,
                            out string?
                                attendanceStatus);

                    SetCellValue(
                        worksheet,
                        worksheetRow,
                        column,
                        GetOfficialAttendanceMark(
                            attendanceStatus));

                    if (attendanceStatus == "A" ||
                        attendanceStatus == "E")
                    {
                        absentTotal++;
                    }
                    else
                    {
                        presentTotal++;
                    }
                }

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    39,
                    absentTotal);

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    41,
                    presentTotal);

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    43,
                    learner.Remarks);
            }
        }

        private static void ClearLearnerRow(
            object worksheet,
            int worksheetRow)
        {
            SetCellValue(
                worksheet,
                worksheetRow,
                1,
                string.Empty);

            SetCellValue(
                worksheet,
                worksheetRow,
                3,
                string.Empty);

            foreach (int column
                     in AttendanceColumns)
            {
                SetCellValue(
                    worksheet,
                    worksheetRow,
                    column,
                    string.Empty);
            }

            SetCellValue(
                worksheet,
                worksheetRow,
                39,
                string.Empty);

            SetCellValue(
                worksheet,
                worksheetRow,
                41,
                string.Empty);

            SetCellValue(
                worksheet,
                worksheetRow,
                43,
                string.Empty);
        }

        private static void WriteTotalRows(
            object worksheet,
            IReadOnlyList<SF2LearnerRow> maleLearners,
            IReadOnlyList<SF2LearnerRow> femaleLearners,
            IReadOnlyList<int> recordedDays,
            SF2RowLayout layout)
        {
            WriteGroupTotalRow(
                worksheet,
                layout.MaleTotalRow,
                maleLearners,
                recordedDays);

            WriteGroupTotalRow(
                worksheet,
                layout.FemaleTotalRow,
                femaleLearners,
                recordedDays);

            List<SF2LearnerRow> allLearners =
                maleLearners
                    .Concat(
                        femaleLearners)
                    .ToList();

            WriteGroupTotalRow(
                worksheet,
                layout.CombinedTotalRow,
                allLearners,
                recordedDays);
        }

        private static void WriteGroupTotalRow(
            object worksheet,
            int worksheetRow,
            IReadOnlyList<SF2LearnerRow> learners,
            IReadOnlyList<int> recordedDays)
        {
            SetCellValue(
                worksheet,
                worksheetRow,
                1,
                learners.Count);

            int totalAbsent =
                0;

            int totalPresent =
                0;

            for (int dateIndex = 0;
                 dateIndex <
                    AttendanceColumns.Length;
                 dateIndex++)
            {
                int column =
                    AttendanceColumns[
                        dateIndex];

                if (dateIndex >=
                    recordedDays.Count)
                {
                    SetCellValue(
                        worksheet,
                        worksheetRow,
                        column,
                        string.Empty);

                    continue;
                }

                int dayNumber =
                    recordedDays[
                        dateIndex];

                int presentForDay =
                    0;

                foreach (SF2LearnerRow learner
                         in learners)
                {
                    learner.DailyStatuses
                        .TryGetValue(
                            dayNumber,
                            out string?
                                attendanceStatus);

                    if (attendanceStatus == "A" ||
                        attendanceStatus == "E")
                    {
                        totalAbsent++;
                    }
                    else
                    {
                        presentForDay++;
                        totalPresent++;
                    }
                }

                SetCellValue(
                    worksheet,
                    worksheetRow,
                    column,
                    presentForDay);
            }

            SetCellValue(
                worksheet,
                worksheetRow,
                39,
                totalAbsent);

            SetCellValue(
                worksheet,
                worksheetRow,
                41,
                totalPresent);
        }

        private static void WriteSummaryAndSignatures(
            object worksheet,
            SF2ExportRequest request,
            int rowOffset)
        {
            SetCellValue(
                worksheet,
                51 + rowOffset,
                40,
                request.AdviserName
                    .ToUpperInvariant());

            SetCellValue(
                worksheet,
                57 + rowOffset,
                40,
                request.SchoolHeadName
                    .ToUpperInvariant());

            SetCellValue(
                worksheet,
                26 + rowOffset,
                39,
                $"Month: " +
                $"{request.ReportMonth:MMMM yyyy}");

            int classDayCount =
                request.Learners
                    .SelectMany(
                        learner =>
                            learner.DailyStatuses.Keys)
                    .Distinct()
                    .Count();

            SetCellValue(
                worksheet,
                26 + rowOffset,
                42,
                $"No. of Days of Classes: " +
                $"{classDayCount}");

            List<SF2LearnerRow> maleLearners =
                request.Learners
                    .Where(
                        learner =>
                            IsSex(
                                learner,
                                "Male"))
                    .ToList();

            List<SF2LearnerRow> femaleLearners =
                request.Learners
                    .Where(
                        learner =>
                            IsSex(
                                learner,
                                "Female"))
                    .ToList();

            DateTime reportStart =
                new DateTime(
                    request.ReportMonth.Year,
                    request.ReportMonth.Month,
                    1);

            DateTime reportEnd =
                reportStart
                    .AddMonths(
                        1)
                    .AddDays(
                        -1);

            DateTime enrolmentCutoff =
                GetFirstFridayOfJune(
                    request.AcademicYearStartYear);

            List<SF2LearnerRow> beginningLearners =
                request.Learners
                    .Where(
                        learner =>
                            WasEnrolledOn(
                                learner,
                                enrolmentCutoff))
                    .ToList();

            List<SF2LearnerRow> endOfMonthLearners =
                request.Learners
                    .Where(
                        learner =>
                            WasEnrolledOn(
                                learner,
                                reportEnd))
                    .ToList();

            List<SF2LearnerRow> lateEnrollees =
                request.Learners
                    .Where(
                        learner =>
                            string.Equals(
                                learner.EnrollmentType,
                                "Late Enrollee",
                                StringComparison
                                    .OrdinalIgnoreCase) &&
                            IsDateWithinMonth(
                                learner.EnrollmentDate,
                                reportStart,
                                reportEnd))
                    .ToList();

            List<SF2LearnerRow> transferredIn =
                request.Learners
                    .Where(
                        learner =>
                            string.Equals(
                                learner.EnrollmentType,
                                "Transferred In",
                                StringComparison
                                    .OrdinalIgnoreCase) &&
                            IsDateWithinMonth(
                                learner.EnrollmentDate,
                                reportStart,
                                reportEnd))
                    .ToList();

            List<SF2LearnerRow> transferredOut =
                request.Learners
                    .Where(
                        learner =>
                            string.Equals(
                                learner.ExitReason,
                                "Transferred Out",
                                StringComparison
                                    .OrdinalIgnoreCase) &&
                            IsDateWithinMonth(
                                learner.ExitDate,
                                reportStart,
                                reportEnd))
                    .ToList();

            List<SF2LearnerRow> droppedOut =
                request.Learners
                    .Where(
                        learner =>
                            (string.Equals(
                                 learner.ExitReason,
                                 "Dropped Out",
                                 StringComparison
                                     .OrdinalIgnoreCase) ||
                             string.Equals(
                                 learner.ExitReason,
                                 "Dropped",
                                 StringComparison
                                     .OrdinalIgnoreCase)) &&
                            IsDateWithinMonth(
                                learner.ExitDate,
                                reportStart,
                                reportEnd))
                    .ToList();

            List<SF2LearnerRow> fiveDayAbsentees =
                request.Learners
                    .Where(
                        learner =>
                            HasFiveConsecutiveAbsences(
                                learner))
                    .ToList();

            WriteLearnerCountSummary(
                worksheet,
                28 + rowOffset,
                beginningLearners);

            WriteLearnerCountSummary(
                worksheet,
                30 + rowOffset,
                lateEnrollees);

            WriteLearnerCountSummary(
                worksheet,
                34 + rowOffset,
                endOfMonthLearners);

            WritePercentageSummary(
                worksheet,
                36 + rowOffset,
                GetCount(
                    beginningLearners,
                    "Male"),
                GetCount(
                    beginningLearners,
                    "Female"),
                beginningLearners.Count,
                GetCount(
                    endOfMonthLearners,
                    "Male"),
                GetCount(
                    endOfMonthLearners,
                    "Female"),
                endOfMonthLearners.Count);

            double maleAverageAttendance =
                GetAverageDailyAttendance(
                    maleLearners,
                    classDayCount);

            double femaleAverageAttendance =
                GetAverageDailyAttendance(
                    femaleLearners,
                    classDayCount);

            double totalAverageAttendance =
                GetAverageDailyAttendance(
                    request.Learners,
                    classDayCount);

            WriteDecimalSummary(
                worksheet,
                38 + rowOffset,
                maleAverageAttendance,
                femaleAverageAttendance,
                totalAverageAttendance);

            WriteAttendancePercentageSummary(
                worksheet,
                40 + rowOffset,
                maleAverageAttendance,
                femaleAverageAttendance,
                totalAverageAttendance,
                GetCount(
                    endOfMonthLearners,
                    "Male"),
                GetCount(
                    endOfMonthLearners,
                    "Female"),
                endOfMonthLearners.Count);

            WriteLearnerCountSummary(
                worksheet,
                41 + rowOffset,
                fiveDayAbsentees);

            WriteLearnerCountSummary(
                worksheet,
                42 + rowOffset,
                droppedOut);

            WriteLearnerCountSummary(
                worksheet,
                44 + rowOffset,
                transferredOut);

            WriteLearnerCountSummary(
                worksheet,
                46 + rowOffset,
                transferredIn);
        }

        private static void WriteLearnerCountSummary(
            object worksheet,
            int worksheetRow,
            IReadOnlyCollection<SF2LearnerRow> learners)
        {
            int maleCount =
                GetCount(
                    learners,
                    "Male");

            int femaleCount =
                GetCount(
                    learners,
                    "Female");

            SetCellValue(
                worksheet,
                worksheetRow,
                44,
                maleCount);

            SetCellValue(
                worksheet,
                worksheetRow,
                45,
                femaleCount);

            SetCellValue(
                worksheet,
                worksheetRow,
                46,
                maleCount +
                femaleCount);
        }

        private static void WritePercentageSummary(
            object worksheet,
            int worksheetRow,
            int beginningMale,
            int beginningFemale,
            int beginningTotal,
            int endingMale,
            int endingFemale,
            int endingTotal)
        {
            SetCellValue(
                worksheet,
                worksheetRow,
                44,
                FormatPercentage(
                    endingMale,
                    beginningMale));

            SetCellValue(
                worksheet,
                worksheetRow,
                45,
                FormatPercentage(
                    endingFemale,
                    beginningFemale));

            SetCellValue(
                worksheet,
                worksheetRow,
                46,
                FormatPercentage(
                    endingTotal,
                    beginningTotal));
        }

        private static void WriteDecimalSummary(
            object worksheet,
            int worksheetRow,
            double maleValue,
            double femaleValue,
            double totalValue)
        {
            SetCellValue(
                worksheet,
                worksheetRow,
                44,
                maleValue.ToString(
                    "0.00"));

            SetCellValue(
                worksheet,
                worksheetRow,
                45,
                femaleValue.ToString(
                    "0.00"));

            SetCellValue(
                worksheet,
                worksheetRow,
                46,
                totalValue.ToString(
                    "0.00"));
        }

        private static void
            WriteAttendancePercentageSummary(
                object worksheet,
                int worksheetRow,
                double maleAverage,
                double femaleAverage,
                double totalAverage,
                int maleEnrolment,
                int femaleEnrolment,
                int totalEnrolment)
        {
            SetCellValue(
                worksheet,
                worksheetRow,
                44,
                FormatPercentage(
                    maleAverage,
                    maleEnrolment));

            SetCellValue(
                worksheet,
                worksheetRow,
                45,
                FormatPercentage(
                    femaleAverage,
                    femaleEnrolment));

            SetCellValue(
                worksheet,
                worksheetRow,
                46,
                FormatPercentage(
                    totalAverage,
                    totalEnrolment));
        }

        private static int GetCount(
            IEnumerable<SF2LearnerRow> learners,
            string sex)
        {
            return learners.Count(
                learner =>
                    IsSex(
                        learner,
                        sex));
        }

        private static bool IsSex(
            SF2LearnerRow learner,
            string sex)
        {
            return string.Equals(
                learner.Sex,
                sex,
                StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime
            GetFirstFridayOfJune(
                int schoolYearStartYear)
        {
            DateTime firstDayOfJune =
                new DateTime(
                    schoolYearStartYear,
                    6,
                    1);

            int daysUntilFriday =
                ((int)DayOfWeek.Friday -
                 (int)firstDayOfJune.DayOfWeek +
                 7) % 7;

            return firstDayOfJune.AddDays(
                daysUntilFriday);
        }

        private static bool WasEnrolledOn(
            SF2LearnerRow learner,
            DateTime selectedDate)
        {
            bool alreadyEnrolled =
                !learner.EnrollmentDate.HasValue ||
                learner.EnrollmentDate.Value.Date <=
                    selectedDate.Date;

            bool notYetExited =
                !learner.ExitDate.HasValue ||
                learner.ExitDate.Value.Date >
                    selectedDate.Date;

            return alreadyEnrolled &&
                   notYetExited;
        }

        private static bool IsDateWithinMonth(
            DateTime? selectedDate,
            DateTime reportStart,
            DateTime reportEnd)
        {
            return selectedDate.HasValue &&
                   selectedDate.Value.Date >=
                       reportStart.Date &&
                   selectedDate.Value.Date <=
                       reportEnd.Date;
        }

        private static double
            GetAverageDailyAttendance(
                IEnumerable<SF2LearnerRow> learners,
                int classDayCount)
        {
            if (classDayCount <= 0)
            {
                return 0;
            }

            int totalPresent =
                learners.Sum(
                    learner =>
                        learner.DailyStatuses.Values
                            .Count(
                                status =>
                                    status != "A" &&
                                    status != "E"));

            return Math.Round(
                (double)totalPresent /
                classDayCount,
                2);
        }

        private static bool
            HasFiveConsecutiveAbsences(
                SF2LearnerRow learner)
        {
            int consecutiveAbsences =
                0;

            foreach (KeyValuePair<int, string> entry
                     in learner.DailyStatuses
                         .OrderBy(
                             entry =>
                                 entry.Key))
            {
                if (entry.Value == "A" ||
                    entry.Value == "E")
                {
                    consecutiveAbsences++;

                    if (consecutiveAbsences >= 5)
                    {
                        return true;
                    }
                }
                else
                {
                    consecutiveAbsences =
                        0;
                }
            }

            return false;
        }

        private static string FormatPercentage(
            double numerator,
            double denominator)
        {
            if (denominator <= 0)
            {
                return string.Empty;
            }

            double percentage =
                numerator /
                denominator *
                100;

            return $"{percentage:0.00}%";
        }

        private static string
            GetOfficialAttendanceMark(
                string? attendanceStatus)
        {
            return attendanceStatus switch
            {
                "A" =>
                    "x",

                "L" =>
                    "L",

                "E" =>
                    "x",

                // Present is blank in official SF2.
                "P" =>
                    string.Empty,

                _ =>
                    string.Empty
            };
        }

        private static string GetWeekdayCode(
            DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday =>
                    "M",

                DayOfWeek.Tuesday =>
                    "T",

                DayOfWeek.Wednesday =>
                    "W",

                DayOfWeek.Thursday =>
                    "TH",

                DayOfWeek.Friday =>
                    "F",

                DayOfWeek.Saturday =>
                    "SAT",

                DayOfWeek.Sunday =>
                    "SUN",

                _ =>
                    string.Empty
            };
        }

        private sealed class SF2RowLayout
        {
            public int MaleFirstRow
            {
                get;
                init;
            }

            public int MaleSlots
            {
                get;
                init;
            }

            public int MaleTotalRow
            {
                get;
                init;
            }

            public int FemaleFirstRow
            {
                get;
                init;
            }

            public int FemaleSlots
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

            public int RowOffset
            {
                get;
                init;
            }
        }
    }
}
