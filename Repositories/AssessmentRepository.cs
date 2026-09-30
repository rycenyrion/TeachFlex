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
    public interface IAssessmentRepository
    {
        Task<IReadOnlyList<AssessmentItem>>
            GetItemsAsync(
                int schoolClassId,
                int subjectId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task<AssessmentItem> SaveItemAsync(
            AssessmentItem assessmentItem,
            CancellationToken cancellationToken =
                default);

        Task<bool> ArchiveItemAsync(
            int assessmentItemId,
            CancellationToken cancellationToken =
                default);

        Task<IReadOnlyList<LearnerAssessmentScore>>
            GetScoresAsync(
                int schoolClassId,
                int subjectId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task SaveScoresAsync(
            IEnumerable<LearnerAssessmentScore> scores,
            CancellationToken cancellationToken =
                default);
    }

    public class AssessmentRepository :
        IAssessmentRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public AssessmentRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<
            IReadOnlyList<AssessmentItem>>
                GetItemsAsync(
                    int schoolClassId,
                    int subjectId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database.AssessmentItems
                .AsNoTracking()
                .Where(
                    item =>
                        item.SchoolClassId ==
                            schoolClassId &&
                        item.SubjectId ==
                            subjectId &&
                        item.TermNumber ==
                            termNumber &&
                        item.IsActive)
                .OrderBy(
                    item =>
                        item.DisplayOrder)
                .ThenBy(
                    item =>
                        item.Id)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<AssessmentItem>
            SaveItemAsync(
                AssessmentItem assessmentItem,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                assessmentItem);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            assessmentItem.Category =
                assessmentItem.Category.Trim();

            assessmentItem.AssessmentDomain =
                assessmentItem
                    .AssessmentDomain.Trim();

            assessmentItem.AssessmentName =
                assessmentItem.AssessmentName.Trim();

            assessmentItem.UpdatedAtUtc =
                DateTime.UtcNow;

            if (assessmentItem.Id == 0)
            {
                assessmentItem.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.AssessmentItems
                    .AddAsync(
                        assessmentItem,
                        cancellationToken);
            }
            else
            {
                database.AssessmentItems.Update(
                    assessmentItem);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return assessmentItem;
        }

        public async Task<bool>
            ArchiveItemAsync(
                int assessmentItemId,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            AssessmentItem? assessmentItem =
                await database.AssessmentItems
                    .FirstOrDefaultAsync(
                        item =>
                            item.Id ==
                                assessmentItemId,
                        cancellationToken);

            if (assessmentItem == null)
            {
                return false;
            }

            assessmentItem.IsActive =
                false;

            assessmentItem.UpdatedAtUtc =
                DateTime.UtcNow;

            await database.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<
            IReadOnlyList<LearnerAssessmentScore>>
                GetScoresAsync(
                    int schoolClassId,
                    int subjectId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database
                .LearnerAssessmentScores
                .AsNoTracking()
                .Include(
                    score =>
                        score.AssessmentItem)
                .Where(
                    score =>
                        score.AssessmentItem != null &&
                        score.AssessmentItem
                            .SchoolClassId ==
                                schoolClassId &&
                        score.AssessmentItem
                            .SubjectId ==
                                subjectId &&
                        score.AssessmentItem
                            .TermNumber ==
                                termNumber &&
                        score.AssessmentItem
                            .IsActive)
                .OrderBy(
                    score =>
                        score.LearnerId)
                .ThenBy(
                    score =>
                        score.AssessmentItem!
                            .DisplayOrder)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task SaveScoresAsync(
            IEnumerable<LearnerAssessmentScore>
                scores,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                scores);

            List<LearnerAssessmentScore>
                scoreEntries =
                    scores.ToList();

            if (scoreEntries.Count == 0)
            {
                return;
            }

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            List<int> assessmentItemIds =
                scoreEntries
                    .Select(
                        score =>
                            score.AssessmentItemId)
                    .Distinct()
                    .ToList();

            List<int> learnerIds =
                scoreEntries
                    .Select(
                        score =>
                            score.LearnerId)
                    .Distinct()
                    .ToList();

            List<LearnerAssessmentScore>
                existingScores =
                    await database
                        .LearnerAssessmentScores
                        .Where(
                            savedScore =>
                                assessmentItemIds.Contains(
                                    savedScore
                                        .AssessmentItemId) &&
                                learnerIds.Contains(
                                    savedScore.LearnerId))
                        .ToListAsync(
                            cancellationToken);

            foreach (LearnerAssessmentScore score
                     in scoreEntries)
            {
                LearnerAssessmentScore?
                    existingScore =
                        existingScores
                            .FirstOrDefault(
                                savedScore =>
                                    savedScore
                                        .AssessmentItemId ==
                                            score
                                                .AssessmentItemId &&
                                    savedScore.LearnerId ==
                                        score.LearnerId);

                if (existingScore == null)
                {
                    score.Remarks =
                        score.Remarks.Trim();

                    score.CreatedAtUtc =
                        DateTime.UtcNow;

                    score.UpdatedAtUtc =
                        DateTime.UtcNow;

                    await database
                        .LearnerAssessmentScores
                        .AddAsync(
                            score,
                            cancellationToken);

                    continue;
                }

                existingScore.Score =
                    score.Score;

                existingScore.Remarks =
                    score.Remarks.Trim();

                existingScore.UpdatedAtUtc =
                    DateTime.UtcNow;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }
    }
}