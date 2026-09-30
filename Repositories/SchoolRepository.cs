using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface ISchoolRepository
    {
        Task<School?> GetActiveSchoolAsync(
            CancellationToken cancellationToken =
                default);

        Task<School> SaveAsync(
            School school,
            CancellationToken cancellationToken =
                default);
    }

    public class SchoolRepository :
        ISchoolRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public SchoolRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<School?>
            GetActiveSchoolAsync(
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database.Schools
                .AsNoTracking()
                .Where(
                    school =>
                        school.IsActive)
                .OrderBy(
                    school =>
                        school.Id)
                .FirstOrDefaultAsync(
                    cancellationToken);
        }

        public async Task<School> SaveAsync(
            School school,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                school);

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            school.UpdatedAtUtc =
                DateTime.UtcNow;

            if (school.Id == 0)
            {
                school.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.Schools
                    .AddAsync(
                        school,
                        cancellationToken);
            }
            else
            {
                database.Schools.Update(
                    school);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return school;
        }
    }
}