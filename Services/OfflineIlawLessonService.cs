using System;

namespace TeachFlex.Services
{
    public sealed class OfflineDailyLesson
    {
        public string Title { get; set; } = "";
        public string Objectives { get; set; } = "";
        public string Resources { get; set; } = "";
        public string PreLesson { get; set; } = "";
        public string Activities { get; set; } = "";
        public string Assessment { get; set; } = "";
    }

    /// <summary>Editable offline session examples grounded in selected BOW competency.</summary>
    public sealed class OfflineIlawLessonService
    {
        public OfflineDailyLesson Generate(string subject, int day, string competency,
            string learnerNeeds, bool simplify)
        {
            if (day is < 1 or > 4 || string.IsNullOrWhiteSpace(competency))
                throw new ArgumentException("Select a learning competency and teaching session 1–4.");
            string skill = competency.Trim();
            bool cluster = skill.Contains("kambal-katinig", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("consonant blend", StringComparison.OrdinalIgnoreCase);
            bool ordering = skill.Contains("order numbers", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("arrange numbers", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("ayusin ang mga bilang", StringComparison.OrdinalIgnoreCase);
            bool sound = skill.Contains("tunog na bumubuo sa salita", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("patinig", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("diptonggo", StringComparison.OrdinalIgnoreCase) ||
                skill.Contains("klaster", StringComparison.OrdinalIgnoreCase);
            string[][] clusterWords =
            {
                new[] { "plato (pl)", "braso (br)", "gripo (gr)", "prutas (pr)" },
                new[] { "blusa (bl)", "tren (tr)", "klase (kl)", "pluma (pl)" },
                new[] { "globo (gl)", "krayola (kr)", "prinsesa (pr)", "drayber (dr)" },
                new[] { "plato (pl)", "blusa (bl)", "gripo (gr)", "tren (tr)" }
            };
            int start = 4 + day * 13;
            string[] examples = cluster ? clusterWords[day - 1] : ordering
                ? new[]
                {
                    $"{start + 7}, {start}, {start + 3} → ascending: {start}, {start + 3}, {start + 7}",
                    $"{start + 2}, {start + 9}, {start + 5} → descending: {start + 9}, {start + 5}, {start + 2}",
                    $"{start + 1}, {start + 6}, {start + 4} → ascending: {start + 1}, {start + 4}, {start + 6}",
                    $"{start + 8}, {start + 3}, {start + 5} → descending: {start + 8}, {start + 5}, {start + 3}"
                }
                : sound ? new[]
                {
                    "bata — What is the initial sound? (b)",
                    "ibon — What is the initial vowel? (i)",
                    "bahay — What is the final sound? (y)",
                    "plato — Which consonants form a blend? (pl)"
                } : new[]
                {
                    "Picture: ask learners to point to a part related to the selected skill and explain why.",
                    "Concrete object: show one example and one nonexample; ask learners to compare them.",
                    "Home situation: ask learners to apply the skill to a familiar short scenario.",
                    "School situation: ask learners to show an answer and explain it to a partner."
                };
            string[] motivation =
            {
                $"Show a familiar picture or object and ask learners what they notice. Example: {examples[0]}",
                $"Review the previous session by comparing a correct answer with a nearly correct one. Example: {examples[0]}",
                $"Present a new situation from home or school. Example: {examples[0]}",
                $"Quickly review two learned examples and identify a skill to practice. Example: {examples[0]}"
            };
            string need = string.IsNullOrWhiteSpace(learnerNeeds) ? "" : $"\nLearner support: {learnerNeeds.Trim()}";
            string activity;
            string assessment;
            if (sound || cluster)
            {
                string[] sets =
                {
                    "bata, ibon, bahay, plato, araw", "guro, ulan, tulay, braso, aso",
                    "pusa, eroplano, sabay, gripo, isda", "mesa, itlog, kulay, tren, orasan"
                };
                string[] words = sets[day - 1].Split(", ");
                string focus = cluster ? "circle the consonant blend; if there is none, write 'none'"
                    : "write the initial sound and circle the vowel";
                activity = $"1. Model: read 'bata'; identify the initial sound /b/ and vowel a.\n" +
                    $"2. Activity A — Directions: Read the five words. For each word, {focus}. (5 items)\n" +
                    string.Join("\n", Array.ConvertAll(words, (word) => $"   • {word}")) + "\n" +
                    "3. Activity B (group) — Directions: Sort four word cards: bahay, plato, ibon, braso. " +
                    "Use two columns: with a consonant blend / without a consonant blend. " +
                    "Each group member explains one card. (4 items)\n" +
                    "4. Activity C (individual) — Directions: Write the initial sound of mesa, puno, araw, saging, ulan. (5 items)\n" +
                    "5. Activity D — Directions: Circle the word with a diphthong in each pair: " +
                    "1) bahay/bata, 2) tulay/tasa, 3) sabay/saging, 4) buhay/buko. (4 items)\n" +
                    "6. Synthesis: ask learners to read two answers and explain the sounds they noticed." + need;
                assessment = "Formative check — Directions: Write the initial sound of 1) bola, 2) isda, 3) tasa. " +
                    "Circle the vowels in 4) aso and 5) ulan. (5 items; answers: b, i, t, a/o, u/a). " +
                    "Record learners who need more support.";
            }
            else if (ordering)
            {
                activity = $"1. Model: show 12, 18, 15; arrange them in ascending order: 12, 15, 18.\n" +
                    $"2. Activity A — Directions: Arrange the numbers from least to greatest. (4 items)\n" +
                    $"   1) {start + 7}, {start}, {start + 3}\n   2) {start + 2}, {start + 9}, {start + 5}\n" +
                    $"   3) {start + 1}, {start + 6}, {start + 4}\n   4) {start + 8}, {start + 3}, {start + 5}\n" +
                    "3. Activity B (group) — Directions: Arrange four number cards: 23, 31, 27, 19. " +
                    "Write the ascending and descending order; explain the first number in each list. (2 items)\n" +
                    "4. Activity C (individual) — Directions: Arrange 42, 35, 51 in descending order and 68, 61, 75 in ascending order. (2 items)" + need;
                assessment = "Formative check — Directions: Arrange 14, 9, 20 in ascending order and 37, 45, 32 in descending order. " +
                    "Circle the least number among 26, 62, 16. (3 items; answers: 9-14-20, 45-37-32, 16).";
            }
            else
            {
                activity = $"Selected skill: {skill}\n" +
                    "Choose examples that match the exact BOW competency. " +
                    "The offline generator cannot reliably produce specific items for this skill; " +
                    "add directions, item counts, and actual questions before using this draft in class.";
                assessment = "Add a specific formative check with actual items and expected answers.";
            }
            return new OfflineDailyLesson
            {
                Title = skill,
                Objectives = $"By the end of the session, learners will demonstrate progress in this skill: {skill}",
                Resources = "Selected BOW, familiar pictures or objects, task cards, board, and paper. Adapt to available classroom materials.",
                PreLesson = motivation[day - 1],
                Activities = activity + (simplify ? "\nSupport: guide learners through the first two items before they complete the rest independently." : ""),
                Assessment = assessment
            };
        }
    }
}
