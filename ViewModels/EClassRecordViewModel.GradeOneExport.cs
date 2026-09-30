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
        private async Task ExportOfficialGradeOnePaceAsync()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                _currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Select a Grade 1 class and subject first.",
                    "Official Grade 1 E-Class Record");

                return;
            }

            try
            {
                IsBusy = true;
                NotifyCommandStates();

                StatusMessage =
                    "Preparing the official Grade 1 PACE record...";

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    _dialogService.ShowWarning(
                        "Complete School Setup first.",
                        "Official Grade 1 E-Class Record");

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
                        "This Grade 1 class has no active learners.",
                        "Official Grade 1 E-Class Record");

                    return;
                }

                IReadOnlyList<Subject> classSubjects =
                    await _subjectRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

                List<Subject> paceSubjects =
                    classSubjects
                        .Where(subject =>
                            IsOfficialGradeOnePaceLearningArea(
                                subject.SubjectName))
                        .ToList();

                if (paceSubjects.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "No official Grade 1 PACE learning areas " +
                        "are assigned to this class.",
                        "Official Grade 1 E-Class Record");

                    return;
                }

                GradeOnePaceExportRequest request =
                    new GradeOnePaceExportRequest
                    {
                        SchoolId =
                            school.SchoolId,

                        SchoolName =
                            school.SchoolName,

                        Region =
                            school.Region,

                        Division =
                            school.Division,

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
                            school.SchoolHead
                    };

                foreach (Learner learner in learners)
                {
                    request.Learners.Add(
                        new GradeOnePaceLearnerExportRow
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

                foreach (Subject subject in paceSubjects)
                {
                    IReadOnlyList<PaceCompetency>
                        officialCompetencies =
                            _gradeOnePaceCatalogService
                                .CreateOfficialCompetencies(
                                    subject.Id,
                                    subject.SubjectName);

                    if (officialCompetencies.Count > 0)
                    {
                        await _paceRepository
                            .SaveCompetenciesAsync(
                                officialCompetencies);
                    }

                    for (int termNumber = 1;
                         termNumber <= 3;
                         termNumber++)
                    {
                        IReadOnlyList<PaceCompetency>
                            competencies =
                                await _paceRepository
                                    .GetCompetenciesAsync(
                                        subject.Id,
                                        termNumber);

                        foreach (PaceCompetency competency
                                 in competencies)
                        {
                            request.Competencies.Add(
                                new GradeOnePaceCompetencyExportRow
                                {
                                    PaceCompetencyId =
                                        competency.Id,

                                    LearningArea =
                                        competency.LearningArea,

                                    TermNumber =
                                        competency.TermNumber,

                                    DomainName =
                                        competency.DomainName,

                                    CompetencyCode =
                                        competency.CompetencyCode,

                                    Description =
                                        competency.Description,

                                    DisplayOrder =
                                        competency.DisplayOrder
                                });
                        }

                        IReadOnlyList<LearnerPaceRating>
                            ratings =
                                await _paceRepository
                                    .GetRatingsAsync(
                                        SelectedClass.Id,
                                        subject.Id,
                                        termNumber);

                        foreach (LearnerPaceRating rating
                                 in ratings)
                        {
                            request.Ratings.Add(
                                new GradeOnePaceRatingExportRow
                                {
                                    LearnerId =
                                        rating.LearnerId,

                                    PaceCompetencyId =
                                        rating.PaceCompetencyId,

                                    Rating =
                                        rating.Rating,

                                    Remarks =
                                        rating.Remarks
                                });
                        }
                    }
                }

                for (int termNumber = 1;
                     termNumber <= 3;
                     termNumber++)
                {
                    IReadOnlyList<LearnerPaceSummary>
                        summaries =
                            await _paceRepository
                                .GetSummariesAsync(
                                    SelectedClass.Id,
                                    termNumber);

                    foreach (LearnerPaceSummary summary
                             in summaries)
                    {
                        request.Summaries.Add(
                            new GradeOnePaceSummaryExportRow
                            {
                                LearnerId =
                                    summary.LearnerId,

                                TermNumber =
                                    summary.TermNumber,

                                WhatLearnerCanDo =
                                    summary.WhatLearnerCanDo,

                                WhatLearnerNeedsToImprove =
                                    summary
                                        .WhatLearnerNeedsToImprove,

                                TeacherRemarks =
                                    summary.TeacherRemarks
                            });
                    }
                }

                string safeGradeLevel =
                    MakeSafeECRFileName(
                        SelectedClass.GradeLevel);

                string safeSection =
                    MakeSafeECRFileName(
                        SelectedClass.SectionName);

                string suggestedFileName =
                    $"ECR_{safeGradeLevel}_" +
                    $"{safeSection}_PACE_" +
                    $"{_currentAcademicYear.DisplayName}.xlsx";

                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Save Official Grade 1 PACE Record",

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
                        "Official Grade 1 PACE export cancelled.";

                    return;
                }

                StatusMessage =
                    "Writing the official Grade 1 PACE workbook...";

                string preparedFile =
                    _gradeOnePaceExportService
                        .PrepareOfficialGradeOnePace(
                            request,
                            saveFileDialog.FileName);

                StatusMessage =
                    "Official Grade 1 PACE record prepared: " +
                    preparedFile;

                System.Windows.MessageBoxResult openResult =
                    System.Windows.MessageBox.Show(
                        "The official Grade 1 PACE record was " +
                        "prepared successfully.\n\n" +
                        preparedFile +
                        "\n\nDo you want to open it now?",
                        "Official Grade 1 PACE Record Ready",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                if (openResult ==
                    System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo(
                            preparedFile)
                        {
                            UseShellExecute = true
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
                    "Grade 1 PACE record.\n\n" +
                    errorMessage,
                    "Official Grade 1 PACE Export Error");
            }
            finally
            {
                IsBusy = false;
                NotifyCommandStates();
            }
        }

        private static bool
            IsOfficialGradeOnePaceLearningArea(
                string subjectName)
        {
            string normalized =
                (subjectName ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant();

            return normalized.Contains("reading") ||
                   normalized.Contains("literacy") ||
                   normalized.Contains("language") ||
                   normalized.Contains("math") ||
                   normalized == "gmrc" ||
                   normalized.Contains("good manners") ||
                   normalized.Contains("makabansa");
        }
    }
}
