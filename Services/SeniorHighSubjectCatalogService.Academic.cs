using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddAcademicSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            AddArtsHumanitiesSubjects(
                definitions);

            AddBusinessSubjects(
                definitions);

            AddFieldExperienceSubjects(
                definitions);

            AddStemSubjects(
                definitions);

            AddSportsHealthSubjects(
                definitions);
        }

        private static void AddAcademicSubject(
            ICollection<SeniorHighSubjectDefinition>
                definitions,
            string gradeLevel,
            string subjectCode,
            string subjectName,
            string subjectCluster,
            int displayOrder,
            int totalHours = 80,
            int termsTaught = 1,
            decimal unitsPerTerm = 3m,
            decimal unitsPerYear = 3m,
            int writtenWeight = 20,
            int performanceWeight = 60,
            int summativeExamWeight = 20,
            int testOneShare = 30,
            int testTwoShare = 30,
            int termExamShare = 40,
           string trackStrand =
               "")
        {
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
                        "SSHS - ACADEMIC",

                    SubjectCluster =
                        subjectCluster,

                    TrackStrand =
                     string.IsNullOrWhiteSpace(
                        trackStrand)
                        ? subjectCluster
                        : trackStrand,

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

        static partial void AddArtsHumanitiesSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddBusinessSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddFieldExperienceSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddStemSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);

        static partial void AddSportsHealthSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions);
    }
}