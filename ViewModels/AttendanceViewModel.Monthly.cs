using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class AttendanceViewModel
    {
        private DateTime _selectedMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

        private int _monthlyClassDays;

        private int _monthlyMaleLearners;

        private int _monthlyFemaleLearners;

        public ObservableCollection<
            MonthlyAttendanceRow>
                MonthlyRows
        {
            get;
        } = new ObservableCollection<
            MonthlyAttendanceRow>();

        public DateTime SelectedMonth
        {
            get => _selectedMonth;

            set
            {
                DateTime monthValue =
                    new DateTime(
                        value.Year,
                        value.Month,
                        1);

                if (SetProperty(
                        ref _selectedMonth,
                        monthValue))
                {
                    OnPropertyChanged(
                        nameof(
                            SelectedMonthText));
                }
            }
        }

        public string SelectedMonthText =>
            SelectedMonth.ToString(
                "MMMM yyyy");

        public int MonthlyClassDays
        {
            get => _monthlyClassDays;

            private set => SetProperty(
                ref _monthlyClassDays,
                value);
        }

        public int MonthlyMaleLearners
        {
            get => _monthlyMaleLearners;

            private set => SetProperty(
                ref _monthlyMaleLearners,
                value);
        }

        public int MonthlyFemaleLearners
        {
            get => _monthlyFemaleLearners;

            private set => SetProperty(
                ref _monthlyFemaleLearners,
                value);
        }

        [RelayCommand]
        private async Task LoadMonthlySummaryAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Monthly Attendance");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Preparing monthly attendance summary...";

                IReadOnlyList<AttendanceDay>
                    monthDays =
                        await _attendanceRepository
                            .GetMonthDaysAsync(
                                SelectedClass.Id,
                                SelectedMonth.Year,
                                SelectedMonth.Month);

                List<AttendanceDay>
                    classDays =
                        monthDays
                            .Where(
                                day =>
                                    day.DayStatus ==
                                        "Regular Class Day" ||
                                    day.DayStatus ==
                                        "Make-up Class")
                            .ToList();

                HashSet<int> classDayIds =
                    classDays
                        .Select(
                            day =>
                                day.Id)
                        .ToHashSet();

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                IReadOnlyList<LearnerAttendance>
                    attendanceRecords =
                        await _attendanceRepository
                            .GetMonthAttendancesAsync(
                                SelectedClass.Id,
                                SelectedMonth.Year,
                                SelectedMonth.Month);

                List<LearnerAttendance>
                    validAttendanceRecords =
                        attendanceRecords
                            .Where(
                                record =>
                                    classDayIds.Contains(
                                        record.AttendanceDayId))
                            .ToList();

                MonthlyRows.Clear();

                int number =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    List<LearnerAttendance>
                        learnerRecords =
                            validAttendanceRecords
                                .Where(
                                    record =>
                                        record.LearnerId ==
                                            learner.Id)
                                .ToList();

                    MonthlyAttendanceRow row =
                        new MonthlyAttendanceRow
                        {
                            Number =
                                number++,

                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
                                learner.FullName,

                            Sex =
                                learner.Sex,

                            TotalClassDays =
                                classDays.Count,

                            PresentDays =
                                learnerRecords.Count(
                                    record =>
                                        record.AttendanceStatus ==
                                            "Present"),

                            AbsentDays =
                                learnerRecords.Count(
                                    record =>
                                        record.AttendanceStatus ==
                                            "Absent"),

                            LateDays =
                                learnerRecords.Count(
                                    record =>
                                        record.AttendanceStatus ==
                                            "Late"),

                            ExcusedDays =
                                learnerRecords.Count(
                                    record =>
                                        record.AttendanceStatus ==
                                            "Excused")
                        };

                    foreach (LearnerAttendance record
                             in learnerRecords)
                    {
                        if (record.AttendanceDay ==
                            null)
                        {
                            continue;
                        }

                        int dayNumber =
                            record.AttendanceDay
                                .AttendanceDate.Day;

                        row.DailyStatuses[dayNumber] =
                            GetAttendanceMark(
                                record.AttendanceStatus);
                    }

                    MonthlyRows.Add(
                        row);
                }

                MonthlyClassDays =
                    classDays.Count;

                MonthlyMaleLearners =
                    learners.Count(
                        learner =>
                            learner.Sex ==
                                "Male");

                MonthlyFemaleLearners =
                    learners.Count(
                        learner =>
                            learner.Sex ==
                                "Female");

                StatusMessage =
                    $"{SelectedMonthText} monthly summary loaded.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not prepare the " +
                    $"monthly attendance summary.\n\n" +
                    $"{errorMessage}",
                    "Monthly Attendance Error");
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private static string GetAttendanceMark(
            string attendanceStatus)
        {
            return attendanceStatus switch
            {
                "Present" =>
                    "P",

                "Absent" =>
                    "A",

                "Late" =>
                    "L",

                "Excused" =>
                    "E",

                _ =>
                    string.Empty
            };
        }
    }
}