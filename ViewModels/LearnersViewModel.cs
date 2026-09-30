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
    public partial class LearnersViewModel :
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

        private readonly IDialogService
            _dialogService;

        private SchoolClass?
            _selectedClass;

        private Learner?
            _selectedLearner;

        private School?
            _activeSchool;

        private AcademicYear?
            _currentAcademicYear;

        private AcademicYear?
            _selectedPromotionAcademicYear;

        private SchoolClass?
            _selectedPromotionClass;

        private string _selectedPromotionType =
            "Promoted";

        private string _searchText =
            string.Empty;

        private string _lrn =
            string.Empty;

        private string _lastName =
            string.Empty;

        private string _firstName =
            string.Empty;

        private string _middleName =
            string.Empty;

        private string _suffix =
            string.Empty;

        private string _sex =
            string.Empty;

        private DateTime? _birthDate;

        private string _address =
            string.Empty;

        private string _parentGuardianName =
            string.Empty;

        private string _parentGuardianContactNumber =
            string.Empty;

        private string _learnerStatus =
            "Active";

        public LearnersViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository
                academicYearRepository,
            ISchoolClassRepository
                schoolClassRepository,
            ILearnerRepository learnerRepository,
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

            _dialogService =
                dialogService;

            Classes =
                new ObservableCollection<
                    SchoolClass>();

            Learners =
                new ObservableCollection<
                    Learner>();

            PromotionAcademicYears =
                new ObservableCollection<
                    AcademicYear>();

            PromotionClasses =
                new ObservableCollection<
                    SchoolClass>();

            SexOptions =
                new[]
                {
                    "Male",
                    "Female"
                };

            StatusOptions =
                new[]
                {
                    "Active",
                    "Transferred Out",
                    "Dropped",
                    "Completed"
                };

            PromotionTypeOptions =
                new[]
                {
                    "Promoted",
                    "Retained"
                };

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            SearchCommand =
                new AsyncRelayCommand(
                    LoadLearnersAsync);

            NewLearnerCommand =
                new RelayCommand(
                    ClearForm);

            SaveLearnerCommand =
                new AsyncRelayCommand(
                    SaveLearnerAsync,
                    CanSaveLearner);

            ArchiveLearnerCommand =
                new AsyncRelayCommand(
                    ArchiveLearnerAsync,
                    CanArchiveLearner);

            PromoteClassCommand =
                new AsyncRelayCommand(
                    PromoteClassAsync,
                    CanPromoteClass);

            _ = LoadAsync();
        }

        public ObservableCollection<
            SchoolClass>
                Classes
        {
            get;
        }

        public ObservableCollection<
            Learner>
                Learners
        {
            get;
        }

        public ObservableCollection<AcademicYear>
            PromotionAcademicYears
        {
            get;
        }

        public ObservableCollection<SchoolClass>
            PromotionClasses
        {
            get;
        }

        public IReadOnlyList<string>
            SexOptions
        {
            get;
        }

        public IReadOnlyList<string>
            StatusOptions
        {
            get;
        }

        public IReadOnlyList<string>
            PromotionTypeOptions
        {
            get;
        }

        public IAsyncRelayCommand RefreshCommand
        {
            get;
        }

        public IAsyncRelayCommand SearchCommand
        {
            get;
        }

        public IRelayCommand NewLearnerCommand
        {
            get;
        }

        public IAsyncRelayCommand
            SaveLearnerCommand
        {
            get;
        }

        public IAsyncRelayCommand
            ArchiveLearnerCommand
        {
            get;
        }

        public IAsyncRelayCommand
            PromoteClassCommand
        {
            get;
        }

        public AcademicYear?
            SelectedPromotionAcademicYear
        {
            get => _selectedPromotionAcademicYear;

            set
            {
                if (SetProperty(
                        ref _selectedPromotionAcademicYear,
                        value))
                {
                    _ = LoadPromotionClassesAsync();

                    PromoteClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public SchoolClass?
            SelectedPromotionClass
        {
            get => _selectedPromotionClass;

            set
            {
                if (SetProperty(
                        ref _selectedPromotionClass,
                        value))
                {
                    PromoteClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string SelectedPromotionType
        {
            get => _selectedPromotionType;

            set
            {
                if (SetProperty(
                        ref _selectedPromotionType,
                        value))
                {
                    SelectRecommendedPromotionClass();

                    PromoteClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
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
                    ClearForm();

                    _ = LoadLearnersAsync();

                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();

                    SelectRecommendedPromotionClass();

                    PromoteClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public Learner? SelectedLearner
        {
            get => _selectedLearner;

            set
            {
                if (!SetProperty(
                        ref _selectedLearner,
                        value))
                {
                    return;
                }

                if (value != null)
                {
                    LoadLearnerIntoForm(
                        value);
                }

                ArchiveLearnerCommand
                    .NotifyCanExecuteChanged();

                SaveLearnerCommand
                    .NotifyCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;

            set => SetProperty(
                ref _searchText,
                value);
        }

        public string Lrn
        {
            get => _lrn;

            set
            {
                if (SetProperty(
                        ref _lrn,
                        value))
                {
                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string LastName
        {
            get => _lastName;

            set
            {
                if (SetProperty(
                        ref _lastName,
                        value))
                {
                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string FirstName
        {
            get => _firstName;

            set
            {
                if (SetProperty(
                        ref _firstName,
                        value))
                {
                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string MiddleName
        {
            get => _middleName;

            set => SetProperty(
                ref _middleName,
                value);
        }

        public string Suffix
        {
            get => _suffix;

            set => SetProperty(
                ref _suffix,
                value);
        }

        public string Sex
        {
            get => _sex;

            set
            {
                if (SetProperty(
                        ref _sex,
                        value))
                {
                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public DateTime? BirthDate
        {
            get => _birthDate;

            set => SetProperty(
                ref _birthDate,
                value);
        }

        public string Address
        {
            get => _address;

            set => SetProperty(
                ref _address,
                value);
        }

        public string ParentGuardianName
        {
            get => _parentGuardianName;

            set => SetProperty(
                ref _parentGuardianName,
                value);
        }

        public string ParentGuardianContactNumber
        {
            get =>
                _parentGuardianContactNumber;

            set => SetProperty(
                ref _parentGuardianContactNumber,
                value);
        }

        public string LearnerStatus
        {
            get => _learnerStatus;

            set => SetProperty(
                ref _learnerStatus,
                value);
        }

        private bool CanSaveLearner()
        {
            return !IsBusy &&
                   SelectedClass != null &&
                   !string.IsNullOrWhiteSpace(
                       Lrn) &&
                   !string.IsNullOrWhiteSpace(
    LearnerName) &&
                   !string.IsNullOrWhiteSpace(
                       Sex);
        }

        private bool CanArchiveLearner()
        {
            return !IsBusy &&
                   SelectedLearner != null;
        }

        private bool CanPromoteClass()
        {
            return !IsBusy &&
                   SelectedClass != null &&
                   SelectedPromotionAcademicYear != null &&
                   SelectedPromotionClass != null &&
                   SelectedClass.AcademicYearId !=
                       SelectedPromotionClass.AcademicYearId;
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
                    "Loading classes and learners...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    StatusMessage =
                        "Complete School Setup first.";

                    return;
                }

                _activeSchool =
                    school;

                AcademicYear? academicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            school.Id);

                if (academicYear == null)
                {
                    StatusMessage =
                        "Create the current School Year first.";

                    return;
                }

                _currentAcademicYear =
                    academicYear;

                IReadOnlyList<AcademicYear>
                    allAcademicYears =
                        await _academicYearRepository
                            .GetAllAsync(
                                school.Id);

                PromotionAcademicYears.Clear();

                foreach (AcademicYear futureYear
                         in allAcademicYears
                             .Where(
                                 year =>
                                     year.StartYear >
                                         academicYear.StartYear)
                             .OrderBy(
                                 year =>
                                     year.StartYear))
                {
                    PromotionAcademicYears.Add(
                        futureYear);
                }

                SelectedPromotionAcademicYear =
                    PromotionAcademicYears
                        .FirstOrDefault();

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

                StatusMessage =
                    SelectedClass == null
                        ? "Create a class first."
                        : "Learner records loaded.";
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "Learner records could not be loaded.";

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"learner records.\n\n" +
                    $"{exception.Message}",
                    "Learners Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task LoadLearnersAsync()
        {
            if (SelectedClass == null)
            {
                Learners.Clear();

                return;
            }

            try
            {
                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id,
                                SearchText);

                Learners.Clear();

                foreach (Learner learner
                         in learners)
                {
                    Learners.Add(
                        learner);
                }

                StatusMessage =
                    $"{Learners.Count} learner(s) loaded.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"selected class learners.\n\n" +
                    $"{exception.Message}",
                    "Learners Error");
            }
        }

        private async Task LoadPromotionClassesAsync()
        {
            PromotionClasses.Clear();

            SelectedPromotionClass =
                null;

            if (_activeSchool == null ||
                SelectedPromotionAcademicYear == null)
            {
                return;
            }

            try
            {
                IReadOnlyList<SchoolClass>
                    destinationClasses =
                        await _schoolClassRepository
                            .GetByAcademicYearAsync(
                                _activeSchool.Id,
                                SelectedPromotionAcademicYear.Id);

                foreach (SchoolClass destinationClass
                         in destinationClasses)
                {
                    PromotionClasses.Add(
                        destinationClass);
                }

                SelectRecommendedPromotionClass();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the destination classes.\n\n" +
                    $"{exception.Message}",
                    "Promotion Error");
            }
        }

        private void SelectRecommendedPromotionClass()
        {
            if (SelectedClass == null ||
                PromotionClasses.Count == 0)
            {
                SelectedPromotionClass =
                    null;

                return;
            }

            int sourceGradeOrder =
                GetGradeOrder(
                    SelectedClass.GradeLevel);

            int expectedGradeOrder =
                SelectedPromotionType == "Promoted"
                    ? sourceGradeOrder + 1
                    : sourceGradeOrder;

            SelectedPromotionClass =
                PromotionClasses.FirstOrDefault(
                    schoolClass =>
                        GetGradeOrder(
                            schoolClass.GradeLevel) ==
                                expectedGradeOrder);
        }

        private async Task PromoteClassAsync()
        {
            if (!CanPromoteClass() ||
                SelectedClass == null ||
                SelectedPromotionAcademicYear == null ||
                SelectedPromotionClass == null)
            {
                _dialogService.ShowWarning(
                    "Select the source class, destination school year, promotion type, and destination class.",
                    "Promotion Information Required");

                return;
            }

            SchoolClass sourceClass =
                SelectedClass;

            SchoolClass destinationClass =
                SelectedPromotionClass;

            AcademicYear destinationYear =
                SelectedPromotionAcademicYear;

            bool confirmed =
                _dialogService.Confirm(
                    $"{SelectedPromotionType} all eligible learners?\n\n" +
                    $"From: {sourceClass.DisplayName} ({_currentAcademicYear?.DisplayName})\n" +
                    $"To: {destinationClass.DisplayName} ({destinationYear.DisplayName})\n\n" +
                    "Previous school-year records, grades, and attendance will remain unchanged.",
                    "Confirm Learner Promotion");

            if (!confirmed)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                LearnerPromotionResult result =
                    await _learnerRepository
                        .PromoteClassAsync(
                            sourceClass.Id,
                            destinationClass.Id,
                            SelectedPromotionType);

                StatusMessage =
                    $"{result.EnrolledCount} learner(s) enrolled in {destinationClass.DisplayName}.";

                _dialogService.ShowInformation(
                    $"Promotion completed.\n\n" +
                    $"Eligible: {result.EligibleCount}\n" +
                    $"Enrolled: {result.EnrolledCount}\n" +
                    $"Already enrolled and skipped: {result.SkippedDuplicateCount}",
                    "Learner Promotion Complete");
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "Learner promotion could not be completed.";

                _dialogService.ShowError(
                    $"TeachFlex could not complete the promotion.\n\n" +
                    $"{exception.Message}",
                    "Learner Promotion Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private static int GetGradeOrder(
            string gradeLevel)
        {
            if (gradeLevel == "Kindergarten")
            {
                return 0;
            }

            string numberText =
                gradeLevel
                    .Replace(
                        "Grade",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

            return int.TryParse(
                numberText,
                out int gradeNumber)
                    ? gradeNumber
                    : -100;
        }

        private async Task SaveLearnerAsync()
        {
            if (!CanSaveLearner() ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Class, LRN, Last Name, First Name, and Sex are required.",
                    "Required Learner Information");

                return;
            }
            if (!TryParseLearnerName(
        LearnerName,
        out string parsedLastName,
        out string parsedFirstName,
        out string parsedMiddleName))
            {
                _dialogService.ShowWarning(
                    "Enter the learner name using this format:\n\n" +
                    "LAST NAME, FIRST NAME, M.I.",
                    "Invalid Learner Name");

                return;
            }
            string normalizedLrn =
                new string(
                    Lrn.Where(
                            char.IsDigit)
                        .ToArray());

            if (normalizedLrn.Length != 12)
            {
                _dialogService.ShowWarning(
                    "The LRN must contain exactly 12 digits.",
                    "Invalid LRN");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                Learner learner =
                    SelectedLearner
                    ?? new Learner();

                learner.SchoolClassId =
                    SelectedClass.Id;

                learner.Lrn =
                    normalizedLrn;

                learner.LastName =
    parsedLastName;

                learner.FirstName =
                    parsedFirstName;

                learner.MiddleName =
                    parsedMiddleName;

                learner.Suffix =
                    Suffix.Trim();

                learner.Sex =
                    Sex;

                learner.BirthDate =
                    BirthDate;

                learner.Address =
                    Address.Trim();

                learner.ParentGuardianName =
                    ParentGuardianName.Trim();

                learner.ParentGuardianContactNumber =
                    ParentGuardianContactNumber.Trim();

                learner.Status =
                    LearnerStatus;
                learner.EnrollmentDate =
    EnrollmentDate?.Date;

                learner.EnrollmentType =
                    EnrollmentType.Trim();

                learner.PreviousSchoolName =
                    PreviousSchoolName.Trim();

                learner.ExitDate =
                    ExitDate?.Date;

                learner.ExitReason =
                    ExitReason.Trim();

                learner.NextSchoolName =
                    NextSchoolName.Trim();

                SaveSf1Information(
                    learner);

                await _learnerRepository
                                    .SaveAsync(
                        learner);

                StatusMessage =
                    $"{learner.FullName} saved successfully.";

                ClearForm();

                await LoadLearnersAsync();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"learner record.\n\n" +
                    $"{exception.Message}",
                    "Save Learner Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task ArchiveLearnerAsync()
        {
            if (SelectedLearner == null)
            {
                return;
            }

            bool confirmed =
                _dialogService.Confirm(
                    $"Archive {SelectedLearner.FullName}?\n\n" +
                    $"The learner will no longer appear in the active class list.",
                    "Archive Learner");

            if (!confirmed)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                await _learnerRepository
                    .ArchiveAsync(
                        SelectedLearner.Id);

                ClearForm();

                await LoadLearnersAsync();

                StatusMessage =
                    "Learner archived successfully.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not archive the " +
                    $"learner.\n\n" +
                    $"{exception.Message}",
                    "Archive Learner Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void LoadLearnerIntoForm(
            Learner learner)
        {
            LearnerName =
    FormatLearnerName(
        learner);
            Lrn =
                learner.Lrn;

            LastName =
                learner.LastName;

            FirstName =
                learner.FirstName;

            MiddleName =
                learner.MiddleName;

            Suffix =
                learner.Suffix;

            Sex =
                learner.Sex;

            BirthDate =
                learner.BirthDate;

            Address =
                learner.Address;

            ParentGuardianName =
                learner.ParentGuardianName;

            ParentGuardianContactNumber =
                learner.ParentGuardianContactNumber;

            LearnerStatus =
                learner.Status;
            EnrollmentDate =
    learner.EnrollmentDate;

            EnrollmentType =
                learner.EnrollmentType;

            PreviousSchoolName =
                learner.PreviousSchoolName;

            ExitDate =
                learner.ExitDate;

            ExitReason =
                learner.ExitReason;

            NextSchoolName =
                learner.NextSchoolName;

            LoadSf1Information(
                learner);
        }

        private void ClearForm()
        {
            SelectedLearner =
                null;

            Lrn =
                string.Empty;
            LearnerName =
    string.Empty;

            LastName =
                string.Empty;

            FirstName =
                string.Empty;

            MiddleName =
                string.Empty;

            Suffix =
                string.Empty;

            Sex =
                string.Empty;

            BirthDate =
                null;

            Address =
                string.Empty;

            ParentGuardianName =
                string.Empty;

            ParentGuardianContactNumber =
                string.Empty;

            LearnerStatus =
                "Active";
            EnrollmentDate =
    DateTime.Today;

            EnrollmentType =
                "Regular";

            PreviousSchoolName =
                string.Empty;

            ExitDate =
                null;

            ExitReason =
                string.Empty;

            NextSchoolName =
                string.Empty;

            ClearSf1Information();
        }

        private void NotifyCommandStates()
        {
            SaveLearnerCommand
                .NotifyCanExecuteChanged();

            ArchiveLearnerCommand
                .NotifyCanExecuteChanged();

            PromoteClassCommand
                .NotifyCanExecuteChanged();
        }
    }
}
