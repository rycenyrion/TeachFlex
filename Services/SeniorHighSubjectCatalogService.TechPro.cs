using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void
            AddTechnicalProfessionalSubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions)
        {
            AddAestheticCareSubjects(
                definitions);

            AddAgriFisherySubjects(
                definitions);

            AddArtisanrySubjects(
                definitions);

            AddAutomotiveSubjects(
                definitions);

            AddConstructionSubjects(
                definitions);

            AddCreativeTechnologySubjects(
                definitions);

            AddHospitalitySubjects(
                definitions);

            AddIctSubjects(
                definitions);

            AddIndustrialSubjects(
                definitions);

            AddMaritimeSubjects(
                definitions);

            AddTechProWorkImmersionSubjects(
                definitions);
        }

        private static void
            AddTechProSubjectForBothGrades(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions,
                string codeSuffix,
                string subjectName,
                string cluster,
                int displayOrder)
        {
            AddTechProSubject(
                definitions,
                "Grade 11",
                $"SHS11-{codeSuffix}",
                subjectName,
                cluster,
                displayOrder);

            AddTechProSubject(
                definitions,
                "Grade 12",
                $"SHS12-{codeSuffix}",
                subjectName,
                cluster,
                displayOrder);
        }

        private static void AddTechProSubject(
            ICollection<SeniorHighSubjectDefinition>
                definitions,
            string gradeLevel,
            string subjectCode,
            string subjectName,
            string cluster,
            int displayOrder,
            int totalHours = 0,
            int? termsTaught = null,
            decimal? unitsPerTerm = null,
            decimal? unitsPerYear = null,
            int writtenWeight = 15,
            int performanceWeight = 65,
            int summativeExamWeight = 20,
            int testOneShare = 30,
            int testTwoShare = 30,
            int termExamShare = 40)
        {
            bool isGradeEleven =
                gradeLevel == "Grade 11";

            definitions.Add(
                new SeniorHighSubjectDefinition
                {
                    GradeLevel =
                        gradeLevel,

                    SubjectCode =
                        subjectCode,

                    SubjectName =
                        subjectName,

                    SubjectCategory =
                        "SSHS - TECH-PRO",

                    SubjectCluster =
                        cluster,

                    TrackStrand =
                        cluster,

                    DisplayOrder =
                        displayOrder,

                    IsCoreSubject =
                        false,

                    TotalHours =
                        totalHours,

                    TermsTaught =
                        termsTaught
                        ?? (isGradeEleven ? 3 : 1),

                    UnitsPerTerm =
                        unitsPerTerm
                        ?? (isGradeEleven ? 4m : 12m),

                    UnitsPerYear =
                        unitsPerYear
                        ?? 12m,

                    WrittenOralWorksWeight =
                        writtenWeight,

                    PerformanceTasksWeight =
                        performanceWeight,

                    SummativeTermExamWeight =
                        summativeExamWeight,

                    SummativeTestOneShare =
                        testOneShare,

                    SummativeTestTwoShare =
                        testTwoShare,

                    TermExamShare =
                        termExamShare
                });
        }

        static partial void AddAestheticCareSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddAgriFisherySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddArtisanrySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddAutomotiveSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddConstructionSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void
            AddCreativeTechnologySubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions);

        static partial void AddHospitalitySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddIctSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddIndustrialSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddMaritimeSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void
            AddTechProWorkImmersionSubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions);
    }
}