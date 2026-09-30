using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class GradesViewModel :
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

        private readonly IPaceRepository _paceRepository;
        private readonly IKindergartenRecordRepository _kindergartenRecordRepository;

        private readonly IGradingCalculationService
            _gradingCalculationService;
        private readonly IGradesPdfExportService
    _gradesPdfExportService;

        private readonly IDialogService
            _dialogService; private School?
    _currentSchool;
        private AcademicYear?
            _currentAcademicYear;

        private SchoolClass?
            _selectedClass;

        private Subject?
            _selectedSubject;

        private bool
            _suppressSelectionLoading;

        private string _schoolYearText =
            "No active school year";

        public GradesViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository
                academicYearRepository,
            ISchoolClassRepository
                schoolClassRepository,
            ISubjectRepository subjectRepository,
            ILearnerRepository learnerRepository,
            IAssessmentRepository
                assessmentRepository,
            IPaceRepository paceRepository,
            IKindergartenRecordRepository kindergartenRecordRepository,
            IGradingCalculationService
    gradingCalculationService,
IGradesPdfExportService
    gradesPdfExportService,
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

            _paceRepository = paceRepository;
            _kindergartenRecordRepository = kindergartenRecordRepository;

            _gradingCalculationService =
     gradingCalculationService;

            _gradesPdfExportService =
        gradesPdfExportService;
            _dialogService =
                dialogService;

            Classes =
                new ObservableCollection<
                    SchoolClass>();

            Subjects =
                new ObservableCollection<
                    Subject>();

            GradeRows =
                new ObservableCollection<
                    GradeSummaryRow>();

            DescriptiveRows = new ObservableCollection<DescriptiveProgressRow>();

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

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

        public ObservableCollection<GradeSummaryRow>
            GradeRows
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

                GradeRows.Clear();
                DescriptiveRows.Clear();
                NotifyGradeSummary();
                ClearConsolidatedGrades();

                OnPropertyChanged(
                    nameof(IsKindergartenClass));
                NotifyDescriptiveMode();

                if (!_suppressSelectionLoading)
                {
                    _ = LoadSubjectsAsync();
                }
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
                    GradeRows.Clear();
                    DescriptiveRows.Clear();
                    NotifyGradeSummary();
                }
            }
        }

        public string SchoolYearText
        {
            get => _schoolYearText;

            private set => SetProperty(
                ref _schoolYearText,
                value);
        }

        public bool HasGradeRows =>
            GradeRows.Count > 0;

        public bool IsKindergartenClass =>
            SelectedClass?.GradeLevel.Equals(
                "Kindergarten",
                StringComparison.OrdinalIgnoreCase)
            == true;

        public bool IsDescriptiveClass => IsKindergartenClass ||
            (SelectedClass != null && _currentAcademicYear != null &&
             GradingPolicy.Create(SelectedClass.GradeLevel,
                 _currentAcademicYear.StartYear,
                 SelectedSubject?.SubjectName ?? string.Empty)
                 .UsesNumericalGrades == false);

        public bool IsNumericalClass => !IsDescriptiveClass;
        public string SummaryTitle => IsKindergartenClass
            ? "Kindergarten ECD Rating Progress"
            : IsDescriptiveClass ? "Descriptive PACE Rating Progress"
            : "Three-Term Grade Summary";
        public string SelectionNote => IsKindergartenClass
            ? "Kindergarten: ECD ratings are shown by term. Record or edit them in Kindergarten ECR."
            : IsDescriptiveClass
                ? "Descriptive ratings are shown by subject and term. Record or edit them in Grade 1 PACE."
                : "Numerical grades are computed from the Electronic Class Record.";
        public string ThirdCardTitle => IsDescriptiveClass ? "NEEDS RECORDING" : "PASSED";
        public int ThirdCardValue => IsDescriptiveClass
            ? DescriptiveRows.Count(row => !row.IsComplete) : PassedLearners;
        public int VisibleLearnerCount => IsDescriptiveClass
            ? DescriptiveRows.Count : TotalLearners;
        public int VisibleCompleteCount => IsDescriptiveClass
            ? DescriptiveRows.Count(row => row.IsComplete) : CompletedLearners;

        private void NotifyDescriptiveMode()
        {
            OnPropertyChanged(nameof(IsDescriptiveClass));
            OnPropertyChanged(nameof(IsNumericalClass));
            OnPropertyChanged(nameof(SummaryTitle));
            OnPropertyChanged(nameof(SelectionNote));
            OnPropertyChanged(nameof(ThirdCardTitle));
            OnPropertyChanged(nameof(ThirdCardValue));
            OnPropertyChanged(nameof(VisibleLearnerCount));
            OnPropertyChanged(nameof(VisibleCompleteCount));
        }

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
                    "Loading classes for the Grades module...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();
                _currentSchool =
    school;

                Classes.Clear();
                Subjects.Clear();
                GradeRows.Clear();
                DescriptiveRows.Clear();

                if (school == null)
                {
                    SchoolYearText =
                        "No active school year";

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
                    SchoolYearText =
                        "No active school year";

                    StatusMessage =
                        "Create the current School Year first.";

                    return;
                }

                SchoolYearText =
                    $"{_currentAcademicYear.StartYear}-" +
                    $"{_currentAcademicYear.EndYear}";

                var loadedClasses =
                    await _schoolClassRepository
                        .GetByAcademicYearAsync(
                            school.Id,
                            _currentAcademicYear.Id);

                foreach (SchoolClass schoolClass
                         in loadedClasses)
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

                if (SelectedClass != null)
                {
                    await LoadSubjectsAsync();
                }

                StatusMessage =
                    Classes.Count == 0
                        ? "No active classes are available."
                        : "Select a class and subject.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"Grades module.\n\n" +
                    $"{exception.Message}",
                    "Grades Error");

                StatusMessage =
                    "Grades could not be loaded.";
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private async Task LoadSubjectsAsync()
        {
            Subjects.Clear();
            GradeRows.Clear();
            DescriptiveRows.Clear();
            NotifyGradeSummary();

            if (SelectedClass == null)
            {
                SelectedSubject =
                    null;

                return;
            }

            if (IsKindergartenClass)
            {
                SelectedSubject =
                    null;

                StatusMessage =
                    "Kindergarten uses the ECD checklist.";

                NotifyDescriptiveMode();

                return;
            }

            try
            {
                var loadedSubjects =
                    await _subjectRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

                foreach (Subject subject
                         in loadedSubjects)
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

                StatusMessage =
                    SelectedSubject == null
                        ? "No subjects are assigned to this class."
                        : "Select a subject, then load the grades.";
                NotifyDescriptiveMode();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"assigned subjects.\n\n" +
                    $"{exception.Message}",
                    "Grades Error");

                StatusMessage =
                    "Subjects could not be loaded.";
            }
        }
    }
}
