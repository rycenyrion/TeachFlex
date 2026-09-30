using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddSportsHealthSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Sports, Health, and Wellness";

            string[] subjects =
            {
                "Exercise and Sports Programming",
                "First Aid",
                "Fundamentals of Basic Life Support",
                "Human Movement 1 (Basic Anatomy in Sports and Exercise)",
                "Human Movement 2 (Motor Skills Development)",
                "Physical Education 1 (Fitness and Recreation)",
                "Physical Education 2 (Sports and Dance)",
                "Sports Activity Management",
                "Sports Coaching",
                "Sports Officiating"
            };

            for (int index = 0;
                 index < subjects.Length;
                 index++)
            {
                string sequence =
                    (index + 1)
                        .ToString("00");

                int displayOrder =
                    501 + index;

                AddAcademicSubject(
                    definitions,
                    "Grade 11",
                    $"SHS11-SHW-{sequence}",
                    subjects[index],
                    cluster,
                    displayOrder);

                AddAcademicSubject(
                    definitions,
                    "Grade 12",
                    $"SHS12-SHW-{sequence}",
                    subjects[index],
                    cluster,
                    displayOrder);
            }
        }
    }
}