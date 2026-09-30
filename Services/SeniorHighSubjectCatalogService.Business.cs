using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddBusinessSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Business and Entrepreneurship";

            AddBusinessSubjectForBothGrades(
                definitions,
                "BUS-01",
                "Business 1 (Basic Accounting)",
                cluster,
                101);

            AddBusinessSubjectForBothGrades(
                definitions,
                "BUS-02",
                "Business 2 (Business Finance and Income Taxation)",
                cluster,
                102);

            AddBusinessSubjectForBothGrades(
                definitions,
                "BUS-03",
                "Contemporary Marketing",
                cluster,
                103);

            AddBusinessSubjectForBothGrades(
                definitions,
                "BUS-04",
                "Entrepreneurship",
                cluster,
                104);

            AddBusinessSubjectForBothGrades(
                definitions,
                "BUS-05",
                "Introduction to Organization and Management",
                cluster,
                105);

            AddAcademicSubject(
                definitions,
                "Grade 12",
                "SHS12-BUS-06",
                "Business 3 (Business Economics)",
                cluster,
                106,
                writtenWeight: 20,
                performanceWeight: 50,
                summativeExamWeight: 30);
        }

        private static void
            AddBusinessSubjectForBothGrades(
                ICollection<SeniorHighSubjectDefinition>
                    definitions,
                string codeSuffix,
                string subjectName,
                string cluster,
                int displayOrder)
        {
            AddAcademicSubject(
                definitions,
                "Grade 11",
                $"SHS11-{codeSuffix}",
                subjectName,
                cluster,
                displayOrder,
                writtenWeight: 20,
                performanceWeight: 50,
                summativeExamWeight: 30);

            AddAcademicSubject(
                definitions,
                "Grade 12",
                $"SHS12-{codeSuffix}",
                subjectName,
                cluster,
                displayOrder,
                writtenWeight: 20,
                performanceWeight: 50,
                summativeExamWeight: 30);
        }
    }
}