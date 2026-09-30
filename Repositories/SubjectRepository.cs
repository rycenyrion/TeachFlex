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
    public interface ISubjectRepository
    {
        Task<IReadOnlyList<Subject>>
            GetByGradeLevelAsync(
                string gradeLevel,
                CancellationToken cancellationToken =
                    default);

        Task<IReadOnlyList<Subject>>
            GetByClassAsync(
                int schoolClassId,
                CancellationToken cancellationToken =
                    default);

        Task<Subject> SaveAsync(
            Subject subject,
            CancellationToken cancellationToken =
                default);

        Task AssignGradeLevelSubjectsAsync(
            int schoolClassId,
            string gradeLevel,
            string trackStrand =
                "Not Applicable",
            CancellationToken cancellationToken =
                default);
    }

    public class SubjectRepository :
        ISubjectRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public SubjectRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<IReadOnlyList<Subject>>
            GetByGradeLevelAsync(
                string gradeLevel,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database.Subjects
                .AsNoTracking()
                .Where(
                    subject =>
                        subject.GradeLevel ==
                            gradeLevel &&
                        subject.IsActive)
                .OrderBy(
                    subject =>
                        subject.DisplayOrder)
                .ThenBy(
                    subject =>
                        subject.SubjectName)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<IReadOnlyList<Subject>>
            GetByClassAsync(
                int schoolClassId,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database.ClassSubjects
                .AsNoTracking()
                .Where(
                    classSubject =>
                        classSubject.SchoolClassId ==
                            schoolClassId &&
                        classSubject.IsActive &&
                        classSubject.Subject != null &&
                        classSubject.Subject.IsActive)
                .Select(
                    classSubject =>
                        classSubject.Subject!)
                .OrderBy(
                    subject =>
                        subject.DisplayOrder)
                .ThenBy(
                    subject =>
                        subject.SubjectName)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<Subject> SaveAsync(
            Subject subject,
            CancellationToken cancellationToken =
                default)
        {
            ArgumentNullException.ThrowIfNull(
                subject);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            subject.UpdatedAtUtc =
                DateTime.UtcNow;

            bool isNewSubject =
                subject.Id == 0;

            if (isNewSubject)
            {
                subject.CreatedAtUtc =
                    DateTime.UtcNow;

                await database.Subjects.AddAsync(
                    subject,
                    cancellationToken);
            }
            else
            {
                database.Subjects.Update(
                    subject);
            }

            await database.SaveChangesAsync(
                cancellationToken);

            if (isNewSubject &&
                subject.GradeLevel is
                    "Grade 7" or
                    "Grade 8" or
                    "Grade 9" or
                    "Grade 10")
            {
                List<int> matchingClassIds =
                    await database.Classes
                        .Where(
                            schoolClass =>
                                schoolClass.GradeLevel ==
                                    subject.GradeLevel)
                        .Select(
                            schoolClass =>
                                schoolClass.Id)
                        .ToListAsync(
                            cancellationToken);

                foreach (int schoolClassId
                         in matchingClassIds)
                {
                    await database.ClassSubjects.AddAsync(
                        new ClassSubject
                        {
                            SchoolClassId =
                                schoolClassId,

                            SubjectId =
                                subject.Id,

                            IsActive =
                                true,

                            CreatedAtUtc =
                                DateTime.UtcNow,

                            UpdatedAtUtc =
                                DateTime.UtcNow
                        },
                        cancellationToken);
                }

                await database.SaveChangesAsync(
                    cancellationToken);
            }

            return subject;
        }

        public async Task
            AssignGradeLevelSubjectsAsync(
                int schoolClassId,
                string gradeLevel,
                string trackStrand =
                    "Not Applicable",
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            bool isSeniorHigh =
                gradeLevel == "Grade 11" ||
                gradeLevel == "Grade 12";

            string selectedTrackStrand =
                string.IsNullOrWhiteSpace(
                    trackStrand)
                    ? "All"
                    : trackStrand.Trim();
            bool isAcademicCluster =
    selectedTrackStrand is
        "Arts, Social Sciences, and Humanities" or
        "Business and Entrepreneurship" or
        "Science, Technology, Engineering, and Mathematics" or
        "Sports, Health, and Wellness";

            bool isTechnicalProfessionalCluster =
                selectedTrackStrand is
                    "Aesthetic, Wellness, and Human Care" or
                    "Agri-Fishery Business and Food Innovation" or
                    "Artisanry and Creative Enterprise" or
                    "Automotive and Small Engine Technologies" or
                    "Construction and Building Technologies" or
                    "Creative Arts and Design Technologies" or
                    "Hospitality and Tourism" or
                    "ICT Support and Computer Programming Technologies" or
                    "Industrial Technologies" or
                    "Maritime";

            List<int> subjectIds =
                await database.Subjects
                    .Where(
                        subject =>
                            subject.GradeLevel ==
                                gradeLevel &&
                            subject.IsActive &&
                            (
                                !isSeniorHigh ||
                               subject.TrackStrand ==
    "All" ||
subject.TrackStrand ==
    selectedTrackStrand ||
subject.TrackStrand ==
    string.Empty ||
(
    subject.TrackStrand ==
        "All Academic" &&
    isAcademicCluster
) ||
(
    subject.TrackStrand ==
        "All Technical-Professional" &&
    isTechnicalProfessionalCluster
)
                            ))
                    .Select(
                        subject =>
                            subject.Id)
                    .ToListAsync(
                        cancellationToken);

            List<ClassSubject>
                existingAssignments =
                    await database.ClassSubjects
                        .Where(
                            classSubject =>
                                classSubject
                                    .SchoolClassId ==
                                        schoolClassId)
                        .ToListAsync(
                            cancellationToken);

            foreach (ClassSubject assignment
                     in existingAssignments)
            {
                assignment.IsActive =
                    subjectIds.Contains(
                        assignment.SubjectId);

                assignment.UpdatedAtUtc =
                    DateTime.UtcNow;
            }

            foreach (int subjectId
                     in subjectIds)
            {
                ClassSubject? existingAssignment =
                    existingAssignments
                        .FirstOrDefault(
                            assignment =>
                                assignment.SubjectId ==
                                    subjectId);

                if (existingAssignment != null)
                {
                    existingAssignment.IsActive =
                        true;

                    existingAssignment.UpdatedAtUtc =
                        DateTime.UtcNow;

                    continue;
                }

                await database.ClassSubjects.AddAsync(
                    new ClassSubject
                    {
                        SchoolClassId =
                            schoolClassId,

                        SubjectId =
                            subjectId,

                        IsActive =
                            true,

                        CreatedAtUtc =
                            DateTime.UtcNow,

                        UpdatedAtUtc =
                            DateTime.UtcNow
                    },
                    cancellationToken);
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }
    }
}
