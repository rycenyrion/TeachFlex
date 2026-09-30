using System;
using System.Text.RegularExpressions;

namespace TeachFlex.Services
{
    public sealed class StandardLessonSuggestion
    {
        public string Title { get; set; } = "";
        public string Competency { get; set; } = "";
        public string Objective { get; set; } = "";
    }

    /// <summary>Editable teaching suggestions from a standard; not official BOW competency text.</summary>
    public static class StandardLessonSuggestionService
    {
        public static StandardLessonSuggestion Create(string standard, string? subject, int session)
        {
            string text = Regex.Replace(standard ?? "", @"\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Enter a standard first.");
            int day = Math.Clamp(session, 1, 4) - 1;
            bool Has(string term) => text.Contains(term, StringComparison.OrdinalIgnoreCase);
            if ((Has("pagbasa") || Has("pagbigkas")) && Has("pangungusap"))
            {
                string[] titles = { "Pronouncing Words in Sentences", "Reading Short Sentences Accurately", "Reading with Expression", "Understanding a Read Sentence" };
                string[] skills =
                {
                    "Pronounce words in short sentences clearly.",
                    "Read short sentences accurately and fluently.",
                    "Read short sentences with appropriate expression.",
                    "Explain the main idea of a sentence in one's own words."
                };
                return new StandardLessonSuggestion
                {
                    Title = titles[day],
                    Competency = skills[day],
                    Objective = $"By the end of Session {session}, learners will practice {titles[day]}, demonstrate this skill: {skills[day]} and explain their answers."
                };
            }
            if (Has("order numbers") || Has("arrange numbers") || Has("ayusin ang mga bilang"))
            {
                string bound = Has("1000") ? "1000" : "100";
                string[] focus = { "Comparing Numbers", "Arranging Numbers in Ascending Order", "Arranging Numbers in Descending Order", "Applying Number Order to a Situation" };
                string[] skills =
                {
                    "Identify the smaller and larger numbers in a set.",
                    "Arrange numbers from least to greatest.",
                    "Arrange numbers from greatest to least.",
                    "Apply ascending or descending order to a simple situation."
                };
                return new StandardLessonSuggestion
                {
                    Title = focus[day] + " up to " + bound,
                    Competency = skills[day].TrimEnd('.') + " using numbers up to " + bound + ".",
                    Objective = $"By the end of Session {session}, learners will complete two examples of {focus[day]} " +
                        "and explain why their answers are correct."
                };
            }
            if (Has("circle") || Has("bilog") || Has("triangle") || Has("tatsulok"))
            {
                return new StandardLessonSuggestion
                {
                    Title = "Identifying and Describing Shapes · Session " + session,
                    Competency = "Identify and describe shapes by their visible features.",
                    Objective = $"By the end of Session {session}, learners will identify shapes in pictures or objects, " +
                        "describe their features, and give examples from their surroundings."
                };
            }
            if (Has("kambal-katinig") || Has("consonant blend"))
            {
                return new StandardLessonSuggestion
                {
                    Title = "Recognizing and Reading Consonant Blends · Session " + session,
                    Competency = "Recognize and read familiar words with consonant blends.",
                    Objective = $"By the end of Session {session}, learners will identify consonant blends in words, " +
                        "read sample words, and use one word in a sentence."
                };
            }
            // Broad standards cannot be reduced reliably to one official competency.
            // Provide an explicitly editable draft tied to a short excerpt of the teacher's standard.
            string area = string.IsNullOrWhiteSpace(subject) ? "the selected learning area" : subject.Trim();
            string excerpt = text.Length <= 110 ? text.TrimEnd('.') : text[..110].TrimEnd(' ', ',', ';', ':') + "…";
            return new StandardLessonSuggestion
            {
                Title = "Developing Skills in " + area + " · Session " + session,
                Competency = "Identify, explain, and demonstrate in a simple task a skill related to: " + excerpt,
                Objective = $"By the end of Session {session}, learners will identify one key idea from the selected standard, " +
                    "explain it in their own words, and demonstrate an appropriate example or task."
            };
        }
    }
}
