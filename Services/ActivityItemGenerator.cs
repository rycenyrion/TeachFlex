using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TeachFlex.Services
{
    public sealed class ActivityDraft
    {
        public string Instructions { get; set; } = "";
        public string Questions { get; set; } = "";
        public string AnswerKey { get; set; } = "";
    }

    /// <summary>Verified number-ordering questions across four cognitive levels.</summary>
    public sealed class ActivityItemGenerator
    {
        private static readonly string[] Levels = { "Remembering", "Understanding", "Applying", "Analyzing" };
        private static readonly char[] Letters = { 'A', 'B', 'C', 'D' };

        public bool CanGenerate(string competency) =>
            competency.Contains("order numbers", StringComparison.OrdinalIgnoreCase) ||
            competency.Contains("arrange numbers", StringComparison.OrdinalIgnoreCase) ||
            competency.Contains("ayusin ang mga bilang", StringComparison.OrdinalIgnoreCase);

        public ActivityDraft Generate(string competency, int count)
        {
            if (!CanGenerate(competency))
                throw new NotSupportedException("Wala pang verified item generator para sa competency na ito.");
            if (count is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(count));
            int maximum = Regex.IsMatch(competency, @"(?:up to|hanggang)\s*1000", RegexOptions.IgnoreCase) ? 1000 : 100;
            var usedSets = new HashSet<string>();
            var questions = new StringBuilder();
            var answers = new StringBuilder();
            // For 20 items there are five questions per cognitive level; shuffle their order.
            var levels = Enumerable.Range(0, count).Select(i => i % 4)
                .OrderBy(_ => Random.Shared.Next()).ToArray();
            for (int item = 1; item <= count; item++)
            {
                int[] values;
                string signature;
                do
                {
                    values = Enumerable.Range(1, maximum).OrderBy(_ => Random.Shared.Next()).Take(4).ToArray();
                    signature = string.Join(",", values.OrderBy(x => x));
                } while (!usedSets.Add(signature));
                int level = levels[item - 1];
                bool ascending = Random.Shared.Next(2) == 0;
                string direction = ascending ? "pinakamaliit hanggang pinakamalaki" : "pinakamalaki hanggang pinakamaliit";
                int[] ordered = ascending ? values.OrderBy(x => x).ToArray() : values.OrderByDescending(x => x).ToArray();
                string prompt;
                string correct;
                List<string> distractors;
                if (level == 0)
                {
                    bool askSmallest = Random.Shared.Next(2) == 0;
                    prompt = $"Alin ang {(askSmallest ? "pinakamaliit" : "pinakamalaki")} sa {string.Join(", ", values)}?";
                    correct = (askSmallest ? values.Min() : values.Max()).ToString();
                    distractors = values.Select(x => x.ToString()).Where(x => x != correct).ToList();
                }
                else if (level == 3)
                {
                    int swap = Random.Shared.Next(3);
                    var wrong = ordered.ToArray();
                    (wrong[swap], wrong[swap + 1]) = (wrong[swap + 1], wrong[swap]);
                    prompt = $"Dapat nakaayos mula {direction}: {string.Join(" – ", wrong)}. Aling dalawang bilang ang kailangang pagpalitin upang maitama ito?";
                    correct = Pair(ordered[swap], ordered[swap + 1]);
                    distractors = new List<string>();
                    for (int a = 0; a < 4; a++)
                        for (int b = a + 1; b < 4; b++)
                        {
                            string pair = Pair(ordered[a], ordered[b]);
                            if (pair != correct) distractors.Add(pair);
                        }
                    distractors = distractors.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
                }
                else if (level == 2)
                {
                    string[] groups = { "A", "B", "C", "D" };
                    prompt = $"Nakaipon ng tansan ang apat na pangkat: A={values[0]}, B={values[1]}, C={values[2]}, D={values[3]}. " +
                        $"Anong pagkakasunod-sunod ng mga pangkat mula {direction} ayon sa dami ng tansan?";
                    correct = string.Join(" – ", Enumerable.Range(0, 4)
                        .OrderBy(index => ascending ? values[index] : -values[index]).Select(index => groups[index]));
                    var seen = new HashSet<string> { correct };
                    distractors = new List<string>();
                    while (distractors.Count < 3)
                    {
                        string wrong = string.Join(" – ", groups.OrderBy(_ => Random.Shared.Next()));
                        if (seen.Add(wrong)) distractors.Add(wrong);
                    }
                }
                else
                {
                    prompt = $"Alin ang tamang ayos ng {string.Join(", ", values)} mula {direction}?";
                    correct = Sequence(ordered);
                    var seen = new HashSet<string> { correct };
                    distractors = new List<string>();
                    while (distractors.Count < 3)
                    {
                        string wrong = Sequence(values.OrderBy(_ => Random.Shared.Next()));
                        if (seen.Add(wrong)) distractors.Add(wrong);
                    }
                }
                var choices = distractors.Append(correct).OrderBy(_ => Random.Shared.Next()).ToArray();
                int answerIndex = Array.IndexOf(choices, correct);
                questions.AppendLine($"{item}. {prompt}");
                for (int option = 0; option < 4; option++)
                    questions.AppendLine($"   {Letters[option]}. {choices[option]}");
                questions.AppendLine();
                answers.AppendLine($"{item}. {Letters[answerIndex]} — {correct} | {Levels[level]}");
            }
            return new ActivityDraft
            {
                Instructions = $"Basahin ang bawat tanong at bilugan ang titik ng tamang sagot. Saklaw: 1–{maximum}. May apat na pagpipilian sa bawat item.",
                Questions = questions.ToString().TrimEnd(),
                AnswerKey = "ANSWER KEY AT COGNITIVE LEVELS:\n" + answers.ToString().TrimEnd() +
                    "\n\nRemembering: pagkilala sa pinakamaliit/pinakamalaki. Understanding: pag-unawa sa wastong ayos. Applying: pag-aayos ng pangkat ayon sa dami. Analyzing: pagtukoy ng dalawang bilang na mali ang puwesto."
            };
        }

        private static string Sequence(IEnumerable<int> values) => string.Join(" – ", values);
        private static string Pair(int a, int b) => $"{Math.Min(a, b)} at {Math.Max(a, b)}";
    }
}
