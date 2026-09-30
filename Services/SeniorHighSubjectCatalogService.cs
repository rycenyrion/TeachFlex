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
    public interface
        ISeniorHighSubjectCatalogService
    {
        Task SeedAsync(
            CancellationToken cancellationToken =
                default);
    }

    public partial class
        SeniorHighSubjectCatalogService :
            ISeniorHighSubjectCatalogService
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public SeniorHighSubjectCatalogService(
            IDbContextFactory<TeachFlexDbContext>
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

            IReadOnlyList<
                SeniorHighSubjectDefinition>
                    definitions =
                        CreateDefinitions();

            foreach (
                SeniorHighSubjectDefinition definition
                in definitions)
            {
                Subject? subject =
                    await database.Subjects
                        .FirstOrDefaultAsync(
                            existing =>
                                existing.GradeLevel ==
                                    definition.GradeLevel &&
                                existing.SubjectCode ==
                                    definition.SubjectCode,
                            cancellationToken);

                if (subject == null)
                {
                    subject =
                        new Subject
                        {
                            GradeLevel =
                                definition.GradeLevel,

                            SubjectCode =
                                definition.SubjectCode,

                            CreatedAtUtc =
                                DateTime.UtcNow
                        };

                    await database.Subjects.AddAsync(
                        subject,
                        cancellationToken);
                }

                subject.SubjectName =
                    definition.SubjectName;

                subject.LearningArea =
                    definition.SubjectCluster;

                subject.SubjectCategory =
                    definition.SubjectCategory;

                subject.SubjectCluster =
                    definition.SubjectCluster;

                subject.TrackStrand =
                    definition.TrackStrand;

                subject.DisplayOrder =
                    definition.DisplayOrder;

                subject.IsCoreSubject =
                    definition.IsCoreSubject;

                subject.TotalHours =
                    definition.TotalHours;

                subject.TermsTaught =
                    definition.TermsTaught;

                subject.UnitsPerTerm =
                    definition.UnitsPerTerm;

                subject.UnitsPerYear =
                    definition.UnitsPerYear;

                subject.WrittenOralWorksWeight =
                    definition.WrittenOralWorksWeight;

                subject.PerformanceTasksWeight =
                    definition.PerformanceTasksWeight;

                subject.SummativeTermExamWeight =
                    definition.SummativeTermExamWeight;

                subject.SummativeTestOneShare =
                    definition.SummativeTestOneShare;

                subject.SummativeTestTwoShare =
                    definition.SummativeTestTwoShare;

                subject.TermExamShare =
                    definition.TermExamShare;

                subject.IsActive =
                    true;

                subject.UpdatedAtUtc =
                    DateTime.UtcNow;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        private static IReadOnlyList<
            SeniorHighSubjectDefinition>
                CreateDefinitions()
        {
            List<SeniorHighSubjectDefinition>
                definitions =
                    new();

            AddCoreSubject(
                definitions,
                "SHS11-CORE-01",
                "Effective Communication",
                1);

            AddCoreSubject(
                definitions,
                "SHS11-CORE-02",
                "General Mathematics",
                2);

            AddCoreSubject(
                definitions,
                "SHS11-CORE-03",
                "General Science",
                3);

            AddCoreSubject(
                definitions,
                "SHS11-CORE-04",
                "Life and Career Skills",
                4);

            AddCoreSubject(
                definitions,
                "SHS11-CORE-05",
                "Mabisang Komunikasyon",
                5);

            AddCoreSubject(
                definitions,
                "SHS11-CORE-06",
                "Pag-aaral ng Kasaysayan at Lipunang Pilipino",
                6);

            AddAcademicSubjects(
                definitions);

            AddTechnicalProfessionalSubjects(
                definitions);

            return definitions;
        }

        private static void AddCoreSubject(
            ICollection<SeniorHighSubjectDefinition>
                definitions,
            string subjectCode,
            string subjectName,
            int displayOrder)
        {
            definitions.Add(
                new SeniorHighSubjectDefinition
                {
                    GradeLevel =
                        "Grade 11",

                    SubjectCode =
                        subjectCode,

                    SubjectName =
                        subjectName,

                    SubjectCategory =
                        "SSHS - CORE",

                    SubjectCluster =
                        "Core",

                    TrackStrand =
                        "All",

                    DisplayOrder =
                        displayOrder,

                    IsCoreSubject =
                        true,

                    TotalHours =
                        160,

                    TermsTaught =
                        3,

                    UnitsPerTerm =
                        2m,

                    UnitsPerYear =
                        6m,

                    WrittenOralWorksWeight =
                        20,

                    PerformanceTasksWeight =
                        50,

                    SummativeTermExamWeight =
                        30,

                    SummativeTestOneShare =
                        30,

                    SummativeTestTwoShare =
                        30,

                    TermExamShare =
                        40
                });
        }

        static partial void AddAcademicSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void
            AddTechnicalProfessionalSubjects(
                ICollection<SeniorHighSubjectDefinition>
                    definitions);
    }
}