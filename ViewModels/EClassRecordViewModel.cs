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
    public partial class EClassRecordViewModel :
        ViewModelBase
    {
        private readonly ISchoolRepository
            _schoolRepository;

        private readonly IAcademicYearRepository
            _academicYearRepository;

        private readonly ISchoolClassRepository
            _schoolClassRepository;

        private readonly ISubjectRepository
            _subjectRepository;

        private readonly ILearnerRepository
            _learnerRepository;

        private readonly IAssessmentRepository
            _assessmentRepository;

        private readonly ILearnerGradeService
            _learnerGradeService;

        private readonly IPaceRepository
            _paceRepository;
        private readonly IKindergartenRecordRepository
    _kindergartenRecordRepository;

        private readonly IGradeOnePaceCatalogService
            _gradeOnePaceCatalogService;

        private readonly IGradingCalculationService
            _gradingCalculationService;

        private readonly IECRExportService
            _ecrExportService;

        private readonly IGradeOnePaceExportService
            _gradeOnePaceExportService;
        private readonly IKindergartenECRExportService
    _kindergartenEcrExportService;
        private readonly ISeniorHighECRExportService
    _seniorHighEcrExportService;

        private readonly IDialogService
            _dialogService;

        private AcademicYear?
            _currentAcademicYear;
        private School?
    _currentSchool;

        private SchoolClass?
            _selectedClass;

        private Subject?
            _selectedSubject;

        private int _selectedTerm =
            1;

        private GradingPolicy?
            _currentPolicy;

        private bool
            _suppressSelectionLoading;

        public EClassRecordViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository
                academicYearRepository,
            ISchoolClassRepository
                schoolClassRepository,
            ISubjectRepository subjectRepository,
            ILearnerRepository learnerRepository,
            IAssessmentRepository
                assessmentRepository,
            ILearnerGradeService learnerGradeService,
           IPaceRepository paceRepository,
IKindergartenRecordRepository
    kindergartenRecordRepository,
IGradeOnePaceCatalogService
    gradeOnePaceCatalogService,
            IGradingCalculationService
                gradingCalculationService,
            IECRExportService
                ecrExportService,
            IGradeOnePaceExportService
    gradeOnePaceExportService,
IKindergartenECRExportService
    kindergartenEcrExportService,
ISeniorHighECRExportService
    seniorHighEcrExportService,
IDialogService dialogService)
        {
            _schoolRepository =
                schoolRepository;

            _academicYearRepository =
                academicYearRepository;

            _schoolClassRepository =
                schoolClassRepository;

            _subjectRepository =
                subjectRepository;

            _learnerRepository =
                learnerRepository;

            _assessmentRepository =
                assessmentRepository;
            _learnerGradeService =
                learnerGradeService;

            _paceRepository =
                paceRepository;
            _kindergartenRecordRepository =
    kindergartenRecordRepository;

            _gradeOnePaceCatalogService =
                gradeOnePaceCatalogService;

            _gradingCalculationService =
                gradingCalculationService;

            _ecrExportService =
                ecrExportService;

            _gradeOnePaceExportService =
     gradeOnePaceExportService;

            _kindergartenEcrExportService =
                kindergartenEcrExportService;
            _seniorHighEcrExportService =
    seniorHighEcrExportService;

            _dialogService =
                dialogService;

            Classes =
                new ObservableCollection<
                    SchoolClass>();

            Subjects =
                new ObservableCollection<
                    Subject>();

            AssessmentItems =
                new ObservableCollection<
                    AssessmentItem>();

            LearnerRows =
                new ObservableCollection<
                    EClassRecordLearnerRow>();

            TermOptions =
                new[]
                {
                    1,
                    2,
                    3
                };

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            LoadRecordCommand =
                new AsyncRelayCommand(
                    LoadRecordAsync,
                    CanLoadRecord);

            SaveScoresCommand =
                new AsyncRelayCommand(
                    SaveScoresAsync,
                    CanSaveScores);

            InitializeKindergartenCompletionTracking();

            _ = LoadAsync();
        }

        public ObservableCollection<SchoolClass>
            Classes
        {
            get;
        }

        public ObservableCollection<Subject>
            Subjects
        {
            get;
        }

        public ObservableCollection<AssessmentItem>
            AssessmentItems
        {
            get;
        }

        public ObservableCollection<
            EClassRecordLearnerRow>
                LearnerRows
        {
            get;
        }

        public IReadOnlyList<int>
            TermOptions
        {
            get;
        }

        public IAsyncRelayCommand
            RefreshCommand
        {
            get;
        }

        public IAsyncRelayCommand
            LoadRecordCommand
        {
            get;
        }

        public IAsyncRelayCommand
            SaveScoresCommand
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

                OnPropertyChanged(
                    nameof(IsKindergartenRecord));

                ClearRecord();
                ThreeTermSummaryRows.Clear();
                TermSubjectSections.Clear();
                OnPropertyChanged(nameof(HasThreeTermSummary));

                if (!_suppressSelectionLoading)
                {
                    if (IsKindergartenRecord)
                    {
                        _ = LoadKindergartenClassAsync();
                    }
                    else
                    {
                        KindergartenLearners.Clear();
                        KindergartenRatingRows.Clear();

                        SelectedKindergartenLearner =
                            null;

                        _ = LoadSubjectsAsync();
                    }
                }

                NotifyCommandStates();
            }
        }

        public Subject? SelectedSubject
        {
            get => _selectedSubject;

            set
            {
                if (SetProperty(
                        ref _selectedSubject,
                        value))
                {
                    UpdateGradingPolicy();
                    ClearRecord();
                    NotifyCommandStates();
                }
            }
        }

        public int SelectedTerm
        {
            get => _selectedTerm;

            set
            {
                int safeTerm =
                    Math.Clamp(
                        value,
                        1,
                        3);

                if (SetProperty(
                        ref _selectedTerm,
                        safeTerm))
                {
                    ClearRecord();

                    OnPropertyChanged(
                        nameof(
                            SelectedTermText));

                    NotifyCommandStates();
                }
            }
        }

        public GradingPolicy? CurrentPolicy
        {
            get => _currentPolicy;

            private set
            {
                if (SetProperty(
                        ref _currentPolicy,
                        value))
                {
                    OnPropertyChanged(
                        nameof(
                            GradingSystemText));

                    OnPropertyChanged(
                        nameof(
                            AssessmentFormatText));

                    OnPropertyChanged(
                        nameof(
                            UsesDescriptiveGrades));

                    OnPropertyChanged(
                        nameof(
                            UsesNumericalGrades));
                }
            }
        }

        public string SelectedTermText =>
            $"Term {SelectedTerm}";

        public string GradingSystemText =>
            CurrentPolicy?.GradingSystem switch
            {
                GradingSystemType.Descriptive =>
                    "Descriptive Grading",

                GradingSystemType
                    .NumericalZeroBased =>
                        "Numerical — Zero-Based",

                GradingSystemType
                    .NumericalAdjusted =>
                        "Numerical — Adjusted Transmutation",

                _ =>
                    "Select a class and subject"
            };

        public string AssessmentFormatText
        {
            get
            {
                if (CurrentPolicy == null)
                {
                    return string.Empty;
                }

                if (CurrentPolicy
                    .UsesDescriptiveGrades)
                {
                    return
                        "PACE Descriptive Record";
                }

                if (CurrentPolicy
                    .UsesDomainBasedAssessment)
                {
                    return
                        "GMRC / Values Education — " +
                        "Flexible Domain-Based Assessment";
                }

                string weightText =
                    $"WW " +
                    $"{CurrentPolicy.WrittenWorkWeight:0}% • " +
                    $"PT " +
                    $"{CurrentPolicy.PerformanceTaskWeight:0}% • " +
                    $"EX " +
                    $"{CurrentPolicy.ExaminationWeight:0}%";

                if (CurrentPolicy
                    .UsesMapehComponents)
                {
                    return
                        $"MAPEH Per Component • {weightText}";
                }

                if (CurrentPolicy
                    .SupportsEppTleComponents)
                {
                    return
                        $"EPP/TLE • {weightText}";
                }

                return weightText;
            }
        }

        public bool UsesDescriptiveGrades =>
            CurrentPolicy?
                .UsesDescriptiveGrades == true;

        public bool UsesNumericalGrades =>
            CurrentPolicy?
                .UsesNumericalGrades == true;

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
                    "Loading E-Class Record...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();
                _currentSchool =
    school;

                if (school == null)
                {
                    Classes.Clear();
                    Subjects.Clear();
                    ClearRecord();

                    StatusMessage =
                        "Complete School Setup first.";

                    return;
                }

                _currentAcademicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            school.Id);

                if (_currentAcademicYear == null)
                {
                    Classes.Clear();
                    Subjects.Clear();
                    ClearRecord();

                    StatusMessage =
                        "Create the current School Year first.";

                    return;
                }

                IReadOnlyList<SchoolClass>
                    classes =
                        await _schoolClassRepository
                            .GetByAcademicYearAsync(
                                school.Id,
                                _currentAcademicYear.Id);

                Classes.Clear();

                foreach (SchoolClass schoolClass
                         in classes)
                {
                    Classes.Add(
                        schoolClass);
                }

                _suppressSelectionLoading =
                    true;

                SelectedClass =
                    Classes.FirstOrDefault();

                _suppressSelectionLoading =
                    false;

                if (SelectedClass == null)
                {
                    Subjects.Clear();

                    StatusMessage =
                        "Create a class first.";

                    return;
                }

                await LoadSubjectsAsync();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"E-Class Record.\n\n" +
                    $"{exception.Message}",
                    "E-Class Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task LoadSubjectsAsync()
        {
            Subjects.Clear();
            ClearRecord();

            if (SelectedClass == null)
            {
                StatusMessage =
                    "Select a class first.";

                return;
            }

            try
            {
                IReadOnlyList<Subject>
                    subjects =
                        await _subjectRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                foreach (Subject subject
                         in subjects)
                {
                    Subjects.Add(
                        subject);
                }

                _suppressSelectionLoading =
                    true;

                SelectedSubject =
                    Subjects.FirstOrDefault();

                _suppressSelectionLoading =
                    false;

                UpdateGradingPolicy();

                StatusMessage =
                    SelectedSubject == null
                        ? "No active subjects are assigned " +
                          "to this class."
                        : "Select a subject and term, " +
                          "then load the class record.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"class subjects.\n\n" +
                    $"{exception.Message}",
                    "E-Class Record Error");
            }
            finally
            {
                NotifyCommandStates();
            }
        }

        private void UpdateGradingPolicy()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                _currentAcademicYear == null)
            {
                CurrentPolicy =
                    null;

                ConfigureComponentPolicy();

                return;
            }

            CurrentPolicy =
                GradingPolicy.Create(
                    SelectedClass.GradeLevel,
                    _currentAcademicYear.StartYear,
                    SelectedSubject.SubjectName);

            ConfigureComponentPolicy();
        }

        private void ClearRecord()
        {
            AssessmentItems.Clear();
            LearnerRows.Clear();
            ClearPaceRecord();
        }

        private bool CanLoadRecord()
        {
            return !IsBusy &&
                   SelectedClass != null &&
                   SelectedSubject != null &&
                   CurrentPolicy != null;
        }

        private bool CanSaveScores()
        {
            return !IsBusy &&
                   SelectedClass != null &&
                   SelectedSubject != null &&
                   CurrentPolicy?
                       .UsesNumericalGrades == true &&
                   LearnerRows.Count > 0;
        }

        private void NotifyCommandStates()
        {
            LoadRecordCommand
                .NotifyCanExecuteChanged();

            SaveScoresCommand
                .NotifyCanExecuteChanged();
        }
    }
}
