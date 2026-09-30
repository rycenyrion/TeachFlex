using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;
using TeachFlex.Repositories;

namespace TeachFlex.ViewModels
{
    public partial class SchoolSetupViewModel
    {
        private readonly ITeacherRepository
            _teacherRepository;

        private Teacher?
            _teacher;

        private string _employeeNumber =
            string.Empty;

        private string _teacherFirstName =
            string.Empty;

        private string _teacherMiddleName =
            string.Empty;

        private string _teacherLastName =
            string.Empty;

        private string _teacherSuffix =
            string.Empty;

        private string _teacherPosition =
            "Teacher I";

        private string _pagIbigNumber =
            string.Empty;

        private string _umidNumber =
            string.Empty;

        private string _sssNumber =
            string.Empty;

        private string _philHealthNumber =
            string.Empty;

        private string _emailAddress =
            string.Empty;

        private string _contactNumber =
            string.Empty;

        private string _profileImagePath =
            string.Empty;

        public IReadOnlyList<string>
            TeacherPositions
        {
            get;
        } =
            new[]
            {
                "Teacher I",
                "Teacher II",
                "Teacher III",
                "Teacher IV",
                "Teacher V",
                "Teacher VI",
                "Teacher VII",
                "Master Teacher I",
                "Master Teacher II",
                "Master Teacher III",
                "Master Teacher IV",
                "Master Teacher V"
            };

        public IAsyncRelayCommand
            SaveTeacherCommand
        {
            get;
        }

        public string EmployeeNumber
        {
            get => _employeeNumber;

            set => SetProperty(
                ref _employeeNumber,
                value);
        }

        public string TeacherFirstName
        {
            get => _teacherFirstName;

            set
            {
                if (SetProperty(
                        ref _teacherFirstName,
                        value))
                {
                    SaveTeacherCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string TeacherMiddleName
        {
            get => _teacherMiddleName;

            set => SetProperty(
                ref _teacherMiddleName,
                value);
        }

        public string TeacherLastName
        {
            get => _teacherLastName;

            set
            {
                if (SetProperty(
                        ref _teacherLastName,
                        value))
                {
                    SaveTeacherCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        public string TeacherSuffix
        {
            get => _teacherSuffix;

            set => SetProperty(
                ref _teacherSuffix,
                value);
        }

        public string TeacherPosition
        {
            get => _teacherPosition;

            set => SetProperty(
                ref _teacherPosition,
                value);
        }

        public string PagIbigNumber
        {
            get => _pagIbigNumber;

            set => SetProperty(
                ref _pagIbigNumber,
                value);
        }

        public string UmidNumber
        {
            get => _umidNumber;

            set => SetProperty(
                ref _umidNumber,
                value);
        }

        public string SssNumber
        {
            get => _sssNumber;

            set => SetProperty(
                ref _sssNumber,
                value);
        }

        public string PhilHealthNumber
        {
            get => _philHealthNumber;

            set => SetProperty(
                ref _philHealthNumber,
                value);
        }

        public string EmailAddress
        {
            get => _emailAddress;

            set => SetProperty(
                ref _emailAddress,
                value);
        }

        public string ContactNumber
        {
            get => _contactNumber;

            set => SetProperty(
                ref _contactNumber,
                value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;

            set => SetProperty(
                ref _profileImagePath,
                value);
        }

        private bool CanSaveTeacher()
        {
            return !IsBusy &&
                   _school != null &&
                   !string.IsNullOrWhiteSpace(
                       TeacherFirstName) &&
                   !string.IsNullOrWhiteSpace(
                       TeacherLastName);
        }

        private async Task LoadTeacherAsync()
        {
            if (_school == null)
            {
                return;
            }

            _teacher =
                await _teacherRepository
                    .GetActiveTeacherAsync(
                        _school.Id);

            if (_teacher == null)
            {
                return;
            }

            EmployeeNumber =
                _teacher.EmployeeNumber;

            TeacherFirstName =
                _teacher.FirstName;

            TeacherMiddleName =
                _teacher.MiddleName;

            TeacherLastName =
                _teacher.LastName;

            TeacherSuffix =
                _teacher.Suffix;

            TeacherPosition =
                _teacher.Position;

            PagIbigNumber =
                _teacher.PagIbigNumber;

            UmidNumber =
                _teacher.UmidNumber;

            SssNumber =
                _teacher.SssNumber;

            PhilHealthNumber =
                _teacher.PhilHealthNumber;

            EmailAddress =
                _teacher.EmailAddress;

            ContactNumber =
                _teacher.ContactNumber;

            ProfileImagePath =
                _teacher.ProfileImagePath;
        }

        private async Task SaveTeacherAsync()
        {
            if (!CanSaveTeacher() ||
                _school == null)
            {
                _dialogService.ShowWarning(
                    "Save the School Information first, then enter the teacher's First Name and Last Name.",
                    "Required Teacher Information");

                return;
            }

            try
            {
                IsBusy =
                    true;

                SaveTeacherCommand
                    .NotifyCanExecuteChanged();

                StatusMessage =
                    "Saving teacher information...";

                _teacher ??=
                    new Teacher
                    {
                        SchoolId =
                            _school.Id
                    };

                _teacher.EmployeeNumber =
                    EmployeeNumber.Trim();

                _teacher.FirstName =
                    TeacherFirstName.Trim();

                _teacher.MiddleName =
                    TeacherMiddleName.Trim();

                _teacher.LastName =
                    TeacherLastName.Trim();

                _teacher.Suffix =
                    TeacherSuffix.Trim();

                _teacher.Position =
                    TeacherPosition;

                _teacher.PagIbigNumber =
                    PagIbigNumber.Trim();

                _teacher.UmidNumber =
                    UmidNumber.Trim();

                _teacher.SssNumber =
                    SssNumber.Trim();

                _teacher.PhilHealthNumber =
                    PhilHealthNumber.Trim();

                _teacher.EmailAddress =
                    EmailAddress.Trim();

                _teacher.ContactNumber =
                    ContactNumber.Trim();

                _teacher.ProfileImagePath =
                    ProfileImagePath.Trim();

                _teacher.IsActive =
                    true;

                _teacher =
                    await _teacherRepository
                        .SaveAsync(
                            _teacher);

                StatusMessage =
                    "Teacher information saved successfully.";

                _dialogService.ShowInformation(
                    "The teacher information was saved successfully.",
                    "Teacher Information");
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "Teacher information could not be saved.";

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"teacher information.\n\n" +
                    $"{exception.Message}",
                    "Teacher Information Error");
            }
            finally
            {
                IsBusy =
                    false;

                SaveTeacherCommand
                    .NotifyCanExecuteChanged();
            }
        }
    }
}