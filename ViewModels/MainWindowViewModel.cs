using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Services;
using TeachFlex.Repositories;

namespace TeachFlex.ViewModels
{
    public partial class MainWindowViewModel :
    ViewModelBase
    {
        private readonly NavigationStore
            _navigationStore;

        private readonly IClockService
            _clockService;

        private readonly ISchoolRepository
            _schoolRepository;

        private readonly IAcademicYearRepository
            _academicYearRepository;
        private readonly Func<
    DashboardViewModel>
        _dashboardViewModelFactory;
        private readonly Func<
    SchoolSetupViewModel>
        _schoolSetupViewModelFactory;
        private readonly Func<
    MyClassesViewModel>
        _myClassesViewModelFactory;
        private readonly Func<
    LearnersViewModel>
        _learnersViewModelFactory;
        private readonly Func<
    SubjectSetupViewModel>
        _subjectSetupViewModelFactory;
        private readonly Func<
    AttendanceViewModel>
        _attendanceViewModelFactory;
        private readonly Func<
    SchoolFormsViewModel>
        _schoolFormsViewModelFactory;
        private readonly Func<
    EClassRecordViewModel>
        _eClassRecordViewModelFactory;
        private readonly Func<
    GradesViewModel>
        _gradesViewModelFactory;

        private readonly Func<LessonPlannerViewModel> _lessonPlannerViewModelFactory;
        private readonly Func<ActivitySheetViewModel> _activitySheetViewModelFactory;
        private readonly Func<ReadingTrackerViewModel> _readingTrackerViewModelFactory;
        private readonly Func<ReportsViewModel> _reportsViewModelFactory;

        private readonly Func<
            BackupRestoreViewModel>
                _backupRestoreViewModelFactory;

        private string _pageTitle =
            "Dashboard";

        private string _currentPageKey =
            "Dashboard";

        private double _sidebarWidth =
            260;

        private bool _isSidebarExpanded =
            true;

        private double _zoomScale =
            0.80;

        private string _activeSchoolYearText =
            "NO ACTIVE SCHOOL YEAR";

        public MainWindowViewModel(
     NavigationStore navigationStore,
     IClockService clockService,
     ISchoolRepository schoolRepository,
     ITeacherRepository teacherRepository,
     IAcademicYearRepository academicYearRepository,
     Func<DashboardViewModel>
         dashboardViewModelFactory,
     Func<SchoolSetupViewModel>
        schoolSetupViewModelFactory,
   Func<MyClassesViewModel>
    myClassesViewModelFactory,
Func<LearnersViewModel>
    learnersViewModelFactory,
Func<SubjectSetupViewModel>
    subjectSetupViewModelFactory,
Func<AttendanceViewModel>
    attendanceViewModelFactory,
Func<SchoolFormsViewModel>
    schoolFormsViewModelFactory,
Func<EClassRecordViewModel>
    eClassRecordViewModelFactory,
Func<GradesViewModel>
    gradesViewModelFactory,
Func<LessonPlannerViewModel> lessonPlannerViewModelFactory,
Func<ActivitySheetViewModel> activitySheetViewModelFactory,
Func<ReadingTrackerViewModel> readingTrackerViewModelFactory,
Func<ReportsViewModel> reportsViewModelFactory,
Func<BackupRestoreViewModel>
    backupRestoreViewModelFactory)
        {
            _navigationStore =
                navigationStore;

            _clockService =
                clockService;

            _schoolRepository =
                schoolRepository;

            _academicYearRepository =
                academicYearRepository;

            _academicYearRepository
                .CurrentAcademicYearChanged +=
                    AcademicYearRepository_CurrentAcademicYearChanged;

            ConfigureTeacherProfile(
    schoolRepository,
    teacherRepository);
            _dashboardViewModelFactory =
    dashboardViewModelFactory;
            _schoolSetupViewModelFactory =
    schoolSetupViewModelFactory;
            _myClassesViewModelFactory =
    myClassesViewModelFactory;
            _learnersViewModelFactory =
    learnersViewModelFactory;
            _subjectSetupViewModelFactory =
    subjectSetupViewModelFactory;
            _attendanceViewModelFactory =
    attendanceViewModelFactory;
            _schoolFormsViewModelFactory =
    schoolFormsViewModelFactory;
            _eClassRecordViewModelFactory =
    eClassRecordViewModelFactory;
            _gradesViewModelFactory =
    gradesViewModelFactory;
            _lessonPlannerViewModelFactory = lessonPlannerViewModelFactory;
            _activitySheetViewModelFactory = activitySheetViewModelFactory;
            _readingTrackerViewModelFactory = readingTrackerViewModelFactory;
            _reportsViewModelFactory = reportsViewModelFactory;

            _backupRestoreViewModelFactory =
                backupRestoreViewModelFactory;

            NavigateCommand =
                new RelayCommand<string>(
                    Navigate);

            ToggleSidebarCommand =
                new RelayCommand(
                    ToggleSidebar);

            ZoomInCommand =
                new RelayCommand(
                    ZoomIn);

            ZoomOutCommand =
                new RelayCommand(
                    ZoomOut);

            ResetZoomCommand =
                new RelayCommand(
                    ResetZoom);

            _navigationStore.PropertyChanged +=
                NavigationStore_PropertyChanged;

            Navigate(
                "Dashboard");

            _ = LoadActiveSchoolYearAsync();
        }

        public ViewModelBase?
            CurrentViewModel =>
                _navigationStore
                    .CurrentViewModel;

        public IRelayCommand<string>
            NavigateCommand
        {
            get;
        }

        public IRelayCommand
            ToggleSidebarCommand
        {
            get;
        }

        public IRelayCommand
            ZoomInCommand
        {
            get;
        }

        public IRelayCommand
            ZoomOutCommand
        {
            get;
        }

        public IRelayCommand
            ResetZoomCommand
        {
            get;
        }

        public string PageTitle
        {
            get => _pageTitle;

            private set => SetProperty(
                ref _pageTitle,
                value);
        }

        public string CurrentPageKey
        {
            get => _currentPageKey;

            private set => SetProperty(
                ref _currentPageKey,
                value);
        }

        public double SidebarWidth
        {
            get => _sidebarWidth;

            private set => SetProperty(
                ref _sidebarWidth,
                value);
        }

        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;

            private set => SetProperty(
                ref _isSidebarExpanded,
                value);
        }

