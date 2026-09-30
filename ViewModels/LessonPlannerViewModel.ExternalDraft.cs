using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Input;

namespace TeachFlex.ViewModels
{
    public partial class LessonPlannerViewModel
    {
        private string _externalDraftText = "";
        private string _externalDraftPreview = "";
        private string _externalDraftStatus = "Paste the AI response, then preview it before applying it to the session.";
        private Dictionary<string, string>? _previewedExternalSections;
        private string _previewedExternalContext = "";

        public string ExternalDraftText
        {
            get => _externalDraftText;
            set
            {
                if (SetProperty(ref _externalDraftText, value))
                {
                    _previewedExternalSections = null;
                    ExternalDraftPreview = "";
                    ExternalDraftStatus = "The text has changed. Preview it again before applying.";
                }
            }
        }

        public string ExternalDraftPreview
        {
            get => _externalDraftPreview;
            private set => SetProperty(ref _externalDraftPreview, value);
        }

        public string ExternalDraftStatus
        {
            get => _externalDraftStatus;
            private set => SetProperty(ref _externalDraftStatus, value);
        }

        private static readonly string[] ExternalLabels =
        {
            "TITLE",
            "COMPETENCY",
            "OBJECTIVES",
            "MOTIVATION",
            "ACTIVITIES",
            "FORMATIVE_CHECKS",
            "RESOURCES",
            "WAYS_FORWARD"
        };

        [RelayCommand]
        private void PasteExternalDraft()
        {
            try
            {
                ExternalDraftText = System.Windows.Clipboard.GetText();
            }
            catch (Exception ex)
            {
                _dialogs.ShowError(ex.Message, "Paste AI Draft");
            }
        }

        [RelayCommand]
        private void PreviewExternalDraft()
        {
            _previewedExternalSections = null;
            ExternalDraftPreview = "";

            if (SelectedClass == null ||
                SelectedSubject == null ||
                string.IsNullOrWhiteSpace(LearningCompetency) ||
                string.IsNullOrWhiteSpace(ExternalDraftText))
            {
                _dialogs.ShowWarning(
                    "Select the class and competency, then paste the AI response first.",
                    "ILAW Draft");

                return;
            }

            Dictionary<string, string> result;

            try
            {
                result = ParseExternalSections(ExternalDraftText);
            }
            catch (InvalidOperationException ex)
            {
                ExternalDraftStatus = ex.Message;
                return;
            }

            bool weekly = result.Keys.Any(
                k => k.StartsWith("SESSION_", StringComparison.Ordinal));

            if (weekly)
            {
                var missing = new List<string>();

                for (int day = 1; day <= 4; day++)
                {
                    foreach (var label in ExternalLabels)
                    {
                        if (!result.ContainsKey($"SESSION_{day}/{label}"))
                        {
                            missing.Add($"Session {day}: {label}");
                        }
                    }
                }

                if (missing.Count > 0)
                {
                    ExternalDraftStatus =
                        "Weekly response is incomplete. Paste all four sessions before applying. Missing: "
                        + string.Join(", ", missing);

                    return;
                }

                _previewedExternalSections = result;
                _previewedExternalContext = CurrentChatContext();

                var weeklyPreview =
                    new StringBuilder("WEEKLY MAPPING · SESSIONS 1–4\n");

                foreach (var field in result.Where(
                    field => !field.Key.EndsWith(
                        "/MARKER",
                        StringComparison.Ordinal)))
                {
                    weeklyPreview
                        .AppendLine()
                        .AppendLine($"[{field.Key}]")
                        .AppendLine(field.Value);
                }

                ExternalDraftPreview = weeklyPreview.ToString();

                ExternalDraftStatus =
                    "All four sessions recognized. Review the weekly preview, then apply. Session 5 will also be mapped when supplied.";

                return;
            }

            if (!result.ContainsKey("ACTIVITIES") || result.Count < 3)
            {
                ExternalDraftStatus =
                    "The sections could not be recognized. Ask the AI to use the [TITLE], [OBJECTIVES], [MOTIVATION], [ACTIVITIES], [FORMATIVE_CHECKS], [RESOURCES] and [WAYS_FORWARD] labels.";

                return;
            }

            _previewedExternalSections = result;
            _previewedExternalContext = CurrentChatContext();

            var preview = new StringBuilder();

            preview.AppendLine(
                $"Session {SelectedDay} • {SelectedSubject.SubjectName} • {LearningCompetency}");

            foreach (var label in ExternalLabels)
            {
                preview
                    .AppendLine()
                    .AppendLine($"[{label}]");

                preview.AppendLine(
                    result.TryGetValue(label, out var content)
                        ? content
                        : "(empty; the current field will be kept)");
            }

            ExternalDraftPreview = preview.ToString();

            ExternalDraftStatus =
                "Review the preview. Empty sections will not replace the current fields.";
        }

