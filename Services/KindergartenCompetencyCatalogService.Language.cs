using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class
        KindergartenCompetencyCatalogService
    {
        static partial void
            AddLanguageCompetencies(
                List<KindergartenCompetency>
                    competencies,
                ref int displayOrder)
        {
            const string languageArea =
                "Language, Literacy, and Communication Development";

            AddCompetency(
                competencies,
                "LLC-01",
                languageArea,
                "Listening and Viewing",
                "Identifies familiar environmental sounds.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-02",
                languageArea,
                "Listening and Viewing",
                "Recalls what happens first, middle, " +
                "and last in a story.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-03",
                languageArea,
                "Listening and Viewing",
                "Retells a story in sequence.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-04",
                languageArea,
                "Listening and Viewing",
                "Follows one-to-two-step instructions.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-05",
                languageArea,
                "Sight Word Recognition",
                "Recognizes non-decodable words in and " +
                "out of context automatically.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-06",
                languageArea,
                "Sight Word Recognition",
                "Recognizes sight words.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-07",
                languageArea,
                "Speaking",
                "Identifies first and last name.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-08",
                languageArea,
                "Speaking",
                "Identifies classmates, teachers, " +
                "and family members.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-09",
                languageArea,
                "Speaking",
                "Identifies familiar objects at home, " +
                "in school, and in the community.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-10",
                languageArea,
                "Speaking",
                "Uses polite greetings and courteous " +
                "expressions in varied situations.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-11",
                languageArea,
                "Speaking",
                "Relates personal experiences to story events.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-12",
                languageArea,
                "Speaking",
                "Expresses ideas and feelings using " +
                "phrases and simple sentences.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-13",
                languageArea,
                "Reading - Phonological/Phonemic Awareness",
                "Orally segments sounds by syllable, " +
                "onset and rime, and phoneme by phoneme.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-14",
                languageArea,
                "Reading - Letter Knowledge",
                "Identifies uppercase letters.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-15",
                languageArea,
                "Reading - Letter Knowledge",
                "Identifies lowercase letters.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-16",
                languageArea,
                "Reading - Letter Knowledge",
                "Matches uppercase and lowercase letters.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-17",
                languageArea,
                "Reading - Letter Sound Relationship",
                "Identifies letter sounds.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-18",
                languageArea,
                "Reading - Letter Sound Relationship",
                "Matches letters and their corresponding sounds.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-19",
                languageArea,
                "Comprehension",
                "Uses a variety of strategies to gain " +
                "meaning from leveled texts.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-20",
                languageArea,
                "Comprehension",
                "Uses print and illustrations to make meaning.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-21",
                languageArea,
                "Concepts of Print",
                "Demonstrates book-handling skills.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-22",
                languageArea,
                "Concepts of Print",
                "Distinguishes between letters, words, " +
                "and sentences.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-23",
                languageArea,
                "Concepts of Print",
                "Demonstrates awareness of print direction " +
                "from left to right and top to bottom.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-24",
                languageArea,
                "Writing",
                "Traces, draws, or copies shapes, " +
                "designs, and pictures.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-25",
                languageArea,
                "Writing",
                "Traces, copies, or writes names and words.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-26",
                languageArea,
                "Writing",
                "Writes uppercase and lowercase letters.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-27",
                languageArea,
                "Writing",
                "Spells sight words.",
                ref displayOrder);

            AddCompetency(
                competencies,
                "LLC-28",
                languageArea,
                "Writing",
                "Spells simple words phonetically.",
                ref displayOrder);
        }
    }
}