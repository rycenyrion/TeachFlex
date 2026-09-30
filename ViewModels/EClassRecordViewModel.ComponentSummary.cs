using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        public ObservableCollection<
            ComponentTermGradeRow>
                ComponentSummaryRows
        {
            get;
        } = new ObservableCollection<
            ComponentTermGradeRow>();

        public bool HasComponentSummary =>
            ComponentSummaryRows.Count > 0;

        public string ComponentSummaryTitle =>
            CurrentPolicy?
                .UsesMapehComponents == true
                    ? "MAPEH Component Grade Summary"
                    : "EPP/TLE Component Grade Summary";

        [RelayCommand]
        private async Task
            LoadComponentSummaryAsync()
        {
            if (SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy == null)
            {
                _dialogService.ShowWarning(
                    "Select a class, subject, and term first.",
                    "Component Grade Summary");

                return;
            }

            if (!UsesComponentRecords)
            {
                _dialogService.ShowWarning(
                    "Select Per Component record mode first.",
                    "Component Grade Summary");

                return;
            }

            List<string> componentNames =
                ComponentOptions
                    .Where(
                        component =>
                            !string.IsNullOrWhiteSpace(
                                component))
                    .Select(
                        component =>
                            component.Trim())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .Take(
                        2)
                    .ToList();

            if (componentNames.Count < 2)
            {
                _dialogService.ShowWarning(
                    "Enter two different component names first.",
                    "Component Grade Summary");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    "Preparing the component grade summary...";

                IReadOnlyList<AssessmentItem>
                    allAssessmentItems =
                        await _assessmentRepository
                            .GetItemsAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                SelectedTerm);

                IReadOnlyList<
                    LearnerAssessmentScore>
                        allSavedScores =
                            await _assessmentRepository
                                .GetScoresAsync(
                                    SelectedClass.Id,
                                    SelectedSubject.Id,
                                    SelectedTerm);

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                string componentOneName =
                    componentNames[0];

                string componentTwoName =
                    componentNames[1];

                List<AssessmentItem>
                    componentOneItems =
                        allAssessmentItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        componentOneName))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                List<AssessmentItem>
                    componentTwoItems =
                        allAssessmentItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        componentTwoName))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                if (componentOneItems.Count == 0 ||
                    componentTwoItems.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "Create the official columns for both " +
                        "components first.",
                        "Component Grade Summary");

                    return;
                }

                Dictionary<
                    (int AssessmentItemId,
                     int LearnerId),
                    LearnerAssessmentScore>
                        savedScoreLookup =
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

                ComponentSummaryRows.Clear();

                int learnerNumber =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    double? componentOneGrade =
                        CalculateComponentGrade(
                            learner,
                            componentOneItems,
                            savedScoreLookup);

                    double? componentTwoGrade =
                        CalculateComponentGrade(
                            learner,
                            componentTwoItems,
                            savedScoreLookup);

                    ComponentTermGradeRow summaryRow =
                        new ComponentTermGradeRow
                        {
                            Number =
                                learnerNumber++,

                            LearnerId =
                                learner.Id,

                            LearnerName =
                                learner.FullName,

                            Sex =
                                learner.Sex,

                            ComponentOneName =
                                componentOneName,

                            ComponentTwoName =
                                componentTwoName,

                            ComponentOneGrade =
                                componentOneGrade,

                            ComponentTwoGrade =
                                componentTwoGrade
                        };

                    ComponentSummaryRows.Add(
                        summaryRow);
                }

                OnPropertyChanged(
                    nameof(
                        HasComponentSummary));

                OnPropertyChanged(
                    nameof(
                        ComponentSummaryTitle));

                StatusMessage =
                    $"{ComponentSummaryTitle} prepared.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not prepare the " +
                    $"component grade summary.\n\n" +
                    $"{errorMessage}",
                    "Component Grade Summary Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private double? CalculateComponentGrade(
            Learner learner,
            IReadOnlyList<AssessmentItem>
                assessmentItems,
            IReadOnlyDictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    savedScoreLookup)
        {
            EClassRecordLearnerRow temporaryRow =
                new EClassRecordLearnerRow
                {
                    LearnerId =
                        learner.Id,

                    Lrn =
                        learner.Lrn,

                    LearnerName =
                        learner.FullName,

                    Sex =
                        learner.Sex
                };

            bool hasEncodedScore =
                false;

            foreach (AssessmentItem assessmentItem
                     in assessmentItems)
            {
                savedScoreLookup.TryGetValue(
                    (
                        assessmentItem.Id,
                        learner.Id
                    ),
                    out LearnerAssessmentScore?
                        savedScore);

                if (savedScore?.Score.HasValue ==
                    true)
                {
                    hasEncodedScore =
                        true;
                }

                AssessmentScoreCell scoreCell =
                    new AssessmentScoreCell
                    {
                        AssessmentItemId =
                            assessmentItem.Id,

                        AssessmentName =
                            assessmentItem.AssessmentName,

                        Category =
                            assessmentItem.Category,

                        AssessmentDomain =
                            assessmentItem.AssessmentDomain,

                        HighestPossibleScore =
                            assessmentItem
                                .HighestPossibleScore,

                        Score =
                            savedScore?.Score
                    };

                temporaryRow.ScoreCells.Add(
                    scoreCell);
            }

            if (!hasEncodedScore)
            {
                return null;
            }

            CalculateLearnerResult(
                temporaryRow);

            return Convert.ToDouble(
                temporaryRow.TermGrade);
        }
    }
}