        public double ZoomScale
        {
            get => _zoomScale;

            private set => SetProperty(
                ref _zoomScale,
                value);
        }

        public string ZoomText =>
            $"{Math.Round(
                ZoomScale * 100)}%";

        public string ActiveSchoolYearText
        {
            get => _activeSchoolYearText;

            private set => SetProperty(
                ref _activeSchoolYearText,
                value);
        }

        private async Task LoadActiveSchoolYearAsync()
        {
            try
            {
                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    ActiveSchoolYearText =
                        "NO ACTIVE SCHOOL YEAR";

                    return;
                }

                AcademicYear? academicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            school.Id);

                ActiveSchoolYearText =
                    academicYear == null
                        ? "NO ACTIVE SCHOOL YEAR"
                        : $"ACTIVE SCHOOL YEAR: {academicYear.DisplayName}";
            }
            catch
            {
                ActiveSchoolYearText =
                    "SCHOOL YEAR UNAVAILABLE";
            }
        }

        private void AcademicYearRepository_CurrentAcademicYearChanged(
            AcademicYear academicYear)
        {
            ActiveSchoolYearText =
                $"ACTIVE SCHOOL YEAR: {academicYear.DisplayName}";
        }

        private void Navigate(
            string? pageKey)
        {
            if (string.IsNullOrWhiteSpace(
                    pageKey))
            {
                return;
            }

            ViewModelBase destination =
                CreateDestination(
                    pageKey);

            CurrentPageKey =
                pageKey;

            PageTitle =
                GetPageTitle(
                    pageKey);

            _navigationStore.NavigateTo(
                destination);
        }

        private ViewModelBase CreateDestination(
    string pageKey)
        {
            if (pageKey.Equals("LessonPlanner", StringComparison.OrdinalIgnoreCase))
                return _lessonPlannerViewModelFactory();
            if (pageKey.Equals("ActivitySheet", StringComparison.OrdinalIgnoreCase))
                return _activitySheetViewModelFactory();
            if (pageKey.Equals("ReadingTracker", StringComparison.OrdinalIgnoreCase))
                return _readingTrackerViewModelFactory();
            if (pageKey.Equals("Reports", StringComparison.OrdinalIgnoreCase))
                return _reportsViewModelFactory();

            if (pageKey.Equals(
                    "BackupRestore",
                    StringComparison.OrdinalIgnoreCase))
            {
                return _backupRestoreViewModelFactory();
            }

            if (pageKey.Equals(
                "Grades",
                StringComparison.OrdinalIgnoreCase))
            {
                return _gradesViewModelFactory();
            }

            if (pageKey.Equals(
                "ECR",
                StringComparison.OrdinalIgnoreCase))
            {
                return _eClassRecordViewModelFactory();
            }
            if (pageKey.Equals(
        "Attendance",
        StringComparison.OrdinalIgnoreCase))
            {
                return _attendanceViewModelFactory();
            }
            if (pageKey.Equals(
        "SubjectSetup",
        StringComparison.OrdinalIgnoreCase))
            {
                return _subjectSetupViewModelFactory();
            }
            if (pageKey.Equals(
        "Learners",
        StringComparison.OrdinalIgnoreCase))
            {
                return _learnersViewModelFactory();
            }
            if (pageKey.Equals(
        "MyClasses",
        StringComparison.OrdinalIgnoreCase))
            {
                return _myClassesViewModelFactory();
            }
            if (pageKey.Equals(
        "Settings",
        StringComparison.OrdinalIgnoreCase))
            {
                return _schoolSetupViewModelFactory();
            }
            if (pageKey.Equals(
        "Dashboard",
        StringComparison.OrdinalIgnoreCase))
            {
                return _dashboardViewModelFactory();
            }
            if (pageKey.Equals(
                    "SFForms",
                    StringComparison.OrdinalIgnoreCase))
            {
                return _schoolFormsViewModelFactory();
            }

            if (pageKey.Equals(
                    "About",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new AboutViewModel();
            }
            return new ModuleViewModel(
                GetPageTitle(
                    pageKey),

                GetPageDescription(
                    pageKey),

                GetDevelopmentPhase(
                    pageKey));
        }

        private static string GetPageTitle(
            string pageKey)
        {
            return pageKey switch
            {
                "MyClasses" =>
                    "My Classes",

                "Learners" =>
                    "Learners",

                "SubjectSetup" =>
                    "Subject Setup",

                "Attendance" =>
                    "Attendance Center",

                "ECR" =>
                    "Electronic Class Record",

                "Grades" =>
                    "Grades",

                "SFForms" =>
                    "SF Forms",

                "LessonPlanner" =>
                    "Lesson Planner",

                "ActivitySheet" =>
                    "Activity Sheet Generator",

                "Assessment" =>
                    "Assessment Generator",

                "PPTGenerator" =>
                    "PPT Generator",

                "ReadingTracker" =>
     "Reading Tracker",

                "NumeracyTracker" =>
                    "Numeracy Tracker",

                "Reports" =>
                                    "Reports",

                "FileManager" =>
                    "File Manager",

                "Update" =>
                    "UPDATE",

                "Themes" =>
                    "Themes",

                "AITeachingAssistant" =>
                    "AI Teaching Assistant",

                "Settings" =>
                    "Settings",

                "BackupRestore" =>
                    "Backup & Restore",

                "About" =>
                    "About TeachFlex",

                _ =>
                    "Dashboard"
            };
        }

        private static string GetPageDescription(
            string pageKey)
        {
            return pageKey switch
            {
                "MyClasses" =>
                    "Create and manage grade levels, sections, subjects, schedules, and advisory classes.",

                "Learners" =>
                    "Manage learner profiles, enrollment information, searching, filtering, importing, and printing.",

                "SubjectSetup" =>
                    "Manage Grade 1 to Grade 12 subjects, learning areas, classifications, and Track or Strand assignments.",

                "Attendance" =>
                    "Record daily attendance and prepare monthly and learner attendance reports.",

                "ECR" =>
                    "Manage written works, performance tasks, summative tests, term examinations, and final ratings.",

                "Grades" =>
                    "Review, encode, compute, and summarize learner grades for every term.",

                "SFForms" =>
                    "Prepare SF1, SF2, SF5, SF9, and SF10 using centralized school and learner records.",

                "LessonPlanner" =>
                    "Create, edit, save, print, and export structured lesson plans.",

                "ActivitySheet" =>
                    "Create editable learner activities with teacher versions and answer keys.",

                "Assessment" =>
                    "Prepare formative assessments, summative tests, quizzes, and term examinations.",

                "PPTGenerator" =>
                    "Create editable classroom presentations from learning topics and competencies.",

                "ReadingTracker" =>
    "Monitor reading levels, intervention records, assessment results, and learner progress.",

                "NumeracyTracker" =>
                    "Monitor numeracy levels, assessed mathematical skills, interventions, scores, and learner progress.",

                "Reports" =>
                                    "View and print class, attendance, grade, performance, reading, and intervention reports.",

                "FileManager" =>
                    "Organize TeachFlex documents, lesson plans, assessments, presentations, and school forms.",

                "Update" =>
                    "Check for and install available TeachFlex updates.",

                "Themes" =>
                    "Choose and manage the visual theme of the TeachFlex workspace.",

                "AITeachingAssistant" =>
                    "Generate editable teaching materials, strategies, remediation, and enrichment suggestions.",

                "Settings" =>
                    "Configure school information, teacher preferences, grading options, and application behavior.",

                "BackupRestore" =>
                    "Create safe database backups, select backup locations, and restore previous records.",

                "About" =>
                    "View TeachFlex application information, version details, and acknowledgements.",

                _ =>
                    "Your professional offline-first teaching dashboard."
            };
        }

        private static string GetDevelopmentPhase(
            string pageKey)
        {
            return pageKey switch
            {
                "MyClasses" or
                "Learners" or
                "SubjectSetup" or
                "Settings" =>
                    "Scheduled for Phase 2",

                "Attendance" or
                "ECR" or
                "Grades" =>
                    "Scheduled for Phase 3",

                "SFForms" or
                "Reports" =>
                    "Scheduled for Phase 4",

                "LessonPlanner" or
                "ActivitySheet" or
                "Assessment" or
                "PPTGenerator" =>
                    "Scheduled for Phase 5",

                "ReadingTracker" =>
                    "Active module",

"NumeracyTracker" or
"AITeachingAssistant" =>
    "Scheduled for Phase 6",

                "BackupRestore" =>
                    "Scheduled for Phase 7",

                "FileManager" or
                "Update" or
                "Themes" or
                "About" =>
                    "Scheduled for final polishing",

                _ =>
                    "Phase 1"
            };
        }

        private void ToggleSidebar()
        {
            IsSidebarExpanded =
                !IsSidebarExpanded;

            SidebarWidth =
                IsSidebarExpanded
                    ? 260
                    : 78;
        }

        private void ZoomIn()
        {
            ZoomScale =
                Math.Min(
                    1.30,
                    ZoomScale + 0.10);

            OnPropertyChanged(
                nameof(ZoomText));
        }

        private void ZoomOut()
        {
            ZoomScale =
                Math.Max(
                    0.80,
                    ZoomScale - 0.10);

            OnPropertyChanged(
                nameof(ZoomText));
        }

        private void ResetZoom()
        {
            ZoomScale =
                0.80;

            OnPropertyChanged(
                nameof(ZoomText));
        }

        private void NavigationStore_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName ==
                nameof(
                    NavigationStore
                        .CurrentViewModel))
            {
                OnPropertyChanged(
                    nameof(
                        CurrentViewModel));
            }
        }
    }
}
