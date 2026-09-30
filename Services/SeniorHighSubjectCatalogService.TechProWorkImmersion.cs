using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void
            AddTechProWorkImmersionSubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions)
        {
            AddTechProWorkImmersion(
                definitions,
                "SHS12-WI-TP-320",
                "Work Immersion for Tech-Pro Track (320 hours)",
                320,
                1,
                12m,
                12m,
                901);

            AddTechProWorkImmersion(
                definitions,
                "SHS12-WI-TP-540-1",
                "Work Immersion for Tech-Pro Track (540 hours) - 1 Term",
                540,
                1,
                21m,
                21m,
                902);

            AddTechProWorkImmersion(
                definitions,
                "SHS12-WI-TP-640-1",
                "Work Immersion for Tech-Pro Track (640 hours) - 1 Term",
                640,
                1,
                24m,
                24m,
                903);

            AddTechProWorkImmersion(
                definitions,
                "SHS12-WI-TP-540-2",
                "Work Immersion for Tech-Pro Track (540 hours) - 2 Terms",
                540,
                2,
                10.5m,
                21m,
                904);

            AddTechProWorkImmersion(
                definitions,
                "SHS12-WI-TP-640-2",
                "Work Immersion for Tech-Pro Track (640 hours) - 2 Terms",
                640,
                2,
                12m,
                24m,
                905);
        }

        private static void AddTechProWorkImmersion(
            ICollection<SeniorHighSubjectDefinition>
                definitions,
            string subjectCode,
            string subjectName,
            int totalHours,
            int termsTaught,
            decimal unitsPerTerm,
            decimal unitsPerYear,
            int displayOrder)
        {
            definitions.Add(
                new SeniorHighSubjectDefinition
                {
                    GradeLevel =
                        "Grade 12",

                    SubjectCode =
                        subjectCode,

                    SubjectName =
                        subjectName,

                    SubjectCategory =
                        "SSHS - TECH-PRO",

                    SubjectCluster =
                        "Work Immersion",

                    TrackStrand =
                        "All Technical-Professional",

                    DisplayOrder =
                        displayOrder,

                    IsCoreSubject =
                        false,

                    TotalHours =
                        totalHours,

                    TermsTaught =
                        termsTaught,

                    UnitsPerTerm =
                        unitsPerTerm,

                    UnitsPerYear =
                        unitsPerYear,

                    WrittenOralWorksWeight =
                        20,

                    PerformanceTasksWeight =
                        80,

                    SummativeTermExamWeight =
                        0,

                    SummativeTestOneShare =
                        0,

                    SummativeTestTwoShare =
                        0,

                    TermExamShare =
                        0
                });
        }
    }
}