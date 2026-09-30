using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public class DashboardViewModel :
        ViewModelBase,
        IDisposable
    {
        private readonly IClockService
            _clockService;

        private readonly IDashboardSummaryService
            _dashboardSummaryService;

        private string _teacherName =
            "Teacher";

        private string _schoolName =
            "Complete the School Setup";

        private string _schoolId =
            "School ID not set";

        private string _schoolLogoPath =
            string.Empty;

        private string _schoolYear =
            "School Year not set";

        private string _gradeAndSection =
            "No class selected";

        private DashboardClassOption?
            _selectedClassOption;

        private string? _selectedGradeLevel;
        private readonly List<DashboardClassOption> _allClasses = new();

        private int _numberOfLearners;

        private int _maleLearners;

        private int _femaleLearners;

        private int _presentToday;

        private int _absentToday;

        private string _averageClassPerformance =
            "—";

        private string _readingPerformance =
            "No records yet";

        private string _currentTime =
            string.Empty;

        private string _currentDate =
            string.Empty;

        public DashboardViewModel(
            IClockService clockService,
            IDashboardSummaryService
                dashboardSummaryService)
        {
            _clockService =
                clockService;

            _dashboardSummaryService =
                dashboardSummaryService;

            AvailableClasses =
                new ObservableCollection<
                    DashboardClassOption>();

            AvailableGradeLevels = new ObservableCollection<string>();

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadSummaryAsync);

            _clockService.TimeChanged +=
                ClockService_TimeChanged;

            UpdateDateAndTime();

            _clockService.Start();

            _ = LoadSummaryAsync();
        }

        public ObservableCollection<
            DashboardClassOption>
                AvailableClasses
        {
            get;
        }

        public ObservableCollection<string> AvailableGradeLevels { get; }

        public string? SelectedGradeLevel
        {
            get => _selectedGradeLevel;
            set
            {
                if (SetProperty(ref _selectedGradeLevel, value))
                    ApplyGradeFilter();
            }
        }

        public IAsyncRelayCommand RefreshCommand
        {
            get;
        }

        public string TeacherName
        {
            get => _teacherName;

            private set => SetProperty(
                ref _teacherName,
                value);
        }

        public string SchoolName
        {
            get => _schoolName;

            private set => SetProperty(
                ref _schoolName,
                value);
        }

        public string SchoolId
        {
            get => _schoolId;

            private set => SetProperty(
                ref _schoolId,
                value);
        }

        public string SchoolLogoPath
        {
            get => _schoolLogoPath;

            private set => SetProperty(
                ref _schoolLogoPath,
                value);
        }

        public string SchoolYear
        {
            get => _schoolYear;

            private set => SetProperty(
                ref _schoolYear,
                value);
        }

        public string GradeAndSection
        {
            get => _gradeAndSection;

            private set => SetProperty(
                ref _gradeAndSection,
                value);
        }

        public DashboardClassOption?
            SelectedClassOption
        {
            get => _selectedClassOption;

            set
            {
                if (!SetProperty(
                        ref _selectedClassOption,
                        value))
                {
                    return;
                }

                ApplySelectedClass();
            }
        }

        public int NumberOfLearners
        {
            get => _numberOfLearners;

            private set => SetProperty(
                ref _numberOfLearners,
                value);
        }

        public int MaleLearners
        {
            get => _maleLearners;

            private set => SetProperty(
                ref _maleLearners,
                value);
        }

        public int FemaleLearners
        {
            get => _femaleLearners;

            private set => SetProperty(
                ref _femaleLearners,
                value);
        }

        public int PresentToday
        {
            get => _presentToday;

            private set => SetProperty(
                ref _presentToday,
                value);
        }

        public int AbsentToday
        {
            get => _absentToday;

            private set => SetProperty(
                ref _absentToday,
                value);
        }

        public string AverageClassPerformance
        {
            get => _averageClassPerformance;

            private set => SetProperty(
                ref _averageClassPerformance,
                value);
        }

        public string ReadingPerformance
        {
            get => _readingPerformance;

            private set => SetProperty(
                ref _readingPerformance,
                value);
        }

        public string CurrentTime
        {
            get => _currentTime;

            private set => SetProperty(
                ref _currentTime,
                value);
        }

        public string CurrentDate
        {
            get => _currentDate;

            private set => SetProperty(
                ref _currentDate,
                value);
        }

        public void Dispose()
        {
            _clockService.TimeChanged -=
                ClockService_TimeChanged;

            _clockService.Stop();

            GC.SuppressFinalize(
                this);
        }

        private async Task LoadSummaryAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Loading dashboard...";

                int? previouslySelectedClassId =
                    SelectedClassOption?
                        .SchoolClassId;
                string? previouslySelectedGradeLevel = SelectedGradeLevel;

                DashboardSummary summary =
                    await _dashboardSummaryService
                        .LoadAsync();

                TeacherName =
                    summary.TeacherName;

                SchoolName =
                    summary.SchoolName;

                SchoolId =
                    summary.SchoolId;

                SchoolLogoPath =
                    summary.SchoolLogoPath;

                SchoolYear =
                    summary.SchoolYear;

                PresentToday =
                    summary.PresentToday;

                AbsentToday =
                    summary.AbsentToday;

                AverageClassPerformance =
                    summary.AverageClassPerformance;

                ReadingPerformance =
                    summary.ReadingPerformance;

                _allClasses.Clear();
                _allClasses.AddRange(summary.AvailableClasses);
                AvailableGradeLevels.Clear();
                foreach (string grade in _allClasses
                             .Select(option => option.GradeLevel)
                             .Distinct(StringComparer.OrdinalIgnoreCase))
                    AvailableGradeLevels.Add(grade);

                _selectedGradeLevel = _allClasses.FirstOrDefault(option =>
                        option.SchoolClassId == previouslySelectedClassId)?.GradeLevel
                    ?? AvailableGradeLevels.FirstOrDefault(grade =>
                        string.Equals(grade, previouslySelectedGradeLevel,
                            StringComparison.OrdinalIgnoreCase))
                    ?? _allClasses.FirstOrDefault(option =>
                        option.SchoolClassId == summary.DefaultSchoolClassId)?.GradeLevel
                    ?? AvailableGradeLevels.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedGradeLevel));
                ApplyGradeFilter(previouslySelectedClassId,
                    summary.DefaultSchoolClassId);

                if (SelectedClassOption == null)
                {
                    GradeAndSection =
                        "No class selected";

                    NumberOfLearners =
                        0;

                    MaleLearners =
                        0;

                    FemaleLearners =
                        0;
                }

                StatusMessage =
                    "Dashboard updated.";
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "Dashboard could not be updated: " +
                    exception.Message;
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        public string PresentTodayText => SelectedClassOption?.PresentToday?.ToString() ?? "—";
        public string AbsentTodayText => SelectedClassOption?.AbsentToday?.ToString() ?? "—";
        public string ClassRoleText => SelectedClassOption?.IsAdvisoryClass == true
            ? "Enrolled in advisory class" : "Enrolled in selected class";

        private void ApplyGradeFilter(int? selectedClassId = null,
            int? defaultClassId = null)
        {
            int? currentId = selectedClassId ?? SelectedClassOption?.SchoolClassId;
            AvailableClasses.Clear();
            foreach (DashboardClassOption option in _allClasses.Where(option =>
                         string.Equals(option.GradeLevel, SelectedGradeLevel,
                             StringComparison.OrdinalIgnoreCase)))
                AvailableClasses.Add(option);

            SelectedClassOption = AvailableClasses.FirstOrDefault(option =>
                    option.SchoolClassId == currentId)
                ?? AvailableClasses.FirstOrDefault(option =>
                    option.SchoolClassId == defaultClassId)
                ?? AvailableClasses.FirstOrDefault();
            ApplySelectedClass();
        }

        private void ApplySelectedClass()
        {
            OnPropertyChanged(nameof(PresentTodayText));
            OnPropertyChanged(nameof(AbsentTodayText));
            OnPropertyChanged(nameof(ClassRoleText));
            if (SelectedClassOption == null)
            {
                GradeAndSection =
                    "No class selected";

                NumberOfLearners =
                    0;

                MaleLearners =
                    0;

                FemaleLearners =
                    0;

                return;
            }

            GradeAndSection =
                SelectedClassOption.DisplayName;

            NumberOfLearners =
                SelectedClassOption
                    .NumberOfLearners;

            MaleLearners =
                SelectedClassOption
                    .MaleLearners;

            FemaleLearners =
                SelectedClassOption
                    .FemaleLearners;

            StatusMessage =
                "Dashboard class changed to " +
                SelectedClassOption.DisplayName +
                ".";
        }

        private void ClockService_TimeChanged(
            object? sender,
            EventArgs e)
        {
            UpdateDateAndTime();
        }

        private void UpdateDateAndTime()
        {
            DateTime currentDateTime =
                _clockService.CurrentDateTime;

            CurrentTime =
                currentDateTime.ToString(
                    "hh:mm:ss tt");

            CurrentDate =
                currentDateTime.ToString(
                    "dddd, MMMM dd, yyyy");
        }
    }
}
