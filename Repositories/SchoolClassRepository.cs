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
    public interface ISchoolClassRepository
    {
        Task<IReadOnlyList<SchoolClass>>
            GetByAcademicYearAsync(
                int schoolId,
                int academicYearId,
                CancellationToken cancellationToken =
                    default);
        Task AssignAdviserToActiveClassesAsync(
    int schoolId,
    int academicYearId,
    int teacherId,
    CancellationToken cancellationToken =
        default);


        Task<SchoolClass> SaveAsync(
            SchoolClass schoolClass,
            CancellationToken cancellationToken =
                default);

        Task<bool> DeleteAsync(
            int schoolClassId,
            CancellationToken cancellationToken =
                default);
    }

    public class SchoolClassRepository :
        ISchoolClassRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public SchoolClassRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<
            IReadOnlyList<SchoolClass>>
                GetByAcademicYearAsync(
                    int schoolId,
                    int academicYearId,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            return await database.Classes
                .AsNoTracking()
                .Include(
                    schoolClass =>
                        schoolClass.Adviser)
                .Where(
                    schoolClass =>
                        schoolClass.SchoolId ==
                            schoolId &&
                        schoolClass.AcademicYearId ==
                            academicYearId &&
                        schoolClass.IsActive)
                .OrderBy(
                    schoolClass =>
                        schoolClass.GradeLevel)
                .ThenBy(
                    schoolClass =>
                        schoolClass.SectionName)
                .ToListAsync(
                    cancellationToken);
        }
        public async Task
    AssignAdviserToActiveClassesAsync(
        int schoolId,
        int academicYearId,
        int teacherId,
        CancellationToken cancellationToken =
            default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            List<SchoolClass> activeClasses =
                await database
                    .Classes
                    .Where(
                        schoolClass =>
                            schoolClass.SchoolId ==
                                schoolId &&
                            schoolClass.AcademicYearId ==
                                academicYearId &&
                            schoolClass.IsActive)
                    .ToListAsync(
                        cancellationToken);

            DateTime currentTime =
                DateTime.UtcNow;

            foreach (SchoolClass schoolClass
                     in activeClasses)
            {
                schoolClass.AdviserId =
                    teacherId;

                schoolClass.UpdatedAtUtc =
                    currentTime;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }
        public async Task<SchoolClass> SaveAsync(
    SchoolClass schoolClass,
    CancellationToken cancellationToken =
        default)
        {
            ArgumentNullException.ThrowIfNull(
                schoolClass);

            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            schoolClass.SectionName =
                schoolClass.SectionName.Trim();

            DateTime currentTime =
                DateTime.UtcNow;

            if (schoolClass.Id == 0)
            {
                SchoolClass? existingClass =
                    await database.Classes
                        .FirstOrDefaultAsync(
                            savedClass =>
                                savedClass.SchoolId ==
                                    schoolClass.SchoolId &&
                                savedClass.AcademicYearId ==
                                    schoolClass.AcademicYearId &&
                                savedClass.GradeLevel ==
                                    schoolClass.GradeLevel &&
                                savedClass.SectionName ==
                                    schoolClass.SectionName,
                            cancellationToken);

                if (existingClass != null)
                {
                    existingClass.AdviserId =
                        schoolClass.AdviserId;

                    existingClass.KeyStage =
                        schoolClass.KeyStage;

                    existingClass.TrackStrand =
                        schoolClass.TrackStrand;

                    existingClass.Schedule =
                        schoolClass.Schedule;

                    existingClass.Room =
                        schoolClass.Room;

                    existingClass.IsActive =
                        true;

                    existingClass.UpdatedAtUtc =
                        currentTime;

                    await database.SaveChangesAsync(
                        cancellationToken);

                    return existingClass;
                }

                schoolClass.IsActive =
                    true;

                schoolClass.CreatedAtUtc =
                    currentTime;

                schoolClass.UpdatedAtUtc =
                    currentTime;

                await database.Classes.AddAsync(
                    schoolClass,
                    cancellationToken);
            }
            else
            {
                schoolClass.UpdatedAtUtc =
                    currentTime;

                database.Classes.Update(
                    schoolClass);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return schoolClass;
        }

        public async Task<bool> DeleteAsync(
            int schoolClassId,
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext
                database =
                    await _dbContextFactory
                        .CreateDbContextAsync(
                            cancellationToken);

            SchoolClass? schoolClass =
                await database.Classes
                    .FirstOrDefaultAsync(
                        existingClass =>
                            existingClass.Id ==
                                schoolClassId,
                        cancellationToken);

            if (schoolClass == null)
            {
                return false;
            }

            bool hasLearners =
                await database.Learners
                    .AnyAsync(
                        learner =>
                            learner.SchoolClassId ==
                                schoolClassId,
                        cancellationToken);

            if (hasLearners)
            {
                schoolClass.IsActive =
                    false;

                schoolClass.UpdatedAtUtc =
                    DateTime.UtcNow;
            }
            else
            {
                database.Classes.Remove(
                    schoolClass);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }
}