using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private async Task
            ExportOfficialKindergartenECRAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class first.",
                    "Kindergarten E-Class Record");

                return;
            }

            if (_currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Create the current School Year first.",
                    "Kindergarten E-Class Record");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    "Preparing Kindergarten E-Class Record data...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    _dialogService.ShowWarning(
                        "Complete School Setup first.",
                        "Kindergarten E-Class Record");

                    return;
                }

                IReadOnlyList<Learner>
    loadedLearners =
        await _learnerRepository
            .GetByClassAsync(
                SelectedClass.Id);

                List<Learner> learners =
                    loadedLearners
                        .OrderBy(
                            learner =>
                                learner.LastName)
                        .ThenBy(
                            learner =>
                                learner.FirstName)
                        .ThenBy(
                            learner =>
                                learner.MiddleName)
                        .ToList();

                if (learners.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "This Kindergarten class has no active learners.",
                        "Kindergarten E-Class Record");

                    return;
                }

                IReadOnlyList<KindergartenCompetency>
                    competencies =
                        await _kindergartenRecordRepository
                            .GetCompetenciesAsync();

                if (competencies.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "No Kindergarten competencies were found.",
                        "Kindergarten E-Class Record");

                    return;
                }

                KindergartenEcrExportRequest request =
                    new KindergartenEcrExportRequest
                    {
                        SchoolId =
                            school.SchoolId,

                        SchoolName =
                            school.SchoolName,

                        Region =
                            school.Region,

                        Division =
                            school.Division,

                        CityMunicipality =
                            school.SchoolAddress,

                        District =
                            school.District,

                        SchoolYear =
                            _currentAcademicYear.DisplayName,

                        GradeLevel =
                            SelectedClass.GradeLevel,

                        SectionName =
                            SelectedClass.SectionName,

                        AdviserName =
                            SelectedClass.Adviser?.FullName
                            ?? string.Empty,

                        SchoolHeadName =
                            school.SchoolHead,

                        SchoolLogoPath =
                            school.SchoolLogoPath
                    };

                foreach (Learner learner in learners)
                {
                    request.Learners.Add(
                        new KindergartenEcrLearnerExportRow
                        {
                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
    learner.OfficialName,

                            Sex =
                                learner.Sex,

                            BirthDate =
                                learner.BirthDate
                        });
                }

                foreach (
                    KindergartenCompetency competency
                    in competencies)
                {
                    request.Competencies.Add(
                        new KindergartenEcrCompetencyExportRow
                        {
                            KindergartenCompetencyId =
                                competency.Id,

                            CompetencyCode =
                                competency.CompetencyCode,

                            DisplayOrder =
                                competency.DisplayOrder
                        });
                }

                for (int termNumber = 1;
                     termNumber <= 3;
                     termNumber++)
                {
                    IReadOnlyList<
                        KindergartenLearnerRating>
                            ratings =
                                await
                                    _kindergartenRecordRepository
                                        .GetRatingsAsync(
                                            SelectedClass.Id,
                                            termNumber);

                    foreach (
                        KindergartenLearnerRating rating
                        in ratings)
                    {
                        request.Ratings.Add(
                            new KindergartenEcrRatingExportRow
                            {
                                LearnerId =
                                    rating.LearnerId,

                                KindergartenCompetencyId =
                                    rating
                                        .KindergartenCompetencyId,

                                TermNumber =
                                    rating.TermNumber,

                                Rating =
                                    rating.Rating,

                                Observation =
                                    rating.Observation
                            });
                    }
                }

                IReadOnlyList<KindergartenTermRemark>
                    remarks =
                        await _kindergartenRecordRepository
                            .GetTermRemarksAsync(
                                SelectedClass.Id);

                foreach (
                    KindergartenTermRemark remark
                    in remarks)
                {
                    request.Remarks.Add(
                        new KindergartenEcrRemarkExportRow
                        {
                            LearnerId =
                                remark.LearnerId,

                            TermNumber =
                                remark.TermNumber,

                            TeacherComment =
                                remark.TeacherComment,

                            LearnerStrengths =
                                remark.LearnerStrengths,

                            SuggestedInterventions =
                                remark.SuggestedInterventions
                        });
                }

                string safeSection =
                    MakeSafeECRFileName(
                        SelectedClass.SectionName);

                string suggestedFileName =
                    $"ECR_Kindergarten_" +
                    $"{safeSection}_" +
                    $"{_currentAcademicYear.DisplayName}.xlsx";

                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Save Official Kindergarten E-Class Record",

                        Filter =
                            "Excel Workbook (*.xlsx)|*.xlsx",

                        DefaultExt =
                            ".xlsx",

                        AddExtension =
                            true,

                        FileName =
                            suggestedFileName,

                        OverwritePrompt =
                            true
                    };

                bool? dialogResult =
                    saveFileDialog.ShowDialog();

                if (dialogResult != true)
                {
                    StatusMessage =
                        "Kindergarten E-Class Record export cancelled.";

                    return;
                }

                StatusMessage =
                    "Writing the official Kindergarten workbook...";

                string preparedFile =
                    _kindergartenEcrExportService
                        .PrepareOfficialKindergartenECR(
                            request,
                            saveFileDialog.FileName);

                StatusMessage =
                    "Kindergarten E-Class Record prepared: " +
                    preparedFile;

                System.Windows.MessageBoxResult openResult =
                    System.Windows.MessageBox.Show(
                        "The official Kindergarten E-Class Record " +
                        "was prepared successfully.\n\n" +
                        preparedFile +
                        "\n\nDo you want to open it now?",
                        "Kindergarten E-Class Record Ready",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                if (openResult ==
                    System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo(
                            preparedFile)
                        {
                            UseShellExecute =
                                true
                        });
                }
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    "TeachFlex could not prepare the official " +
                    "Kindergarten E-Class Record.\n\n" +
                    errorMessage,
                    "Kindergarten E-Class Record Export Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }
    }
}