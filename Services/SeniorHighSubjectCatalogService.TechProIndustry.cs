using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddArtisanrySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Artisanry and Creative Enterprise";

            string[] subjects =
            {
                "Garments Artisanry",
                "Handicrafts: Weaving"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "ACE",
                701);
        }

        static partial void AddAutomotiveSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Automotive and Small Engine Technologies";

            string[] bothGradeSubjects =
            {
                "Driving and Automotive Servicing",
                "Motorcycle and Small Engine Servicing"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                bothGradeSubjects,
                cluster,
                "AUTO",
                721);

            AddTechProSubject(
                definitions,
                "Grade 12",
                "SHS12-AUTO-03",
                "Automotive Servicing (Electrical Repair)",
                cluster,
                723);

            AddTechProSubject(
                definitions,
                "Grade 12",
                "SHS12-AUTO-04",
                "Automotive Servicing (Engine and Chassis Repairs)",
                cluster,
                724);
        }

        static partial void AddConstructionSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Construction and Building Technologies";

            string[] subjects =
            {
                "Carpentry",
                "Construction Operation",
                "Manual Metal Arc Welding",
                "Technical Drafting"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "CBT",
                741);
        }

        static partial void
            AddCreativeTechnologySubjects(
                ICollection<
                    SeniorHighSubjectDefinition>
                        definitions)
        {
            const string cluster =
                "Creative Arts and Design Technologies";

            string[] subjects =
            {
                "Animation",
                "Illustration",
                "Visual Graphic Design"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "CADT",
                761);
        }
    }
}