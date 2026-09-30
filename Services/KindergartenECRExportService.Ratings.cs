using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class KindergartenECRExportService
    {
        private const int KinderRatingColumnCount =
            60;

        private const int KinderLearnerRowCount =
            49;

        static partial void WriteTermRatings(
            object workbook,
            KindergartenEcrExportRequest request)
        {
            List<KindergartenEcrLearnerExportRow>
                maleLearners =
                    request.Learners
                        .Where(
                            learner =>
                                IsMaleKinderLearner(
                                    learner.Sex))
                        .OrderBy(
                            learner =>
                                learner.LearnerName)
                        .Take(
                            KinderLearnerRowCount)
                        .ToList();

            List<KindergartenEcrLearnerExportRow>
                femaleLearners =
                    request.Learners
                        .Where(
                            learner =>
                                !IsMaleKinderLearner(
                                    learner.Sex))
                        .OrderBy(
                            learner =>
                                learner.LearnerName)
                        .Take(
                            KinderLearnerRowCount)
                        .ToList();

            List<KindergartenEcrCompetencyExportRow>
                competencies =
                    request.Competencies
                        .OrderBy(
                            competency =>
                                competency.DisplayOrder)
                        .Take(
                            KinderRatingColumnCount)
                        .ToList();

            Dictionary<
                (int LearnerId,
                 int CompetencyId,
                 int TermNumber),
                string> ratingLookup =
                    request.Ratings
                        .GroupBy(
                            rating =>
                                (
                                    rating.LearnerId,
                                    rating.KindergartenCompetencyId,
                                    rating.TermNumber
                                ))
                        .ToDictionary(
                            group =>
                                group.Key,
                            group =>
                                NormalizeKinderRating(
                                    group.Last().Rating));

            WriteSingleTermRatings(
                workbook,
                "TERM 1 SUMMARY",
                1,
                maleLearners,
                femaleLearners,
                competencies,
                ratingLookup);

            WriteSingleTermRatings(
                workbook,
                "TERM 2 SUMMARY",
                2,
                maleLearners,
                femaleLearners,
                competencies,
                ratingLookup);

            WriteSingleTermRatings(
                workbook,
                "TERM 3 SUMMARY",
                3,
                maleLearners,
                femaleLearners,
                competencies,
                ratingLookup);
        }

        private static void WriteSingleTermRatings(
            object workbook,
            string worksheetName,
            int termNumber,
            IReadOnlyList<
                KindergartenEcrLearnerExportRow>
                    maleLearners,
            IReadOnlyList<
                KindergartenEcrLearnerExportRow>
                    femaleLearners,
            IReadOnlyList<
                KindergartenEcrCompetencyExportRow>
                    competencies,
            IReadOnlyDictionary<
                (int LearnerId,
                 int CompetencyId,
                 int TermNumber),
                string> ratingLookup)
        {
            object? worksheet =
                null;

            try
            {
                worksheet =
                    GetWorksheet(
                        workbook,
                        worksheetName);

                object?[,] maleValues =
                    CreateRatingMatrix(
                        maleLearners,
                        competencies,
                        termNumber,
                        ratingLookup);

                object?[,] femaleValues =
                    CreateRatingMatrix(
                        femaleLearners,
                        competencies,
                        termNumber,
                        ratingLookup);

                SetRangeValues(
                    worksheet,
                    "N16:BU64",
                    maleValues);

                SetRangeValues(
                    worksheet,
                    "N66:BU114",
                    femaleValues);
            }
            finally
            {
                ReleaseComObject(
                    worksheet);
            }
        }

        private static object?[,] CreateRatingMatrix(
            IReadOnlyList<
                KindergartenEcrLearnerExportRow>
                    learners,
            IReadOnlyList<
                KindergartenEcrCompetencyExportRow>
                    competencies,
            int termNumber,
            IReadOnlyDictionary<
                (int LearnerId,
                 int CompetencyId,
                 int TermNumber),
                string> ratingLookup)
        {
            object?[,] values =
                new object[
                    KinderLearnerRowCount,
                    KinderRatingColumnCount];

            for (int learnerIndex = 0;
                 learnerIndex < KinderLearnerRowCount;
                 learnerIndex++)
            {
                for (int competencyIndex = 0;
                     competencyIndex <
                        KinderRatingColumnCount;
                     competencyIndex++)
                {
                    values[
                        learnerIndex,
                        competencyIndex] =
                            string.Empty;
                }
            }

            for (int learnerIndex = 0;
                 learnerIndex < learners.Count;
                 learnerIndex++)
            {
                KindergartenEcrLearnerExportRow learner =
                    learners[learnerIndex];

                for (int competencyIndex = 0;
                     competencyIndex <
                        competencies.Count;
                     competencyIndex++)
                {
                    KindergartenEcrCompetencyExportRow
                        competency =
                            competencies[
                                competencyIndex];

                    ratingLookup.TryGetValue(
                        (
                            learner.LearnerId,
                            competency
                                .KindergartenCompetencyId,
                            termNumber
                        ),
                        out string? rating);

                    values[
                        learnerIndex,
                        competencyIndex] =
                            rating
                            ?? string.Empty;
                }
            }

            return values;
        }

        private static void SetRangeValues(
            object worksheet,
            string rangeAddress,
            object?[,] values)
        {
            object? range =
                null;

            try
            {
                dynamic selectedWorksheet =
                    worksheet;

                range =
                    selectedWorksheet.Range[
                        rangeAddress];

                dynamic selectedRange =
                    range;

                selectedRange.Value2 =
                    values;
            }
            finally
            {
                ReleaseComObject(
                    range);
            }
        }

        private static string NormalizeKinderRating(
            string rating)
        {
            string normalized =
                rating?
                    .Trim()
                    .ToUpperInvariant()
                ?? string.Empty;

            return normalized is
                "BG" or "DV" or "CO"
                    ? normalized
                    : string.Empty;
        }
    }
}