using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class AttendanceViewModel :
     ViewModelBase
    {
        private readonly ISchoolRepository
            _schoolRepository;

        private readonly IAcademicYearRepository
            _academicYearRepository;

        private readonly ISchoolClassRepository
            _schoolClassRepository;

        private readonly ILearnerRepository
            _learnerRepository;

        private readonly IAttendanceRepository
            _attendanceRepository;
        private readonly ITeacherRepository
    _teacherRepository;

        private readonly IDialogService
            _dialogService;
        private readonly ISF2ExportService
    _sf2ExportService;

        private SchoolClass?
            _selectedClass;

        private DateTime _selectedDate =
            DateTime.Today;

        private string _dayStatus =
            "Regular Class Day";

        private string _dayDescription =
            string.Empty;

        public AttendanceViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository academicYearRepository,
            ISchoolClassRepository schoolClassRepository,
            ILearnerRepository learnerRepository,
           IAttendanceRepository attendanceRepository,
ITeacherRepository teacherRepository,
IDialogService dialogService,
ISF2ExportService sf2ExportService)
        {
            _schoolRepository =
                schoolRepository;

            _academicYearRepository =
                academicYearRepository;

            _schoolClassRepository =
                schoolClassRepository;

            _learnerRepository =
                learnerRepository;

            _attendanceRepository =
                attendanceRepository;
            _teacherRepository =
    teacherRepository;

            _dialogService =
                dialogService;
            _sf2ExportService =
    sf2ExportService;

            Classes =
                new ObservableCollection<SchoolClass>();

            LearnerRows =
                new ObservableCollection<
                    AttendanceLearnerRow>();

            DayStatusOptions =
                new[]
                {
                    "Regular Class Day",
                    "Holiday",
                    "Class Suspended",
                    "No Classes / School Activity",
                    "Weekend",
                    "Make-up Class"
                };

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            TodayCommand =
                new RelayCommand(
                    SelectToday);

            SaveAttendanceCommand =
                new AsyncRelayCommand(
                    SaveAttendanceAsync,
                    CanSaveAttendance);

            MarkAllPresentCommand =
                new RelayCommand(
                    MarkAllPresent,
                    CanEditLearnerAttendance);

            ClearAttendanceCommand =
                new RelayCommand(
                    ClearAttendance,
                    CanEditLearnerAttendance);

            MarkPresentCommand =
                new RelayCommand<
                    AttendanceLearnerRow>(
                    row =>
                        SetLearnerStatus(
                            row,
                            "Present"));

            MarkAbsentCommand =
                new RelayCommand<
                    AttendanceLearnerRow>(
                    row =>
                        SetLearnerStatus(
                            row,
                            "Absent"));

            MarkLateCommand =
                new RelayCommand<
                    AttendanceLearnerRow>(
                    row =>
                        SetLearnerStatus(
                            row,
                            "Late"));

            MarkExcusedCommand =
                new RelayCommand<
                    AttendanceLearnerRow>(
                    row =>
                        SetLearnerStatus(
                            row,
                            "Excused"));

            _ = LoadAsync();
        }

        public ObservableCollection<SchoolClass>
            Classes
        {
            get;
        }

        public ObservableCollection<
            AttendanceLearnerRow>
                LearnerRows
        {
            get;
        }

        public IReadOnlyList<string>
            DayStatusOptions
        {
            get;
        }

        public IAsyncRelayCommand
            RefreshCommand
        {
            get;
        }

        public IRelayCommand
            TodayCommand
        {
            get;
        }

        public IAsyncRelayCommand
            SaveAttendanceCommand
        {
            get;
        }

        public IRelayCommand
            MarkAllPresentCommand
        {
            get;
        }

        public IRelayCommand
            ClearAttendanceCommand
        {
            get;
        }

        public IRelayCommand<
            AttendanceLearnerRow>
                MarkPresentCommand
        {
            get;
        }

        public IRelayCommand<
            AttendanceLearnerRow>
                MarkAbsentCommand
        {
            get;
        }

        public IRelayCommand<
            AttendanceLearnerRow>
                MarkLateCommand
        {
            get;
        }

        public IRelayCommand<
            AttendanceLearnerRow>
                MarkExcusedCommand
        {
            get;
        }

        public SchoolClass? SelectedClass
        {
            get => _selectedClass;

            set
            {
                if (SetProperty(
                        ref _selectedClass,
                        value))
                {
                    NotifyCommandStates();

                    _ = LoadAttendanceAsync();
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;

            set
            {
                DateTime selectedValue =
                    value.Date;

                if (SetProperty(
                        ref _selectedDate,
                        selectedValue))
                {
                    _ = LoadAttendanceAsync();
                }
            }
        }

        public string DayStatus
        {
            get => _dayStatus;

            set
            {
                if (SetProperty(
                        ref _dayStatus,
                        value))
                {
                    OnPropertyChanged(
                        nameof(
                            CanRecordLearnerAttendance));

                    NotifyCommandStates();
                }
            }
        }

        public string DayDescription
        {
            get => _dayDescription;

            set => SetProperty(
                ref _dayDescription,
                value);
        }

        public bool CanRecordLearnerAttendance =>
            DayStatus == "Regular Class Day" ||
            DayStatus == "Make-up Class";

        public int TotalLearners =>
            LearnerRows.Count;

        public int PresentCount =>
            LearnerRows.Count(
                row =>
                    row.AttendanceStatus ==
                        "Present");

        public int AbsentCount =>
            LearnerRows.Count(
                row =>
                    row.AttendanceStatus ==
                        "Absent");

        public int LateCount =>
            LearnerRows.Count(
                row =>
                    row.AttendanceStatus ==
                        "Late");

        public int ExcusedCount =>
            LearnerRows.Count(
                row =>
                    row.AttendanceStatus ==
                        "Excused");

        private async Task LoadAsync()
        {
            if (IsBusy)
            {
                return;
            }

            bool loadAttendanceAfter =
                false;

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Loading attendance classes...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    Classes.Clear();
                    LearnerRows.Clear();

                    StatusMessage =
                        "Complete School Setup first.";

                    return;
                }

                AcademicYear? academicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            school.Id);

                if (academicYear == null)
                {
                    Classes.Clear();
                    LearnerRows.Clear();

                    StatusMessage =
                        "Create the current School Year first.";

                    return;
                }

                IReadOnlyList<SchoolClass>
                    classes =
                        await _schoolClassRepository
                            .GetByAcademicYearAsync(
                                school.Id,
                                academicYear.Id);

                Classes.Clear();

                foreach (SchoolClass schoolClass
                         in classes)
                {
                    Classes.Add(
                        schoolClass);
                }

                SelectedClass =
                    Classes.FirstOrDefault();

                loadAttendanceAfter =
                    SelectedClass != null;

                StatusMessage =
                    SelectedClass == null
                        ? "Create a class first."
                        : "Select a date and record attendance.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"attendance classes.\n\n" +
                    $"{exception.Message}",
                    "Attendance Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }

            if (loadAttendanceAfter)
            {
                await LoadAttendanceAsync();
            }
        }

        private async Task LoadAttendanceAsync()
        {
            if (IsBusy ||
                SelectedClass == null)
            {
                if (SelectedClass == null)
                {
                    LearnerRows.Clear();

                    NotifySummary();
                }

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Loading daily attendance...";

                AttendanceDay? attendanceDay =
                    await _attendanceRepository
                        .GetDayAsync(
                            SelectedClass.Id,
                            SelectedDate);

                if (attendanceDay == null)
                {
                    DayStatus =
                        SelectedDate.DayOfWeek ==
                            DayOfWeek.Saturday ||
                        SelectedDate.DayOfWeek ==
                            DayOfWeek.Sunday
                            ? "Weekend"
                            : "Regular Class Day";

                    DayDescription =
                        string.Empty;
                }
                else
                {
                    DayStatus =
                        attendanceDay.DayStatus;

                    DayDescription =
                        attendanceDay.Description;
                }

                IReadOnlyList<Learner> learners =
                    await _learnerRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

                IReadOnlyList<LearnerAttendance>
                    savedAttendances =
                        attendanceDay == null
                            ? Array.Empty<
                                LearnerAttendance>()
                            : await _attendanceRepository
                                .GetLearnerAttendancesAsync(
                                    attendanceDay.Id);

                Dictionary<int, LearnerAttendance>
                    attendanceByLearner =
                        savedAttendances.ToDictionary(
                            record =>
                                record.LearnerId);

                LearnerRows.Clear();

                int number =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    attendanceByLearner.TryGetValue(
                        learner.Id,
                        out LearnerAttendance?
                            savedAttendance);

                    LearnerRows.Add(
                        new AttendanceLearnerRow
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

                            AttendanceStatus =
                                savedAttendance?
                                    .AttendanceStatus
                                ?? "Present",

                            Remarks =
                                savedAttendance?
                                    .Remarks
                                ?? string.Empty
                        });
                }

                StatusMessage =
                    attendanceDay == null
                        ? "New attendance record."
                        : "Saved attendance record loaded.";

                NotifySummary();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"daily attendance.\n\n" +
                    $"{exception.Message}",
                    "Attendance Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task SaveAttendanceAsync()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Attendance Class Required");

                return;
            }

            if (CanRecordLearnerAttendance &&
                LearnerRows.Any(
                    row =>
                        string.IsNullOrWhiteSpace(
                            row.AttendanceStatus)))
            {
                _dialogService.ShowWarning(
                    "Select an attendance status for every learner.",
                    "Incomplete Attendance");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                AttendanceDay attendanceDay =
                    await _attendanceRepository
                        .SaveDayAsync(
                            new AttendanceDay
                            {
                                SchoolClassId =
                                    SelectedClass.Id,

                                AttendanceDate =
                                    SelectedDate,

                                DayStatus =
                                    DayStatus,

                                Description =
                                    DayDescription.Trim()
                            });

                if (CanRecordLearnerAttendance)
                {
                    List<LearnerAttendance>
                        learnerAttendances =
                            LearnerRows
                                .Select(
                                    row =>
                                        new LearnerAttendance
                                        {
                                            AttendanceDayId =
                                                attendanceDay.Id,

                                            LearnerId =
                                                row.LearnerId,

                                            AttendanceStatus =
                                                row.AttendanceStatus,

                                            Remarks =
                                                row.Remarks.Trim()
                                        })
                                .ToList();

                    await _attendanceRepository
                        .SaveLearnerAttendancesAsync(
                            attendanceDay.Id,
                            learnerAttendances);
                }

                StatusMessage =
                    $"{SelectedDate:MMM dd, yyyy} attendance saved successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"attendance.\n\n" +
                    $"{errorMessage}",
                    "Save Attendance Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private bool CanSaveAttendance()
        {
            return !IsBusy &&
                   SelectedClass != null;
        }

        private bool CanEditLearnerAttendance()
        {
            return !IsBusy &&
                   SelectedClass != null &&
                   CanRecordLearnerAttendance &&
                   LearnerRows.Count > 0;
        }

        private void SelectToday()
        {
            SelectedDate =
                DateTime.Today;
        }

        private void MarkAllPresent()
        {
            foreach (AttendanceLearnerRow row
                     in LearnerRows)
            {
                row.AttendanceStatus =
                    "Present";
            }

            NotifySummary();
        }

        private void ClearAttendance()
        {
            foreach (AttendanceLearnerRow row
                     in LearnerRows)
            {
                row.AttendanceStatus =
                    string.Empty;

                row.Remarks =
                    string.Empty;
            }

            NotifySummary();
        }

        private void SetLearnerStatus(
            AttendanceLearnerRow? row,
            string attendanceStatus)
        {
            if (row == null ||
                !CanRecordLearnerAttendance)
            {
                return;
            }

            row.AttendanceStatus =
                attendanceStatus;

            NotifySummary();
        }

        private void NotifySummary()
        {
            OnPropertyChanged(
                nameof(TotalLearners));

            OnPropertyChanged(
                nameof(PresentCount));

            OnPropertyChanged(
                nameof(AbsentCount));

            OnPropertyChanged(
                nameof(LateCount));

            OnPropertyChanged(
                nameof(ExcusedCount));
        }

        private void NotifyCommandStates()
        {
            SaveAttendanceCommand
                .NotifyCanExecuteChanged();

            MarkAllPresentCommand
                .NotifyCanExecuteChanged();

            ClearAttendanceCommand
                .NotifyCanExecuteChanged();
        }
    }
}