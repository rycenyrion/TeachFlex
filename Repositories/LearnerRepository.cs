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
    public interface ILearnerRepository
    {
        Task<IReadOnlyList<Learner>>
            GetByClassAsync(
                int schoolClassId,
                string searchText = "",
                CancellationToken cancellationToken =
                    default);

        Task<Learner> SaveAsync(
            Learner learner,
            CancellationToken cancellationToken =
                default);

        Task<LearnerPromotionResult> PromoteClassAsync(
            int sourceClassId,
            int destinationClassId,
            string promotionType,
            CancellationToken cancellationToken =
                default);

        Task<bool> ArchiveAsync(
            int learnerId,
            CancellationToken cancellationToken =
                default);

        Task<bool> DeleteAsync(
            int learnerId,
            CancellationToken cancellationToken =
                default);
    }

    public class LearnerRepository :
        ILearnerRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public LearnerRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<IReadOnlyList<Learner>>
            GetByClassAsync(
                int schoolClassId,
                string searchText = "",
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            IQueryable<Learner> query =
                database.Learners
                    .AsNoTracking()
                    .Where(
                        learner =>
                            learner.SchoolClassId ==
                                schoolClassId &&
                            learner.Status !=
                                "Archived");

            if (!string.IsNullOrWhiteSpace(
                    searchText))
            {
                string[] searchTerms =
                    searchText
                        .Replace(
                            ",",
                            " ")
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries |
                            StringSplitOptions.TrimEntries);

                foreach (string searchTerm
                         in searchTerms)
                {
                    string term = searchTerm;

                    query =
                        query.Where(
                            learner =>
                                learner.Lrn.Contains(term) ||
                                learner.LastName.Contains(term) ||
                                learner.FirstName.Contains(term) ||
                                learner.MiddleName.Contains(term) ||
                                learner.Suffix.Contains(term));
                }
            }

            return await query
                .OrderBy(
                    learner =>
                        learner.Sex == "Male"
                            ? 0
                            : 1)
                .ThenBy(
                    learner =>
                        learner.LastName)
                .ThenBy(
                    learner =>
                        learner.FirstName)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<Learner> SaveAsync(
            Learner learner,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                learner);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            int destinationAcademicYearId =
                await database.Classes
                    .Where(
                        schoolClass =>
                            schoolClass.Id == learner.SchoolClassId)
                    .Select(
                        schoolClass =>
                            schoolClass.AcademicYearId)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (destinationAcademicYearId == 0)
            {
                throw new InvalidOperationException(
                    "The selected learner class could not be found.");
            }

            bool duplicateInSchoolYear =
                await database.Learners
                    .AsNoTracking()
                    .AnyAsync(
                        existingLearner =>
                            existingLearner.Id != learner.Id &&
                            existingLearner.Lrn == learner.Lrn &&
                            existingLearner.SchoolClass != null &&
                            existingLearner.SchoolClass.AcademicYearId ==
                                destinationAcademicYearId,
                        cancellationToken);

            if (duplicateInSchoolYear)
            {
                throw new InvalidOperationException(
                    $"LRN {learner.Lrn} is already enrolled in the selected school year.");
            }

            learner.UpdatedAtUtc =
                DateTime.UtcNow;

            if (learner.Id == 0)
            {
                learner.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.Learners.AddAsync(
                    learner,
                    cancellationToken);
            }
            else
            {
                database.Learners.Update(
                    learner);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return learner;
        }

        public async Task<LearnerPromotionResult>
            PromoteClassAsync(
                int sourceClassId,
                int destinationClassId,
                string promotionType,
                CancellationToken cancellationToken =
                    default)
        {
            if (sourceClassId == destinationClassId)
            {
                throw new InvalidOperationException(
                    "Select a different destination class.");
            }

            if (promotionType != "Promoted" &&
                promotionType != "Retained")
            {
                throw new InvalidOperationException(
                    "Select Promoted or Retained.");
            }

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            await using var transaction =
                await database.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            SchoolClass sourceClass =
                await database.Classes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        schoolClass =>
                            schoolClass.Id == sourceClassId,
                        cancellationToken)
                ?? throw new InvalidOperationException(
                    "The source class could not be found.");

            SchoolClass destinationClass =
                await database.Classes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        schoolClass =>
                            schoolClass.Id == destinationClassId &&
                            schoolClass.IsActive,
                        cancellationToken)
                ?? throw new InvalidOperationException(
                    "The destination class could not be found.");

            if (sourceClass.SchoolId != destinationClass.SchoolId ||
                sourceClass.AcademicYearId == destinationClass.AcademicYearId)
            {
                throw new InvalidOperationException(
                    "The destination must belong to another school year in the same school.");
            }

            ValidateGradeDestination(
                sourceClass.GradeLevel,
                destinationClass.GradeLevel,
                promotionType);

            List<Learner> sourceLearners =
                await database.Learners
                    .AsNoTracking()
                    .Where(
                        learner =>
                            learner.SchoolClassId == sourceClassId &&
                            learner.Status != "Archived" &&
                            learner.Status != "Transferred Out" &&
                            learner.Status != "Dropped")
                    .OrderBy(
                        learner =>
                            learner.LastName)
                    .ThenBy(
                        learner =>
                            learner.FirstName)
                    .ToListAsync(
                        cancellationToken);

            HashSet<string> existingLrns =
                (await database.Learners
                    .AsNoTracking()
                    .Where(
                        learner =>
                            learner.SchoolClass != null &&
                            learner.SchoolClass.AcademicYearId ==
                                destinationClass.AcademicYearId)
                    .Select(
                        learner =>
                            learner.Lrn)
                    .ToListAsync(
                        cancellationToken))
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

            int promotedCount = 0;
            int skippedCount = 0;
            DateTime currentTime =
                DateTime.UtcNow;

            foreach (Learner sourceLearner in sourceLearners)
            {
                if (existingLrns.Contains(
                        sourceLearner.Lrn))
                {
                    skippedCount++;
                    continue;
                }

                Learner destinationLearner =
                    CloneForEnrollment(
                        sourceLearner,
                        destinationClassId,
                        promotionType,
                        currentTime);

                await database.Learners.AddAsync(
                    destinationLearner,
                    cancellationToken);

                existingLrns.Add(
                    destinationLearner.Lrn);

                promotedCount++;
            }

            await database.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return new LearnerPromotionResult(
                sourceLearners.Count,
                promotedCount,
                skippedCount);
        }

        private static Learner CloneForEnrollment(
            Learner source,
            int destinationClassId,
            string promotionType,
            DateTime currentTime)
        {
            return new Learner
            {
                SchoolClassId = destinationClassId,
                Lrn = source.Lrn,
                LastName = source.LastName,
                FirstName = source.FirstName,
                MiddleName = source.MiddleName,
                Suffix = source.Suffix,
                Sex = source.Sex,
                BirthDate = source.BirthDate,
                Address = source.Address,
                ParentGuardianName = source.ParentGuardianName,
                ParentGuardianContactNumber =
                    source.ParentGuardianContactNumber,
                MotherTongue = source.MotherTongue,
                IndigenousPeopleEthnicGroup =
                    source.IndigenousPeopleEthnicGroup,
                Religion = source.Religion,
                HouseStreetSitioPurok =
                    source.HouseStreetSitioPurok,
                Barangay = source.Barangay,
                MunicipalityCity = source.MunicipalityCity,
                Province = source.Province,
                FatherName = source.FatherName,
                MotherMaidenName = source.MotherMaidenName,
                GuardianName = source.GuardianName,
                GuardianRelationship =
                    source.GuardianRelationship,
                LearningModality = source.LearningModality,
                IsCctRecipient = source.IsCctRecipient,
                CctReferenceNumber = source.CctReferenceNumber,
                IsBalikAral = source.IsBalikAral,
                LastSchoolAttended = source.LastSchoolAttended,
                LastSchoolYearAttended =
                    source.LastSchoolYearAttended,
                IsSpecialNeedsEducation =
                    source.IsSpecialNeedsEducation,
                SpecialNeedsDetails = source.SpecialNeedsDetails,
                IsAccelerated = source.IsAccelerated,
                AcceleratedLevel = source.AcceleratedLevel,
                Sf1Remarks = source.Sf1Remarks,
                Status = "Active",
                EnrollmentDate = DateTime.Today,
                EnrollmentType = promotionType,
                PreviousSchoolName = string.Empty,
                ExitDate = null,
                ExitReason = string.Empty,
                NextSchoolName = string.Empty,
                CreatedAtUtc = currentTime,
                UpdatedAtUtc = currentTime
            };
        }

        private static void ValidateGradeDestination(
            string sourceGrade,
            string destinationGrade,
            string promotionType)
        {
            int sourceOrder =
                GetGradeOrder(
                    sourceGrade);

            int destinationOrder =
                GetGradeOrder(
                    destinationGrade);

            bool valid =
                promotionType == "Promoted"
                    ? destinationOrder == sourceOrder + 1
                    : destinationOrder == sourceOrder;

            if (!valid)
            {
                string expected =
                    promotionType == "Promoted"
                        ? "the next grade level"
                        : "the same grade level";

                throw new InvalidOperationException(
                    $"For {promotionType}, select a destination class in {expected}.");
            }
        }

        private static int GetGradeOrder(
            string gradeLevel)
        {
            if (gradeLevel == "Kindergarten")
            {
                return 0;
            }

            string numberText =
                gradeLevel
                    .Replace(
                        "Grade",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

            return int.TryParse(
                numberText,
                out int gradeNumber)
                    ? gradeNumber
                    : -100;
        }

        public async Task<bool> ArchiveAsync(
            int learnerId,
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            Learner? learner =
                await database.Learners
                    .FirstOrDefaultAsync(
                        record =>
                            record.Id ==
                                learnerId,
                        cancellationToken);

            if (learner == null)
            {
                return false;
            }

            learner.Status =
                "Archived";

            learner.UpdatedAtUtc =
                DateTime.UtcNow;

            await database.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            int learnerId,
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            Learner? learner =
                await database.Learners
                    .FirstOrDefaultAsync(
                        record =>
                            record.Id ==
                                learnerId,
                        cancellationToken);

            if (learner == null)
            {
                return false;
            }

            database.Learners.Remove(
                learner);

            await database.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }

    public sealed record LearnerPromotionResult(
        int EligibleCount,
        int EnrolledCount,
        int SkippedDuplicateCount);
}
