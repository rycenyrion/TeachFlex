using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface ITeacherRepository
    {
        Task<Teacher?> GetActiveTeacherAsync(
            int schoolId,
            CancellationToken cancellationToken =
                default);

        Task<Teacher> SaveAsync(
            Teacher teacher,
            CancellationToken cancellationToken =
                default);
    }

    public class TeacherRepository :
        ITeacherRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public TeacherRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<Teacher?>
            GetActiveTeacherAsync(
                int schoolId,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database.Teachers
                .AsNoTracking()
                .Where(
                    teacher =>
                        teacher.SchoolId ==
                            schoolId &&
                        teacher.IsActive)
                .OrderBy(
                    teacher =>
                        teacher.Id)
                .FirstOrDefaultAsync(
                    cancellationToken);
        }

        public async Task<Teacher> SaveAsync(
            Teacher teacher,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                teacher);

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            teacher.UpdatedAtUtc =
                DateTime.UtcNow;

            if (teacher.Id == 0)
            {
                teacher.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.Teachers
                    .AddAsync(
                        teacher,
                        cancellationToken);
            }
            else
            {
                database.Teachers.Update(
                    teacher);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return teacher;
        }
    }
}