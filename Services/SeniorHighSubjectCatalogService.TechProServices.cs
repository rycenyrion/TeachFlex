using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        SeniorHighSubjectCatalogService
    {
        static partial void AddHospitalitySubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Hospitality and Tourism";

            string[] subjects =
            {
                "Bakery Operations",
                "Events Management Services",
                "Food and Beverage Operation",
                "Hotel Operations (Front Office Services)",
                "Hotel Operations (Housekeeping Services)",
                "Kitchen Operations",
                "Tourism Services"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "HT",
                801);
        }

        static partial void AddIctSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "ICT Support and Computer Programming Technologies";

            string[] subjects =
            {
                "Broadband Installation",
                "Computer Programming (.Net Technology)",
                "Computer Programming (Java)",
                "Computer Programming (Oracle Database)",
                "Computer Systems Servicing",
                "Contact Center Services"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "ICT",
                821);
        }

        static partial void AddIndustrialSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Industrial Technologies";

            string[] bothGradeSubjects =
            {
                "Domestic Refrigeration and Air-Conditioning Servicing",
                "Electrical Installation and Maintenance",
                "Electronic Products Assembly and Servicing",
                "Photovoltaic Systems Installation"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                bothGradeSubjects,
                cluster,
                "IND",
                841);

            AddTechProSubject(
                definitions,
                "Grade 12",
                "SHS12-IND-05",
                "Commercial Air-Conditioning Installation and Servicing",
                cluster,
                845);

            AddTechProSubject(
                definitions,
                "Grade 12",
                "SHS12-IND-06",
                "Mechatronics",
                cluster,
                846);
        }

        static partial void AddMaritimeSubjects(
            ICollection<SeniorHighSubjectDefinition>
                definitions)
        {
            const string cluster =
                "Maritime";

            string[] subjects =
            {
                "Marine Engineering at the Support Level",
                "Marine Transportation at the Support Level",
                "Ships Catering Services"
            };

            AddTechProSubjectsForBothGrades(
                definitions,
                subjects,
                cluster,
                "MAR",
                861);
        }
    }
}