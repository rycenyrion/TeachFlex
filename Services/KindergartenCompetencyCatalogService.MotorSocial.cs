using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        KindergartenCompetencyCatalogService
    {
        static partial void
            AddMotorAndSocialCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder)
        {
            const string motorArea =
                "Sensory Perceptual and Motor Development";

            AddCompetency(
                competencies,
                "SPM-01",
                motorArea,
                string.Empty,
                "Identifies external body parts and their functions.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SPM-02",
                motorArea,
                string.Empty,
                "Identifies ways to care for and protect one’s body.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SPM-03",
                motorArea,
                string.Empty,
                "Demonstrates gross motor skills " +
                "(locomotor and non-locomotor).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SPM-04",
                motorArea,
                string.Empty,
                "Moves body parts as directed.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SPM-05",
                motorArea,
                string.Empty,
                "Demonstrates fine motor skills " +
                "(tearing, cutting, rolling, and " +
                "molding with playdough).",
                ref displayOrder);

            const string socialArea =
                "Socio-emotional Development";

            AddCompetency(
                competencies,
                "SED-01",
                socialArea,
                string.Empty,
                "Identifies and expresses feelings " +
                "in appropriate ways.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-02",
                socialArea,
                string.Empty,
                "Recognizes and respects the feelings of others.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-03",
                socialArea,
                string.Empty,
                "Expresses needs and preferences.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-04",
                socialArea,
                string.Empty,
                "Behaves appropriately in different situations.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-05",
                socialArea,
                string.Empty,
                "Participates in classroom routines and activities.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-06",
                socialArea,
                string.Empty,
                "Follows classroom and school rules.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "SED-07",
                socialArea,
                string.Empty,
                "Fulfills classroom responsibilities.",
                ref displayOrder);
        }
    }
}