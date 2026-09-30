using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class LessonPlannerViewModel
    {
        private readonly TeachFlexIlawChatService _lessonChat = new();
        private TeachFlexIlawDraft? _chatDraft;
        private string _chatContext = "";
        private string _teacherChatPrompt = "Create an ILAW lesson for this session in English. Include four specific activities with directions, item counts and actual items, a group activity, and a five-item formative check with answers.";
        private string _chatTranscript = "";
        private string _chatDraftPreview = "";
        private string _chatStatus = "TeachFlex ILAW Assistant is ready when the AI server is connected.";
        private bool _chatBusy;

        public string TeacherChatPrompt { get => _teacherChatPrompt; set => SetProperty(ref _teacherChatPrompt, value); }
        public string ChatTranscript { get => _chatTranscript; private set => SetProperty(ref _chatTranscript, value); }
        public string ChatDraftPreview { get => _chatDraftPreview; private set => SetProperty(ref _chatDraftPreview, value); }
        public string ChatStatus { get => _chatStatus; private set => SetProperty(ref _chatStatus, value); }
        public bool ChatBusy { get => _chatBusy; private set => SetProperty(ref _chatBusy, value); }

        private string CurrentChatContext() => $"{SelectedClass?.Id}|{SelectedSubject?.Id}|{TermNumber}|{WeekNumber}|{SelectedDay}|{LearningCompetency}";

        [RelayCommand]
        private async Task SendLessonChatAsync()
        {
            if (ChatBusy) return;
            if (SelectedClass == null || SelectedSubject == null ||
                string.IsNullOrWhiteSpace(LearningCompetency) || string.IsNullOrWhiteSpace(TeacherChatPrompt))
            {
                _dialogs.ShowWarning("Select the class, subject and learning competency, then enter a prompt.", "TeachFlex ILAW Assistant");
                return;
            }
            string context = CurrentChatContext();
            string prompt = TeacherChatPrompt.Trim();
            try
            {
                ChatBusy = true;
                ChatStatus = "Generating a lesson draft. This may take a few minutes...";
                var draft = await _lessonChat.AskAsync(SelectedClass.GradeLevel,
                    SelectedSubject.SubjectName, TermNumber, WeekNumber, SelectedDay,
                    ContentStandard, PerformanceStandard, LearningCompetency, LearnerNeeds,
                    prompt, context == _chatContext ? ChatDraftPreview : "");
                if (CurrentChatContext() != context)
                {
                    ChatStatus = "The selected session or competency has changed. Send the prompt again for the current selection.";
                    return;
                }
                ChatTranscript = (context == _chatContext ? ChatTranscript + "\n\n" : "") +
                    "TEACHER: " + prompt + "\nTEACHFLEX: The draft is shown below. You can request changes.";
                _chatDraft = draft;
                _chatContext = context;
                ChatDraftPreview = draft.Preview;
                ChatStatus = "Review the draft. Send a follow-up prompt or apply it to the selected session.";
            }
            catch (Exception ex)
            {
                ChatStatus = ex.Message;
                _dialogs.ShowError(ex.Message, "TeachFlex ILAW Assistant");
            }
            finally { ChatBusy = false; }
        }

        [RelayCommand]
        private void ApplyLessonChatDraft()
        {
            if (_chatDraft == null || CurrentChatContext() != _chatContext)
            {
                _dialogs.ShowWarning("Generate a draft for the current class, competency and session first.", "TeachFlex ILAW Assistant");
                return;
            }
            if (!_dialogs.Confirm($"Apply the AI draft to Session {SelectedDay}? This will replace the title, objectives, motivation, activities, formative checks and resources.", "Apply AI Draft"))
                return;
            Title = _chatDraft.Title;
            Objectives = _chatDraft.Objectives;
            Materials = _chatDraft.Resources;
            switch (SelectedDay)
            {
                case 2: Session2Motivation = _chatDraft.Motivation; break;
                case 3: Session3Motivation = _chatDraft.Motivation; break;
                case 4: Session4Motivation = _chatDraft.Motivation; break;
                default: Session1Motivation = _chatDraft.Motivation; break;
            }
            CurrentDayActivities = _chatDraft.LessonFlow;
            CurrentDayAssessment = _chatDraft.FormativeChecks;
            ChatStatus = "AI draft applied. Review and edit the details before saving the lesson plan.";
        }
    }
}
