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
    public partial class SchoolFormsViewModel :
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

        private readonly ISubjectRepository
            _subjectRepository;

        private readonly IPaceRepository
            _paceRepository;

        private readonly IAutomatedRemarksService
            _automatedRemarksService;

        private readonly ILearnerTermRemarkRepository
            _learnerTermRemarkRepository;

        private readonly IKindergartenRecordRepository
            _kindergartenRecordRepository;

        private readonly IKindergartenProgressReportPdfService
            _kindergartenProgressReportPdfService;

        private readonly IAttendanceRepository
            _attendanceRepository;

        private readonly ITeacherRepository
            _teacherRepository;

        private readonly ILearnerGradeService
            _learnerGradeService;

        private readonly ISF1ExportService
            _sf1ExportService;

        private readonly ISF1PdfExportService
            _sf1PdfExportService;

        private readonly ISF2ExportService
            _sf2ExportService;

        private readonly ISF9ExportService
            _sf9ExportService;

        private readonly IAssessmentRepository _assessmentRepository;

        private readonly IDialogService
            _dialogService;

        private School?
            _currentSchool;

        private AcademicYear?
            _currentAcademicYear;

        private SchoolClass?
            _selectedClass;

        private Learner?
            _selectedSf9Learner;

        private DateTime _selectedMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

        private int _learnerCount;

        private int _maleLearnerCount;

        private int _femaleLearnerCount;

        public SchoolFormsViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository
                academicYearRepository,
            ISchoolClassRepository
                schoolClassRepository,
            ILearnerRepository learnerRepository,
            ISubjectRepository subjectRepository,
            IPaceRepository paceRepository,
            IAutomatedRemarksService automatedRemarksService,
            ILearnerTermRemarkRepository learnerTermRemarkRepository,
            IKindergartenRecordRepository kindergartenRecordRepository,
            IKindergartenProgressReportPdfService
                kindergartenProgressReportPdfService,
            IAttendanceRepository attendanceRepository,
            ITeacherRepository teacherRepository,
            ILearnerGradeService learnerGradeService,
            ISF1ExportService sf1ExportService,
            ISF1PdfExportService sf1PdfExportService,
            ISF2ExportService sf2ExportService,
            ISF9ExportService sf9ExportService,
            IAssessmentRepository assessmentRepository,
            IDialogService dialogService)
        {
            _schoolRepository =
                schoolRepository;

            _academicYearRepository =
                academicYearRepository;

            _schoolClassRepository =
                schoolClassRepository;

            _learnerRepository =
                learnerRepository;

            _subjectRepository =
                subjectRepository;

            _paceRepository =
                paceRepository;

            _automatedRemarksService =
                automatedRemarksService;

            _learnerTermRemarkRepository =
                learnerTermRemarkRepository;

            _kindergartenRecordRepository =
                kindergartenRecordRepository;

            _kindergartenProgressReportPdfService =
                kindergartenProgressReportPdfService;

            _attendanceRepository =
                attendanceRepository;

            _teacherRepository =
                teacherRepository;

            _learnerGradeService =
                learnerGradeService;

            _sf1ExportService =
                sf1ExportService;

            _sf1PdfExportService =
                sf1PdfExportService;

            _sf2ExportService =
                sf2ExportService;

            _sf9ExportService =
                sf9ExportService;

            _assessmentRepository = assessmentRepository;

            _dialogService =
                dialogService;

            Classes =
                new ObservableCollection<
                    SchoolClass>();

            CurrentLearners =
                new ObservableCollection<
                    Learner>();


            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            _ = LoadAsync();
        }

        public ObservableCollection<
            SchoolClass> Classes
        {
            get;
        }

        public ObservableCollection<
            Learner> CurrentLearners
        {
            get;
        }


        public IAsyncRelayCommand RefreshCommand
        {
            get;
        }

        public SchoolClass? SelectedClass
        {
            get => _selectedClass;

            set
            {
                if (!SetProperty(
                        ref _selectedClass,
                        value))
                {
                    return;
                }

                SelectedSf9Learner =
                    null;

                NotifySchoolFormAvailability();

                _ = LoadSelectedClassAsync();
            }
        }

        public Learner? SelectedSf9Learner
        {
            get => _selectedSf9Learner;

            set
            {
                if (!SetProperty(
                        ref _selectedSf9Learner,
                        value))
                {
                    return;
                }

                OnPropertyChanged(
                    nameof(
                        CanPrepareSf9));

                OnPropertyChanged(
                    nameof(
                        CanPrepareKindergartenProgressReport));

                OnPropertyChanged(
                    nameof(
                        IsRemarksEditorAvailable));

                _ = LoadRemarkEditorAsync();
            }
        }

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

                if (!SetProperty(
                        ref _selectedMonth,
                        monthValue))
                {
                    return;
                }

                OnPropertyChanged(
                    nameof(
                        SelectedMonthText));
            }
        }

        public string SelectedMonthText =>
            SelectedMonth.ToString(
                "MMMM yyyy");

        public int LearnerCount
        {
            get => _learnerCount;

            private set => SetProperty(
                ref _learnerCount,
                value);
        }

        public int MaleLearnerCount
        {
            get => _maleLearnerCount;

            private set => SetProperty(
                ref _maleLearnerCount,
                value);
        }

        public int FemaleLearnerCount
        {
            get => _femaleLearnerCount;

            private set => SetProperty(
                ref _femaleLearnerCount,
                value);
        }

        public string SchoolYearText =>
            _currentAcademicYear?.DisplayName
            ?? "No active school year";

        public bool IsGradeElevenAcademicClass =>
            SelectedClass != null &&
            IsGradeEleven(
                SelectedClass.GradeLevel) &&
            !IsTechnicalProfessionalTrack(
                SelectedClass.TrackStrand);

        public bool IsGradeTwelveAcademicClass =>
            SelectedClass != null &&
            IsGradeTwelve(
                SelectedClass.GradeLevel) &&
            !IsTechnicalProfessionalTrack(
                SelectedClass.TrackStrand);

        public bool IsSupportedSf9Class =>
            SelectedClass != null &&
            (IsGradeOneToTen(
                 SelectedClass.GradeLevel) ||
             IsGradeEleven(
                 SelectedClass.GradeLevel) ||
             IsGradeTwelve(
                 SelectedClass.GradeLevel));

        public bool IsKindergartenClass =>
            SelectedClass != null &&
            IsKindergarten(SelectedClass.GradeLevel);

        public bool IsLearnerProgressReportClass =>
            IsSupportedSf9Class || IsKindergartenClass;

        public bool CanPrepareSf1 =>
            _currentSchool != null &&
            _currentAcademicYear != null &&
            SelectedClass != null &&
            LearnerCount > 0 &&
            LearnerCount <= 50;

        public bool CanPrepareSf2 =>
            _currentSchool != null &&
            _currentAcademicYear != null &&
            SelectedClass != null &&
            LearnerCount > 0;

        public bool CanPrepareSf9 =>
            _currentSchool != null &&
            _currentAcademicYear != null &&
            SelectedClass != null &&
            SelectedSf9Learner != null &&
            IsSupportedSf9Class;

        public bool CanPrepareKindergartenProgressReport =>
            _currentSchool != null &&
            _currentAcademicYear != null &&
            SelectedClass != null &&
            SelectedSf9Learner != null &&
            IsKindergartenClass;

        private async Task LoadAsync()
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
                    "Loading School Forms information...";

                _currentSchool =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (_currentSchool == null)
                {
                    Classes.Clear();

                    CurrentLearners.Clear();

                    SelectedSf9Learner =
                        null;

                    UpdateLearnerCounts();

                    StatusMessage =
                        "Complete School Setup first.";

                    return;
                }

                _currentAcademicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            _currentSchool.Id);

                OnPropertyChanged(
                    nameof(
                        SchoolYearText));

                if (_currentAcademicYear == null)
                {
                    Classes.Clear();

                    CurrentLearners.Clear();

                    SelectedSf9Learner =
                        null;

                    UpdateLearnerCounts();

                    StatusMessage =
                        "Create the current School Year first.";

                    return;
                }

                IReadOnlyList<SchoolClass>
                    classes =
                        await _schoolClassRepository
                            .GetByAcademicYearAsync(
                                _currentSchool.Id,
                                _currentAcademicYear.Id);

                SchoolClass? previousSelection =
                    SelectedClass;

                Classes.Clear();

                foreach (SchoolClass schoolClass
                         in classes)
                {
                    Classes.Add(
                        schoolClass);
                }

                SelectedClass =
                    previousSelection == null
                        ? Classes.FirstOrDefault()
                        : Classes.FirstOrDefault(
                            schoolClass =>
                                schoolClass.Id ==
                                previousSelection.Id)
                          ?? Classes.FirstOrDefault();

                if (SelectedClass == null)
                {
                    CurrentLearners.Clear();

                    SelectedSf9Learner =
                        null;

                    UpdateLearnerCounts();

                    StatusMessage =
                        "Create an advisory class first.";

                    return;
                }

                await LoadSelectedClassAsync();
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "School Forms information could not be loaded.";

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"School Forms information.\n\n" +
                    $"{exception.Message}",
                    "School Forms Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifySchoolFormAvailability();
            }
        }

        private async Task LoadSelectedClassAsync()
        {
            if (SelectedClass == null)
            {
                CurrentLearners.Clear();

                SelectedSf9Learner =
                    null;

                UpdateLearnerCounts();

                return;
            }

            try
            {
                int? previousLearnerId =
                    SelectedSf9Learner?.Id;

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                CurrentLearners.Clear();

                foreach (Learner learner
                         in learners)
                {
                    CurrentLearners.Add(
                        learner);
                }

                SelectedSf9Learner =
                    previousLearnerId.HasValue
                        ? CurrentLearners
                            .FirstOrDefault(
                                learner =>
                                    learner.Id ==
                                    previousLearnerId.Value)
                          ?? CurrentLearners
                              .FirstOrDefault()
                        : CurrentLearners
                            .FirstOrDefault();

                UpdateLearnerCounts();

                StatusMessage =
                    CreateClassStatusMessage();
            }
            catch (Exception exception)
            {
                CurrentLearners.Clear();

                SelectedSf9Learner =
                    null;

                UpdateLearnerCounts();

                StatusMessage =
                    "The selected class could not be loaded.";

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"selected class learners.\n\n" +
                    $"{exception.Message}",
                    "School Forms Error");
            }
            finally
            {
                NotifySchoolFormAvailability();
            }
        }

        private void UpdateLearnerCounts()
        {
            LearnerCount =
                CurrentLearners.Count;

            MaleLearnerCount =
                CurrentLearners.Count(
                    learner =>
                        learner.Sex.Equals(
                            "Male",
                            StringComparison.OrdinalIgnoreCase));

            FemaleLearnerCount =
                CurrentLearners.Count(
                    learner =>
                        learner.Sex.Equals(
                            "Female",
                            StringComparison.OrdinalIgnoreCase));

            NotifySchoolFormAvailability();
        }

        private void NotifySchoolFormAvailability()
        {
            OnPropertyChanged(
                nameof(
                    CanPrepareSf1));

            OnPropertyChanged(
                nameof(
                    CanPrepareSf2));

            OnPropertyChanged(
                nameof(
                    IsGradeElevenAcademicClass));

            OnPropertyChanged(
                nameof(
                    IsGradeTwelveAcademicClass));

            OnPropertyChanged(
                nameof(
                    IsSupportedSf9Class));

            OnPropertyChanged(
                nameof(
                    IsKindergartenClass));

            OnPropertyChanged(
                nameof(
                    IsLearnerProgressReportClass));

            OnPropertyChanged(
                nameof(
                    CanPrepareSf9));

            OnPropertyChanged(
                nameof(
                    CanPrepareKindergartenProgressReport));
        }

        private string CreateClassStatusMessage()
        {
            if (LearnerCount == 0)
            {
                return "The selected class has no learners.";
            }

            if (LearnerCount > 50)
            {
                return "SF1 supports a maximum of 50 learners.";
            }

            if (SelectedClass != null &&
                IsGradeOneToTen(
                    SelectedClass.GradeLevel))
            {
                return $"{LearnerCount} learner(s) ready " +
                       $"for SF1, SF2, and " +
                       $"{SelectedClass.GradeLevel} SF9.";
            }

            if (SelectedClass != null &&
                IsKindergarten(SelectedClass.GradeLevel))
            {
                return $"{LearnerCount} learner(s) ready for SF1, SF2, " +
                       "and the Kindergarten Progress Report.";
            }

            if (SelectedClass != null &&
                IsGradeEleven(
                    SelectedClass.GradeLevel))
            {
                string trackType =
                    IsTechnicalProfessionalTrack(
                        SelectedClass.TrackStrand)
                        ? "TechPro"
                        : "Academic";

                return $"{LearnerCount} learner(s) ready " +
                       $"for SF1, SF2, and Grade 11 {trackType} SF9.";
            }

            if (SelectedClass != null &&
                IsGradeTwelve(
                    SelectedClass.GradeLevel))
            {
                string trackType =
                    IsTechnicalProfessionalTrack(
                        SelectedClass.TrackStrand)
                        ? "TechPro"
                        : "Academic";

                return $"{LearnerCount} learner(s) ready " +
                       $"for SF1, SF2, and Grade 12 {trackType} SF9.";
            }

            return $"{LearnerCount} learner(s) ready " +
                   "for School Forms.";
        }

        private static bool IsGradeEleven(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return false;
            }

            string normalized =
                gradeLevel
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "grade",
                        string.Empty)
                    .Trim();

            return normalized == "11" ||
                   normalized == "g11";
        }

        private static bool IsKindergarten(string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(gradeLevel))
            {
                return false;
            }

            string normalized = gradeLevel
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);

            return normalized is "kindergarten" or "kinder" or "k";
        }

        private static bool IsGradeOneToTen(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return false;
            }

            string normalized =
                gradeLevel
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "grade",
                        string.Empty)
                    .Replace(
                        "g",
                        string.Empty)
                    .Trim();

            return int.TryParse(
                       normalized,
                       out int gradeNumber) &&
                   gradeNumber is >= 1 and <= 10;
        }

        private static bool IsGradeTwelve(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return false;
            }

            string normalized =
                gradeLevel
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "grade",
                        string.Empty)
                    .Trim();

            return normalized == "12" ||
                   normalized == "g12";
        }

        private static bool
            IsTechnicalProfessionalTrack(
                string trackStrand)
        {
            if (string.IsNullOrWhiteSpace(
                    trackStrand))
            {
                return false;
            }

            string normalized =
                trackStrand
                    .Trim()
                    .ToLowerInvariant();

            string[] technicalTracks =
            {
                "aesthetic, wellness, and human care",
                "agri-fishery business and food innovation",
                "artisanry and creative enterprise",
                "automotive and small engine technologies",
                "construction and building technologies",
                "creative arts and design technologies",
                "hospitality and tourism",
                "ict support and computer programming technologies",
                "industrial technologies",
                "maritime",
                "technical-professional",
                "techpro",
                "tech pro"
            };

            return technicalTracks.Any(
                technicalTrack =>
                    normalized.Contains(
                        technicalTrack));
        }
    }
}