        private static Dictionary<string, string> ParseExternalSections(string text)
        {
            var result =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            var body = new StringBuilder();

            string? current = null;
            string scope = "";

            void Finish()
            {
                if (current != null &&
                    !string.IsNullOrWhiteSpace(body.ToString()))
                {
                    if (result.ContainsKey(current))
                    {
                        throw new InvalidOperationException(
                            "Duplicate section: "
                            + current
                            + ". Use a separate [SESSION_1] to [SESSION_4] heading for each session.");
                    }

                    result[current] = body.ToString().Trim();
                }

                body.Clear();
            }

            var labels = ExternalLabels
                .Concat(new[]
                {
                    "WEEKLY_TITLE",
                    "STANDARDS_AND_COMPETENCIES",
                    "WEEKLY_OBJECTIVES",
                    "WEEKLY_RESOURCES",
                    "COVERAGE",
                    "TEST_ITEMS",
                    "ANSWER_KEY_OR_SCORING_GUIDE"
                })
                .ToArray();

            foreach (var raw in text
                .Replace("\r\n", "\n")
                .Split('\n'))
            {
                var heading = raw
                    .Trim()
                    .Trim('#', '*', ' ')
                    .Trim()
                    .TrimEnd(':')
                    .Trim();

                var session = Regex.Match(
                    heading,
                    @"^\[?SESSION[ _]([1-4])\]?$",
                    RegexOptions.IgnoreCase);

                bool test = Regex.IsMatch(
                    heading,
                    @"^\[?SESSION[ _]5[ _]WEEKLY[ _]TEST\]?$",
                    RegexOptions.IgnoreCase);

                if (session.Success || test)
                {
                    Finish();

                    scope = test
                        ? "SESSION_5"
                        : "SESSION_" + session.Groups[1].Value;

                    result.TryAdd(
                        scope + "/MARKER",
                        "Recognized");

                    current = null;
                    continue;
                }

                var label = labels.FirstOrDefault(
                    l =>
                        heading.Equals(
                            "[" + l + "]",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        heading.Equals(
                            l,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        heading.Equals(
                            l.Replace('_', ' '),
                            StringComparison.OrdinalIgnoreCase));

                if (label != null)
                {
                    Finish();

                    current =
                        label.StartsWith(
                            "WEEKLY_",
                            StringComparison.Ordinal)
                        ||
                        label == "STANDARDS_AND_COMPETENCIES"
                        ||
                        scope.Length == 0
                            ? label
                            : scope + "/" + label;
                }
                else if (
                    current != null &&
                    !raw.Trim().StartsWith(
                        "```",
                        StringComparison.Ordinal))
                {
                    body.AppendLine(raw);
                }
            }

            Finish();

            return result;
        }

        [RelayCommand]
        private void ApplyExternalDraft()
        {
            if (_previewedExternalSections == null ||
                _previewedExternalContext != CurrentChatContext())
            {
                _dialogs.ShowWarning(
                    "Preview the AI response again for the current class, competency and session.",
                    "ILAW Draft");

                return;
            }

            if (_previewedExternalSections.Keys.Any(
                k => k.StartsWith(
                    "SESSION_",
                    StringComparison.Ordinal)))
            {
                if (!_dialogs.Confirm(
                    "Apply the weekly draft to Sessions 1–4 and the supplied Session 5 test? Existing content in the mapped fields will be replaced.",
                    "Apply Weekly ILAW Draft"))
                {
                    return;
                }

                ApplyWeeklyExternalDraft(
                    _previewedExternalSections);

                ExternalDraftStatus =
                    "Weekly draft applied to Sessions 1–4. Review all sessions and save the lesson plan.";

                _previewedExternalSections = null;

                return;
            }

            if (!_dialogs.Confirm(
                $"Apply the previewed AI draft to Session {SelectedDay}? Only fields with content in the draft will be replaced.",
                "Apply ILAW Draft"))
            {
                return;
            }

            var fields = _previewedExternalSections;

            if (fields.TryGetValue("TITLE", out var title))
                Title = title;

            if (fields.TryGetValue("OBJECTIVES", out var objectives))
                Objectives = objectives;

            if (fields.TryGetValue("RESOURCES", out var resources))
                Materials = resources;

            if (fields.TryGetValue("MOTIVATION", out var motivation))
            {
                switch (SelectedDay)
                {
                    case 2:
                        Session2Motivation = motivation;
                        break;

                    case 3:
                        Session3Motivation = motivation;
                        break;

                    case 4:
                        Session4Motivation = motivation;
                        break;

                    default:
                        Session1Motivation = motivation;
                        break;
                }
            }

            if (fields.TryGetValue("ACTIVITIES", out var activities))
                CurrentDayActivities = activities;

            if (fields.TryGetValue("FORMATIVE_CHECKS", out var checks))
                CurrentDayAssessment = checks;

            if (fields.TryGetValue("WAYS_FORWARD", out var ways))
            {
                switch (SelectedDay)
                {
                    case 2:
                        Session2ExtendLearning = ways;
                        break;

                    case 3:
                        Session3ExtendLearning = ways;
                        break;

                    case 4:
                        Session4ExtendLearning = ways;
                        break;

                    default:
                        Session1ExtendLearning = ways;
                        break;
                }
            }

            ExternalDraftStatus =
                $"AI draft applied to Session {SelectedDay}. Review and edit each field before saving the lesson plan.";

            _previewedExternalSections = null;
        }

        private void ApplyWeeklyExternalDraft(
            Dictionary<string, string> fields)
        {
            string Get(int day, string label) =>
                fields.TryGetValue(
                    $"SESSION_{day}/{label}",
                    out var text)
                    ? text.Trim()
                    : string.Empty;

            // =========================================================
            // WEEKLY TITLE
            // =========================================================

            if (fields.TryGetValue(
                "WEEKLY_TITLE",
                out var weeklyTitle) &&
                !string.IsNullOrWhiteSpace(weeklyTitle))
            {
                Title = weeklyTitle.Trim();
            }

            // =========================================================
            // WEEKLY RESOURCES
            // =========================================================

            string resources = string.Join(
                "\n\n",
                Enumerable.Range(1, 4)
                    .Select(day => Get(day, "RESOURCES"))
                    .Where(text =>
                        !string.IsNullOrWhiteSpace(text))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase));

            if (fields.TryGetValue(
                "WEEKLY_RESOURCES",
                out var weeklyResources) &&
                !string.IsNullOrWhiteSpace(weeklyResources))
            {
                resources =
                    string.IsNullOrWhiteSpace(resources)
                        ? weeklyResources.Trim()
                        : weeklyResources.Trim()
                            + "\n\n"
                            + resources;
            }

            if (!string.IsNullOrWhiteSpace(resources))
                Materials = resources.Trim();

            // =========================================================
            // IMPORTANT:
            //
            // Each session gets its OWN:
            // - Title
            // - Competency
            // - Objectives
            //
            // Session 1 = index 0
            // Session 2 = index 1
            // Session 3 = index 2
            // Session 4 = index 3
            // =========================================================

            for (int day = 1; day <= 4; day++)
            {
                _sessionTitles[day - 1] =
                    Get(day, "TITLE");

                _sessionCompetencies[day - 1] =
                    Get(day, "COMPETENCY");

                _sessionObjectives[day - 1] =
                    Get(day, "OBJECTIVES");

                string flow =
                    Get(day, "ACTIVITIES");

                string sessionTitle =
                    Get(day, "TITLE");

                if (!string.IsNullOrWhiteSpace(sessionTitle))
                {
                    flow =
                        string.IsNullOrWhiteSpace(flow)
                            ? sessionTitle
                            : sessionTitle
                                + "\n\n"
                                + flow;
                }

                switch (day)
                {
                    case 1:

                        Session1Motivation =
                            Get(1, "MOTIVATION");

                        Day1 = flow;

                        Day1Assessment =
                            Get(1, "FORMATIVE_CHECKS");

                        Session1ExtendLearning =
                            Get(1, "WAYS_FORWARD");

                        break;

                    case 2:

                        Session2Motivation =
                            Get(2, "MOTIVATION");

                        Day2 = flow;

                        Day2Assessment =
                            Get(2, "FORMATIVE_CHECKS");

                        Session2ExtendLearning =
                            Get(2, "WAYS_FORWARD");

                        break;

                    case 3:

                        Session3Motivation =
                            Get(3, "MOTIVATION");

                        Day3 = flow;

                        Day3Assessment =
                            Get(3, "FORMATIVE_CHECKS");

                        Session3ExtendLearning =
                            Get(3, "WAYS_FORWARD");

                        break;

                    case 4:

                        Session4Motivation =
                            Get(4, "MOTIVATION");

                        Day4 = flow;

                        Day4Assessment =
                            Get(4, "FORMATIVE_CHECKS");

                        Session4ExtendLearning =
                            Get(4, "WAYS_FORWARD");

                        break;
                }
            }

            // =========================================================
            // REFRESH CURRENT SESSION
            //
            // This is what makes Session 1 show only Session 1
            // objectives, Session 2 only Session 2 objectives, etc.
            // =========================================================

            _switchingSession = true;

            try
            {
                LoadSessionIntention();
            }
            finally
            {
                _switchingSession = false;
            }

            OnPropertyChanged(
                nameof(CurrentDayActivities));

            OnPropertyChanged(
                nameof(CurrentDayAssessment));

            // =========================================================
            // SESSION 5 — WEEKLY TEST
            // =========================================================

            string test = string.Join(
                "\n\n",
                new[]
                {
                    "COVERAGE",
                    "TEST_ITEMS",
                    "ANSWER_KEY_OR_SCORING_GUIDE"
                }
                .Where(label =>
                    !string.IsNullOrWhiteSpace(
                        Get(5, label)))
                .Select(label =>
                    label.Replace('_', ' ')
                    + "\n"
                    + Get(5, label)));

            if (!string.IsNullOrWhiteSpace(test))
                WeeklyTest = test;

            string session5WaysForward =
                Get(5, "WAYS_FORWARD");

            if (!string.IsNullOrWhiteSpace(
                session5WaysForward))
            {
                Session5ExtendLearning =
                    session5WaysForward;
            }
        }
    }
}