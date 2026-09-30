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
    public partial class MyClassesViewModel :
    ViewModelBase
    {
        private readonly ISchoolRepository
            _schoolRepository;

        private readonly IAcademicYearRepository
            _academicYearRepository;

        private readonly ISchoolClassRepository
            _schoolClassRepository;
        private readonly ITeacherRepository
    _teacherRepository;

        private readonly IDialogService
            _dialogService;

        private School?
            _school;
        private Teacher?
    _activeTeacher;

        private AcademicYear?
            _selectedAcademicYear;

        private SchoolClass?
            _selectedClass;

        private int _schoolYearStart =
            DateTime.Today.Year;

        private int _schoolYearEnd =
            DateTime.Today.Year + 1;

        private string _gradeLevel =
            string.Empty;

        private string _sectionName =
            string.Empty;

        private string _trackStrand =
            "Not Applicable";

        private string _schedule =
            string.Empty;

        private string _room =
            string.Empty;

        public MyClassesViewModel(
            ISchoolRepository schoolRepository,
            IAcademicYearRepository
                academicYearRepository,
            ISchoolClassRepository
    schoolClassRepository,
ITeacherRepository
    teacherRepository,
ISubjectRepository subjectRepository,
IDialogService dialogService)
        {
            _schoolRepository =
                schoolRepository;

            _academicYearRepository =
                academicYearRepository;

            _schoolClassRepository =
                schoolClassRepository;
            _teacherRepository =
    teacherRepository;
            _subjectRepository =
    subjectRepository;

            _dialogService =
                dialogService;

            AcademicYears =
                new ObservableCollection<
                    AcademicYear>();

            Classes =
                new ObservableCollection<
                    SchoolClass>();
            ClassSubjects =
    new ObservableCollection<
        Subject>();

            GradeLevels =
                new[]
                {
                    "Kindergarten",
                    "Grade 1",
                    "Grade 2",
                    "Grade 3",
                    "Grade 4",
                    "Grade 5",
                    "Grade 6",
                    "Grade 7",
                    "Grade 8",
                    "Grade 9",
                    "Grade 10",
                    "Grade 11",
                    "Grade 12"
                };

            TrackStrandOptions =
    new[]
    {
        "Arts, Social Sciences, and Humanities",
        "Business and Entrepreneurship",
        "Science, Technology, Engineering, and Mathematics",
        "Sports, Health, and Wellness",
        "Aesthetic, Wellness, and Human Care",
        "Agri-Fishery Business and Food Innovation",
        "Artisanry and Creative Enterprise",
        "Automotive and Small Engine Technologies",
        "Construction and Building Technologies",
        "Creative Arts and Design Technologies",
        "Hospitality and Tourism",
        "ICT Support and Computer Programming Technologies",
        "Industrial Technologies",
        "Maritime"
    };

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            CreateSchoolYearCommand =
                new AsyncRelayCommand(
                    CreateSchoolYearAsync,
                    CanCreateSchoolYear);

            UseSelectedSchoolYearCommand =
                new AsyncRelayCommand(
                    UseSelectedSchoolYearAsync,
                    CanUseSelectedSchoolYear);

            NewClassCommand =
                new RelayCommand(
                    ClearClassForm);

            SaveClassCommand =
                new AsyncRelayCommand(
                    SaveClassAsync,
                    CanSaveClass);

            DeleteClassCommand =
                new AsyncRelayCommand(
                    DeleteSelectedClassAsync,
                    CanDeleteClass);

            _ = LoadAsync();
        }

        public ObservableCollection<
            AcademicYear>
                AcademicYears
        {
            get;
        }

        public ObservableCollection<
            SchoolClass>
                Classes
        {
            get;
        }

        public IReadOnlyList<string>
            GradeLevels
        {
            get;
        }

        public IReadOnlyList<string>
            TrackStrandOptions
        {
            get;
        }

        public IAsyncRelayCommand RefreshCommand
        {
            get;
        }

        public IAsyncRelayCommand
            CreateSchoolYearCommand
        {
            get;
        }

        public IAsyncRelayCommand
            UseSelectedSchoolYearCommand
        {
            get;
        }

        public IRelayCommand NewClassCommand
        {
            get;
        }

        public IAsyncRelayCommand SaveClassCommand
        {
            get;
        }

        public IAsyncRelayCommand DeleteClassCommand
        {
            get;
        }

        public AcademicYear?
            SelectedAcademicYear
        {
            get => _selectedAcademicYear;

            set
            {
                if (SetProperty(
                        ref _selectedAcademicYear,
                        value))
                {
                    SaveClassCommand
                        .NotifyCanExecuteChanged();

                    UseSelectedSchoolYearCommand
                        .NotifyCanExecuteChanged();

                    _ = LoadClassesAsync();
                }
            }
        }

        public SchoolClass?
            SelectedClass
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

                if (value != null)
                {
                    GradeLevel =
                        value.GradeLevel;

                    SectionName =
                        value.SectionName;

                    Schedule =
                        value.Schedule;

                    Room =
                        value.Room;

                    TrackStrand =
                        string.IsNullOrWhiteSpace(
                            value.TrackStrand)
                            ? "Not Applicable"
                            : value.TrackStrand;
                }
                _ = LoadClassSubjectsAsync();

                DeleteClassCommand
                    .NotifyCanExecuteChanged();

                SaveClassCommand
                    .NotifyCanExecuteChanged();
            }
        }

        public int SchoolYearStart
        {
            get => _schoolYearStart;

            set
            {
                if (SetProperty(
                        ref _schoolYearStart,
                        value))
                {
                    CreateSchoolYearCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public int SchoolYearEnd
        {
            get => _schoolYearEnd;

            set
            {
                if (SetProperty(
                        ref _schoolYearEnd,
                        value))
                {
                    CreateSchoolYearCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string GradeLevel
        {
            get => _gradeLevel;

            set
            {
                if (SetProperty(
                        ref _gradeLevel,
                        value))
                {
                    OnPropertyChanged(
                        nameof(IsSeniorHigh));

                    if (IsSeniorHigh)
                    {
                        if (TrackStrand ==
                "Not Applicable")
                        {
                            TrackStrand =
                                string.Empty;
                        }
                    }
                    else
                    {
                        TrackStrand =
                            "Not Applicable";
                    }

                    SaveClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string TrackStrand
        {
            get => _trackStrand;

            set
            {
                if (SetProperty(
                        ref _trackStrand,
                        value))
                {
                    SaveClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsSeniorHigh =>
            GradeLevel == "Grade 11" ||
            GradeLevel == "Grade 12";

        public string SectionName
        {
            get => _sectionName;

            set
            {
                if (SetProperty(
                        ref _sectionName,
                        value))
                {
                    SaveClassCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string Schedule
        {
            get => _schedule;

            set => SetProperty(
                ref _schedule,
                value);
        }

        public string Room
        {
            get => _room;

            set => SetProperty(
                ref _room,
                value);
        }

        private bool CanCreateSchoolYear()
        {
            return !IsBusy &&
                   _school != null &&
                   SchoolYearStart >= 2000 &&
                   SchoolYearEnd ==
                       SchoolYearStart + 1;
        }

        private bool CanUseSelectedSchoolYear()
        {
            return !IsBusy &&
                   _school != null &&
                   SelectedAcademicYear != null &&
                   !SelectedAcademicYear.IsCurrent;
        }

        private bool CanSaveClass()
        {
            return !IsBusy &&
                   _school != null &&
                   SelectedAcademicYear != null &&
                   !string.IsNullOrWhiteSpace(
                       GradeLevel) &&
                   !string.IsNullOrWhiteSpace(
                       SectionName) &&
                   (!IsSeniorHigh ||
                    !string.IsNullOrWhiteSpace(
                        TrackStrand));
        }

        private bool CanDeleteClass()
        {
            return !IsBusy &&
                   SelectedClass != null;
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
                    "Loading classes...";

                _school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (_school == null)
                {
                    AcademicYears.Clear();
                    Classes.Clear();

                    StatusMessage =
                        "Complete the School Setup first.";

                    _dialogService.ShowWarning(
                        "Save the school information in Settings before creating a class.",
                        "School Setup Required");

                    return;
                }
                _activeTeacher =
    await _teacherRepository
        .GetActiveTeacherAsync(
            _school.Id);

                if (_activeTeacher == null)
                {
                    StatusMessage =
                        "Complete Teacher Information first.";

                    _dialogService.ShowWarning(
                        "Save the teacher information before managing classes.",
                        "Teacher Information Required");

                    return;
                }

                IReadOnlyList<AcademicYear>
                    academicYears =
                        await _academicYearRepository
                            .GetAllAsync(
                                _school.Id);

                AcademicYears.Clear();

                foreach (AcademicYear academicYear
                         in academicYears)
                {
                    AcademicYears.Add(
                        academicYear);
                }

                SelectedAcademicYear =
                    AcademicYears
                        .FirstOrDefault(
                            academicYear =>
                                academicYear.IsCurrent)
                    ?? AcademicYears
                        .FirstOrDefault();

                if (SelectedAcademicYear == null)
                {
                    Classes.Clear();

                    StatusMessage =
                        "Create the current school year.";
                }
                else
                {
                    StatusMessage =
                        "Class records loaded.";
                }
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "Classes could not be loaded.";

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"class records.\n\n" +
                    $"{exception.Message}",
                    "My Classes Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task LoadClassesAsync()
        {
            if (_school == null ||
                SelectedAcademicYear == null)
            {
                Classes.Clear();

                return;
            }

            try
            {
                if (_activeTeacher != null)
                {
                    await _schoolClassRepository
                        .AssignAdviserToActiveClassesAsync(
                            _school.Id,
                            SelectedAcademicYear.Id,
                            _activeTeacher.Id);
                }

                IReadOnlyList<SchoolClass>
                    classes =
                        await _schoolClassRepository
                            .GetByAcademicYearAsync(
                                _school.Id,
                                SelectedAcademicYear.Id);

                Classes.Clear();

                foreach (SchoolClass schoolClass
                         in classes)
                {
                    Classes.Add(
                        schoolClass);
                }
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"selected school year's classes.\n\n" +
                    $"{exception.Message}",
                    "My Classes Error");
            }
        }

        private async Task CreateSchoolYearAsync()
        {
            if (!CanCreateSchoolYear() ||
                _school == null)
            {
                _dialogService.ShowWarning(
                    "Enter a valid consecutive school year, such as 2026 and 2027.",
                    "Invalid School Year");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                IReadOnlyList<AcademicYear>
                    existingSchoolYears =
                        await _academicYearRepository
                            .GetAllAsync(
                                _school.Id);

                AcademicYear? existingSchoolYear =
                    existingSchoolYears.FirstOrDefault(
                        academicYear =>
                            academicYear.StartYear ==
                                SchoolYearStart &&
                            academicYear.EndYear ==
                                SchoolYearEnd);

                if (existingSchoolYear != null)
                {
                    await _academicYearRepository
                        .SetCurrentAsync(
                            _school.Id,
                            existingSchoolYear.Id);

                    StatusMessage =
                        $"School Year {existingSchoolYear.DisplayName} is now active.";

                    await LoadAcademicYearsAsync(
                        existingSchoolYear.StartYear,
                        existingSchoolYear.EndYear);

                    return;
                }

                AcademicYear academicYear =
                    new AcademicYear
                    {
                        SchoolId =
                            _school.Id,

                        StartYear =
                            SchoolYearStart,

                        EndYear =
                            SchoolYearEnd,

                        IsCurrent =
                            true
                    };

                await _academicYearRepository
                    .SaveAsync(
                        academicYear);

                StatusMessage =
                    $"School Year {academicYear.DisplayName} created.";

                await LoadAcademicYearsAsync(
                    academicYear.StartYear,
                    academicYear.EndYear);
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not create the " +
                    $"school year.\n\n" +
                    $"{exception.Message}",
                    "School Year Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task UseSelectedSchoolYearAsync()
        {
            if (!CanUseSelectedSchoolYear() ||
                _school == null ||
                SelectedAcademicYear == null)
            {
                return;
            }

            AcademicYear selectedSchoolYear =
                SelectedAcademicYear;

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                await _academicYearRepository
                    .SetCurrentAsync(
                        _school.Id,
                        selectedSchoolYear.Id);

                StatusMessage =
                    $"School Year {selectedSchoolYear.DisplayName} is now active.";

                await LoadAcademicYearsAsync(
                    selectedSchoolYear.StartYear,
                    selectedSchoolYear.EndYear);

            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not activate the selected school year.\n\n" +
                    $"{errorMessage}",
                    "School Year Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task LoadAcademicYearsAsync(
            int selectedStartYear,
            int selectedEndYear)
        {
            if (_school == null)
            {
                return;
            }

            IReadOnlyList<AcademicYear>
                academicYears =
                    await _academicYearRepository
                        .GetAllAsync(
                            _school.Id);

            AcademicYears.Clear();

            foreach (AcademicYear academicYear
                     in academicYears)
            {
                AcademicYears.Add(
                    academicYear);
            }

            SelectedAcademicYear =
                AcademicYears.FirstOrDefault(
                    academicYear =>
                        academicYear.StartYear ==
                            selectedStartYear &&
                        academicYear.EndYear ==
                            selectedEndYear);
        }

        private async Task SaveClassAsync()
        {
            if (!CanSaveClass() ||
                _school == null ||
                SelectedAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Select a school year, grade level, and enter a section name.",
                    "Required Class Information");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                SchoolClass schoolClass =
                    SelectedClass
                    ?? new SchoolClass();

                schoolClass.SchoolId =
                    _school.Id;

                schoolClass.AcademicYearId =
                    SelectedAcademicYear.Id;
                schoolClass.AdviserId =
    _activeTeacher?.Id;

                schoolClass.GradeLevel =
                    GradeLevel.Trim();

                schoolClass.SectionName =
                    SectionName.Trim();

                schoolClass.KeyStage =
                    GetKeyStage(
                        schoolClass.GradeLevel);

                schoolClass.TrackStrand =
                    IsSeniorHigh
                        ? TrackStrand
                        : "Not Applicable";

                schoolClass.Schedule =
                    Schedule.Trim();

                schoolClass.Room =
                    Room.Trim();

                schoolClass.IsActive =
                    true;

                schoolClass =
                    await _schoolClassRepository
                        .SaveAsync(
                            schoolClass);
                await _subjectRepository
    .AssignGradeLevelSubjectsAsync(
        schoolClass.Id,
        schoolClass.GradeLevel,
        schoolClass.TrackStrand);

                StatusMessage =
                    $"{schoolClass.DisplayName} saved successfully.";

                ClearClassForm();

                await LoadClassesAsync();
            }
            catch (Exception exception)
            {
                string errorMessage =
    exception.InnerException?.Message
    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"class record.\n\n" +
                    $"{errorMessage}",
                    "My Classes Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task DeleteSelectedClassAsync()
        {
            if (SelectedClass == null)
            {
                return;
            }

            bool confirmed =
                _dialogService.Confirm(
                    $"Remove {SelectedClass.DisplayName}?\n\n" +
                    $"A class with learners will be archived instead of permanently deleted.",
                    "Remove Class");

            if (!confirmed)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                await _schoolClassRepository
                    .DeleteAsync(
                        SelectedClass.Id);

                StatusMessage =
                    "Class removed successfully.";

                ClearClassForm();

                await LoadClassesAsync();
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not remove the " +
                    $"class record.\n\n" +
                    $"{exception.Message}",
                    "Remove Class Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void ClearClassForm()
        {
            SelectedClass =
                null;

            GradeLevel =
                string.Empty;

            SectionName =
                string.Empty;

            TrackStrand =
                "Not Applicable";

            Schedule =
                string.Empty;

            Room =
                string.Empty;
        }

        private void NotifyCommandStates()
        {
            CreateSchoolYearCommand
                .NotifyCanExecuteChanged();

            UseSelectedSchoolYearCommand
                .NotifyCanExecuteChanged();

            SaveClassCommand
                .NotifyCanExecuteChanged();

            DeleteClassCommand
                .NotifyCanExecuteChanged();
        }

        private static string GetKeyStage(
            string gradeLevel)
        {
            return gradeLevel switch
            {
                "Kindergarten" =>
                    "Kindergarten",

                "Grade 1" or
                "Grade 2" or
                "Grade 3" =>
                    "Key Stage 1",

                "Grade 4" or
                "Grade 5" or
                "Grade 6" =>
                    "Key Stage 2",

                "Grade 7" or
                "Grade 8" or
                "Grade 9" or
                "Grade 10" =>
                    "Key Stage 3",

                "Grade 11" or
                "Grade 12" =>
                    "Key Stage 4",

                _ =>
                    string.Empty
            };
        }
    }
}
