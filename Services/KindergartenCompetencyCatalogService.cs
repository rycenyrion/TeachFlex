using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface
        IKindergartenCompetencyCatalogService
    {
        IReadOnlyList<
            KindergartenCompetency>
                CreateOfficialCompetencies();
    }

    public partial class
        KindergartenCompetencyCatalogService :
            IKindergartenCompetencyCatalogService
    {
        public IReadOnlyList<
            KindergartenCompetency>
                CreateOfficialCompetencies()
        {
            List<KindergartenCompetency>
                competencies =
                    new List<
                        KindergartenCompetency>();

            int displayOrder =
                1;

            AddMotorAndSocialCompetencies(
                competencies,
                ref displayOrder);

            AddCognitiveCompetencies(
                competencies,
                ref displayOrder);

            AddLanguageCompetencies(
                competencies,
                ref displayOrder);

            return competencies;
        }

        static partial void
            AddMotorAndSocialCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder);

        static partial void
            AddCognitiveCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder);

        static partial void
            AddLanguageCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder);

        private static void AddCompetency(
            ICollection<KindergartenCompetency>
                competencies,
            string competencyCode,
            string developmentArea,
            string subDomain,
            string description,
            ref int displayOrder)
        {
            competencies.Add(
                new KindergartenCompetency
                {
                    CompetencyCode =
                        competencyCode,

                    DevelopmentArea =
                        developmentArea,

                    SubDomain =
                        subDomain,

                    Description =
                        description,

                    DisplayOrder =
                        displayOrder++,

                    IsActive =
                        true
                });
        }
    }
}