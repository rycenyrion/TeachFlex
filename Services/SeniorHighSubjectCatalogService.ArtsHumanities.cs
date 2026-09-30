using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void
            AddArtsHumanitiesSubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions)
        {
            const string cluster =
                "Arts, Social Sciences, and Humanities";

            string[] bothGradeSubjects =
            {
                "Art Criticism and Creative Markets",
                "Citizenship and Civic Engagement",
                "Contemporary Literature 1",
                "Contemporary Literature 2",
                "Creative Composition 1",
                "Creative Composition 2",
                "Creative Industries - Applied and Traditional Arts",
                "Creative Industries - Dance",
                "Creative Industries - Literary Arts",
                "Creative Industries - Media Arts",
                "Creative Industries - Music",
                "Creative Industries - Theater Arts",
                "Creative Industries - Visual Arts",
                "Filipino 1 (Wika at Komunikasyon sa Akademikong Filipino)",
                "Filipino 2 (Filipino sa Isports)",
                "Filipino 2 (Filipino sa Larang Teknikal-Propesyonal)",
                "Filipino 2 (Filipino sa Sining at Disenyo)",
                "Filipino Identity Through the Arts",
                "Introduction to the Philosophy of the Human Person",
                "Leadership and Management in the Arts",
                "Malikhaing Pagsulat",
                "Performance Criticism and Creative Markets",
                "Philippine Governance (Philippine Politics and Governance)",
                "Social Sciences (Theory and Practice)"
            };

            int displayOrder =
                301;

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
                    $"SHS11-ASH-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    displayOrder);

                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-ASH-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    displayOrder);

                displayOrder++;
            }

            AddAcademicSubject(
                definitions,
                "Grade 12",
                "SHS12-ASH-25",
                "Creative Production and Presentation",
                cluster,
                displayOrder,
                totalHours: 160,
                termsTaught: 1,
                unitsPerTerm: 6m,
                unitsPerYear: 6m,
                writtenWeight: 15,
                performanceWeight: 70,
                summativeExamWeight: 15);
        }
    }
}