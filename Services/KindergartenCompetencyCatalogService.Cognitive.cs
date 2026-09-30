using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        KindergartenCompetencyCatalogService
    {
        static partial void
            AddCognitiveCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder)
        {
            const string cognitiveArea =
                "Cognitive Development";

            AddCompetency(
                competencies,
                "COG-01",
                cognitiveArea,
                string.Empty,
                "Identifies attributes of objects " +
                "(color, shape, and size).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-02",
                cognitiveArea,
                string.Empty,
                "Matches objects based on attributes.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-03",
                cognitiveArea,
                string.Empty,
                "Describes objects based on attributes " +
                "(shape, color, taste, and texture).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-04",
                cognitiveArea,
                string.Empty,
                "Classifies objects by a single attribute " +
                "(color, shape, or size).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-05",
                cognitiveArea,
                string.Empty,
                "Reclassifies objects according to " +
                "multiple attributes.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-06",
                cognitiveArea,
                string.Empty,
                "Arranges objects according to " +
                "specific attributes.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-07",
                cognitiveArea,
                string.Empty,
                "Recognizes, extends, and creates patterns " +
                "using concrete objects.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-08",
                cognitiveArea,
                string.Empty,
                "Measures size, length, capacity, and mass " +
                "of objects using non-standard measuring tools.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-09",
                cognitiveArea,
                string.Empty,
                "Identifies the position of objects " +
                "(in, on, over, under, top, and bottom).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-10",
                cognitiveArea,
                string.Empty,
                "Compares quantities of objects " +
                "(more or less).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-11",
                cognitiveArea,
                string.Empty,
                "Counts with one-to-one correspondence.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-12",
                cognitiveArea,
                string.Empty,
                "Recognizes numerals.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-13",
                cognitiveArea,
                string.Empty,
                "Matches numerals to objects.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-14",
                cognitiveArea,
                string.Empty,
                "Adds and subtracts using concrete objects.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-15",
                cognitiveArea,
                string.Empty,
                "Recognizes a clock as a measure of time " +
                "(hours and minutes).",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-16",
                cognitiveArea,
                string.Empty,
                "Shows awareness and care for the natural " +
                "and physical environment.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-17",
                cognitiveArea,
                string.Empty,
                "Talks about participation in cultural " +
                "and religious activities.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-18",
                cognitiveArea,
                string.Empty,
                "Shows awareness of the importance of caring " +
                "for the natural and physical environment " +
                "through simple practices such as sorting " +
                "trash and helping to clean up.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-19",
                cognitiveArea,
                string.Empty,
                "Predicts outcomes in familiar stories " +
                "read aloud in class.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "COG-20",
                cognitiveArea,
                string.Empty,
                "Suggests solutions to problems in class " +
                "activities and stories read aloud in class.",
                ref displayOrder);
        }
    }
}