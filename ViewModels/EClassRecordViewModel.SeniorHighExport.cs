using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private static bool IsSeniorHighGradeLevel(
            string gradeLevel)
        {
            return gradeLevel == "Grade 11" ||
                   gradeLevel == "Grade 12";
        }

        private static SeniorHighEcrExportRequest
            CreateSeniorHighEcrExportRequest(
                ECRExportRequest source,
                Subject subject,
                School school)
        {
            int startingTermNumber =
                source.Terms
                    .Where(
                        term =>
                            term.AssessmentColumns.Any(
                                column =>
                                    column.HighestPossibleScore >
                                        0) ||
                            term.Learners.Any(
                                learner =>
                                    learner.Scores.Values.Any(
                                        score =>
                                            score.HasValue)))
                    .Select(
                        term =>
                            term.TermNumber)
                    .DefaultIfEmpty(
                        1)
                    .Min();

            return new SeniorHighEcrExportRequest
            {
                SchoolId =
                    source.SchoolId,

                SchoolName =
                    source.SchoolName,

                Region =
                    source.Region,

                Division =
                    source.Division,

                SchoolYear =
                    source.SchoolYear,

                GradeLevel =
                    source.GradeLevel,

                SectionName =
                    source.SectionName,

                SubjectName =
                    source.SubjectName,

                AdviserName =
                    source.AdviserName,

                SchoolHeadName =
                    source.SchoolHeadName,

                GradingSystem =
                    source.GradingSystem,

                AssessmentType =
                    source.AssessmentType,

                UsesComponentRecords =
                    source.UsesComponentRecords,

                ComponentOneName =
                    source.ComponentOneName,

                ComponentTwoName =
                    source.ComponentTwoName,

                Terms =
                    source.Terms,

                FinalGrades =
                    source.FinalGrades,

                SubjectCategory =
                    subject.SubjectCategory,

                SubjectCluster =
                    subject.SubjectCluster,

                TrackStrand =
                    subject.TrackStrand,

                SchoolLogoPath =
                    school.SchoolLogoPath,

                TotalHours =
                    subject.TotalHours,

                TermsTaught =
                    subject.TermsTaught,

                StartingTermNumber =
                    startingTermNumber,

                UnitsPerTerm =
                    subject.UnitsPerTerm,

                UnitsPerYear =
                    subject.UnitsPerYear,

                WrittenOralWorksWeight =
                    subject.WrittenOralWorksWeight,

                PerformanceTasksWeight =
                    subject.PerformanceTasksWeight,

                SummativeTermExamWeight =
                    subject.SummativeTermExamWeight,

                SummativeTestOneShare =
                    subject.SummativeTestOneShare,

                SummativeTestTwoShare =
                    subject.SummativeTestTwoShare,

                TermExamShare =
                    subject.TermExamShare
            };
        }
    }
}