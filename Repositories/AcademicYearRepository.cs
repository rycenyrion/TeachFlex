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
    public interface IAcademicYearRepository
    {
        event Action<AcademicYear>?
            CurrentAcademicYearChanged;

        Task<IReadOnlyList<AcademicYear>>
            GetAllAsync(
                int schoolId,
                CancellationToken cancellationToken =
                    default);

        Task<AcademicYear?>
            GetCurrentAsync(
                int schoolId,
                CancellationToken cancellationToken =
                    default);

        Task<AcademicYear> SaveAsync(
            AcademicYear academicYear,
            CancellationToken cancellationToken =
                default);

        Task<AcademicYear> SetCurrentAsync(
            int schoolId,
            int academicYearId,
            CancellationToken cancellationToken =
                default);
    }

    public class AcademicYearRepository :
        IAcademicYearRepository
    {
        public event Action<AcademicYear>?
            CurrentAcademicYearChanged;

        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public AcademicYearRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<
            IReadOnlyList<AcademicYear>>
                GetAllAsync(
                    int schoolId,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database.AcademicYears
                .AsNoTracking()
                .Where(
                    academicYear =>
                        academicYear.SchoolId ==
                            schoolId)
                .OrderByDescending(
                    academicYear =>
                        academicYear.StartYear)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<AcademicYear?>
            GetCurrentAsync(
                int schoolId,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database.AcademicYears
                .AsNoTracking()
                .Where(
                    academicYear =>
                        academicYear.SchoolId ==
                            schoolId &&
                        academicYear.IsCurrent)
                .OrderByDescending(
                    academicYear =>
                        academicYear.StartYear)
                .FirstOrDefaultAsync(
                    cancellationToken);
        }

        public async Task<AcademicYear> SaveAsync(
            AcademicYear academicYear,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                academicYear);

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            await using var transaction =
                await database.Database
                    .BeginTransactionAsync(
                        cancellationToken);

            if (academicYear.IsCurrent)
            {
                List<AcademicYear>
                    otherCurrentYears =
                        await database
                            .AcademicYears
                            .Where(
                                existingYear =>
                                    existingYear
                                        .SchoolId ==
                                            academicYear
                                                .SchoolId &&
                                    existingYear.Id !=
                                        academicYear.Id &&
                                    existingYear
                                        .IsCurrent)
                            .ToListAsync(
                                cancellationToken);

                foreach (AcademicYear existingYear
                         in otherCurrentYears)
                {
                    existingYear.IsCurrent =
                        false;

                    existingYear.UpdatedAtUtc =
                        DateTime.UtcNow;
                }
            }

            academicYear.UpdatedAtUtc =
                DateTime.UtcNow;

            if (academicYear.Id == 0)
            {
                academicYear.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.AcademicYears
                    .AddAsync(
                        academicYear,
                        cancellationToken);
            }
            else
            {
                database.AcademicYears.Update(
                    academicYear);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            if (academicYear.IsCurrent)
            {
                CurrentAcademicYearChanged?.Invoke(
                    academicYear);
            }

            return academicYear;
        }

        public async Task<AcademicYear> SetCurrentAsync(
            int schoolId,
            int academicYearId,
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory.CreateDbContextAsync(
                    cancellationToken);

            await using var transaction =
                await database.Database.BeginTransactionAsync(
                    cancellationToken);

            AcademicYear selectedYear =
                await database.AcademicYears
                    .FirstOrDefaultAsync(
                        academicYear =>
                            academicYear.Id == academicYearId &&
                            academicYear.SchoolId == schoolId,
                        cancellationToken)
                ?? throw new InvalidOperationException(
                    "The selected school year could not be found.");

            List<AcademicYear> schoolYears =
                await database.AcademicYears
                    .Where(
                        academicYear =>
                            academicYear.SchoolId == schoolId)
                    .ToListAsync(
                        cancellationToken);

            DateTime changedAtUtc =
                DateTime.UtcNow;

            foreach (AcademicYear schoolYear in schoolYears)
            {
                bool shouldBeCurrent =
                    schoolYear.Id == academicYearId;

                if (schoolYear.IsCurrent != shouldBeCurrent)
                {
                    schoolYear.IsCurrent =
                        shouldBeCurrent;

                    schoolYear.UpdatedAtUtc =
                        changedAtUtc;
                }
            }

            await database.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            CurrentAcademicYearChanged?.Invoke(
                selectedYear);

            return selectedYear;
        }
    }
}
