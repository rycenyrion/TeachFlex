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
    public partial class EClassRecordViewModel
    {
        private bool
    _isExportingOfficialEcr;

        public bool IsExportingOfficialEcr
        {
            get =>
                _isExportingOfficialEcr;

            private set => SetProperty(
                ref _isExportingOfficialEcr,
                value);
        }
        [RelayCommand]
        private async Task ExportOfficialECRAsync()
        {
            if (IsExportingOfficialEcr)
            {
                return;
            }

            try
            {
                IsExportingOfficialEcr =
                    true;

                StatusMessage =
                    "Preparing the official E-Class Record...";

                await ExportOfficialECRCoreAsync();
            }
            finally
            {
                IsExportingOfficialEcr =
                    false;
            }
        }

        private async Task ExportOfficialECRCoreAsync()
        {
            if (IsBusy)
            {
                return;
            }
            if (IsKindergartenRecord)
            {
                await ExportOfficialKindergartenECRAsync();

                return;
            }

            if (SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy == null)
            {
                _dialogService.ShowWarning(
                    "Select a class and subject first.",
                    "Official E-Class Record");

                return;
            }

            if (CurrentPolicy.UsesDescriptiveGrades)
            {
                await ExportOfficialGradeOnePaceAsync();

                return;
            }

            if (_currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Create the current School Year first.",
                    "Official E-Class Record");

                return;
            }

            List<string> componentNames =
                UsesComponentRecords
                    ? ComponentOptions
                        .Where(
                            component =>
                                !string.IsNullOrWhiteSpace(
                                    component))
                        .Select(
                            component =>
                                component.Trim())
                        .Distinct(
                            StringComparer
                                .OrdinalIgnoreCase)
                        .Take(
                            2)
                        .ToList()
                    : new List<string>
                    {
                        "General"
                    };

            if (UsesComponentRecords &&
                componentNames.Count < 2)
            {
                _dialogService.ShowWarning(
                    "Enter two different component names first.",
                    "Official E-Class Record");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    "Preparing official E-Class Record data...";

                if (await HasUnsavedCurrentRecordChangesAsync())
                {
                    _dialogService.ShowWarning(
                        "The currently displayed record has " +
                        "unsaved HPS or learner scores.\n\n" +
                        "Click Save HPS and Save Scores first, " +
                        "then export again.",
                        "Unsaved E-Class Record Changes");

                    return;
                }

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    _dialogService.ShowWarning(
                        "Complete School Setup first.",
                        "Official E-Class Record");

                    return;
                }

                IReadOnlyList<Learner>
    loadedLearners =
        await _learnerRepository
            .GetByClassAsync(
                SelectedClass.Id);

                List<Learner>
                    learners =
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
                        "This class has no active learners.",
                        "Official E-Class Record");

                    return;
                }

                ECRExportRequest request =
                    new ECRExportRequest
                    {
                        SchoolId =
                            school.SchoolId,

                        SchoolName =
                            school.SchoolName,

                        Region =
                            school.Region,

                        Division =
                            school.Division,

                        SchoolYear =
                            _currentAcademicYear
                                .DisplayName,

                        GradeLevel =
                            SelectedClass.GradeLevel,

                        SectionName =
                            SelectedClass.SectionName,

                        SubjectName =
                            SelectedSubject.SubjectName,

                        AdviserName =
                            SelectedClass.Adviser?
                                .FullName
                            ?? string.Empty,

                        SchoolHeadName =
                            school.SchoolHead,

                        GradingSystem =
                            CurrentPolicy.GradingSystem,

                        AssessmentType =
                            CurrentPolicy.AssessmentType,

                        UsesComponentRecords =
                            UsesComponentRecords,

                        ComponentOneName =
                            UsesComponentRecords
                                ? componentNames[0]
                                : "General",

                        ComponentTwoName =
                            UsesComponentRecords
                                ? componentNames[1]
                                : string.Empty
                    };

                List<string> incompleteRecords =
                    new List<string>();

                for (int termNumber = 1;
                     termNumber <= 3;
                     termNumber++)
                {
                    IReadOnlyList<AssessmentItem>
                        allAssessmentItems =
                            await _assessmentRepository
                                .GetItemsAsync(
                                    SelectedClass.Id,
                                    SelectedSubject.Id,
                                    termNumber);

                    IReadOnlyList<
                        LearnerAssessmentScore>
                            allSavedScores =
                                await _assessmentRepository
                                    .GetScoresAsync(
                                        SelectedClass.Id,
                                        SelectedSubject.Id,
                                        termNumber);

                    Dictionary<
                        (int AssessmentItemId,
                         int LearnerId),
                        LearnerAssessmentScore>
                            scoreLookup =
                                allSavedScores
                                    .GroupBy(
                                        score =>
                                            (
                                                score.AssessmentItemId,
                                                score.LearnerId
                                            ))
                                    .ToDictionary(
                                        group =>
                                            group.Key,
                                        group =>
                                            group
                                                .OrderByDescending(
                                                    score =>
                                                        score.UpdatedAtUtc)
                                                .First());

                    foreach (string componentName
                             in componentNames)
                    {
                        List<AssessmentItem>
                            componentItems =
                                allAssessmentItems
                                    .Where(
                                        item =>
                                            IsSameComponent(
                                                item.ComponentName,
                                                componentName))
                                    .OrderBy(
                                        item =>
                                            item.DisplayOrder)
                                    .ToList();

                        List<AssessmentItem>
                            includedItems =
                                componentItems
                                    .Where(
                                        item =>
                                            item.IsActive &&
                                            item.HighestPossibleScore > 0)
                                    .ToList();

                        bool hasAnySavedScore =
                            includedItems.Any(
                                item =>
                                    learners.Any(
                                        learner =>
                                            scoreLookup.TryGetValue(
                                                (
                                                    item.Id,
                                                    learner.Id
                                                ),
                                                out LearnerAssessmentScore?
                                                    savedScore) &&
                                            savedScore.Score.HasValue));

                        if (includedItems.Count == 0 ||
                            !hasAnySavedScore)
                        {
                            incompleteRecords.Add(
                                UsesComponentRecords
                                    ? $"Term {termNumber} — " +
                                      $"{componentName}"
                                    : $"Term {termNumber}");
                        }

                        ECRTermExportData termData =
                            new ECRTermExportData
                            {
                                TermNumber =
                                    termNumber,

                                ComponentName =
                                    componentName
                            };

                        foreach (
                            AssessmentItem assessmentItem
                            in componentItems)
                        {
                            termData.AssessmentColumns.Add(
                                new
                                ECRAssessmentExportColumn
                                {
                                    AssessmentItemId =
                                        assessmentItem.Id,

                                    AssessmentName =
                                        assessmentItem
                                            .AssessmentName,

                                    Category =
                                        assessmentItem.Category,

                                    AssessmentDomain =
                                        assessmentItem
                                            .AssessmentDomain,

                                    HighestPossibleScore =
                                        assessmentItem
                                            .HighestPossibleScore,

                                    DisplayOrder =
                                        assessmentItem.DisplayOrder
                                });
                        }

                        foreach (Learner learner
                                 in learners)
                        {
                            ECRLearnerExportRow exportRow =
                                CreateECRLearnerExportRow(
                                    learner,
                                    componentItems,
                                    scoreLookup);

                            termData.Learners.Add(
                                exportRow);
                        }

                        request.Terms.Add(
                            termData);
                    }
                }

                Dictionary<int, double?>
                    termOneGrades =
                        await LoadTermGradesAsync(
                            1,
                            learners,
                            componentNames);

                Dictionary<int, double?>
                    termTwoGrades =
                        await LoadTermGradesAsync(
                            2,
                            learners,
                            componentNames);

                Dictionary<int, double?>
                    termThreeGrades =
                        await LoadTermGradesAsync(
                            3,
                            learners,
                            componentNames);

                foreach (Learner learner
                         in learners)
                {
                    termOneGrades.TryGetValue(
                        learner.Id,
                        out double? termOneGrade);

                    termTwoGrades.TryGetValue(
                        learner.Id,
                        out double? termTwoGrade);

                    termThreeGrades.TryGetValue(
                        learner.Id,
                        out double? termThreeGrade);

                    double? finalGrade =
                        null;

                    if (termOneGrade.HasValue &&
                        termTwoGrade.HasValue &&
                        termThreeGrade.HasValue)
                    {
                        finalGrade =
                            Math.Round(
                                (
                                    termOneGrade.Value +
                                    termTwoGrade.Value +
                                    termThreeGrade.Value
                                ) / 3.0,
                                0,
                                MidpointRounding
                                    .AwayFromZero);
                    }

                    request.FinalGrades.Add(
                        new ECRFinalGradeExportRow
                        {
                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
                                learner.OfficialName,

                            Sex =
                                learner.Sex,

                            TermOneGrade =
                                termOneGrade,

                            TermTwoGrade =
                                termTwoGrade,

                            TermThreeGrade =
                                termThreeGrade,

                            FinalGrade =
                                finalGrade,

                            CompletionStatus =
                                finalGrade.HasValue
                                    ? "Complete"
                                    : "Incomplete"
                        });
                }

                if (incompleteRecords.Count > 0)
                {
                    string incompleteMessage =
                        CreateIncompleteRecordsMessage(
                            incompleteRecords);

                    System.Windows.MessageBoxResult
                        continueResult =
                            System.Windows.MessageBox.Show(
                                incompleteMessage,
                                "Incomplete E-Class Record",
                                System.Windows.MessageBoxButton.YesNo,
                                System.Windows.MessageBoxImage.Warning,
                                System.Windows.MessageBoxResult.No);

                    if (continueResult !=
                        System.Windows.MessageBoxResult.Yes)
                    {
                        StatusMessage =
                            "Official E-Class Record export cancelled.";

                        return;
                    }
                }

                string safeGradeLevel =
                    MakeSafeECRFileName(
                        SelectedClass.GradeLevel);

                string safeSection =
                    MakeSafeECRFileName(
                        SelectedClass.SectionName);

                string safeSubject =
                    MakeSafeECRFileName(
                        SelectedSubject.SubjectName);

                string suggestedFileName =
                    $"ECR_{safeGradeLevel}_" +
                    $"{safeSection}_" +
                    $"{safeSubject}_" +
                    $"{_currentAcademicYear.DisplayName}" +
                    $".xlsx";

                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Save Official DepEd E-Class Record",

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

                bool? result =
                    saveFileDialog.ShowDialog();

                if (result != true)
                {
                    StatusMessage =
                        "Official E-Class Record export cancelled.";

                    return;
                }

                StatusMessage =
                    "Writing the official E-Class Record...";

                string preparedFile;

                if (IsSeniorHighGradeLevel(
                        SelectedClass.GradeLevel))
                {
                    SeniorHighEcrExportRequest
                        seniorHighRequest =
                            CreateSeniorHighEcrExportRequest(
                                request,
                                SelectedSubject,
                                school);

                    preparedFile =
                        _seniorHighEcrExportService
                            .PrepareOfficialSeniorHighECR(
                                seniorHighRequest,
                                saveFileDialog.FileName);
                }
                else
                {
                    preparedFile =
                        _ecrExportService
                            .PrepareOfficialECR(
                                request,
                                saveFileDialog.FileName);
                }

                StatusMessage =
                    $"Official E-Class Record prepared: " +
                    $"{preparedFile}";

                System.Windows.MessageBoxResult openResult =
                    System.Windows.MessageBox.Show(
                    $"The official DepEd E-Class Record " +
                    $"was prepared successfully.\n\n" +
                    $"{preparedFile}\n\n" +
                    $"Do you want to open it now?",
                    "Official E-Class Record Ready",
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
                    $"TeachFlex could not prepare the " +
                    $"official E-Class Record.\n\n" +
                    $"{errorMessage}",
                    "Official E-Class Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private ECRLearnerExportRow
            CreateECRLearnerExportRow(
                Learner learner,
                IReadOnlyList<AssessmentItem>
                    assessmentItems,
                IReadOnlyDictionary<
                    (int AssessmentItemId,
                     int LearnerId),
                    LearnerAssessmentScore>
                        scoreLookup)
        {
            EClassRecordLearnerRow temporaryRow =
                new EClassRecordLearnerRow
                {
                    LearnerId =
                        learner.Id,

                    Lrn =
                        learner.Lrn,

                    LearnerName =
                        learner.OfficialName,

                    Sex =
                        learner.Sex
                };

            ECRLearnerExportRow exportRow =
                new ECRLearnerExportRow
                {
                    LearnerId =
                        learner.Id,

                    Lrn =
                        learner.Lrn,

                    LearnerName =
                        learner.OfficialName,

                    Sex =
                        learner.Sex
                };

            foreach (AssessmentItem assessmentItem
                     in assessmentItems)
            {
                scoreLookup.TryGetValue(
                    (
                        assessmentItem.Id,
                        learner.Id
                    ),
                    out LearnerAssessmentScore?
                        savedScore);

                AssessmentScoreCell scoreCell =
                    new AssessmentScoreCell
                    {
                        AssessmentItemId =
                            assessmentItem.Id,

                        AssessmentName =
                            assessmentItem
                                .AssessmentName,

                        Category =
                            assessmentItem.Category,

                        AssessmentDomain =
                            assessmentItem
                                .AssessmentDomain,

                        HighestPossibleScore =
                            assessmentItem
                                .HighestPossibleScore,

                        Score =
                            savedScore?.Score
                    };

                temporaryRow.ScoreCells.Add(
                    scoreCell);

                exportRow.Scores[
                    assessmentItem.Id] =
                        savedScore?.Score;
            }

            CalculateLearnerResult(
                temporaryRow);

            exportRow.WrittenWorkPercentage =
                temporaryRow
                    .WrittenWorkPercentage;

            exportRow.PerformanceTaskPercentage =
                temporaryRow
                    .PerformanceTaskPercentage;

            exportRow.ExaminationPercentage =
                temporaryRow
                    .ExaminationPercentage;

            exportRow.InitialGrade =
                temporaryRow.InitialGrade;

            exportRow.TermGrade =
                temporaryRow.TermGrade;

            exportRow.Descriptor =
                temporaryRow.Descriptor;

            return exportRow;
        }

        private async Task<bool>
            HasUnsavedCurrentRecordChangesAsync()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                AssessmentItems.Count == 0 ||
                LearnerRows.Count == 0)
            {
                return false;
            }

            IReadOnlyList<AssessmentItem>
                savedItems =
                    await _assessmentRepository
                        .GetItemsAsync(
                            SelectedClass.Id,
                            SelectedSubject.Id,
                            SelectedTerm);

            Dictionary<int, AssessmentItem>
                savedItemLookup =
                    savedItems
                        .ToDictionary(
                            item =>
                                item.Id);

            foreach (AssessmentItem displayedItem
                     in AssessmentItems)
            {
                if (!savedItemLookup.TryGetValue(
                        displayedItem.Id,
                        out AssessmentItem? savedItem) ||
                    displayedItem.HighestPossibleScore !=
                        savedItem.HighestPossibleScore)
                {
                    return true;
                }
            }

            IReadOnlyList<LearnerAssessmentScore>
                savedScores =
                    await _assessmentRepository
                        .GetScoresAsync(
                            SelectedClass.Id,
                            SelectedSubject.Id,
                            SelectedTerm);

            Dictionary<
                (int AssessmentItemId,
                 int LearnerId),
                decimal?> savedScoreLookup =
                    savedScores
                        .GroupBy(
                            score =>
                                (
                                    score.AssessmentItemId,
                                    score.LearnerId
                                ))
                        .ToDictionary(
                            group =>
                                group.Key,
                            group =>
                                group
                                    .OrderByDescending(
                                        score =>
                                            score.UpdatedAtUtc)
                                    .First()
                                    .Score);

            foreach (EClassRecordLearnerRow row
                     in LearnerRows)
            {
                foreach (AssessmentScoreCell cell
                         in row.ScoreCells)
                {
                    savedScoreLookup.TryGetValue(
                        (
                            cell.AssessmentItemId,
                            row.LearnerId
                        ),
                        out decimal? savedScore);

                    if (cell.Score != savedScore)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string
            CreateIncompleteRecordsMessage(
                IReadOnlyList<string>
                    incompleteRecords)
        {
            const int maximumDisplayedRecords =
                8;

            List<string> displayedRecords =
                incompleteRecords
                    .Take(
                        maximumDisplayedRecords)
                    .Select(
                        record =>
                            $"• {record}")
                    .ToList();

            if (incompleteRecords.Count >
                maximumDisplayedRecords)
            {
                displayedRecords.Add(
                    $"• and " +
                    $"{incompleteRecords.Count - maximumDisplayedRecords} " +
                    $"more record(s)");
            }

            return
                "The following term/component records " +
                "have no active assessments or no saved " +
                "learner scores:\n\n" +
                string.Join(
                    "\n",
                    displayedRecords) +
                "\n\nThe Excel file will contain blank or " +
                "incomplete sections. Continue exporting?";
        }

        private static string MakeSafeECRFileName(
            string value)
        {
            string safeValue =
                string.IsNullOrWhiteSpace(
                    value)
                    ? "Unspecified"
                    : value.Trim();

            foreach (char invalidCharacter
                     in System.IO.Path
                         .GetInvalidFileNameChars())
            {
                safeValue =
                    safeValue.Replace(
                        invalidCharacter,
                        '-');
            }

            return safeValue.Replace(
                " ",
                "_");
        }
    }
}
