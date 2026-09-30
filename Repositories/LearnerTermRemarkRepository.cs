using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public class LearnerTermRemarkRepository :
        ILearnerTermRemarkRepository
    {
        private readonly IDbContextFactory<TeachFlexDbContext>
            _dbContextFactory;

        public LearnerTermRemarkRepository(
            IDbContextFactory<TeachFlexDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<LearnerTermRemark?> GetAsync(
            int schoolClassId,
            int learnerId,
            int termNumber,
            CancellationToken cancellationToken = default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory.CreateDbContextAsync(
                    cancellationToken);

            return await database.LearnerTermRemarks
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    remark =>
                        remark.SchoolClassId == schoolClassId &&
                        remark.LearnerId == learnerId &&
                        remark.TermNumber == termNumber,
                    cancellationToken);
        }

        public async Task<LearnerTermRemark> SaveAsync(
            LearnerTermRemark remark,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(remark);

            if (remark.TermNumber < 1 || remark.TermNumber > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remark.TermNumber),
                    "Term number must be from 1 to 3.");
            }

            await using TeachFlexDbContext database =
                await _dbContextFactory.CreateDbContextAsync(
                    cancellationToken);

            LearnerTermRemark? existing =
                await database.LearnerTermRemarks
                    .FirstOrDefaultAsync(
                        savedRemark =>
                            savedRemark.SchoolClassId ==
                                remark.SchoolClassId &&
                            savedRemark.LearnerId ==
                                remark.LearnerId &&
                            savedRemark.TermNumber ==
                                remark.TermNumber,
                        cancellationToken);

            DateTime now = DateTime.UtcNow;

            if (existing == null)
            {
                remark.SuggestedRemark =
                    remark.SuggestedRemark.Trim();
                remark.FinalRemark =
                    remark.FinalRemark.Trim();
                remark.CreatedAtUtc = now;
                remark.UpdatedAtUtc = now;
                remark.ApprovedAtUtc =
                    remark.IsTeacherApproved
                        ? now
                        : null;

                await database.LearnerTermRemarks.AddAsync(
                    remark,
                    cancellationToken);

                await database.SaveChangesAsync(
                    cancellationToken);

                return remark;
            }

            existing.GradeLevel = remark.GradeLevel;
            existing.PerformanceLevel =
                remark.PerformanceLevel;
            existing.SourceGeneralAverage =
                remark.SourceGeneralAverage;
            existing.SuggestedRemark =
                remark.SuggestedRemark.Trim();
            existing.FinalRemark =
                remark.FinalRemark.Trim();
            existing.SuggestionVariation =
                remark.SuggestionVariation;
            existing.IsTeacherApproved =
                remark.IsTeacherApproved;
            existing.NeedsReview = remark.NeedsReview;
            existing.ApprovedAtUtc =
                remark.IsTeacherApproved
                    ? remark.ApprovedAtUtc ?? now
                    : null;
            existing.UpdatedAtUtc = now;

            await database.SaveChangesAsync(
                cancellationToken);

            return existing;
        }

        public async Task MarkNeedsReviewAsync(
            int schoolClassId,
            int learnerId,
            int termNumber,
            decimal? currentGeneralAverage,
            CancellationToken cancellationToken = default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory.CreateDbContextAsync(
                    cancellationToken);

            LearnerTermRemark? existing =
                await database.LearnerTermRemarks
                    .FirstOrDefaultAsync(
                        remark =>
                            remark.SchoolClassId == schoolClassId &&
                            remark.LearnerId == learnerId &&
                            remark.TermNumber == termNumber,
                        cancellationToken);

            if (existing == null)
            {
                return;
            }

            existing.NeedsReview =
                existing.SourceGeneralAverage !=
                    currentGeneralAverage;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await database.SaveChangesAsync(
                cancellationToken);
        }
    }
}
