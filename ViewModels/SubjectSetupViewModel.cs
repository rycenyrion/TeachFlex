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
    public class SubjectSetupViewModel :
        ViewModelBase
    {
        private readonly ISubjectRepository
            _subjectRepository;

        private readonly IDialogService
            _dialogService;

        private Subject?
            _selectedSubject;

        private string _selectedGradeLevel =
            "Grade 1";

        private string _subjectName =
            string.Empty;

        private string _learningArea =
            string.Empty;

        private string _subjectCategory =
            "Core";

        private string _trackStrand =
            "All";

        private int _displayOrder =
            1;

        private bool _includeInSf9;

        private bool _includeInGeneralAverage;

        public SubjectSetupViewModel(
            ISubjectRepository subjectRepository,
            IDialogService dialogService)
        {
            _subjectRepository =
                subjectRepository;

            _dialogService =
                dialogService;

            Subjects =
                new ObservableCollection<Subject>();

            GradeLevels =
                new[]
                {
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

            SubjectCategories =
                new[]
                {
                    "Core",
                    "Applied",
                    "Specialized"
                };

            TrackStrandOptions =
                new[]
                {
                    "All",
                    "STEM",
                    "ABM",
                    "HUMSS",
                    "GAS",
                    "TVL",
                    "Sports",
                    "Arts and Design"
                };

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadSubjectsAsync);

            NewSubjectCommand =
                new RelayCommand(
                    ClearForm);

            SaveSubjectCommand =
                new AsyncRelayCommand(
                    SaveSubjectAsync,
                    CanSaveSubject);

            DeactivateSubjectCommand =
                new AsyncRelayCommand(
                    DeactivateSubjectAsync,
                    CanDeactivateSubject);

            _ = LoadSubjectsAsync();
        }

        public ObservableCollection<Subject>
            Subjects
        {
            get;
        }

        public IReadOnlyList<string>
            GradeLevels
        {
            get;
        }

        public IReadOnlyList<string>
            SubjectCategories
        {
            get;
        }

        public IReadOnlyList<string>
            TrackStrandOptions
        {
            get;
        }

        public IAsyncRelayCommand
            RefreshCommand
        {
            get;
        }

        public IRelayCommand
            NewSubjectCommand
        {
            get;
        }

        public IAsyncRelayCommand
            SaveSubjectCommand
        {
            get;
        }

        public IAsyncRelayCommand
            DeactivateSubjectCommand
        {
            get;
        }

        public Subject? SelectedSubject
        {
            get => _selectedSubject;

            set
            {
                if (!SetProperty(
                        ref _selectedSubject,
                        value))
                {
                    return;
                }

                if (value != null)
                {
                    LoadSubjectIntoForm(
                        value);
                }

                NotifyCommandStates();
            }
        }

        public string SelectedGradeLevel
        {
            get => _selectedGradeLevel;

            set
            {
                if (!SetProperty(
                        ref _selectedGradeLevel,
                        value))
                {
                    return;
                }

                OnPropertyChanged(
                    nameof(IsSeniorHigh));

                OnPropertyChanged(
                    nameof(IsJuniorHigh));

                OnPropertyChanged(
                    nameof(CanConfigureSf9Options));

                ClearForm();

                _ = LoadSubjectsAsync();
            }
        }

        public string SubjectName
        {
            get => _subjectName;

            set
            {
                if (SetProperty(
                        ref _subjectName,
                        value))
                {
                    SaveSubjectCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string LearningArea
        {
            get => _learningArea;

            set => SetProperty(
                ref _learningArea,
                value);
        }

        public string SubjectCategory
        {
            get => _subjectCategory;

            set => SetProperty(
                ref _subjectCategory,
                value);
        }

        public string TrackStrand
        {
            get => _trackStrand;

            set => SetProperty(
                ref _trackStrand,
                value);
        }

        public int DisplayOrder
        {
            get => _displayOrder;

            set => SetProperty(
                ref _displayOrder,
                value);
        }

        public bool IncludeInSf9
        {
            get => _includeInSf9;

            set
            {
                if (!SetProperty(
                        ref _includeInSf9,
                        value))
                {
                    return;
                }

                if (!value)
                {
                    IncludeInGeneralAverage =
                        false;
                }
            }
        }

        public bool IncludeInGeneralAverage
        {
            get => _includeInGeneralAverage;

            set
            {
                if (SetProperty(
                        ref _includeInGeneralAverage,
                        value) &&
                    value)
                {
                    IncludeInSf9 =
                        true;
                }
            }
        }

        public bool IsSeniorHigh =>
            SelectedGradeLevel == "Grade 11" ||
            SelectedGradeLevel == "Grade 12";

        public bool IsJuniorHigh =>
            SelectedGradeLevel is
                "Grade 7" or
                "Grade 8" or
                "Grade 9" or
                "Grade 10";

        public bool CanConfigureSf9Options =>
            IsJuniorHigh &&
            (
                SelectedSubject == null ||
                IsAdditionalSubjectCategory(
                    SelectedSubject.SubjectCategory)
            );

        private bool CanSaveSubject()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(
                       SelectedGradeLevel) &&
                   !string.IsNullOrWhiteSpace(
                       SubjectName);
        }

        private bool CanDeactivateSubject()
        {
            return !IsBusy &&
                   SelectedSubject != null;
        }

        private async Task LoadSubjectsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                IReadOnlyList<Subject> subjects =
                    await _subjectRepository
                        .GetByGradeLevelAsync(
                            SelectedGradeLevel);

                Subjects.Clear();

                foreach (Subject subject
                         in subjects)
                {
                    Subjects.Add(
                        subject);
                }

                StatusMessage =
                    $"{Subjects.Count} subject(s) loaded.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"subjects.\n\n" +
                    $"{exception.Message}",
                    "Subject Setup Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task SaveSubjectAsync()
        {
            if (!CanSaveSubject())
            {
                _dialogService.ShowWarning(
                    "Grade Level and Subject Name are required.",
                    "Required Subject Information");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                Subject subject =
                    SelectedSubject
                    ?? new Subject
                    {
                        SubjectCode =
                            CreateSubjectCode(
                                SelectedGradeLevel,
                                SubjectName),

                        GradeLevel =
                            SelectedGradeLevel,

                        IsActive =
                            true
                    };

                subject.SubjectName =
                    SubjectName.Trim();

                subject.GradeLevel =
                    SelectedGradeLevel;

                subject.LearningArea =
                    LearningArea.Trim();

                subject.DisplayOrder =
                    DisplayOrder < 1
                        ? 1
                        : DisplayOrder;

                subject.SubjectCategory =
                    IsSeniorHigh
                        ? SubjectCategory
                        : IsJuniorHigh &&
                          (
                              SelectedSubject == null ||
                              IsAdditionalSubjectCategory(
                                  SelectedSubject.SubjectCategory)
                          )
                            ? GetAdditionalSubjectCategory()
                            : "Core";

                subject.IsCoreSubject =
                    subject.SubjectCategory ==
                        "Core";

                subject.TrackStrand =
                    IsSeniorHigh
                        ? TrackStrand
                        : "All";

                subject.IsActive =
                    true;

                await _subjectRepository
                    .SaveAsync(
                        subject);

                string savedSubjectName =
    subject.SubjectName;

                ClearForm();

                IsBusy =
                    false;

                await LoadSubjectsAsync();

                StatusMessage =
                    $"{savedSubjectName} saved successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"subject.\n\n" +
                    $"{errorMessage}",
                    "Save Subject Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task DeactivateSubjectAsync()
        {
            if (SelectedSubject == null)
            {
                return;
            }

            string subjectName =
                SelectedSubject.SubjectName;

            bool confirmed =
                _dialogService.Confirm(
                    $"Deactivate {subjectName}?\n\n" +
                    $"It will no longer be assigned to new classes.",
                    "Deactivate Subject");

            if (!confirmed)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                SelectedSubject.IsActive =
                    false;

                await _subjectRepository
                    .SaveAsync(
                        SelectedSubject);

                ClearForm();

                IsBusy =
                    false;

                await LoadSubjectsAsync();

                StatusMessage =
                    $"{subjectName} deactivated successfully.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not deactivate the " +
                    $"subject.\n\n" +
                    $"{exception.Message}",
                    "Deactivate Subject Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void LoadSubjectIntoForm(
            Subject subject)
        {
            SubjectName =
                subject.SubjectName;

            LearningArea =
                subject.LearningArea;

            SubjectCategory =
                string.IsNullOrWhiteSpace(
                    subject.SubjectCategory)
                    ? "Core"
                    : subject.SubjectCategory;

            TrackStrand =
                string.IsNullOrWhiteSpace(
                    subject.TrackStrand)
                    ? "All"
                    : subject.TrackStrand;

            DisplayOrder =
                subject.DisplayOrder;

            IncludeInSf9 =
                subject.SubjectCategory.Equals(
                    "Additional - SF9 Only",
                    StringComparison.OrdinalIgnoreCase) ||
                subject.SubjectCategory.Equals(
                    "Additional - SF9 and General Average",
                    StringComparison.OrdinalIgnoreCase) ||
                !IsAdditionalSubjectCategory(
                    subject.SubjectCategory);

            IncludeInGeneralAverage =
                subject.SubjectCategory.Equals(
                    "Additional - SF9 and General Average",
                    StringComparison.OrdinalIgnoreCase) ||
                !IsAdditionalSubjectCategory(
                    subject.SubjectCategory);

            OnPropertyChanged(
                nameof(CanConfigureSf9Options));
        }

        private void ClearForm()
        {
            SelectedSubject =
                null;

            SubjectName =
                string.Empty;

            LearningArea =
                string.Empty;

            SubjectCategory =
                "Core";

            TrackStrand =
                "All";

            DisplayOrder =
                Subjects.Count + 1;

            IncludeInSf9 =
                false;

            IncludeInGeneralAverage =
                false;

            OnPropertyChanged(
                nameof(CanConfigureSf9Options));

            NotifyCommandStates();
        }

        private void NotifyCommandStates()
        {
            SaveSubjectCommand
                .NotifyCanExecuteChanged();

            DeactivateSubjectCommand
                .NotifyCanExecuteChanged();
        }

        private static string CreateSubjectCode(
            string gradeLevel,
            string subjectName)
        {
            string gradeCode =
                gradeLevel.Replace(
                    "Grade ",
                    "G",
                    StringComparison.OrdinalIgnoreCase);

            string nameCode =
                new string(
                    subjectName
                        .Where(
                            char.IsLetterOrDigit)
                        .Take(18)
                        .ToArray())
                    .ToUpperInvariant();

            string uniquePart =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 6)
                    .ToUpperInvariant();

            return
                $"{gradeCode}-{nameCode}-{uniquePart}";
        }

        private string GetAdditionalSubjectCategory()
        {
            if (IncludeInGeneralAverage)
            {
                return "Additional - SF9 and General Average";
            }

            return IncludeInSf9
                ? "Additional - SF9 Only"
                : "Additional - ECR Only";
        }

        private static bool IsAdditionalSubjectCategory(
            string? category)
        {
            return category?.StartsWith(
                       "Additional -",
                       StringComparison.OrdinalIgnoreCase)
                   == true;
        }
    }
}
