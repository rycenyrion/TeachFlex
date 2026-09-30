using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private async Task LoadRecordAsync()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy == null)
            {
                _dialogService.ShowWarning(
                    "Select a class, subject, and term first.",
                    "E-Class Record");

                return;
            }

            if (CurrentPolicy.UsesDescriptiveGrades)
            {
                AssessmentItems.Clear();
                LearnerRows.Clear();

                await LoadPaceRecordAsync();

                return;
            }

            ClearPaceRecord();

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    $"Loading {ActiveComponentName}, " +
                    $"{SelectedTermText} record...";

                IReadOnlyList<AssessmentItem>
                    allAssessmentItems =
                        await _assessmentRepository
                            .GetItemsAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                SelectedTerm);

                List<AssessmentItem>
                    assessmentItems =
                        allAssessmentItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        ActiveComponentName))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                IReadOnlyList<
                    LearnerAssessmentScore>
                        allSavedScores =
                            await _assessmentRepository
                                .GetScoresAsync(
                                    SelectedClass.Id,
                                    SelectedSubject.Id,
                                    SelectedTerm);

                HashSet<int> activeAssessmentIds =
                    assessmentItems
                        .Select(
                            item =>
                                item.Id)
                        .ToHashSet();

                List<LearnerAssessmentScore>
                    savedScores =
                        allSavedScores
                            .Where(
                                score =>
                                    activeAssessmentIds.Contains(
                                        score.AssessmentItemId))
                            .ToList();

                AssessmentItems.Clear();

                foreach (AssessmentItem assessmentItem
                         in assessmentItems)
                {
                    AssessmentItems.Add(
                        assessmentItem);
                }

                Dictionary<
                    (int AssessmentItemId,
                     int LearnerId),
                    LearnerAssessmentScore>
                        savedScoreLookup =
                            savedScores.ToDictionary(
                                score =>
                                    (
                                        score.AssessmentItemId,
                                        score.LearnerId
                                    ));

                LearnerRows.Clear();

                int learnerNumber =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    EClassRecordLearnerRow row =
                        new EClassRecordLearnerRow
                        {
                            Number =
                                learnerNumber++,

                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
                                learner.FullName,

                            Sex =
                                learner.Sex
                        };

                    foreach (
                        AssessmentItem assessmentItem
                        in assessmentItems)
                    {
                        savedScoreLookup.TryGetValue(
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

                        scoreCell.PropertyChanged +=
                            (
                                sender,
                                eventArgs) =>
                            {
                                ScoreCell_PropertyChanged(
                                    row,
                                    sender,
                                    eventArgs);
                            };

                        row.ScoreCells.Add(
                            scoreCell);
                    }

                    CalculateLearnerResult(
                        row);

                    LearnerRows.Add(
                        row);
                }

                StatusMessage =
                    assessmentItems.Count == 0
                        ? $"No assessment items yet for " +
                          $"{ActiveComponentName}, " +
                          $"{SelectedTermText}."
                        : $"{ActiveComponentName}, " +
                          $"{SelectedTermText} class " +
                          $"record loaded.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"class record.\n\n" +
                    $"{errorMessage}",
                    "E-Class Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private async Task SaveScoresAsync()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy == null ||
                !CurrentPolicy.UsesNumericalGrades)
            {
                return;
            }

            EClassRecordLearnerRow?
                invalidRow =
                    LearnerRows.FirstOrDefault(
                        row =>
                            row.HasInvalidScore);

            if (invalidRow != null)
            {
                _dialogService.ShowWarning(
                    $"Check the scores of " +
                    $"{invalidRow.LearnerName}. A score " +
                    $"cannot be negative or greater than " +
                    $"the highest possible score.",
                    "Invalid Learner Score");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    $"Saving {ActiveComponentName} scores...";

                List<LearnerAssessmentScore>
                    learnerScores =
                        LearnerRows
                            .SelectMany(
                                row =>
                                    row.ScoreCells.Select(
                                        cell =>
                                            new
                                            LearnerAssessmentScore
                                            {
                                                AssessmentItemId =
                                                    cell
                                                        .AssessmentItemId,

                                                LearnerId =
                                                    row.LearnerId,

                                                Score =
                                                    cell.Score,

                                                Remarks =
                                                    string.Empty
                                            }))
                            .ToList();

                await _assessmentRepository
                    .SaveScoresAsync(
                        learnerScores);

                foreach (
                    EClassRecordLearnerRow row
                    in LearnerRows)
                {
                    CalculateLearnerResult(
                        row);
                }

                StatusMessage =
                    $"{ActiveComponentName}, " +
                    $"{SelectedTermText} scores saved " +
                    $"successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"learner scores.\n\n" +
                    $"{errorMessage}",
                    "Save Scores Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void ScoreCell_PropertyChanged(
            EClassRecordLearnerRow row,
            object? sender,
            PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName !=
                nameof(
                    AssessmentScoreCell.Score))
            {
                return;
            }

            CalculateLearnerResult(
                row);
        }

        private void CalculateLearnerResult(
            EClassRecordLearnerRow row)
        {
            if (CurrentPolicy == null ||
                !CurrentPolicy.UsesNumericalGrades)
            {
                return;
            }

            bool hasEncodedScore =
                row.ScoreCells.Any(
                    cell =>
                        cell.Score.HasValue);

            if (!hasEncodedScore)
            {
                row.WrittenWorkPercentage =
                    0;

                row.PerformanceTaskPercentage =
                    0;

                row.ExaminationPercentage =
                    0;

                row.InitialGrade =
                    0;

                row.TermGrade =
                    0;

                row.Descriptor =
                    string.Empty;

                return;
            }

            decimal writtenWorkPercentage;
            decimal performanceTaskPercentage;

            if (CurrentPolicy
                .UsesDomainBasedAssessment)
            {
                decimal writtenCognitive =
                    CalculateCategoryPercentage(
                        row,
                        "Written Work",
                        "Cognitive");

                decimal writtenAffective =
                    CalculateCategoryPercentage(
                        row,
                        "Written Work",
                        "Affective");

                writtenWorkPercentage =
                    writtenCognitive *
                        0.50m +
                    writtenAffective *
                        0.50m;

                decimal performanceCognitive =
                    CalculateCategoryPercentage(
                        row,
                        "Performance Task",
                        "Cognitive");

                decimal performanceAffective =
                    CalculateCategoryPercentage(
                        row,
                        "Performance Task",
                        "Affective");

                decimal performanceBehavioral =
                    CalculateCategoryPercentage(
                        row,
                        "Performance Task",
                        "Behavioral");

                performanceTaskPercentage =
                    performanceCognitive *
                        0.20m +
                    performanceAffective *
                        0.20m +
                    performanceBehavioral *
                        0.60m;
            }
            else
            {
                writtenWorkPercentage =
                    CalculateCategoryPercentage(
                        row,
                        "Written Work");

                performanceTaskPercentage =
                    CalculateCategoryPercentage(
                        row,
                        "Performance Task");
            }

            decimal summativeTest1Percentage =
                CalculateCategoryPercentage(
                    row,
                    "Summative Test 1");

            decimal summativeTest2Percentage =
                CalculateCategoryPercentage(
                    row,
                    "Summative Test 2");

            decimal termExaminationPercentage =
                CalculateCategoryPercentage(
                    row,
                    "Term Examination");

            decimal examinationPercentage =
                summativeTest1Percentage *
                    0.30m +
                summativeTest2Percentage *
                    0.30m +
                termExaminationPercentage *
                    0.40m;

            row.WrittenWorkPercentage =
                Math.Round(
                    writtenWorkPercentage,
                    2,
                    MidpointRounding.AwayFromZero);

            row.PerformanceTaskPercentage =
                Math.Round(
                    performanceTaskPercentage,
                    2,
                    MidpointRounding.AwayFromZero);

            row.ExaminationPercentage =
                Math.Round(
                    examinationPercentage,
                    2,
                    MidpointRounding.AwayFromZero);

            row.InitialGrade =
                _gradingCalculationService
                    .CalculateInitialGrade(
                        row.WrittenWorkPercentage,
                        row.PerformanceTaskPercentage,
                        row.ExaminationPercentage,
                        CurrentPolicy);

            row.TermGrade =
                _gradingCalculationService
                    .CalculateNumericalGrade(
                        row.InitialGrade,
                        CurrentPolicy.GradingSystem);

            row.Descriptor =
                _gradingCalculationService
                    .GetDescriptor(
                        row.TermGrade);
        }

        private decimal
            CalculateCategoryPercentage(
                EClassRecordLearnerRow row,
                string category,
                string? assessmentDomain =
                    null)
        {
            List<AssessmentScoreCell>
                matchingCells =
                    row.ScoreCells
                        .Where(
                            cell =>
                                cell.Category.Equals(
                                    category,
                                    StringComparison
                                        .OrdinalIgnoreCase) &&
                                (
                                    assessmentDomain == null ||
                                    cell.AssessmentDomain.Equals(
                                        assessmentDomain,
                                        StringComparison
                                            .OrdinalIgnoreCase)
                                ))
                        .ToList();

            decimal highestPossibleScore =
                matchingCells.Sum(
                    cell =>
                        cell.HighestPossibleScore);

            decimal learnerScore =
                matchingCells.Sum(
                    cell =>
                        cell.Score ?? 0);

            return _gradingCalculationService
                .CalculatePercentageScore(
                    learnerScore,
                    highestPossibleScore);
        }
    }
}
