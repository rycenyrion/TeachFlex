using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddAestheticCareSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Aesthetic, Wellness, and Human Care";

            string[] subjects =
            {
                "Aesthetic Services (Beauty Care)",
                "Caregiving (Adult Care)",
                "Caregiving (Child Care)",
                "Hairdressing Services"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "AWHC",
                601);
        }

        static partial void AddAgriFisherySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Agri-Fishery Business and Food Innovation";

            string[] subjects =
            {
                "Agricultural Crops Production",
                "Agro-entrepreneurship",
                "Aquaculture",
                "Fish Capture Operation",
                "Food Processing",
                "Organic Agriculture Production",
                "Poultry Production (Chicken)",
                "Ruminants Production",
                "Swine Production"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "AFBFI",
                621);
        }

        private static void
            AddTechProSubjectsForBothGrades(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions,
                IReadOnlyList<string> subjects,
                string cluster,
                string codePrefix,
                int startingDisplayOrder)
        {
            for (int index = 0;
                 index < subjects.Count;
                 index++)
            {
                string sequence =
                    (index + 1)
                        .ToString("00");

                AddTechProSubjectForBothGrades(
                    definitions,
                    $"{codePrefix}-{sequence}",
                    subjects[index],
                    cluster,
                    startingDisplayOrder + index);
            }
        }
    }
}