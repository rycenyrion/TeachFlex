using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class SchoolSetupViewModel :
     ViewModelBase
    {
        private readonly ISchoolRepository
            _schoolRepository;

        private readonly IDialogService
            _dialogService;

        private School?
            _school;

        private string _schoolName =
            string.Empty;

        private string _schoolId =
            string.Empty;

        private string _region =
            string.Empty;

        private string _division =
            string.Empty;

        private string _district =
            string.Empty;

        private string _schoolAddress =
            string.Empty;

        private string _schoolHead =
            string.Empty;

        private string _schoolClassification =
            "Public School";

        public SchoolSetupViewModel(
    ISchoolRepository schoolRepository,
    ITeacherRepository teacherRepository,
    IDialogService dialogService,
    ILogoService logoService)
        {
            _schoolRepository =
                schoolRepository;
            _teacherRepository =
    teacherRepository;

            _dialogService =
                dialogService;
            _logoService =
    logoService;

            SelectDepEdLogoCommand =
                new RelayCommand(
                    SelectDepEdLogo);

            SelectSchoolLogoCommand =
                new RelayCommand(
                    SelectSchoolLogo);

            SelectTeacherProfileImageCommand =
    new RelayCommand(
        SelectTeacherProfileImage);

            SaveTeacherCommand =
    new AsyncRelayCommand(
        SaveTeacherAsync,
        CanSaveTeacher);

            SaveCommand =
                new AsyncRelayCommand(
                    SaveAsync,
                    CanSave);

            RefreshCommand =
                new AsyncRelayCommand(
                    LoadAsync);

            _ = LoadAsync();
        }

        public IAsyncRelayCommand SaveCommand
        {
            get;
        }

        public IAsyncRelayCommand RefreshCommand
        {
            get;
        }

        public string SchoolName
        {
            get => _schoolName;

            set
            {
                if (SetProperty(
                        ref _schoolName,
                        value))
                {
                    SaveCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string SchoolId
        {
            get => _schoolId;

            set
            {
                if (SetProperty(
                        ref _schoolId,
                        value))
                {
                    SaveCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string Region
        {
            get => _region;

            set => SetProperty(
                ref _region,
                value);
        }

        public string Division
        {
            get => _division;

            set => SetProperty(
                ref _division,
                value);
        }

        public string District
        {
            get => _district;

            set => SetProperty(
                ref _district,
                value);
        }

        public string SchoolAddress
        {
            get => _schoolAddress;

            set => SetProperty(
                ref _schoolAddress,
                value);
        }

        public string SchoolHead
        {
            get => _schoolHead;

            set => SetProperty(
                ref _schoolHead,
                value);
        }

        public string SchoolClassification
        {
            get => _schoolClassification;

            set => SetProperty(
                ref _schoolClassification,
                value);
        }

        private bool CanSave()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(
                       SchoolName) &&
                   !string.IsNullOrWhiteSpace(
                       SchoolId);
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
                    "Loading school information...";

                _school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (_school == null)
                {
                    StatusMessage =
                        "Enter the school information.";

                    return;
                }

                SchoolName =
                    _school.SchoolName;

                SchoolId =
                    _school.SchoolId;

                Region =
                    _school.Region;

                Division =
                    _school.Division;

                District =
                    _school.District;

                SchoolAddress =
                    _school.SchoolAddress;

                SchoolHead =
                    _school.SchoolHead;

                SchoolClassification =
                    _school.SchoolClassification;
                DepEdLogoPath =
    _school.DepEdLogoPath;

                SchoolLogoPath =
                    _school.SchoolLogoPath;
                await LoadTeacherAsync();

                StatusMessage =
                    "School information loaded.";
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "School information could not be loaded.";

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"school information.\n\n" +
                    $"{exception.Message}",
                    "School Setup Error");
            }
            finally
            {
                IsBusy =
                    false;

                SaveCommand
                    .NotifyCanExecuteChanged();
            }
        }

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                _dialogService.ShowWarning(
                    "School Name and School ID are required.",
                    "Required Information");

                return;
            }

            try
            {
                IsBusy =
                    true;

                SaveCommand
                    .NotifyCanExecuteChanged();

                StatusMessage =
                    "Saving school information...";

                _school ??=
                    new School();

                _school.SchoolName =
                    SchoolName.Trim();

                _school.SchoolId =
                    SchoolId.Trim();

                _school.Region =
                    Region.Trim();

                _school.Division =
                    Division.Trim();

                _school.District =
                    District.Trim();

                _school.SchoolAddress =
                    SchoolAddress.Trim();

                _school.SchoolHead =
                    SchoolHead.Trim();

                _school.SchoolClassification =
                    SchoolClassification.Trim();
                _school.DepEdLogoPath =
    DepEdLogoPath;

                _school.SchoolLogoPath =
                    SchoolLogoPath;

                _school.IsActive =
                    true;

                _school =
                    await _schoolRepository
                        .SaveAsync(
                            _school);
                SaveTeacherCommand
    .NotifyCanExecuteChanged();

                StatusMessage =
                    "School information saved successfully.";

                _dialogService.ShowInformation(
                    "The school information was saved successfully.",
                    "School Setup");
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "School information could not be saved.";

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"school information.\n\n" +
                    $"{exception.Message}",
                    "School Setup Error");
            }
            finally
            {
                IsBusy =
                    false;

                SaveCommand
                    .NotifyCanExecuteChanged();
            }
        }
    }
}