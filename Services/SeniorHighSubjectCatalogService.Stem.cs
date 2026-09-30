using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddStemSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Science, Technology, Engineering, and Mathematics";

            string[] bothGradeSubjects =
            {
                "Advanced Mathematics",
                "Basic Calculus",
                "Biology 1",
                "Biology 2",
                "Biology 3",
                "Chemistry 1",
                "Chemistry 2",
                "Chemistry 3",
                "Earth and Space Science 1",
                "Earth and Space Science 2",
                "Earth and Space Science 3",
                "Empowerment Technologies",
                "Finite Mathematics 1",
                "Finite Mathematics 2",
                "Physics 1",
                "Physics 2",
                "Physics 3",
                "Pre-Calculus"
            };

            int displayOrder =
                201;

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
                    $"SHS11-STEM-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    displayOrder);

                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-STEM-{sequence}",
                    bothGradeSubjects[index],
                    cluster,
                    displayOrder);

                displayOrder++;
            }

            string[] gradeTwelveOnlySubjects =
            {
                "Biology 4",
                "Chemistry 4",
                "Conceptual Biology and Earth and Space Science",
                "Conceptual Physics and Chemistry in Daily Life",
                "Database Management",
                "Earth and Space Science 4",
                "Fundamentals of Data Analytics",
                "Physics 4"
            };

            for (int index = 0;
                 index < gradeTwelveOnlySubjects.Length;
                 index++)
            {
                string sequence =
                    (index + 19)
                        .ToString("00");

                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-STEM-{sequence}",
                    gradeTwelveOnlySubjects[index],
                    cluster,
                    displayOrder);

                displayOrder++;
            }
        }
    }
}