using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private PaceLearnerRow?
            _selectedPaceLearner;

        private bool _hasUnsavedPaceChanges;
        private bool _paceNavigationConfirmedWhileDirty;
        private string _savedPaceSnapshot = string.Empty;

        public ObservableCollection<PaceLearnerRow>
            PaceLearners
        {
            get;
        } = new ObservableCollection<
            PaceLearnerRow>();

        public IReadOnlyList<string>
            PaceRatingOptions
        {
            get;
        } = new[]
        {
            string.Empty,
            "A",
            "B",
            "C",
            "D",
            "E"
        };

        public PaceLearnerRow?
            SelectedPaceLearner
        {
            get => _selectedPaceLearner;

            set
            {
                if (SetProperty(
                        ref _selectedPaceLearner,
                        value))
                {
                    OnPropertyChanged(
                        nameof(
                            HasSelectedPaceLearner));

                    OnPropertyChanged(
                        nameof(GuardedSelectedPaceLearner));

                    NotifyPaceNavigationCommands();
                }
            }
        }

        public PaceLearnerRow? GuardedSelectedPaceLearner
        {
            get => SelectedPaceLearner;
            set
            {
                if (ReferenceEquals(value, SelectedPaceLearner))
                {
                    return;
                }

                if (!ConfirmPaceNavigationWithUnsavedChanges())
                {
                    OnPropertyChanged(nameof(GuardedSelectedPaceLearner));
                    return;
                }

                SelectedPaceLearner = value;
            }
        }

        public bool HasUnsavedPaceChanges
        {
            get => _hasUnsavedPaceChanges;
            private set => SetProperty(ref _hasUnsavedPaceChanges, value);
        }

        public bool HasSelectedPaceLearner =>
            SelectedPaceLearner != null;

        [RelayCommand]
        private async Task LoadPaceRecordAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy?
                    .UsesDescriptiveGrades != true)
            {
                _dialogService.ShowWarning(
                    "Select a Grade 1 class and learning " +
                    "area first.",
                    "Grade 1 PACE Record");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    $"Loading {SelectedSubject.SubjectName}, " +
                    $"{SelectedTermText} PACE record...";

                IReadOnlyList<PaceCompetency>
                    competencies =
                        await _paceRepository
                            .GetCompetenciesAsync(
                                SelectedSubject.Id,
                                SelectedTerm);

                if (competencies.Count == 0)
                {
                    IReadOnlyList<PaceCompetency>
                        officialCompetencies =
                            _gradeOnePaceCatalogService
                                .CreateOfficialCompetencies(
                                    SelectedSubject.Id,
                                    SelectedSubject.SubjectName);

                    if (officialCompetencies.Count == 0)
                    {
                        PaceLearners.Clear();
                        SelectedPaceLearner =
                            null;

                        _dialogService.ShowWarning(
                            "No official Grade 1 PACE catalog " +
                            "was found for this learning area. " +
                            "Use Language, Reading & Literacy, " +
                            "Mathematics, GMRC, or Makabansa.",
                            "PACE Catalog Not Found");

                        return;
                    }

                    StatusMessage =
                        "Preparing the official Grade 1 " +
                        "PACE competencies...";

                    await _paceRepository
                        .SaveCompetenciesAsync(
                            officialCompetencies);

                    competencies =
                        await _paceRepository
                            .GetCompetenciesAsync(
                                SelectedSubject.Id,
                                SelectedTerm);
                }

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                IReadOnlyList<LearnerPaceRating>
                    savedRatings =
                        await _paceRepository
                            .GetRatingsAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                SelectedTerm);

                IReadOnlyList<LearnerPaceSummary>
                    savedSummaries =
                        await _paceRepository
                            .GetSummariesAsync(
                                SelectedClass.Id,
                                SelectedTerm);

                Dictionary<
                    (int LearnerId,
                     int PaceCompetencyId),
                    LearnerPaceRating>
                        ratingLookup =
                            savedRatings
                                .GroupBy(
                                    rating =>
                                        (
                                            rating.LearnerId,
                                            rating.PaceCompetencyId
                                        ))
                                .ToDictionary(
                                    group =>
                                        group.Key,
                                    group =>
                                        group
                                            .OrderByDescending(
                                                rating =>
                                                    rating.UpdatedAtUtc)
                                            .First());

                Dictionary<int, LearnerPaceSummary>
                    summaryLookup =
                        savedSummaries
                            .GroupBy(
                                summary =>
                                    summary.LearnerId)
                            .ToDictionary(
                                group =>
                                    group.Key,
                                group =>
                                    group
                                        .OrderByDescending(
                                            summary =>
                                                summary.UpdatedAtUtc)
                                        .First());

                ClearPaceRecord();

                int learnerNumber =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    summaryLookup.TryGetValue(
                        learner.Id,
                        out LearnerPaceSummary?
                            savedSummary);

                    PaceLearnerRow learnerRow =
                        new PaceLearnerRow
                        {
                            Number =
                                learnerNumber++,

                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
                                learner.FullName,

                            Sex =
                                learner.Sex,

                            WhatLearnerCanDo =
                                savedSummary?
                                    .WhatLearnerCanDo
                                ?? string.Empty,

                            WhatLearnerNeedsToImprove =
                                savedSummary?
                                    .WhatLearnerNeedsToImprove
                                ?? string.Empty,

                            TeacherRemarks =
                                savedSummary?
                                    .TeacherRemarks
                                ?? string.Empty
                        };

                    learnerRow.PropertyChanged +=
                        PaceLearnerRow_PropertyChanged;

                    foreach (PaceCompetency competency
                             in competencies)
                    {
                        ratingLookup.TryGetValue(
                            (
                                learner.Id,
                                competency.Id
                            ),
                            out LearnerPaceRating?
                                savedRating);

                        PaceRatingRow ratingRow =
                            new PaceRatingRow
                            {
                                PaceCompetencyId =
                                    competency.Id,

                                LearningArea =
                                    competency.LearningArea,

                                DomainName =
                                    competency.DomainName,

                                CompetencyCode =
                                    competency.CompetencyCode,

                                Description =
                                    competency.Description,

                                DisplayOrder =
                                    competency.DisplayOrder,

                                Rating =
                                    savedRating?.Rating
                                    ?? string.Empty,

                                Remarks =
                                    savedRating?.Remarks
                                    ?? string.Empty
                            };

                        ratingRow.PropertyChanged +=
                            (
                                sender,
                                eventArgs) =>
                            {
                                PaceRatingRow_PropertyChanged(
                                    learnerRow,
                                    sender,
                                    eventArgs);
                            };

                        learnerRow.Ratings.Add(
                            ratingRow);
                    }

                    learnerRow.RefreshCompletion();

                    PaceLearners.Add(
                        learnerRow);
                }

                SelectedPaceLearner =
                    PaceLearners.FirstOrDefault();

                CapturePaceSnapshot();
                HasUnsavedPaceChanges = false;
                _paceNavigationConfirmedWhileDirty = false;

                StatusMessage =
                    learners.Count == 0
                        ? "This class has no active learners."
                        : $"{SelectedSubject.SubjectName}, " +
                          $"{SelectedTermText} PACE record " +
                          $"loaded.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    "TeachFlex could not load the Grade 1 " +
                    $"PACE record.\n\n{errorMessage}",
                    "PACE Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        [RelayCommand]
        private async Task SavePaceRecordAsync()
        {
            if (IsBusy ||
                SelectedClass == null ||
                SelectedSubject == null ||
                CurrentPolicy?
                    .UsesDescriptiveGrades != true ||
                PaceLearners.Count == 0)
            {
                return;
            }

            PaceLearnerRow? invalidLearner =
                PaceLearners.FirstOrDefault(
                    learner =>
                        learner.HasInvalidRating);

            if (invalidLearner != null)
            {
                _dialogService.ShowWarning(
                    $"Check the ratings of " +
                    $"{invalidLearner.LearnerName}. " +
                    $"Use A, B, C, D, E, or leave the " +
                    $"rating blank.",
                    "Invalid PACE Rating");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    $"Saving {SelectedSubject.SubjectName}, " +
                    $"{SelectedTermText} PACE record...";

                List<LearnerPaceRating>
                    ratings =
                        PaceLearners
                            .SelectMany(
                                learner =>
                                    learner.Ratings.Select(
                                        rating =>
                                            new LearnerPaceRating
                                            {
                                                LearnerId =
                                                    learner.LearnerId,

                                                PaceCompetencyId =
                                                    rating
                                                        .PaceCompetencyId,

                                                Rating =
                                                    rating.Rating,

                                                Remarks =
                                                    rating.Remarks
                                            }))
                            .ToList();

                await _paceRepository
                    .SaveRatingsAsync(
                        ratings);

                foreach (PaceLearnerRow learner
                         in PaceLearners)
                {
                    learner.RefreshCompletion();
                }

                StatusMessage =
                    $"{SelectedSubject.SubjectName}, " +
                    $"{SelectedTermText} PACE record " +
                    $"saved successfully.";

                CapturePaceSnapshot();
                HasUnsavedPaceChanges = false;
                _paceNavigationConfirmedWhileDirty = false;

                _dialogService.ShowInformation(
                    $"The Grade 1 PACE record for " +
                    $"{SelectedSubject.SubjectName}, " +
                    $"{SelectedTermText} was saved successfully.",
                    "PACE Record Saved");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    "TeachFlex could not save the Grade 1 " +
                    $"PACE record.\n\n{errorMessage}",
                    "Save PACE Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void
            PaceRatingRow_PropertyChanged(
                PaceLearnerRow learnerRow,
                object? sender,
                PropertyChangedEventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(eventArgs.PropertyName) &&
                eventArgs.PropertyName != nameof(PaceRatingRow.Rating) &&
                eventArgs.PropertyName != nameof(PaceRatingRow.Remarks))
            {
                return;
            }

            learnerRow.RefreshCompletion();

            MarkPaceRecordAsChanged();
        }

        private void TryGenerateAutomaticPaceRemarks(
            PaceLearnerRow learnerRow)
        {
            bool isComplete =
                learnerRow.Ratings.Count > 0 &&
                learnerRow.Ratings.All(
                    rating => !string.IsNullOrWhiteSpace(rating.Rating));

            bool remarksAreBlank =
                string.IsNullOrWhiteSpace(learnerRow.WhatLearnerCanDo) &&
                string.IsNullOrWhiteSpace(learnerRow.WhatLearnerNeedsToImprove) &&
                string.IsNullOrWhiteSpace(learnerRow.TeacherRemarks);

            if (isComplete && remarksAreBlank)
            {
                ApplySuggestedPaceRemarks(learnerRow);
                RefreshPaceRemarksDisplay(learnerRow);
            }
        }

        [RelayCommand]
        private void GenerateSuggestedPaceRemarks()
        {
            if (SelectedPaceLearner == null)
            {
                _dialogService.ShowWarning(
                    "Select a learner first.",
                    "Grade 1 PACE Remarks");
                return;
            }

            bool isComplete =
                SelectedPaceLearner.Ratings.Count > 0 &&
                SelectedPaceLearner.Ratings.All(
                    rating => !string.IsNullOrWhiteSpace(rating.Rating));

            if (!isComplete)
            {
                int missing =
                    SelectedPaceLearner.Ratings.Count(
                        rating => string.IsNullOrWhiteSpace(rating.Rating));

                _dialogService.ShowWarning(
                    $"Complete the {missing} blank rating(s) before " +
                    "generating suggested remarks.",
                    "Incomplete PACE Ratings");
                return;
            }

            bool hasExistingRemarks =
                !string.IsNullOrWhiteSpace(
                    SelectedPaceLearner.WhatLearnerCanDo) ||
                !string.IsNullOrWhiteSpace(
                    SelectedPaceLearner.WhatLearnerNeedsToImprove) ||
                !string.IsNullOrWhiteSpace(
                    SelectedPaceLearner.TeacherRemarks);

            if (hasExistingRemarks)
            {
                bool replaceRemarks =
                    _dialogService.Confirm(
                        "May existing remarks na ang learner. Gusto mo bang " +
                        "palitan ang mga ito ng bagong suggested remarks?",
                        "Replace PACE Remarks");

                if (!replaceRemarks)
                {
                    return;
                }
            }

            ApplySuggestedPaceRemarks(SelectedPaceLearner);
            RefreshPaceRemarksDisplay(SelectedPaceLearner);
            MarkPaceRecordAsChanged();

            StatusMessage =
                $"Suggested remarks generated for " +
                $"{SelectedPaceLearner.LearnerName}. Review, edit, then save.";

            _dialogService.ShowInformation(
                "Suggested remarks were generated successfully. " +
                "Review the three remarks fields, then click Save PACE Record.",
                "PACE Remarks Generated");
        }

        private void RefreshPaceRemarksDisplay(
            PaceLearnerRow learnerRow)
        {
            if (!ReferenceEquals(learnerRow, SelectedPaceLearner))
            {
                return;
            }

            // Re-evaluate the selected learner DataContext even when an older
            // PaceLearnerRow implementation does not raise notifications for
            // all three summary properties.
            OnPropertyChanged(nameof(SelectedPaceLearner));
            OnPropertyChanged(nameof(GuardedSelectedPaceLearner));
        }

        private static void ApplySuggestedPaceRemarks(
            PaceLearnerRow learnerRow)
        {
            List<PaceRatingRow> ratedRows =
                learnerRow.Ratings
                    .Where(
                        rating =>
                            !string.IsNullOrWhiteSpace(rating.Rating))
                    .ToList();

            List<string> strengthDomains =
                ratedRows
                    .Where(
                        rating =>
                            rating.Rating == "A" ||
                            rating.Rating == "B")
                    .Select(rating => rating.DomainName)
                    .Where(domain => !string.IsNullOrWhiteSpace(domain))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList();

            if (strengthDomains.Count == 0)
            {
                strengthDomains =
                    ratedRows
                        .OrderByDescending(
                            rating => PaceRatingScore(rating.Rating))
                        .Select(rating => rating.DomainName)
                        .Where(domain => !string.IsNullOrWhiteSpace(domain))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(2)
                        .ToList();
            }

            List<string> improvementDomains =
                ratedRows
                    .Where(
                        rating =>
                            rating.Rating == "D" ||
                            rating.Rating == "E")
                    .Select(rating => rating.DomainName)
                    .Where(domain => !string.IsNullOrWhiteSpace(domain))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(3)
                    .ToList();

            if (improvementDomains.Count == 0)
            {
                improvementDomains =
                    ratedRows
                        .Where(rating => rating.Rating == "C")
                        .Select(rating => rating.DomainName)
                        .Where(domain => !string.IsNullOrWhiteSpace(domain))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(2)
                        .ToList();
            }

            string strengths =
                FormatPaceDomainList(strengthDomains);

            string improvements =
                FormatPaceDomainList(improvementDomains);

            learnerRow.WhatLearnerCanDo =
                strengthDomains.Count > 0
                    ? "Naipamamalas ng mag-aaral ang mahusay na " +
                      $"pag-unawa at kasanayan sa {strengths}."
                    : "Naipamamalas ng mag-aaral ang patuloy na " +
                      "pag-unlad sa mga kasanayang tinalakay sa term na ito.";

            learnerRow.WhatLearnerNeedsToImprove =
                improvementDomains.Count > 0
                    ? "Patuloy na pagsasanay at paggabay ang kailangan " +
                      $"upang higit na mapaunlad ang {improvements}."
                    : "Ipagpatuloy ang regular na pagsasanay upang " +
                      "mapanatili at higit pang mapaunlad ang mga natutuhan.";

            double averageScore =
                ratedRows.Count == 0
                    ? 0
                    : ratedRows.Average(
                        rating => PaceRatingScore(rating.Rating));

            learnerRow.TeacherRemarks =
                averageScore >= 4.50
                    ? "Napakahusay ng ipinakitang pagkatuto. Ipagpatuloy " +
                      "ang masigasig at aktibong pakikilahok sa klase."
                : averageScore >= 3.50
                    ? "Mahusay ang pag-unlad at patuloy na naipamamalas " +
                      "ang mga inaasahang kasanayan."
                : averageScore >= 2.50
                    ? "Patuloy ang pag-unlad. Hinihikayat ang regular na " +
                      "pagsasanay at aktibong pakikilahok sa klase."
                : averageScore >= 1.50
                    ? "Nangangailangan pa ng karagdagang pagsasanay at " +
                      "gabay upang mapaunlad ang mga pangunahing kasanayan."
                    : "Kailangan ng masusing paggabay, tuloy-tuloy na " +
                      "pagsasanay, at pakikipagtulungan ng paaralan at tahanan.";
        }

        private static int PaceRatingScore(string? rating)
        {
            return rating?.Trim().ToUpperInvariant() switch
            {
                "A" => 5,
                "B" => 4,
                "C" => 3,
                "D" => 2,
                "E" => 1,
                _ => 0
            };
        }

        private static string FormatPaceDomainList(
            IReadOnlyList<string> domains)
        {
            if (domains.Count == 0)
            {
                return string.Empty;
            }

            if (domains.Count == 1)
            {
                return domains[0];
            }

            if (domains.Count == 2)
            {
                return $"{domains[0]} at {domains[1]}";
            }

            return $"{string.Join(", ", domains.Take(domains.Count - 1))}, " +
                   $"at {domains[^1]}";
        }

        private void PaceLearnerRow_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs.PropertyName) ||
                eventArgs.PropertyName == nameof(PaceLearnerRow.WhatLearnerCanDo) ||
                eventArgs.PropertyName == nameof(PaceLearnerRow.WhatLearnerNeedsToImprove) ||
                eventArgs.PropertyName == nameof(PaceLearnerRow.TeacherRemarks))
            {
                MarkPaceRecordAsChanged();
            }
        }

        private void MarkPaceRecordAsChanged()
        {
            HasUnsavedPaceChanges = true;
            _paceNavigationConfirmedWhileDirty = false;
        }

        private bool ConfirmPaceNavigationWithUnsavedChanges()
        {
            HasUnsavedPaceChanges =
                HasUnsavedPaceChanges || HasPaceRecordChanged();

            if (!HasUnsavedPaceChanges ||
                _paceNavigationConfirmedWhileDirty)
            {
                return true;
            }

            bool continueWithoutSaving =
                _dialogService.Confirm(
                    "May mga pagbabago sa Grade 1 PACE record na hindi " +
                    "pa nai-save. Gusto mo bang lumipat ng learner nang " +
                    "hindi muna nagse-save?",
                    "Unsaved PACE Changes");

            if (continueWithoutSaving)
            {
                _paceNavigationConfirmedWhileDirty = true;
            }

            return continueWithoutSaving;
        }

        private bool ConfirmDiscardPaceChanges()
        {
            HasUnsavedPaceChanges =
                HasUnsavedPaceChanges || HasPaceRecordChanged();

            if (!HasUnsavedPaceChanges)
            {
                return true;
            }

            return _dialogService.Confirm(
                "May mga pagbabago sa Grade 1 PACE record na hindi pa " +
                "nai-save. Gusto mo bang i-reload at i-discard ang mga ito?",
                "Discard Unsaved PACE Changes");
        }

        [RelayCommand]
        private async Task ReloadGuardedPaceRecordAsync()
        {
            if (!ConfirmDiscardPaceChanges())
            {
                return;
            }

            await LoadPaceRecordCommand.ExecuteAsync(null);
        }

        public string PaceLearnerPositionText
        {
            get
            {
                if (SelectedPaceLearner == null || PaceLearners.Count == 0)
                {
                    return "No learner selected";
                }

                int index = PaceLearners.IndexOf(SelectedPaceLearner);

                return index < 0
                    ? "No learner selected"
                    : $"Learner {index + 1} of {PaceLearners.Count}";
            }
        }

        public bool HasPreviousPaceLearner =>
            !IsBusy &&
            SelectedPaceLearner != null &&
            PaceLearners.IndexOf(SelectedPaceLearner) > 0;

        [RelayCommand]
        private void MoveToPreviousPaceLearner()
        {
            int index = PaceLearners.IndexOf(SelectedPaceLearner!);

            if (index > 0)
            {
                GuardedSelectedPaceLearner = PaceLearners[index - 1];
            }
        }

        public bool HasNextPaceLearner
        {
            get
            {
                if (IsBusy || SelectedPaceLearner == null)
                {
                    return false;
                }

                int index = PaceLearners.IndexOf(SelectedPaceLearner);
                return index >= 0 && index < PaceLearners.Count - 1;
            }
        }

        [RelayCommand]
        private void MoveToNextPaceLearner()
        {
            int index = PaceLearners.IndexOf(SelectedPaceLearner!);

            if (index >= 0 && index < PaceLearners.Count - 1)
            {
                GuardedSelectedPaceLearner = PaceLearners[index + 1];
            }
        }

        private void NotifyPaceNavigationCommands()
        {
            OnPropertyChanged(nameof(PaceLearnerPositionText));
            OnPropertyChanged(nameof(HasPreviousPaceLearner));
            OnPropertyChanged(nameof(HasNextPaceLearner));
        }

        private void CapturePaceSnapshot()
        {
            _savedPaceSnapshot = CreatePaceSnapshot();
        }

        private bool HasPaceRecordChanged()
        {
            if (PaceLearners.Count == 0 ||
                string.IsNullOrEmpty(_savedPaceSnapshot))
            {
                return HasUnsavedPaceChanges;
            }

            return !string.Equals(
                _savedPaceSnapshot,
                CreatePaceSnapshot(),
                StringComparison.Ordinal);
        }

        private string CreatePaceSnapshot()
        {
            return string.Join(
                "\u001D",
                PaceLearners.Select(
                    learner =>
                        $"{learner.LearnerId}\u001F" +
                        $"{learner.WhatLearnerCanDo ?? string.Empty}\u001F" +
                        $"{learner.WhatLearnerNeedsToImprove ?? string.Empty}\u001F" +
                        $"{learner.TeacherRemarks ?? string.Empty}\u001F" +
                        string.Join(
                            "\u001E",
                            learner.Ratings.Select(
                                rating =>
                                    $"{rating.PaceCompetencyId}\u001F" +
                                    $"{rating.Rating ?? string.Empty}\u001F" +
                                    $"{rating.Remarks ?? string.Empty}"))));
        }

        private void ClearPaceRecord()
        {
            foreach (PaceLearnerRow learner in PaceLearners)
            {
                learner.PropertyChanged -= PaceLearnerRow_PropertyChanged;
            }

            PaceLearners.Clear();

            SelectedPaceLearner =
                null;

            HasUnsavedPaceChanges = false;
            _paceNavigationConfirmedWhileDirty = false;
            _savedPaceSnapshot = string.Empty;
        }
    }
}
