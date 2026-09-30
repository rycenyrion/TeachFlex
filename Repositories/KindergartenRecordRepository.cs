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
    public class KindergartenRecordRepository :
        IKindergartenRecordRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public KindergartenRecordRepository(
            IDbContextFactory<TeachFlexDbContext>
                dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<IReadOnlyList<
            KindergartenCompetency>>
                GetCompetenciesAsync(
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database
                .KindergartenCompetencies
                .AsNoTracking()
                .Where(
                    competency =>
                        competency.IsActive)
                .OrderBy(
                    competency =>
                        competency.DisplayOrder)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveCompetenciesAsync(
            IReadOnlyList<
                KindergartenCompetency>
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

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (KindergartenCompetency competency
                     in competencies)
            {
                KindergartenCompetency?
                    existingCompetency =
                        await database
                            .KindergartenCompetencies
                            .FirstOrDefaultAsync(
                                existing =>
                                    existing.CompetencyCode ==
                                    competency.CompetencyCode,
                                cancellationToken);

                if (existingCompetency == null)
                {
                    competency.CompetencyCode =
                        competency.CompetencyCode
                            .Trim();

                    competency.DevelopmentArea =
                        competency.DevelopmentArea
                            .Trim();

                    competency.SubDomain =
                        competency.SubDomain?
                            .Trim()
                        ?? string.Empty;

                    competency.Description =
                        competency.Description
                            .Trim();

                    competency.CreatedAtUtc =
                        currentTime;

                    competency.UpdatedAtUtc =
                        currentTime;

                    await database
                        .KindergartenCompetencies
                        .AddAsync(
                            competency,
                            cancellationToken);

                    continue;
                }

                existingCompetency.DevelopmentArea =
                    competency.DevelopmentArea
                        .Trim();

                existingCompetency.SubDomain =
                    competency.SubDomain?
                        .Trim()
                    ?? string.Empty;

                existingCompetency.Description =
                    competency.Description
                        .Trim();

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

        public async Task<IReadOnlyList<
            KindergartenLearnerRating>>
                GetRatingsAsync(
                    int schoolClassId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            ValidateTermNumber(
                termNumber);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database
                .KindergartenLearnerRatings
                .AsNoTracking()
                .Where(
                    rating =>
                        rating.SchoolClassId ==
                            schoolClassId &&
                        rating.TermNumber ==
                            termNumber)
                .OrderBy(
                    rating =>
                        rating.LearnerId)
                .ThenBy(
                    rating =>
                        rating.KindergartenCompetencyId)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveRatingsAsync(
            IReadOnlyList<
                KindergartenLearnerRating>
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

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (KindergartenLearnerRating rating
                     in ratings)
            {
                ValidateTermNumber(
                    rating.TermNumber);

                string normalizedRating =
                    NormalizeRating(
                        rating.Rating);

                KindergartenLearnerRating?
                    existingRating =
                        await database
                            .KindergartenLearnerRatings
                            .FirstOrDefaultAsync(
                                existing =>
                                    existing.SchoolClassId ==
                                        rating.SchoolClassId &&
                                    existing.LearnerId ==
                                        rating.LearnerId &&
                                    existing
                                        .KindergartenCompetencyId ==
                                        rating
                                            .KindergartenCompetencyId &&
                                    existing.TermNumber ==
                                        rating.TermNumber,
                                cancellationToken);

                if (string.IsNullOrWhiteSpace(
                        normalizedRating))
                {
                    if (existingRating != null)
                    {
                        database
                            .KindergartenLearnerRatings
                            .Remove(
                                existingRating);
                    }

                    continue;
                }

                if (existingRating == null)
                {
                    rating.Rating =
                        normalizedRating;

                    rating.Observation =
                        rating.Observation?
                            .Trim()
                        ?? string.Empty;

                    rating.CreatedAtUtc =
                        currentTime;

                    rating.UpdatedAtUtc =
                        currentTime;

                    await database
                        .KindergartenLearnerRatings
                        .AddAsync(
                            rating,
                            cancellationToken);

                    continue;
                }

                existingRating.Rating =
                    normalizedRating;

                existingRating.Observation =
                    rating.Observation?
                        .Trim()
                    ?? string.Empty;

                existingRating.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<KindergartenTermRemark?>
            GetTermRemarkAsync(
                int schoolClassId,
                int learnerId,
                int termNumber,
                CancellationToken cancellationToken =
                    default)
        {
            ValidateTermNumber(
                termNumber);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database
                .KindergartenTermRemarks
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    remark =>
                        remark.SchoolClassId ==
                            schoolClassId &&
                        remark.LearnerId ==
                            learnerId &&
                        remark.TermNumber ==
                            termNumber,
                    cancellationToken);
        }
        public async Task<IReadOnlyList<
    KindergartenTermRemark>>
        GetTermRemarksAsync(
            int schoolClassId,
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database
                .KindergartenTermRemarks
                .AsNoTracking()
                .Where(
                    remark =>
                        remark.SchoolClassId ==
                            schoolClassId)
                .OrderBy(
                    remark =>
                        remark.LearnerId)
                .ThenBy(
                    remark =>
                        remark.TermNumber)
                .ToListAsync(
                    cancellationToken);
        }
        public async Task SaveTermRemarkAsync(
            KindergartenTermRemark remark,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                remark);

            ValidateTermNumber(
                remark.TermNumber);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            KindergartenTermRemark?
                existingRemark =
                    await database
                        .KindergartenTermRemarks
                        .FirstOrDefaultAsync(
                            existing =>
                                existing.SchoolClassId ==
                                    remark.SchoolClassId &&
                                existing.LearnerId ==
                                    remark.LearnerId &&
                                existing.TermNumber ==
                                    remark.TermNumber,
                            cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            if (existingRemark == null)
            {
                remark.TeacherComment =
                    CleanText(
                        remark.TeacherComment);

                remark.LearnerStrengths =
                    CleanText(
                        remark.LearnerStrengths);

                remark.SuggestedInterventions =
                    CleanText(
                        remark.SuggestedInterventions);

                remark.CreatedAtUtc =
                    currentTime;

                remark.UpdatedAtUtc =
                    currentTime;

                await database
                    .KindergartenTermRemarks
                    .AddAsync(
                        remark,
                        cancellationToken);
            }
            else
            {
                existingRemark.TeacherComment =
                    CleanText(
                        remark.TeacherComment);

                existingRemark.LearnerStrengths =
                    CleanText(
                        remark.LearnerStrengths);

                existingRemark.SuggestedInterventions =
                    CleanText(
                        remark.SuggestedInterventions);

                existingRemark.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        private static string NormalizeRating(
            string rating)
        {
            string normalized =
                rating?
                    .Trim()
                    .ToUpperInvariant()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    normalized))
            {
                return string.Empty;
            }

            if (normalized is not
                ("BG" or "DV" or "CO"))
            {
                throw new ArgumentException(
                    "Kinder rating must be BG, DV, CO, or blank.",
                    nameof(rating));
            }

            return normalized;
        }

        private static string CleanText(
            string value)
        {
            return value?
                .Trim()
            ?? string.Empty;
        }

        private static void ValidateTermNumber(
            int termNumber)
        {
            if (termNumber < 1 ||
                termNumber > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(termNumber),
                    "Kinder term number must be from 1 to 3.");
            }
        }
    }
}