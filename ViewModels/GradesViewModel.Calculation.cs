using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class GradesViewModel
    {
        public int TotalLearners =>
            GradeRows.Count;

        public int CompletedLearners =>
            GradeRows.Count(
                row =>
                    row.IsComplete);

        public int PassedLearners =>
            GradeRows.Count(
                row =>
                    row.Remarks == "Passed");

        public int LearnersNeedingSupport =>
            GradeRows.Count(
                row =>
                    row.Remarks == "Failed");

        public string ClassAverageText
        {
            get
            {
                List<int> finalGrades =
                    GradeRows
                        .Where(
                            row =>
                                row.FinalGrade.HasValue)
                        .Select(
                            row =>
                                row.FinalGrade!.Value)
                        .ToList();

                if (finalGrades.Count == 0)
                {
                    return "—";
                }

                double average =
                    finalGrades.Average();

                return Math.Round(
                        average,
                        1,
                        MidpointRounding.AwayFromZero)
                    .ToString("0.0");
            }
        }

        [RelayCommand]
        private async Task LoadGradesAsync()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Grades");

                return;
            }

            if (IsKindergartenClass)
            {
                await LoadDescriptiveProgressAsync();
                return;
            }

            if (SelectedSubject == null)
            {
                _dialogService.ShowWarning(
                    "Select a subject first.",
                    "Grades");

                return;
            }

            if (_currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Create or select the current School Year first.",
                    "Grades");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Computing the three-term grade summary...";

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                GradingPolicy policy =
                    GradingPolicy.Create(
                        SelectedClass.GradeLevel,
                        _currentAcademicYear.StartYear,
                        SelectedSubject.SubjectName);

                if (!policy.UsesNumericalGrades)
                {
                    IsBusy = false;
                    await LoadDescriptiveProgressAsync();
                    return;
                }

                Dictionary<int, int?>
                    termOneGrades =
                      await LoadTermGradesAsync(
                            1,
                            learners,
                            policy,
                            SelectedSubject);

                Dictionary<int, int?>
                    termTwoGrades =
                        await LoadTermGradesAsync(
                            2,
                            learners,
                            policy,
                            SelectedSubject);

                Dictionary<int, int?>
                    termThreeGrades =
                        await LoadTermGradesAsync(
                            3,
                            learners,
                            policy,
                            SelectedSubject);

                GradeRows.Clear();
                DescriptiveRows.Clear();

                int learnerNumber =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    termOneGrades.TryGetValue(
                        learner.Id,
                        out int? termOneGrade);

                    termTwoGrades.TryGetValue(
                        learner.Id,
                        out int? termTwoGrade);

                    termThreeGrades.TryGetValue(
                        learner.Id,
                        out int? termThreeGrade);

                    GradeRows.Add(
                        new GradeSummaryRow
                        {
                            Number =
                                learnerNumber++,

                            LearnerId =
                                learner.Id,

                            LearnerName =
                                learner.OfficialName,

                            Sex =
                                learner.Sex,

                            TermOneGrade =
                                termOneGrade,

                            TermTwoGrade =
                                termTwoGrade,

                            TermThreeGrade =
                                termThreeGrade
                        });
                }

                NotifyGradeSummary();

                StatusMessage =
                    GradeRows.Count == 0
                        ? "No active learners were found."
                        : "Three-term grades loaded successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not compute the " +
                    $"three-term grades.\n\n" +
                    $"{errorMessage}",
                    "Grades Error");

                StatusMessage =
                    "Grades could not be computed.";
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private async Task<Dictionary<int, int?>>
    LoadTermGradesAsync(
        int termNumber,
        IReadOnlyList<Learner> learners,
        GradingPolicy policy,
        Subject subject)
        {
            IReadOnlyList<AssessmentItem>
    assessmentItems =
        await _assessmentRepository
            .GetItemsAsync(
                SelectedClass!.Id,
                subject.Id,
                termNumber);

            IReadOnlyList<LearnerAssessmentScore>
                savedScores =
                    await _assessmentRepository
                        .GetScoresAsync(
                            SelectedClass.Id,
                            subject.Id,
                            termNumber);

            Dictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    scoreLookup =
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
                                        .First());

            List<string> componentNames =
                assessmentItems
                    .Select(
                        item =>
                            NormalizeComponentName(
                                item.ComponentName))
                    .Where(
                        component =>
                            !component.Equals(
                                "General",
                                StringComparison.OrdinalIgnoreCase))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            bool usesComponentAverage =
                (
                    policy.UsesMapehComponents ||
                    policy.SupportsEppTleComponents
                ) &&
                componentNames.Count >= 2;

            Dictionary<int, int?>
                result =
                    new Dictionary<int, int?>();

            foreach (Learner learner
                     in learners)
            {
                if (usesComponentAverage)
                {
                    List<int> componentGrades =
                        new List<int>();

                    bool hasIncompleteComponent =
                        false;

                    foreach (string componentName
                             in componentNames)
                    {
                        List<AssessmentItem>
                            componentItems =
                                assessmentItems
                                    .Where(
                                        item =>
                                            NormalizeComponentName(
                                                item.ComponentName)
                                            .Equals(
                                                componentName,
                                                StringComparison
                                                    .OrdinalIgnoreCase))
                                    .OrderBy(
                                        item =>
                                            item.DisplayOrder)
                                    .ToList();

                        int? componentGrade =
                            CalculateLearnerTermGrade(
                                learner.Id,
                                componentItems,
                                scoreLookup,
                                policy);

                        if (!componentGrade.HasValue)
                        {
                            hasIncompleteComponent =
                                true;

                            break;
                        }

                        componentGrades.Add(
                            componentGrade.Value);
                    }

                    result[learner.Id] =
                        hasIncompleteComponent ||
                        componentGrades.Count == 0
                            ? null
                            : (int)Math.Round(
                                componentGrades.Average(),
                                0,
                                MidpointRounding.AwayFromZero);

                    continue;
                }

                List<AssessmentItem>
                    generalItems =
                        assessmentItems
                            .Where(
                                item =>
                                    NormalizeComponentName(
                                        item.ComponentName)
                                    .Equals(
                                        "General",
                                        StringComparison
                                            .OrdinalIgnoreCase))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                if (generalItems.Count == 0 &&
                    componentNames.Count == 1)
                {
                    string onlyComponent =
                        componentNames[0];

                    generalItems =
                        assessmentItems
                            .Where(
                                item =>
                                    NormalizeComponentName(
                                        item.ComponentName)
                                    .Equals(
                                        onlyComponent,
                                        StringComparison
                                            .OrdinalIgnoreCase))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();
                }

                result[learner.Id] =
                    CalculateLearnerTermGrade(
                        learner.Id,
                        generalItems,
                        scoreLookup,
                        policy);
            }

            return result;
        }

        private int? CalculateLearnerTermGrade(
            int learnerId,
            IReadOnlyList<AssessmentItem>
                assessmentItems,
            IReadOnlyDictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    scoreLookup,
            GradingPolicy policy)
        {
            bool hasEncodedScore =
                assessmentItems.Any(
                    item =>
                        scoreLookup.TryGetValue(
                            (
                                item.Id,
                                learnerId
                            ),
                            out LearnerAssessmentScore?
                                score) &&
                        score.Score.HasValue);

            if (!hasEncodedScore)
            {
                return null;
            }

            decimal writtenWorkPercentage;
            decimal performanceTaskPercentage;

            if (policy.UsesDomainBasedAssessment)
            {
                decimal writtenCognitive =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Written Work",
                        "Cognitive");

                decimal writtenAffective =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Written Work",
                        "Affective");

                writtenWorkPercentage =
                    writtenCognitive * 0.50m +
                    writtenAffective * 0.50m;

                decimal performanceCognitive =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Performance Task",
                        "Cognitive");

                decimal performanceAffective =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Performance Task",
                        "Affective");

                decimal performanceBehavioral =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Performance Task",
                        "Behavioral");

                performanceTaskPercentage =
                    performanceCognitive * 0.20m +
                    performanceAffective * 0.20m +
                    performanceBehavioral * 0.60m;
            }
            else
            {
                writtenWorkPercentage =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Written Work");

                performanceTaskPercentage =
                    CalculateCategoryPercentage(
                        learnerId,
                        assessmentItems,
                        scoreLookup,
                        "Performance Task");
            }

            decimal summativeTestOne =
                CalculateCategoryPercentage(
                    learnerId,
                    assessmentItems,
                    scoreLookup,
                    "Summative Test 1");

            decimal summativeTestTwo =
                CalculateCategoryPercentage(
                    learnerId,
                    assessmentItems,
                    scoreLookup,
                    "Summative Test 2");

            decimal termExamination =
                CalculateCategoryPercentage(
                    learnerId,
                    assessmentItems,
                    scoreLookup,
                    "Term Examination");

            decimal examinationPercentage =
                summativeTestOne * 0.30m +
                summativeTestTwo * 0.30m +
                termExamination * 0.40m;

            decimal initialGrade =
                _gradingCalculationService
                    .CalculateInitialGrade(
                        writtenWorkPercentage,
                        performanceTaskPercentage,
                        examinationPercentage,
                        policy);

            return _gradingCalculationService
                .CalculateNumericalGrade(
                    initialGrade,
                    policy.GradingSystem);
        }

        private decimal CalculateCategoryPercentage(
            int learnerId,
            IReadOnlyList<AssessmentItem>
                assessmentItems,
            IReadOnlyDictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    scoreLookup,
            string category,
            string? assessmentDomain =
                null)
        {
            List<AssessmentItem>
                matchingItems =
                    assessmentItems
                        .Where(
                            item =>
                                item.Category.Equals(
                                    category,
                                    StringComparison
                                        .OrdinalIgnoreCase) &&
                                (
                                    assessmentDomain == null ||
                                    item.AssessmentDomain.Equals(
                                        assessmentDomain,
                                        StringComparison
                                            .OrdinalIgnoreCase)
                                ))
                        .ToList();

            decimal highestPossibleScore =
                matchingItems.Sum(
                    item =>
                        item.HighestPossibleScore);

            decimal learnerScore =
                matchingItems.Sum(
                    item =>
                    {
                        scoreLookup.TryGetValue(
                            (
                                item.Id,
                                learnerId
                            ),
                            out LearnerAssessmentScore?
                                savedScore);

                        return savedScore?.Score ?? 0m;
                    });

            return _gradingCalculationService
                .CalculatePercentageScore(
                    learnerScore,
                    highestPossibleScore);
        }

        private static string NormalizeComponentName(
            string? componentName)
        {
            return string.IsNullOrWhiteSpace(
                componentName)
                ? "General"
                : componentName.Trim();
        }

        private void NotifyGradeSummary()
        {
            OnPropertyChanged(
                nameof(HasGradeRows));

            OnPropertyChanged(
                nameof(TotalLearners));

            OnPropertyChanged(
                nameof(CompletedLearners));

            OnPropertyChanged(
                nameof(PassedLearners));

            OnPropertyChanged(
                nameof(LearnersNeedingSupport));

            OnPropertyChanged(
                nameof(ClassAverageText));
            NotifyDescriptiveMode();
        }
    }
}
