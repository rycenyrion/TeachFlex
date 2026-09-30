using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        [RelayCommand]
        private async Task ExportKindergartenProgressReportAsync()
        {
            if (!CanPrepareKindergartenProgressReport ||
                _currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null ||
                SelectedSf9Learner == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class and learner first.",
                    "Kindergarten Progress Report");
                return;
            }

            try
            {
                IsBusy = true;
                NotifySchoolFormAvailability();
                StatusMessage =
                    "Preparing the Kindergarten Progress Report data...";

                IReadOnlyList<KindergartenCompetency> competencies =
                    await _kindergartenRecordRepository
                        .GetCompetenciesAsync();

                if (competencies.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "No Kindergarten competencies were found.",
                        "Kindergarten Progress Report");
                    return;
                }

                List<KindergartenLearnerRating> ratings =
                    new List<KindergartenLearnerRating>();

                for (int term = 1; term <= 3; term++)
                {
                    IReadOnlyList<KindergartenLearnerRating> termRatings =
                        await _kindergartenRecordRepository
                            .GetRatingsAsync(
                                SelectedClass.Id,
                                term);

                    ratings.AddRange(
                        termRatings.Where(
                            item => item.LearnerId == SelectedSf9Learner.Id));
                }

                IReadOnlyList<KindergartenTermRemark> classRemarks =
                    await _kindergartenRecordRepository
                        .GetTermRemarksAsync(SelectedClass.Id);

                List<KindergartenTermRemark> learnerRemarks =
                    classRemarks
                        .Where(
                            item => item.LearnerId == SelectedSf9Learner.Id)
                        .OrderBy(item => item.TermNumber)
                        .ToList();

                List<SF9AttendanceRow> attendanceRows =
                    await CreateSf9AttendanceRowsAsync(
                        SelectedClass.Id,
                        SelectedSf9Learner.Id,
                        _currentAcademicYear);

                SF9ExportRequest baseRequest =
                    new SF9ExportRequest
                    {
                        School = _currentSchool,
                        AcademicYear = _currentAcademicYear,
                        SchoolClass = SelectedClass,
                        Learner = SelectedSf9Learner,
                        SubjectGrades = new List<SF9SubjectGradeRow>(),
                        AttendanceRows = attendanceRows,
                        AdviserName =
                            SelectedClass.Adviser?.FullName
                            ?? string.Empty,
                        SchoolHeadName = _currentSchool.SchoolHead,
                        GeneratedOn = DateTime.Now
                    };

                KindergartenProgressReportRequest request =
                    new KindergartenProgressReportRequest
                    {
                        BaseRequest = baseRequest,
                        Competencies = competencies,
                        Ratings = ratings,
                        Remarks = learnerRemarks
                    };

                SaveFileDialog saveDialog =
                    new SaveFileDialog
                    {
                        Title = "Export Kindergarten Progress Report",
                        Filter = "PDF Document (*.pdf)|*.pdf",
                        DefaultExt = ".pdf",
                        AddExtension = true,
                        OverwritePrompt = true,
                        FileName = CreateSafeSf9FileName(
                            $"Kindergarten Progress Report - " +
                            $"{SelectedSf9Learner.OfficialName} - " +
                            $"{_currentAcademicYear.DisplayName}.pdf")
                    };

                if (saveDialog.ShowDialog() != true)
                {
                    StatusMessage =
                        "Kindergarten Progress Report export cancelled.";
                    return;
                }

                string preparedFile =
                    _kindergartenProgressReportPdfService.ExportToPdf(
                        request,
                        saveDialog.FileName);

                StatusMessage =
                    "Kindergarten Progress Report exported successfully.";

                System.Windows.MessageBoxResult openResult =
                    System.Windows.MessageBox.Show(
                        "The two-page Kindergarten Progress Report was " +
                        "created successfully.\n\n" +
                        preparedFile +
                        "\n\nDo you want to open it now?",
                        "Kindergarten Progress Report Ready",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                if (openResult == System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo(preparedFile)
                        {
                            UseShellExecute = true
                        });
                }
            }
            catch (Exception exception)
            {
                string message =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The Kindergarten Progress Report could not be created.";

                _dialogService.ShowError(
                    "TeachFlex could not create the Kindergarten " +
                    "Progress Report.\n\n" + message,
                    "Kindergarten Progress Report Error");
            }
            finally
            {
                IsBusy = false;
                NotifySchoolFormAvailability();
            }
        }
    }
}
