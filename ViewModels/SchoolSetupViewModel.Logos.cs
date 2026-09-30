using System;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class SchoolSetupViewModel
    {
        private readonly ILogoService
            _logoService;

        private string _depEdLogoPath =
            string.Empty;

        private string _schoolLogoPath =
            string.Empty;

        public IRelayCommand
            SelectDepEdLogoCommand
        {
            get;
        }

        public IRelayCommand
            SelectSchoolLogoCommand
        {
            get;
        }
        public IRelayCommand
    SelectTeacherProfileImageCommand
        {
            get;
        }

        public string DepEdLogoPath
        {
            get => _depEdLogoPath;

            private set => SetProperty(
                ref _depEdLogoPath,
                value);
        }
        private void SelectTeacherProfileImage()
        {
            try
            {
                string? selectedPath =
                    _logoService
                        .SelectAndSaveTeacherProfileImage();

                if (string.IsNullOrWhiteSpace(
                        selectedPath))
                {
                    return;
                }

                ProfileImagePath =
                    selectedPath;

                StatusMessage =
                    "Teacher profile picture selected. " +
                    "Click Save Teacher Information.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"teacher profile picture.\n\n" +
                    $"{exception.Message}",
                    "Teacher Profile Picture Error");
            }
        }
        public string SchoolLogoPath
        {
            get => _schoolLogoPath;

            private set => SetProperty(
                ref _schoolLogoPath,
                value);
        }

        private void SelectDepEdLogo()
        {
            try
            {
                string? selectedPath =
                    _logoService
                        .SelectAndSaveDepEdLogo();

                if (string.IsNullOrWhiteSpace(
                        selectedPath))
                {
                    return;
                }

                DepEdLogoPath =
                    selectedPath;

                StatusMessage =
                    "DepEd logo selected. Click Save School Information.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"DepEd logo.\n\n" +
                    $"{exception.Message}",
                    "DepEd Logo Error");
            }
        }

        private void SelectSchoolLogo()
        {
            try
            {
                string? selectedPath =
                    _logoService
                        .SelectAndSaveSchoolLogo();

                if (string.IsNullOrWhiteSpace(
                        selectedPath))
                {
                    return;
                }

                SchoolLogoPath =
                    selectedPath;

                StatusMessage =
                    "School logo selected. Click Save School Information.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"school logo.\n\n" +
                    $"{exception.Message}",
                    "School Logo Error");
            }
        }
    }
}