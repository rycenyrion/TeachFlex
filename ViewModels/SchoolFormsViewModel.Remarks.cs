using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        private int _selectedRemarkTerm = 1;
        private decimal? _remarkGeneralAverage;
        private AutomatedRemarkLevel _selectedRemarkLevel =
            AutomatedRemarkLevel.Benchmarking;
        private string _suggestedRemark = string.Empty;
        private string _finalRemark = string.Empty;
        private string _remarkStatus =
            "Select a Grade 1-12 learner.";
        private int _suggestionVariation;
        private bool _isRemarkApproved;
        private bool _remarkNeedsReview;
        private string _gradeOneSuggestedCanDo = string.Empty;
        private string _gradeOneFinalCanDo = string.Empty;
        private string _gradeOneSuggestedNeedsImprovement = string.Empty;
        private string _gradeOneFinalNeedsImprovement = string.Empty;
        private string _gradeOneTeacherRemarks = string.Empty;

        public IReadOnlyList<int> RemarkTerms { get; } =
            new[] { 1, 2, 3 };

        public IReadOnlyList<AutomatedRemarkLevel>
            RemarkPerformanceLevels
        { get; } =
                Enum.GetValues<AutomatedRemarkLevel>();

        public int SelectedRemarkTerm
        {
            get => _selectedRemarkTerm;
            set
            {
                if (!SetProperty(ref _selectedRemarkTerm, value))
                {
                    return;
                }

                _ = LoadRemarkEditorAsync();
            }
        }

        public decimal? RemarkGeneralAverage
        {
            get => _remarkGeneralAverage;
            private set
            {
                if (SetProperty(ref _remarkGeneralAverage, value))
                {
                    OnPropertyChanged(nameof(RemarkGeneralAverageText));
                }
            }
        }

        public string RemarkGeneralAverageText
        {
            get
            {
                if (GetSelectedGradeNumber() == 1)
                {
                    return "Descriptive";
                }

                return RemarkGeneralAverage.HasValue
                    ? RemarkGeneralAverage.Value.ToString("0.00")
                    : "No grades yet";
            }
        }

        public AutomatedRemarkLevel SelectedRemarkLevel
        {
            get => _selectedRemarkLevel;
            set
            {
                if (!SetProperty(ref _selectedRemarkLevel, value))
                {
                    return;
                }

                if (GetSelectedGradeNumber() == 1 &&
                    IsRemarksEditorAvailable)
                {
                    _ = LoadRemarkEditorAsync();
                }
            }
        }

        public string SuggestedRemark
        {
            get => _suggestedRemark;
            private set => SetProperty(ref _suggestedRemark, value);
        }

        public string FinalRemark
        {
            get => _finalRemark;
            set
            {
                if (SetProperty(ref _finalRemark, value))
                {
                    IsRemarkApproved = false;
                }
            }
        }

        public string RemarkStatus
        {
            get => _remarkStatus;
            private set => SetProperty(ref _remarkStatus, value);
        }

        public bool IsRemarkApproved
        {
            get => _isRemarkApproved;
            private set => SetProperty(ref _isRemarkApproved, value);
        }

        public bool RemarkNeedsReview
        {
            get => _remarkNeedsReview;
            private set => SetProperty(ref _remarkNeedsReview, value);
        }

        public bool IsGradeOneRemarkEditor =>
            GetSelectedGradeNumber() == 1;

        public bool IsStandardRemarkEditor =>
            GetSelectedGradeNumber() is >= 2 and <= 12;

        public string GradeOneSuggestedCanDo
        {
            get => _gradeOneSuggestedCanDo;
            private set => SetProperty(ref _gradeOneSuggestedCanDo, value);
        }

        public string GradeOneFinalCanDo
        {
            get => _gradeOneFinalCanDo;
            set
            {
                if (SetProperty(ref _gradeOneFinalCanDo, value))
                {
                    IsRemarkApproved = false;
                }
            }
        }

        public string GradeOneSuggestedNeedsImprovement
        {
            get => _gradeOneSuggestedNeedsImprovement;
            private set => SetProperty(
                ref _gradeOneSuggestedNeedsImprovement,
                value);
        }

        public string GradeOneFinalNeedsImprovement
        {
            get => _gradeOneFinalNeedsImprovement;
            set
            {
                if (SetProperty(
                        ref _gradeOneFinalNeedsImprovement,
                        value))
                {
                    IsRemarkApproved = false;
                }
            }
        }

        public bool IsRemarksEditorAvailable
        {
            get
            {
                int gradeNumber = GetSelectedGradeNumber();

                return SelectedClass != null &&
                       SelectedSf9Learner != null &&
                       gradeNumber >= 1 &&
                       gradeNumber <= 12;
            }
        }

        private async Task LoadRemarkEditorAsync()
        {
            OnPropertyChanged(nameof(IsRemarksEditorAvailable));
            OnPropertyChanged(nameof(IsGradeOneRemarkEditor));
            OnPropertyChanged(nameof(IsStandardRemarkEditor));
            OnPropertyChanged(nameof(RemarkGeneralAverageText));

            if (!IsRemarksEditorAvailable ||
                SelectedClass == null ||
                SelectedSf9Learner == null)
            {
                ResetRemarkEditor();
                return;
            }

            int gradeNumber = GetSelectedGradeNumber();

            if (gradeNumber == 1)
            {
                RemarkGeneralAverage = null;
                await LoadGradeOneNarrativeEditorAsync();
                return;
            }

            LearnerTermRemark? saved =
                await _learnerTermRemarkRepository.GetAsync(
                    SelectedClass.Id,
                    SelectedSf9Learner.Id,
                    SelectedRemarkTerm);

            decimal? currentAverage =
                await CalculateSelectedTermAverageAsync();

            RemarkGeneralAverage = currentAverage;

            if (!currentAverage.HasValue)
            {
                SuggestedRemark = string.Empty;
                _finalRemark = string.Empty;
                OnPropertyChanged(nameof(FinalRemark));
                IsRemarkApproved = false;
                RemarkNeedsReview = saved != null;
                RemarkStatus =
                    $"Note: Kumpletuhin muna ang grades ng lahat ng subjects sa Term {SelectedRemarkTerm} bago magkaroon ng automatic teacher's remark.";
                return;
            }

            if (saved != null)
            {
                _selectedRemarkLevel = saved.PerformanceLevel;
                OnPropertyChanged(nameof(SelectedRemarkLevel));

                SuggestedRemark = saved.SuggestedRemark;
                _finalRemark = saved.FinalRemark;
                OnPropertyChanged(nameof(FinalRemark));
                _suggestionVariation = saved.SuggestionVariation;
                IsRemarkApproved = saved.IsTeacherApproved;

                bool averageChanged =
                    saved.SourceGeneralAverage != currentAverage;

                RemarkNeedsReview =
                    saved.NeedsReview || averageChanged;

                RemarkStatus = RemarkNeedsReview
                    ? "Grades changed. Review and approve this remark again."
                    : saved.IsTeacherApproved
                        ? "Approved and ready for SF9."
                        : "Saved draft. Teacher approval is still required.";

                return;
            }

            if (gradeNumber > 1 && currentAverage.HasValue)
            {
                _selectedRemarkLevel =
                    _automatedRemarksService.GetPerformanceLevel(
                        currentAverage.Value);
                OnPropertyChanged(nameof(SelectedRemarkLevel));
            }

            _suggestionVariation = 0;
            IsRemarkApproved = false;
            RemarkNeedsReview = false;
            GenerateInitialSuggestion();
        }

        private async Task<decimal?>
            CalculateSelectedTermAverageAsync()
        {
            SF9ExportRequest request =
                await CreateSf9ExportRequestAsync();

            List<SF9SubjectGradeRow> subjects = request.SubjectGrades
                .Where(grade => !string.Equals(grade.SubjectCategory,
                    "MAPEH Component", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (subjects.Count == 0)
            {
                return null;
            }

            if (!HasCompleteSf9TermGrades(request.SubjectGrades,
                    GetSelectedGradeNumber(), SelectedRemarkTerm))
                return null;

            if (GetSelectedGradeNumber() >= 11)
            {
                List<SF9SubjectGradeRow> electives = subjects
                    .Where(grade => grade.SubjectCategory.Contains("elective",
                        StringComparison.OrdinalIgnoreCase) ||
                        grade.SubjectName.Contains("elective", StringComparison.OrdinalIgnoreCase) ||
                        grade.SubjectName.Contains("immersion", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(grade => grade.DisplayOrder)
                    .ThenBy(grade => grade.SubjectName).ToList();
                int perTerm = GetSelectedGradeNumber() == 12 && electives.Count >= 9 ? 4 : 1;
                subjects = subjects.Except(electives)
                    .Concat(electives.Skip((SelectedRemarkTerm - 1) * perTerm)
                        .Take(perTerm)).ToList();
            }

            List<int?> termGrades = subjects.Select(grade =>
                SelectedRemarkTerm switch
                {
                    1 => grade.TermOneGrade,
                    2 => grade.TermTwoGrade,
                    3 => grade.TermThreeGrade,
                    _ => null
                }).ToList();
            if (termGrades.Any(value => !value.HasValue))
                return null;

            List<int> availableGrades = termGrades.Select(value => value!.Value).ToList();

            return Math.Round(
                availableGrades.Average(
                    value => (decimal)value),
                2,
                MidpointRounding.AwayFromZero);
        }

        private void GenerateInitialSuggestion()
        {
            if (SelectedSf9Learner == null)
            {
                return;
            }

            if (GetSelectedGradeNumber() == 1)
            {
                GenerateGradeOneNarrativePair();
                return;
            }

            SuggestedRemark =
                _automatedRemarksService.GenerateInitialRemark(
                    GetSelectedGradeNumber(),
                    SelectedRemarkLevel,
                    SelectedSf9Learner.Id,
                    SelectedRemarkTerm);

            _finalRemark = SuggestedRemark;
            OnPropertyChanged(nameof(FinalRemark));
            RemarkStatus =
                "Automatic suggestion created. Review or edit before saving.";
        }

        [RelayCommand]
        private async Task GenerateAnotherRemarkAsync()
        {
            if (!IsRemarksEditorAvailable ||
                SelectedSf9Learner == null)
            {
                return;
            }

            if (GetSelectedGradeNumber() == 1)
            {
                string missingArea = await GetIncompleteGradeOneAreaAsync();
                if (missingArea.Length > 0)
                {
                    RemarkStatus =
                        $"Kumpletuhin muna ang ratings sa {missingArea} para sa Term {SelectedRemarkTerm}.";
                    _dialogService.ShowWarning(RemarkStatus, "Incomplete Grade 1 PACE");
                    return;
                }
                _suggestionVariation++;
                GenerateGradeOneNarrativePair();
                RemarkNeedsReview = false;
                RemarkStatus =
                    "Another Grade 1 narrative pair was generated. " +
                    "Review both entries before saving.";
                return;
            }

            RemarkGeneralAverage = await CalculateSelectedTermAverageAsync();
            if (!RemarkGeneralAverage.HasValue)
            {
                RemarkStatus = $"Note: Kumpletuhin muna ang grades ng lahat ng subjects sa Term {SelectedRemarkTerm}.";
                _dialogService.ShowWarning(RemarkStatus, "Incomplete Term Grades");
                return;
            }

            _suggestionVariation++;
            SuggestedRemark =
                _automatedRemarksService.GenerateAnotherRemark(
                    GetSelectedGradeNumber(),
                    SelectedRemarkLevel,
                    SelectedSf9Learner.Id,
                    SelectedRemarkTerm,
                    _suggestionVariation);

            FinalRemark = SuggestedRemark;
            RemarkNeedsReview = false;
            RemarkStatus =
                "Another suggestion was generated. Review before saving.";
        }

        [RelayCommand]
        private async Task ApproveAndSaveRemarkAsync()
        {
            if (!IsRemarksEditorAvailable ||
                SelectedClass == null ||
                SelectedSf9Learner == null)
            {
                return;
            }

            if (GetSelectedGradeNumber() == 1)
            {
                await SaveGradeOneNarrativeAsync();
                return;
            }

            RemarkGeneralAverage = await CalculateSelectedTermAverageAsync();
            if (!RemarkGeneralAverage.HasValue)
            {
                RemarkStatus = $"Note: Kumpletuhin muna ang grades ng lahat ng subjects sa Term {SelectedRemarkTerm}.";
                _dialogService.ShowWarning(RemarkStatus, "Incomplete Term Grades");
                return;
            }

            if (string.IsNullOrWhiteSpace(FinalRemark))
            {
                _dialogService.ShowWarning(
                    "Enter or generate a final remark first.",
                    "Teacher's Remark");
                return;
            }

            LearnerTermRemark record =
                new LearnerTermRemark
                {
                    SchoolClassId = SelectedClass.Id,
                    LearnerId = SelectedSf9Learner.Id,
                    TermNumber = SelectedRemarkTerm,
                    GradeLevel = GetSelectedGradeNumber(),
                    PerformanceLevel = SelectedRemarkLevel,
                    SourceGeneralAverage = RemarkGeneralAverage,
                    SuggestedRemark = SuggestedRemark,
                    FinalRemark = FinalRemark,
                    SuggestionVariation = _suggestionVariation,
                    IsTeacherApproved = true,
                    NeedsReview = false,
                    ApprovedAtUtc = DateTime.UtcNow
                };

            await _learnerTermRemarkRepository.SaveAsync(record);

            IsRemarkApproved = true;
            RemarkNeedsReview = false;
            RemarkStatus = "Approved and saved. Ready for SF9.";

            _dialogService.ShowInformation(
                $"Term {SelectedRemarkTerm} remark was saved successfully.",
                "Teacher's Remark Saved");
        }

        private static readonly string[] GradeOneRequiredAreas =
        {
            "Language", "Reading & Literacy", "Mathematics",
            "Makabansa", "GMRC"
        };

        private static string NormalizeGradeOneArea(string? name)
        {
            string value = name?.Trim().ToLowerInvariant() ?? string.Empty;
            if (value.Contains("reading") || value.Contains("literacy"))
                return "Reading & Literacy";
            if (value.Contains("language")) return "Language";
            if (value.Contains("math")) return "Mathematics";
            if (value.Contains("makabansa")) return "Makabansa";
            if (value == "gmrc" || value.Contains("good manners"))
                return "GMRC";
            return string.Empty;
        }

        public string GradeOneTeacherRemarks
        {
            get => _gradeOneTeacherRemarks;
            set
            {
                if (SetProperty(ref _gradeOneTeacherRemarks, value))
                    IsRemarkApproved = false;
            }
        }

        // Return the first incomplete area, or an empty string when all
        // five subjects have saved ratings for this learner and term.
        private async Task<string> GetIncompleteGradeOneAreaAsync(
            int? termNumber = null)
        {
            if (SelectedClass == null || SelectedSf9Learner == null)
                return "Grade 1";

            int term = termNumber ?? SelectedRemarkTerm;
            IReadOnlyList<Subject> subjects =
                await _subjectRepository.GetByClassAsync(SelectedClass.Id);
            var byArea = subjects
                .Where(subject => GradeOneRequiredAreas.Contains(
                    NormalizeGradeOneArea(subject.SubjectName)))
                .GroupBy(subject => NormalizeGradeOneArea(subject.SubjectName))
                .ToDictionary(group => group.Key, group => group.First());

            foreach (string area in GradeOneRequiredAreas)
            {
                if (!byArea.TryGetValue(area, out Subject? subject))
                    return area;

                IReadOnlyList<PaceCompetency> competencies =
                    await _paceRepository.GetCompetenciesAsync(
                        subject.Id, term);
                if (competencies.Count == 0)
                    return area;

                IReadOnlyList<LearnerPaceRating> ratings =
                    await _paceRepository.GetRatingsAsync(
                        SelectedClass.Id, subject.Id, term);
                var learnerRatings = ratings
                    .Where(rating => rating.LearnerId == SelectedSf9Learner.Id)
                    .GroupBy(rating => rating.PaceCompetencyId)
                    .ToDictionary(group => group.Key,
                        group => group.OrderByDescending(
                            rating => rating.UpdatedAtUtc).First().Rating);

                if (competencies.Any(competency =>
                    !learnerRatings.TryGetValue(competency.Id, out string? rating) ||
                    string.IsNullOrWhiteSpace(rating)))
                    return area;
            }
            return string.Empty;
        }

        private async Task LoadGradeOneNarrativeEditorAsync()
        {
            if (SelectedClass == null || SelectedSf9Learner == null)
            {
                return;
            }

            string missingArea = await GetIncompleteGradeOneAreaAsync();
            if (missingArea.Length > 0)
            {
                GradeOneSuggestedCanDo = string.Empty;
                GradeOneFinalCanDo = string.Empty;
                GradeOneSuggestedNeedsImprovement = string.Empty;
                GradeOneFinalNeedsImprovement = string.Empty;
                GradeOneTeacherRemarks = string.Empty;
                IsRemarkApproved = false;
                RemarkStatus =
                    $"Kumpletuhin muna ang ratings sa {missingArea} para sa Term {SelectedRemarkTerm}.";
                return;
            }

            IReadOnlyList<LearnerPaceSummary> summaries =
                await _paceRepository.GetSummariesAsync(
                    SelectedClass.Id,
                    SelectedRemarkTerm);

            LearnerPaceSummary? summary =
                summaries.FirstOrDefault(
                    item => item.LearnerId == SelectedSf9Learner.Id);

            if (summary != null &&
                (!string.IsNullOrWhiteSpace(summary.WhatLearnerCanDo) ||
                 !string.IsNullOrWhiteSpace(
                     summary.WhatLearnerNeedsToImprove)))
            {
                GradeOneSuggestedCanDo = summary.WhatLearnerCanDo;
                GradeOneFinalCanDo = summary.WhatLearnerCanDo;
                GradeOneSuggestedNeedsImprovement =
                    summary.WhatLearnerNeedsToImprove;
                GradeOneFinalNeedsImprovement =
                    summary.WhatLearnerNeedsToImprove;
                GradeOneTeacherRemarks = summary.TeacherRemarks;
                IsRemarkApproved = true;
                RemarkNeedsReview = false;
                RemarkStatus =
                    "Grade 1 narratives loaded and ready for SF9.";
                return;
            }

            _suggestionVariation = 0;
            IsRemarkApproved = false;
            RemarkNeedsReview = false;
            GenerateGradeOneNarrativePair();
        }

        private void GenerateGradeOneNarrativePair()
        {
            if (SelectedSf9Learner == null)
            {
                return;
            }

            AutomatedRemarkLevel improvementLevel =
                SelectedRemarkLevel switch
                {
                    AutomatedRemarkLevel.Advancing =>
                        AutomatedRemarkLevel.Benchmarking,
                    AutomatedRemarkLevel.Benchmarking =>
                        AutomatedRemarkLevel.Connecting,
                    AutomatedRemarkLevel.Connecting =>
                        AutomatedRemarkLevel.Developing,
                    AutomatedRemarkLevel.Developing =>
                        AutomatedRemarkLevel.Emerging,
                    _ => AutomatedRemarkLevel.Emerging
                };

            GradeOneSuggestedCanDo =
                _automatedRemarksService.GenerateAnotherRemark(
                    1,
                    SelectedRemarkLevel,
                    SelectedSf9Learner.Id,
                    SelectedRemarkTerm,
                    _suggestionVariation);

            GradeOneSuggestedNeedsImprovement =
                _automatedRemarksService.GenerateAnotherRemark(
                    1,
                    improvementLevel,
                    SelectedSf9Learner.Id,
                    SelectedRemarkTerm,
                    _suggestionVariation + 7);

            GradeOneFinalCanDo = GradeOneSuggestedCanDo;
            GradeOneFinalNeedsImprovement =
                GradeOneSuggestedNeedsImprovement;
            RemarkStatus =
                "Automatic Grade 1 narrative pair created. " +
                "Review or edit both entries before saving.";
        }

        private async Task SaveGradeOneNarrativeAsync()
        {
            if (SelectedClass == null || SelectedSf9Learner == null)
            {
                return;
            }

            string missingArea = await GetIncompleteGradeOneAreaAsync();
            if (missingArea.Length > 0)
            {
                _dialogService.ShowWarning(
                    $"Kumpletuhin muna ang ratings sa {missingArea} para sa Term {SelectedRemarkTerm}.",
                    "Incomplete Grade 1 PACE");
                return;
            }

            if (string.IsNullOrWhiteSpace(GradeOneFinalCanDo) ||
                string.IsNullOrWhiteSpace(
                    GradeOneFinalNeedsImprovement))
            {
                _dialogService.ShowWarning(
                    "Complete both Grade 1 narrative fields first.",
                    "Grade 1 Narrative");
                return;
            }

            IReadOnlyList<LearnerPaceSummary> summaries =
                await _paceRepository.GetSummariesAsync(
                    SelectedClass.Id,
                    SelectedRemarkTerm);

            LearnerPaceSummary? existing =
                summaries.FirstOrDefault(
                    item => item.LearnerId == SelectedSf9Learner.Id);

            LearnerPaceSummary summary =
                new LearnerPaceSummary
                {
                    Id = existing?.Id ?? 0,
                    SchoolClassId = SelectedClass.Id,
                    LearnerId = SelectedSf9Learner.Id,
                    TermNumber = SelectedRemarkTerm,
                    WhatLearnerCanDo = GradeOneFinalCanDo.Trim(),
                    WhatLearnerNeedsToImprove =
                        GradeOneFinalNeedsImprovement.Trim(),
                    TeacherRemarks = GradeOneTeacherRemarks.Trim()
                };

            await _paceRepository.SaveSummariesAsync(
                new[] { summary });

            GradeOneSuggestedCanDo = summary.WhatLearnerCanDo;
            GradeOneSuggestedNeedsImprovement =
                summary.WhatLearnerNeedsToImprove;
            IsRemarkApproved = true;
            RemarkNeedsReview = false;
            RemarkStatus =
                $"Term {SelectedRemarkTerm} Grade 1 narratives were " +
                "approved and saved for SF9.";

            _dialogService.ShowInformation(
                $"Term {SelectedRemarkTerm} Grade 1 narratives were " +
                "saved successfully.",
                "Grade 1 Narrative Saved");
        }

        private int GetSelectedGradeNumber()
        {
            if (SelectedClass == null)
            {
                return 0;
            }

            string normalized =
                SelectedClass.GradeLevel
                    .Replace(
                        "Grade",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Replace(
                        "G",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

            return int.TryParse(normalized, out int gradeNumber)
                ? gradeNumber
                : 0;
        }

        private void ResetRemarkEditor()
        {
            RemarkGeneralAverage = null;
            SuggestedRemark = string.Empty;
            _finalRemark = string.Empty;
            OnPropertyChanged(nameof(FinalRemark));
            IsRemarkApproved = false;
            RemarkNeedsReview = false;
            RemarkStatus = "Select a Grade 1-12 learner.";
            GradeOneSuggestedCanDo = string.Empty;
            _gradeOneFinalCanDo = string.Empty;
            OnPropertyChanged(nameof(GradeOneFinalCanDo));
            GradeOneSuggestedNeedsImprovement = string.Empty;
            GradeOneTeacherRemarks = string.Empty;
            _gradeOneFinalNeedsImprovement = string.Empty;
            OnPropertyChanged(nameof(GradeOneFinalNeedsImprovement));
        }
    }
}
