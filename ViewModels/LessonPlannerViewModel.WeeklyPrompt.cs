using System;
using CommunityToolkit.Mvvm.Input;

namespace TeachFlex.ViewModels
{
    public partial class LessonPlannerViewModel
    {
        [RelayCommand]
        private void CopyWeeklyPrompt()
        {
            if (SelectedClass == null || SelectedSubject == null || string.IsNullOrWhiteSpace(LearningCompetency))
            {
                _dialogs.ShowWarning("Select the class, subject and learning competency for the week first.", "Weekly ILAW Prompt");
                return;
            }
            try
            {
                System.Windows.Clipboard.SetText(BuildWeeklyPrompt());
                StatusMessage = "Weekly ILAW prompt copied for Sessions 1–4 and the Session 5 test. Open your AI chatbot and press Ctrl+V.";
            }
            catch (Exception ex)
            {
                _dialogs.ShowError("Could not copy the weekly prompt: " + ex.Message, "Weekly ILAW Prompt");
            }
        }

        private string BuildWeeklyPrompt()
        {
            static string ValueOrMissing(string value) => string.IsNullOrWhiteSpace(value) ? "Not provided" : value.Trim();
            return $"""
                Create a complete, editable weekly ILAW lesson plan in English for FOUR teaching sessions. Session 5 is reserved for the weekly test.

                LESSON CONTEXT
                Grade Level: {SelectedClass!.GradeLevel}
                Subject: {SelectedSubject!.SubjectName}
                Term: {TermNumber}
                Week: {WeekNumber}
                BOW Reference: {ValueOrMissing(BowSource)}
                Content Standard: {ValueOrMissing(ContentStandard)}
                Performance Standard: {ValueOrMissing(PerformanceStandard)}
                Supplied Learning Competency/Competencies:
                {LearningCompetency}
                Learner Needs / Available Materials: {ValueOrMissing(LearnerNeeds)}
                Existing Learning Resources: {ValueOrMissing(Materials)}
                Additional Teacher Request: {ValueOrMissing(ChatPrompt)}

                CURRICULUM REQUIREMENTS
                1. Preserve the exact wording of the supplied standards and competencies. Do not claim that teacher-entered text is verified official BOW text.
                2. Align the title, measurable objectives, activities and assessments with the supplied competencies.
                3. Do not invent official standards, competency codes or DepEd requirements. Do not pretend to have read the original BOW PDF.
                4. For missing standards, state “Not provided in the BOW.” Continue using the available information.
                5. Label any additional suggested competency “Teacher-review suggestion — not an official BOW competency.”
                6. Organize Sessions 1–4 progressively: introduction, guided practice, application and consolidation. Adapt this sequence to the actual competency.
                7. Use English for teacher directions, explanations and assessment instructions. Retain subject-specific words or texts needed to teach the competency.
                8. Treat the additional teacher request as supplementary to this FULL WEEK scope. Provide all four sessions, even if the request refers to one selected session.

                WEEKLY OVERVIEW
                Provide a weekly title, the supplied standards and exact competencies, measurable weekly objectives, a brief focus for each of Sessions 1–4, and common resources and preparation.

                FOR EACH SESSION 1–4, PROVIDE:
                I — INTENTIONS
                - Session title and focus.
                - Two or three measurable learning objectives and simple grade-appropriate success criteria.

                L — LEARNING EXPERIENCES
                A. Pre-Lesson / Motivation
                - A specific engaging sample motivation, materials, teacher steps, actual questions and expected responses.
                B. Lesson Flow / Activities
                - A brief explanation or demonstration with actual examples.
                - THREE TO FIVE specific activities, including at least one group activity.
                - For EACH activity: title, purpose, clear learner directions, item/task count, ACTUAL questions/examples/tasks, expected answers/responses or a scoring guide, and suggested time.
                - For group activities: group size, roles, task and expected output.
                - Vary the questions and tasks across sessions. Include Remembering, Understanding, Applying and Analyzing where developmentally appropriate.
                - Include a simpler version for learners needing support and an extension for learners ready for more challenge.
                - Prefer affordable, readily available materials. If session duration is not supplied, state your suggested duration and keep activity timings consistent with it.

                A — ASSESSMENT
                - FIVE actual formative-check items or observable tasks, clear directions, an answer key or observation checklist, and a simple criterion for identifying learners needing support.
                - Align every item with the session objectives.

                W — WAYS FORWARD
                - Specific remediation and enrichment activities.
                - Teacher reflection questions to complete AFTER teaching.
                - Leave actual learner results and teacher reflections blank. Do not invent them.

                SESSION 5 — WEEKLY TEST
                Provide TEN actual items or age-appropriate performance tasks based ONLY on Sessions 1–4. Include directions, appropriate cognitive levels, answer key or scoring guide, and suggested follow-up for learners needing support.

                OUTPUT FORMAT
                Use these EXACT headings, each on its own line. Do not use a markdown code fence.
                [WEEKLY_TITLE]
                [STANDARDS_AND_COMPETENCIES]
                [WEEKLY_OBJECTIVES]
                [WEEKLY_RESOURCES]

                [SESSION_1]
                [TITLE]
                [OBJECTIVES]
                [MOTIVATION]
                [ACTIVITIES]
                [FORMATIVE_CHECKS]
                [RESOURCES]
                [WAYS_FORWARD]

                Repeat those seven section headings under [SESSION_2], [SESSION_3] and [SESSION_4].
                Finish with:
                [SESSION_5_WEEKLY_TEST]
                [COVERAGE]
                [TEST_ITEMS]
                [ANSWER_KEY_OR_SCORING_GUIDE]
                [WAYS_FORWARD]

                Write COMPLETE content for every session. Do not use “same as above,” unfinished placeholders, or instructions asking the teacher to create the actual questions. If the response is too long, stop after a complete session and identify which session should be continued next.
                Present everything as a suggested draft for teacher review and adaptation.
                """;
        }
    }
}
