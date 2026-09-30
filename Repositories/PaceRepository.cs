using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface IPaceRepository
    {
        Task<IReadOnlyList<PaceCompetency>>
            GetCompetenciesAsync(
                int subjectId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task SaveCompetenciesAsync(
            IReadOnlyList<PaceCompetency>
                competencies,
            CancellationToken cancellationToken =
                default);

        Task<IReadOnlyList<LearnerPaceRating>>
            GetRatingsAsync(
                int schoolClassId,
                int subjectId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task SaveRatingsAsync(
            IReadOnlyList<LearnerPaceRating>
                ratings,
            CancellationToken cancellationToken =
                default);

        Task<IReadOnlyList<LearnerPaceSummary>>
            GetSummariesAsync(
                int schoolClassId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task SaveSummariesAsync(
            IReadOnlyList<LearnerPaceSummary>
                summaries,
            CancellationToken cancellationToken =
                default);
    }

    public class PaceRepository :
        IPaceRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public PaceRepository(
            IDbContextFactory<TeachFlexDbContext>
                dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<
            IReadOnlyList<PaceCompetency>>
                GetCompetenciesAsync(
                    int subjectId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database
                .PaceCompetencies
                .AsNoTracking()
                .Where(
                    competency =>
                        competency.SubjectId ==
                            subjectId &&
                        competency.TermNumber ==
                            termNumber &&
                        competency.IsActive)
                .OrderBy(
                    competency =>
                        competency.DisplayOrder)
                .ThenBy(
                    competency =>
                        competency.CompetencyCode)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveCompetenciesAsync(
            IReadOnlyList<PaceCompetency>
                competencies,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                competencies);

            if (competencies.Count == 0)
            {
                return;
            }

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (PaceCompetency competency
                     in competencies)
            {
                PaceCompetency? existingCompetency =
                    null;

                if (competency.Id > 0)
                {
                    existingCompetency =
                        await database
                            .PaceCompetencies
                            .FirstOrDefaultAsync(
                                existing =>
                                    existing.Id ==
                                        competency.Id,
                                cancellationToken);
                }
                else
                {
                    existingCompetency =
                        await database
                            .PaceCompetencies
                            .FirstOrDefaultAsync(
                                existing =>
                                    existing.SubjectId ==
                                        competency.SubjectId &&
                                    existing.TermNumber ==
                                        competency.TermNumber &&
                                    existing.DomainName ==
                                        competency.DomainName &&
                                    existing.CompetencyCode ==
                                        competency
                                            .CompetencyCode,
                                cancellationToken);
                }

                if (existingCompetency == null)
                {
                    competency.CreatedAtUtc =
                        currentTime;

                    competency.UpdatedAtUtc =
                        currentTime;

                    await database
                        .PaceCompetencies
                        .AddAsync(
                            competency,
                            cancellationToken);

                    continue;
                }

                existingCompetency.LearningArea =
                    competency.LearningArea;

                existingCompetency.DomainName =
                    competency.DomainName;

                existingCompetency.CompetencyCode =
                    competency.CompetencyCode;

                existingCompetency.Description =
                    competency.Description;

                existingCompetency.DisplayOrder =
                    competency.DisplayOrder;

                existingCompetency.IsActive =
                    competency.IsActive;

                existingCompetency.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<
            IReadOnlyList<LearnerPaceRating>>
                GetRatingsAsync(
                    int schoolClassId,
                    int subjectId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database
                .LearnerPaceRatings
                .AsNoTracking()
                .Where(
                    rating =>
                        rating.Learner != null &&
                        rating.Learner.SchoolClassId ==
                            schoolClassId &&
                        rating.PaceCompetency != null &&
                        rating.PaceCompetency.SubjectId ==
                            subjectId &&
                        rating.PaceCompetency.TermNumber ==
                            termNumber)
                .OrderBy(
                    rating =>
                        rating.LearnerId)
                .ThenBy(
                    rating =>
                        rating.PaceCompetency!
                            .DisplayOrder)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveRatingsAsync(
            IReadOnlyList<LearnerPaceRating>
                ratings,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                ratings);

            if (ratings.Count == 0)
            {
                return;
            }

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (LearnerPaceRating rating
                     in ratings)
            {
                LearnerPaceRating? existingRating =
                    await database
                        .LearnerPaceRatings
                        .FirstOrDefaultAsync(
                            existing =>
                                existing.LearnerId ==
                                    rating.LearnerId &&
                                existing
                                    .PaceCompetencyId ==
                                    rating
                                        .PaceCompetencyId,
                            cancellationToken);

                if (existingRating == null)
                {
                    rating.Rating =
                        NormalizeRating(
                            rating.Rating);

                    rating.CreatedAtUtc =
                        currentTime;

                    rating.UpdatedAtUtc =
                        currentTime;

                    await database
                        .LearnerPaceRatings
                        .AddAsync(
                            rating,
                            cancellationToken);

                    continue;
                }

                existingRating.Rating =
                    NormalizeRating(
                        rating.Rating);

                existingRating.Remarks =
                    rating.Remarks?.Trim()
                    ?? string.Empty;

                existingRating.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<
            IReadOnlyList<LearnerPaceSummary>>
                GetSummariesAsync(
                    int schoolClassId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database
                .LearnerPaceSummaries
                .AsNoTracking()
                .Where(
                    summary =>
                        summary.SchoolClassId ==
                            schoolClassId &&
                        summary.TermNumber ==
                            termNumber)
                .OrderBy(
                    summary =>
                        summary.LearnerId)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveSummariesAsync(
            IReadOnlyList<LearnerPaceSummary>
                summaries,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                summaries);

            if (summaries.Count == 0)
            {
                return;
            }

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (LearnerPaceSummary summary
                     in summaries)
            {
                LearnerPaceSummary? existingSummary =
                    await database
                        .LearnerPaceSummaries
                        .FirstOrDefaultAsync(
                            existing =>
                                existing.LearnerId ==
                                    summary.LearnerId &&
                                existing.SchoolClassId ==
                                    summary.SchoolClassId &&
                                existing.TermNumber ==
                                    summary.TermNumber,
                            cancellationToken);

                if (existingSummary == null)
                {
                    summary.WhatLearnerCanDo =
                        summary.WhatLearnerCanDo?
                            .Trim()
                        ?? string.Empty;

                    summary
                        .WhatLearnerNeedsToImprove =
                            summary
                                .WhatLearnerNeedsToImprove?
                                .Trim()
                            ?? string.Empty;

                    summary.TeacherRemarks =
                        summary.TeacherRemarks?
                            .Trim()
                        ?? string.Empty;

                    summary.CreatedAtUtc =
                        currentTime;

                    summary.UpdatedAtUtc =
                        currentTime;

                    await database
                        .LearnerPaceSummaries
                        .AddAsync(
                            summary,
                            cancellationToken);

                    continue;
                }

                existingSummary.WhatLearnerCanDo =
                    summary.WhatLearnerCanDo?
                        .Trim()
                    ?? string.Empty;

                existingSummary
                    .WhatLearnerNeedsToImprove =
                        summary
                            .WhatLearnerNeedsToImprove?
                            .Trim()
                        ?? string.Empty;

                existingSummary.TeacherRemarks =
                    summary.TeacherRemarks?
                        .Trim()
                    ?? string.Empty;

                existingSummary.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        private static string NormalizeRating(
            string? rating)
        {
            string normalizedRating =
                rating?
                    .Trim()
                    .ToUpperInvariant()
                ?? string.Empty;

            return normalizedRating switch
            {
                "A" => "A",
                "B" => "B",
                "C" => "C",
                "D" => "D",
                "E" => "E",
                _ => string.Empty
            };
        }
    }
}