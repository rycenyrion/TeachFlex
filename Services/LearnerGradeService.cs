using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeachFlex.Models;
using TeachFlex.Repositories;

namespace TeachFlex.Services
{
    public class LearnerGradeService :
        ILearnerGradeService
    {
        private readonly IAssessmentRepository
            _assessmentRepository;

        private readonly IGradingCalculationService
            _gradingCalculationService;

        public LearnerGradeService(
            IAssessmentRepository assessmentRepository,
            IGradingCalculationService
                gradingCalculationService)
        {
            _assessmentRepository =
                assessmentRepository;

            _gradingCalculationService =
                gradingCalculationService;
        }

        public async Task<Dictionary<int, int?>>
            GetTermGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                int termNumber,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                schoolClass);

            ArgumentNullException.ThrowIfNull(
                academicYear);

            ArgumentNullException.ThrowIfNull(
                subject);

            ArgumentNullException.ThrowIfNull(
                learners);

            if (termNumber < 1 ||
                termNumber > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(termNumber),
                    "Term number must be from 1 to 3.");
            }

            GradingPolicy policy =
                GradingPolicy.Create(
                    schoolClass.GradeLevel,
                    academicYear.StartYear,
                    subject.SubjectName);

            Dictionary<int, int?> result =
                new Dictionary<int, int?>();

            if (!policy.UsesNumericalGrades)
            {
                foreach (Learner learner
                         in learners)
                {
                    result[learner.Id] =
                        null;
                }

                return result;
            }

            IReadOnlyList<AssessmentItem>
                assessmentItems =
                    await _assessmentRepository
                        .GetItemsAsync(
                            schoolClass.Id,
                            subject.Id,
                            termNumber,
                            cancellationToken);

            IReadOnlyList<LearnerAssessmentScore>
                savedScores =
                    await _assessmentRepository
                        .GetScoresAsync(
                            schoolClass.Id,
                            subject.Id,
                            termNumber,
                            cancellationToken);

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
                        componentName =>
                            !componentName.Equals(
                                "General",
                                StringComparison
                                    .OrdinalIgnoreCase))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            bool usesComponentAverage =
                (
                    policy.UsesMapehComponents ||
                    policy.SupportsEppTleComponents
                ) &&
                componentNames.Count >= 2;

            foreach (Learner learner
                     in learners)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                if (usesComponentAverage)
                {
                    result[learner.Id] =
                        CalculateComponentAverage(
                            learner.Id,
                            componentNames,
                            assessmentItems,
                            scoreLookup,
                            policy);

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

        public async Task<Dictionary<int, int?>>
            GetFinalGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                CancellationToken cancellationToken =
                    default)
        {
            Dictionary<int, int?>
                termOneGrades =
                    await GetTermGradesAsync(
                        schoolClass,
                        academicYear,
                        subject,
                        learners,
                        1,
                        cancellationToken);

            Dictionary<int, int?>
                termTwoGrades =
                    await GetTermGradesAsync(
                        schoolClass,
                        academicYear,
                        subject,
                        learners,
                        2,
                        cancellationToken);

            Dictionary<int, int?>
                termThreeGrades =
                    await GetTermGradesAsync(
                        schoolClass,
                        academicYear,
                        subject,
                        learners,
                        3,
                        cancellationToken);

            Dictionary<int, int?>
                finalGrades =
                    new Dictionary<int, int?>();

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

                if (!termOneGrade.HasValue ||
                    !termTwoGrade.HasValue ||
                    !termThreeGrade.HasValue)
                {
                    finalGrades[learner.Id] =
                        null;

                    continue;
                }

                finalGrades[learner.Id] =
                    (int)Math.Round(
                        (
                            termOneGrade.Value +
                            termTwoGrade.Value +
                            termThreeGrade.Value
                        ) / 3m,
                        0,
                        MidpointRounding.AwayFromZero);
            }

            return finalGrades;
        }

        public async Task<Dictionary<int, int?>>
            GetComponentTermGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                int termNumber,
                string componentName,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                schoolClass);

            ArgumentNullException.ThrowIfNull(
                academicYear);

            ArgumentNullException.ThrowIfNull(
                subject);

            ArgumentNullException.ThrowIfNull(
                learners);

            if (termNumber < 1 ||
                termNumber > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(termNumber),
                    "Term number must be from 1 to 3.");
            }

            if (string.IsNullOrWhiteSpace(
                    componentName))
            {
                throw new ArgumentException(
                    "A component name is required.",
                    nameof(componentName));
            }

            GradingPolicy policy =
                GradingPolicy.Create(
                    schoolClass.GradeLevel,
                    academicYear.StartYear,
                    subject.SubjectName);

            IReadOnlyList<AssessmentItem>
                assessmentItems =
                    await _assessmentRepository
                        .GetItemsAsync(
                            schoolClass.Id,
                            subject.Id,
                            termNumber,
                            cancellationToken);

            IReadOnlyList<LearnerAssessmentScore>
                savedScores =
                    await _assessmentRepository
                        .GetScoresAsync(
                            schoolClass.Id,
                            subject.Id,
                            termNumber,
                            cancellationToken);

            string normalizedComponentName =
                NormalizeComponentKey(
                    componentName);

            List<AssessmentItem> componentItems =
                assessmentItems
                    .Where(
                        item =>
                            NormalizeComponentKey(
                                item.ComponentName)
                            .Equals(
                                normalizedComponentName,
                                StringComparison.Ordinal))
                    .OrderBy(
                        item => item.DisplayOrder)
                    .ToList();

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
                                group => group.Key,
                                group =>
                                    group
                                        .OrderByDescending(
                                            score =>
                                                score.UpdatedAtUtc)
                                        .First());

            Dictionary<int, int?> result =
                new Dictionary<int, int?>();

            foreach (Learner learner in learners)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                result[learner.Id] =
                    policy.UsesNumericalGrades
                        ? CalculateLearnerTermGrade(
                            learner.Id,
                            componentItems,
                            scoreLookup,
                            policy)
                        : null;
            }

            return result;
        }

        private int? CalculateComponentAverage(
            int learnerId,
            IReadOnlyList<string> componentNames,
            IReadOnlyList<AssessmentItem>
                assessmentItems,
            IReadOnlyDictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    scoreLookup,
            GradingPolicy policy)
        {
            List<int> componentGrades =
                new List<int>();

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
                        learnerId,
                        componentItems,
                        scoreLookup,
                        policy);

                if (!componentGrade.HasValue)
                {
                    return null;
                }

                componentGrades.Add(
                    componentGrade.Value);
            }

            if (componentGrades.Count == 0)
            {
                return null;
            }

            return (int)Math.Round(
                componentGrades.Average(),
                0,
                MidpointRounding.AwayFromZero);
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

        private static string NormalizeComponentKey(
            string? componentName)
        {
            string normalized =
                NormalizeComponentName(
                    componentName)
                    .ToLowerInvariant()
                    .Replace("&", "and")
                    .Replace(".", string.Empty)
                    .Replace("  ", " ")
                    .Trim();

            if (normalized == "pe and health" ||
                normalized ==
                    "physical education & health")
            {
                return "physical education and health";
            }

            return normalized;
        }
    }
}
