using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IDefaultSubjectCatalogService
    {
        Task SeedAsync(
            CancellationToken cancellationToken =
                default);
    }

    public class DefaultSubjectCatalogService :
        IDefaultSubjectCatalogService
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public DefaultSubjectCatalogService(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task SeedAsync(
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            IReadOnlyList<SubjectDefinition>
                definitions =
                    CreateDefinitions();

            foreach (SubjectDefinition definition
                     in definitions)
            {
                bool exists =
                    await database.Subjects
                        .AnyAsync(
                            subject =>
                                subject.GradeLevel ==
                                    definition.GradeLevel &&
                                subject.SubjectCode ==
                                    definition.SubjectCode,
                            cancellationToken);

                if (exists)
                {
                    continue;
                }

                await database.Subjects
                    .AddAsync(
                        new Subject
                        {
                            GradeLevel =
                                definition.GradeLevel,

                            SubjectCode =
                                definition.SubjectCode,

                            SubjectName =
                                definition.SubjectName,

                            LearningArea =
                                definition.SubjectName,

                            DisplayOrder =
                                definition.DisplayOrder,

                            IsCoreSubject =
                                true,

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

        private static IReadOnlyList<
            SubjectDefinition>
                CreateDefinitions()
        {
            List<SubjectDefinition> subjects =
                new List<SubjectDefinition>();

            AddGradeSubjects(
                subjects,
                "Grade 1",
                new[]
                {
                    "Language",
                    "Reading and Literacy",
                    "Mathematics",
                    "Makabansa",
                    "GMRC"
                });

            AddGradeSubjects(
                subjects,
                "Grade 2",
                new[]
                {
                    "Filipino",
                    "English",
                    "Mathematics",
                    "Makabansa",
                    "GMRC"
                });

            AddGradeSubjects(
                subjects,
                "Grade 3",
                new[]
                {
                    "Filipino",
                    "English",
                    "Mathematics",
                    "Science",
                    "Makabansa",
                    "GMRC"
                });

            string[] gradeFourToSixSubjects =
            {
                "Filipino",
                "English",
                "Mathematics",
                "Science",
                "Araling Panlipunan",
                "GMRC",
                "EPP",
                "MAPEH"
            };

            AddGradeSubjects(
                subjects,
                "Grade 4",
                gradeFourToSixSubjects);

            AddGradeSubjects(
                subjects,
                "Grade 5",
                gradeFourToSixSubjects);

            AddGradeSubjects(
                subjects,
                "Grade 6",
                gradeFourToSixSubjects);

            string[] gradeSevenToTenSubjects =
            {
                "Filipino",
                "English",
                "Mathematics",
                "Science",
                "Araling Panlipunan",
                "GMRC",
                "TLE",
                "MAPEH"
            };

            AddGradeSubjects(
                subjects,
                "Grade 7",
                gradeSevenToTenSubjects);

            AddGradeSubjects(
                subjects,
                "Grade 8",
                gradeSevenToTenSubjects);

            AddGradeSubjects(
                subjects,
                "Grade 9",
                gradeSevenToTenSubjects);

            AddGradeSubjects(
                subjects,
                "Grade 10",
                gradeSevenToTenSubjects);

            return subjects;
        }

        private static void AddGradeSubjects(
            ICollection<SubjectDefinition>
                destination,
            string gradeLevel,
            IReadOnlyList<string> subjectNames)
        {
            string gradeCode =
                gradeLevel
                    .Replace(
                        "Grade ",
                        "G",
                        StringComparison
                            .OrdinalIgnoreCase)
                    .Replace(
                        " ",
                        string.Empty);

            for (int index = 0;
                 index < subjectNames.Count;
                 index++)
            {
                string subjectName =
                    subjectNames[index];

                string subjectCode =
                    new string(
                        subjectName
                            .Where(
                                char.IsLetterOrDigit)
                            .Take(
                                8)
                            .ToArray())
                    .ToUpperInvariant();

                destination.Add(
                    new SubjectDefinition(
                        gradeLevel,
                        $"{gradeCode}-{subjectCode}",
                        subjectName,
                        index + 1));
            }
        }

        private sealed record SubjectDefinition(
            string GradeLevel,
            string SubjectCode,
            string SubjectName,
            int DisplayOrder);
    }
}