using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private int _kindergartenRatedCount;
        private int _kindergartenTotalCount;
        private bool _hasUnsavedKindergartenChanges;
        private string _savedKindergartenSnapshot =
            string.Empty;

        public bool HasUnsavedKindergartenChanges
        {
            get => _hasUnsavedKindergartenChanges;
            private set => SetProperty(
                ref _hasUnsavedKindergartenChanges,
                value);
        }

        public Learner? GuardedSelectedKindergartenLearner
        {
            get => SelectedKindergartenLearner;
            set
            {
                if (ReferenceEquals(
                        value,
                        SelectedKindergartenLearner))
                {
                    return;
                }

                if (!ConfirmDiscardKindergartenChanges())
                {
                    OnPropertyChanged(
                        nameof(GuardedSelectedKindergartenLearner));
                    return;
                }

                SelectedKindergartenLearner = value;
            }
        }

        public SchoolClass? GuardedSelectedKindergartenClass
        {
            get => SelectedClass;
            set
            {
                if (ReferenceEquals(value, SelectedClass))
                {
                    return;
                }

                if (IsKindergartenRecord &&
                    !ConfirmDiscardKindergartenChanges())
                {
                    OnPropertyChanged(
                        nameof(GuardedSelectedKindergartenClass));
                    return;
                }

                SelectedClass = value;
            }
        }

        public int GuardedSelectedKindergartenTerm
        {
            get => SelectedTerm;
            set
            {
                if (value == SelectedTerm)
                {
                    return;
                }

                if (IsKindergartenRecord &&
                    !ConfirmDiscardKindergartenChanges())
                {
                    OnPropertyChanged(
                        nameof(GuardedSelectedKindergartenTerm));
                    return;
                }

                SelectedTerm = value;
            }
        }

        public int KindergartenRatedCount
        {
            get => _kindergartenRatedCount;
            private set
            {
                if (SetProperty(ref _kindergartenRatedCount, value))
                {
                    OnPropertyChanged(nameof(KindergartenCompletionText));
                    OnPropertyChanged(nameof(IsKindergartenRatingComplete));
                }
            }
        }

        public int KindergartenTotalCount
        {
            get => _kindergartenTotalCount;
            private set
            {
                if (SetProperty(ref _kindergartenTotalCount, value))
                {
                    OnPropertyChanged(nameof(KindergartenCompletionText));
                    OnPropertyChanged(nameof(IsKindergartenRatingComplete));
                }
            }
        }

        public string KindergartenCompletionText =>
            $"{KindergartenRatedCount} of {KindergartenTotalCount} rated";

        public bool IsKindergartenRatingComplete =>
            KindergartenTotalCount > 0 &&
            KindergartenRatedCount == KindergartenTotalCount;

        private void InitializeKindergartenCompletionTracking()
        {
            KindergartenRatingRows.CollectionChanged +=
                KindergartenRatingRows_CollectionChanged;

            KindergartenLearners.CollectionChanged +=
                KindergartenLearners_CollectionChanged;

            PropertyChanged +=
                KindergartenNavigation_PropertyChanged;

            AttachKindergartenRatingRowHandlers();
            UpdateKindergartenCompletion();
            NotifyKindergartenNavigationCommands();
        }

        private void KindergartenLearners_CollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            NotifyKindergartenNavigationCommands();
        }

        private void KindergartenNavigation_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == nameof(SelectedKindergartenLearner) ||
                e.PropertyName == nameof(IsBusy))
            {
                NotifyKindergartenNavigationCommands();
            }

            if (e.PropertyName == nameof(SelectedKindergartenLearner))
            {
                OnPropertyChanged(
                    nameof(GuardedSelectedKindergartenLearner));
            }

            if (e.PropertyName == nameof(SelectedClass))
            {
                OnPropertyChanged(
                    nameof(GuardedSelectedKindergartenClass));
            }

            if (e.PropertyName == nameof(SelectedTerm))
            {
                OnPropertyChanged(
                    nameof(GuardedSelectedKindergartenTerm));
            }

            if (e.PropertyName == nameof(KindergartenTeacherComment) ||
                e.PropertyName == nameof(KindergartenLearnerStrengths) ||
                e.PropertyName == nameof(KindergartenSuggestedInterventions))
            {
                HasUnsavedKindergartenChanges = true;
            }

            if (e.PropertyName == nameof(StatusMessage))
            {
                string status = StatusMessage ?? string.Empty;

                if (status.Contains(
                        "record loaded",
                        StringComparison.OrdinalIgnoreCase) ||
                    status.Contains(
                        "record saved",
                        StringComparison.OrdinalIgnoreCase))
                {
                    CaptureKindergartenSnapshot();
                    HasUnsavedKindergartenChanges = false;
                }
            }
        }

        public string KindergartenLearnerPositionText
        {
            get
            {
                if (SelectedKindergartenLearner == null ||
                    KindergartenLearners.Count == 0)
                {
                    return "No learner selected";
                }

                int index =
                    KindergartenLearners.IndexOf(
                        SelectedKindergartenLearner);

                return index < 0
                    ? "No learner selected"
                    : $"Learner {index + 1} of {KindergartenLearners.Count}";
            }
        }

        private bool CanMoveToPreviousKindergartenLearner()
        {
            if (IsBusy || SelectedKindergartenLearner == null)
            {
                return false;
            }

            return KindergartenLearners.IndexOf(
                       SelectedKindergartenLearner) > 0;
        }

        [RelayCommand(CanExecute = nameof(CanMoveToPreviousKindergartenLearner))]
        private void MoveToPreviousKindergartenLearner()
        {
            int index =
                KindergartenLearners.IndexOf(
                    SelectedKindergartenLearner!);

            if (index > 0)
            {
                GuardedSelectedKindergartenLearner =
                    KindergartenLearners[index - 1];
            }
        }

        private bool CanMoveToNextKindergartenLearner()
        {
            if (IsBusy || SelectedKindergartenLearner == null)
            {
                return false;
            }

            int index =
                KindergartenLearners.IndexOf(
                    SelectedKindergartenLearner);

            return index >= 0 &&
                   index < KindergartenLearners.Count - 1;
        }

        [RelayCommand(CanExecute = nameof(CanMoveToNextKindergartenLearner))]
        private void MoveToNextKindergartenLearner()
        {
            int index =
                KindergartenLearners.IndexOf(
                    SelectedKindergartenLearner!);

            if (index >= 0 &&
                index < KindergartenLearners.Count - 1)
            {
                GuardedSelectedKindergartenLearner =
                    KindergartenLearners[index + 1];
            }
        }

        private bool ConfirmDiscardKindergartenChanges()
        {
            HasUnsavedKindergartenChanges =
                HasUnsavedKindergartenChanges ||
                HasKindergartenRecordChanged();

            if (!HasUnsavedKindergartenChanges)
            {
                return true;
            }

            bool continueWithoutSaving =
                _dialogService.Confirm(
                    "May mga pagbabago sa Kindergarten record na hindi pa " +
                    "nai-save. Gusto mo bang magpatuloy at i-discard ang " +
                    "mga pagbabagong ito?",
                    "Unsaved Kindergarten Changes");

            if (continueWithoutSaving)
            {
                HasUnsavedKindergartenChanges = false;
            }

            return continueWithoutSaving;
        }

        private void CaptureKindergartenSnapshot()
        {
            _savedKindergartenSnapshot =
                CreateKindergartenSnapshot();
        }

        private bool HasKindergartenRecordChanged()
        {
            if (KindergartenRatingRows.Count == 0 ||
                string.IsNullOrEmpty(_savedKindergartenSnapshot))
            {
                return HasUnsavedKindergartenChanges;
            }

            return !string.Equals(
                _savedKindergartenSnapshot,
                CreateKindergartenSnapshot(),
                StringComparison.Ordinal);
        }

        private string CreateKindergartenSnapshot()
        {
            string ratingSnapshot =
                string.Join(
                    "\u001E",
                    KindergartenRatingRows.Select(
                        row =>
                            $"{row.KindergartenCompetencyId}\u001F" +
                            $"{row.Rating ?? string.Empty}\u001F" +
                            $"{row.Observation ?? string.Empty}"));

            return ratingSnapshot +
                   "\u001D" +
                   (KindergartenTeacherComment ?? string.Empty) +
                   "\u001D" +
                   (KindergartenLearnerStrengths ?? string.Empty) +
                   "\u001D" +
                   (KindergartenSuggestedInterventions ?? string.Empty);
        }

        [RelayCommand]
        private async Task ReloadGuardedKindergartenRecordAsync()
        {
            if (!ConfirmDiscardKindergartenChanges())
            {
                return;
            }

            await LoadKindergartenRecordCommand.ExecuteAsync(null);
        }

        private void NotifyKindergartenNavigationCommands()
        {
            OnPropertyChanged(
                nameof(KindergartenLearnerPositionText));

            MoveToPreviousKindergartenLearnerCommand
                .NotifyCanExecuteChanged();

            MoveToNextKindergartenLearnerCommand
                .NotifyCanExecuteChanged();
        }

        private void KindergartenRatingRows_CollectionChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    if (item is INotifyPropertyChanged oldRow)
                    {
                        oldRow.PropertyChanged -=
                            KindergartenRatingRow_PropertyChanged;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    if (item is INotifyPropertyChanged newRow)
                    {
                        newRow.PropertyChanged +=
                            KindergartenRatingRow_PropertyChanged;
                    }
                }
            }

            UpdateKindergartenCompletion();

            // The Kinder record is populated asynchronously, one competency
            // row at a time. Keep a baseline while rows are being loaded so
            // change detection still works even when the final status text
            // does not raise another PropertyChanged event.
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                CaptureKindergartenSnapshot();
                HasUnsavedKindergartenChanges = false;
            }
        }

        private void AttachKindergartenRatingRowHandlers()
        {
            foreach (object item in KindergartenRatingRows)
            {
                if (item is INotifyPropertyChanged row)
                {
                    row.PropertyChanged -=
                        KindergartenRatingRow_PropertyChanged;
                    row.PropertyChanged +=
                        KindergartenRatingRow_PropertyChanged;
                }
            }
        }

        private void KindergartenRatingRow_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                e.PropertyName == "Rating" ||
                e.PropertyName == "Observation")
            {
                HasUnsavedKindergartenChanges = true;
                UpdateKindergartenCompletion();
            }
        }

        private void UpdateKindergartenCompletion()
        {
            KindergartenTotalCount =
                KindergartenRatingRows.Count;

            KindergartenRatedCount =
                KindergartenRatingRows.Count(
                    row => !string.IsNullOrWhiteSpace(row.Rating));

            SaveValidatedKindergartenRecordCommand
                .NotifyCanExecuteChanged();

            ExportValidatedKindergartenPdfCommand
                .NotifyCanExecuteChanged();

        }

        [RelayCommand]
        private void GenerateKindergartenRemarks()
        {
            var ratedRows =
                KindergartenRatingRows
                    .Where(
                        row => !string.IsNullOrWhiteSpace(row.Rating))
                    .ToList();

            if (ratedRows.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Enter at least one BG, DV, or CO rating before " +
                    "generating remarks.",
                    "Kinder Remarks");
                return;
            }

            bool hasExistingRemarks =
                !string.IsNullOrWhiteSpace(KindergartenTeacherComment) ||
                !string.IsNullOrWhiteSpace(KindergartenLearnerStrengths) ||
                !string.IsNullOrWhiteSpace(
                    KindergartenSuggestedInterventions);

            if (hasExistingRemarks &&
                !_dialogService.Confirm(
                    "May existing remarks na ang learner. Palitan ba ang " +
                    "mga ito ng bagong suggestions batay sa kasalukuyang " +
                    "ratings?",
                    "Replace Kinder Remarks"))
            {
                return;
            }

            int consistentCount =
                ratedRows.Count(
                    row => string.Equals(
                        row.Rating,
                        "CO",
                        StringComparison.OrdinalIgnoreCase));

            int developingCount =
                ratedRows.Count(
                    row => string.Equals(
                        row.Rating,
                        "DV",
                        StringComparison.OrdinalIgnoreCase));

            int beginningCount =
                ratedRows.Count(
                    row => string.Equals(
                        row.Rating,
                        "BG",
                        StringComparison.OrdinalIgnoreCase));

            string strongestArea =
                ratedRows
                    .GroupBy(row => row.DevelopmentArea)
                    .OrderByDescending(
                        group => group.Count(
                            row => string.Equals(
                                row.Rating,
                                "CO",
                                StringComparison.OrdinalIgnoreCase)))
                    .ThenByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .FirstOrDefault()
                ?? "the assessed development areas";

            string priorityArea =
                ratedRows
                    .GroupBy(row => row.DevelopmentArea)
                    .OrderByDescending(
                        group => group.Count(
                            row => string.Equals(
                                row.Rating,
                                "BG",
                                StringComparison.OrdinalIgnoreCase)))
                    .ThenByDescending(
                        group => group.Count(
                            row => string.Equals(
                                row.Rating,
                                "DV",
                                StringComparison.OrdinalIgnoreCase)))
                    .Select(group => group.Key)
                    .FirstOrDefault()
                ?? "the skills still being developed";

            KindergartenTeacherComment =
                $"Based on {ratedRows.Count} rated competencies, the learner " +
                $"has {consistentCount} Consistent, {developingCount} " +
                $"Developing, and {beginningCount} Beginning ratings. " +
                "Continued observation and varied learning experiences are " +
                "recommended.";

            KindergartenLearnerStrengths =
                consistentCount > 0
                    ? $"The learner shows notable strengths in {strongestArea} " +
                      "and responds positively to developmentally appropriate " +
                      "learning activities."
                    : "The learner participates in classroom activities and " +
                      "is beginning to demonstrate the assessed competencies.";

            KindergartenSuggestedInterventions =
                beginningCount + developingCount > 0
                    ? $"Provide regular guided practice, modeling, and " +
                      $"play-based activities focusing on {priorityArea}. " +
                      "Coordinate simple follow-up activities with the family " +
                      "and monitor progress consistently."
                    : "Continue providing enrichment, varied play-based " +
                      "activities, and opportunities to apply skills " +
                      "independently in different situations.";

            HasUnsavedKindergartenChanges = true;
        }

        private bool HasSelectedKindergartenRecord()
        {
            return !IsBusy &&
                   IsKindergartenRecord &&
                   SelectedClass != null &&
                   SelectedKindergartenLearner != null &&
                   KindergartenRatingRows.Count > 0;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedKindergartenRecord))]
        private async Task SaveValidatedKindergartenRecordAsync()
        {
            UpdateKindergartenCompletion();

            await SaveKindergartenRecordCommand.ExecuteAsync(null);
        }

        private bool CanExportValidatedKindergartenPdf()
        {
            return HasSelectedKindergartenRecord() &&
                   IsKindergartenRatingComplete;
        }

        [RelayCommand(CanExecute = nameof(CanExportValidatedKindergartenPdf))]
        private async Task ExportValidatedKindergartenPdfAsync()
        {
            UpdateKindergartenCompletion();

            if (!IsKindergartenRatingComplete)
            {
                _dialogService.ShowWarning(
                    "Complete all Kindergarten ratings before exporting the PDF.",
                    "Incomplete Kinder Ratings");

                return;
            }

            await ExportKindergartenSummaryPdfCommand.ExecuteAsync(null);
        }
    }
}
