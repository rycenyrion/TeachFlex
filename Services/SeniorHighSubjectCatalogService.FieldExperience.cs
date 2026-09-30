using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void
            AddFieldExperienceSubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions)
        {
            const string cluster =
                "Field Experience";

            string[] bothGradeSubjects =
            {
                "Design and Innovation",
                "Research 1",
                "Research 2"
            };

            for (int index = 0;
                 index < bothGradeSubjects.Length;
                 index++)
            {
                string sequence =
                    (index + 1)
                        .ToString("00");

                AddAcademicSubject(
                    definitions,
                    "Grade 11",
                    $"SHS11-FE-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    401 + index,
                    writtenWeight: 40,
                    performanceWeight: 60,
                    summativeExamWeight: 0,
                    testOneShare: 0,
                    testTwoShare: 0,
                    termExamShare: 0,
                    trackStrand: "All Academic");

                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-FE-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    401 + index,
                    writtenWeight: 40,
                    performanceWeight: 60,
                    summativeExamWeight: 0,
                    testOneShare: 0,
                    testTwoShare: 0,
                    termExamShare: 0,
                    trackStrand: "All Academic");
            }

            string[] artsApprenticeships =
            {
                "Arts Apprenticeship - Dance",
                "Arts Apprenticeship - Literary Arts",
                "Arts Apprenticeship - Media Arts",
                "Arts Apprenticeship - Music",
                "Arts Apprenticeship - Theater Arts",
                "Arts Apprenticeship - Traditional Cultural Expressions",
                "Arts Apprenticeship - Visual Arts"
            };

            for (int index = 0;
                 index < artsApprenticeships.Length;
                 index++)
            {
                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-FE-ART-{index + 1:00}",
                    artsApprenticeships[index],
                    cluster,
                    410 + index,
                    totalHours: 160,
                    unitsPerTerm: 6m,
                    unitsPerYear: 6m,
                    writtenWeight: 15,
                    performanceWeight: 70,
                    summativeExamWeight: 15,
                    testOneShare: 0,
                    testTwoShare: 0,
                    termExamShare: 100,
                    trackStrand:
                        "Arts, Social Sciences, and Humanities");
            }

            AddAcademicSubject(
                definitions,
                "Grade 12",
                "SHS12-FE-OFF-CAMPUS",
                "Field Exposure (Off Campus)",
                cluster,
                420,
                writtenWeight: 15,
                performanceWeight: 70,
                summativeExamWeight: 15,
                testOneShare: 0,
                testTwoShare: 0,
                termExamShare: 100,
                trackStrand: "All Academic");

            AddAcademicSubject(
                definitions,
                "Grade 12",
                "SHS12-FE-SPORTS",
                "In-Campus Field Exposure for Sports",
                cluster,
                421,
                totalHours: 160,
                writtenWeight: 15,
                performanceWeight: 70,
                summativeExamWeight: 15,
                testOneShare: 0,
                testTwoShare: 0,
                termExamShare: 100,
                trackStrand:
                    "Sports, Health, and Wellness");

            AddAcademicSubject(
                definitions,
                "Grade 12",
                "SHS12-WI-ACADEMIC",
                "Work Immersion for Academic Track",
                "Work Immersion",
                422,
                totalHours: 320,
                unitsPerTerm: 12m,
                unitsPerYear: 12m,
                writtenWeight: 20,
                performanceWeight: 80,
                summativeExamWeight: 0,
                testOneShare: 0,
                testTwoShare: 0,
                termExamShare: 0,
                trackStrand: "All Academic");
        }
    }
}