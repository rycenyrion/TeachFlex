using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private Learner?
            _selectedKindergartenLearner;

        private string
            _kindergartenTeacherComment =
                string.Empty;

        private string
            _kindergartenLearnerStrengths =
                string.Empty;

        private string
            _kindergartenSuggestedInterventions =
                string.Empty;

        public ObservableCollection<Learner>
            KindergartenLearners
        {
            get;
        } = new ObservableCollection<Learner>();

        public ObservableCollection<
            KindergartenCompetencyRatingRow>
                KindergartenRatingRows
        {
            get;
        } = new ObservableCollection<
            KindergartenCompetencyRatingRow>();

        public IReadOnlyList<string>
            KindergartenRatingOptions
        {
            get;
        } = new[]
        {
            string.Empty,
            "BG",
            "DV",
            "CO"
        };

        public bool IsKindergartenRecord =>
            SelectedClass?.GradeLevel
                .Equals(
                    "Kindergarten",
                    StringComparison.OrdinalIgnoreCase)
            == true;

        public Learner?
            SelectedKindergartenLearner
        {
            get =>
                _selectedKindergartenLearner;

            set
            {
                if (!SetProperty(
                        ref _selectedKindergartenLearner,
                        value))
                {
                    return;
                }

                _ = LoadKindergartenLearnerRecordAsync();
            }
        }

        public string KindergartenTeacherComment
        {
            get =>
                _kindergartenTeacherComment;

            set => SetProperty(
                ref _kindergartenTeacherComment,
                value);
        }

        public string KindergartenLearnerStrengths
        {
            get =>
                _kindergartenLearnerStrengths;

            set => SetProperty(
                ref _kindergartenLearnerStrengths,
                value);
        }

        public string
            KindergartenSuggestedInterventions
        {
            get =>
                _kindergartenSuggestedInterventions;

            set => SetProperty(
                ref _kindergartenSuggestedInterventions,
                value);
        }

        private async Task
            LoadKindergartenClassAsync()
        {
            KindergartenLearners.Clear();
            KindergartenRatingRows.Clear();

            SelectedKindergartenLearner =
                null;

            ClearKindergartenRemarks();

            if (!IsKindergartenRecord ||
                SelectedClass == null)
            {
                return;
            }

            IReadOnlyList<Learner> learners =
                await _learnerRepository
                    .GetByClassAsync(
                        SelectedClass.Id);

            IEnumerable<Learner> orderedLearners =
     learners
         .OrderBy(
             learner =>
                 IsMaleKindergartenLearner(
                     learner.Sex)
                     ? 0
                     : 1)
         .ThenBy(
             learner =>
                 learner.LastName)
         .ThenBy(
             learner =>
                 learner.FirstName)
         .ThenBy(
             learner =>
                 learner.MiddleName);

            foreach (Learner learner
                     in orderedLearners)
            {
                KindergartenLearners.Add(
                    learner);
            }

            SelectedKindergartenLearner =
                KindergartenLearners
                    .FirstOrDefault();

            StatusMessage =
                KindergartenLearners.Count == 0
                    ? "This Kindergarten class has no active learners."
                    : "Select a learner and enter BG, DV, or CO ratings.";
        }

        [RelayCommand]
        private async Task
            LoadKindergartenRecordAsync()
        {
            if (!IsKindergartenRecord)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class first.",
                    "Kinder E-Class Record");

                return;
            }

            if (SelectedKindergartenLearner == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten learner first.",
                    "Kinder E-Class Record");

                return;
            }

            await LoadKindergartenLearnerRecordAsync();
        }

        private async Task
            LoadKindergartenLearnerRecordAsync()
        {
            KindergartenRatingRows.Clear();
            ClearKindergartenRemarks();

            if (!IsKindergartenRecord ||
                SelectedClass == null ||
                SelectedKindergartenLearner == null)
            {
                return;
            }

            try
            {
                StatusMessage =
                    "Loading Kinder E-Class Record...";

                IReadOnlyList<
                    KindergartenCompetency>
                        competencies =
                            await _kindergartenRecordRepository
                                .GetCompetenciesAsync();

                IReadOnlyList<
                    KindergartenLearnerRating>
                        savedRatings =
                            await _kindergartenRecordRepository
                                .GetRatingsAsync(
                                    SelectedClass.Id,
                                    SelectedTerm);

                Dictionary<int,
                    KindergartenLearnerRating>
                        ratingLookup =
                            savedRatings
                                .Where(
                                    rating =>
                                        rating.LearnerId ==
                                        SelectedKindergartenLearner.Id)
                                .ToDictionary(
                                    rating =>
                                        rating
                                            .KindergartenCompetencyId);

                foreach (
                    KindergartenCompetency competency
                    in competencies)
                {
                    ratingLookup.TryGetValue(
                        competency.Id,
                        out KindergartenLearnerRating?
                            savedRating);

                    KindergartenRatingRows.Add(
                        new
                        KindergartenCompetencyRatingRow
                        {
                            KindergartenCompetencyId =
                                competency.Id,

                            CompetencyCode =
                                competency.CompetencyCode,

                            DevelopmentArea =
                                competency.DevelopmentArea,

                            SubDomain =
                                competency.SubDomain,

                            Description =
                                competency.Description,

                            DisplayOrder =
                                competency.DisplayOrder,

                            Rating =
                                savedRating?.Rating
                                ?? string.Empty,

                            Observation =
                                savedRating?.Observation
                                ?? string.Empty
                        });
                }

                KindergartenTermRemark?
                    savedRemark =
                        await _kindergartenRecordRepository
                            .GetTermRemarkAsync(
                                SelectedClass.Id,
                                SelectedKindergartenLearner.Id,
                                SelectedTerm);

                KindergartenTeacherComment =
                    savedRemark?.TeacherComment
                    ?? string.Empty;

                KindergartenLearnerStrengths =
                    savedRemark?.LearnerStrengths
                    ?? string.Empty;

                KindergartenSuggestedInterventions =
                    savedRemark?
                        .SuggestedInterventions
                    ?? string.Empty;

                StatusMessage =
                    $"Kinder {SelectedTermText} record loaded for " +
                    $"{SelectedKindergartenLearner.FullName}.";
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    "TeachFlex could not load the Kinder " +
                    "E-Class Record.\n\n" +
                    exception.Message,
                    "Kinder E-Class Record Error");
            }
        }

        [RelayCommand]
        private async Task
            SaveKindergartenRecordAsync()
        {
            if (!IsKindergartenRecord ||
                SelectedClass == null ||
                SelectedKindergartenLearner == null)
            {
                _dialogService.ShowWarning(
                    "Select a Kindergarten class and learner first.",
                    "Kinder E-Class Record");

                return;
            }

            if (KindergartenRatingRows.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Kinder E-Class Record first.",
                    "Kinder E-Class Record");

                return;
            }

            try
            {
                IsBusy =
                    true;

                List<KindergartenLearnerRating>
                    ratings =
                        KindergartenRatingRows
                            .Select(
                                row =>
                                    new KindergartenLearnerRating
                                    {
                                        SchoolClassId =
                                            SelectedClass.Id,

                                        LearnerId =
                                            SelectedKindergartenLearner.Id,

                                        KindergartenCompetencyId =
                                            row.KindergartenCompetencyId,

                                        TermNumber =
                                            SelectedTerm,

                                        Rating =
                                            row.Rating,

                                        Observation =
                                            row.Observation
                                    })
                            .ToList();

                await _kindergartenRecordRepository
                    .SaveRatingsAsync(
                        ratings);

                await _kindergartenRecordRepository
                    .SaveTermRemarkAsync(
                        new KindergartenTermRemark
                        {
                            SchoolClassId =
                                SelectedClass.Id,

                            LearnerId =
                                SelectedKindergartenLearner.Id,

                            TermNumber =
                                SelectedTerm,

                            TeacherComment =
                                KindergartenTeacherComment,

                            LearnerStrengths =
                                KindergartenLearnerStrengths,

                            SuggestedInterventions =
                                KindergartenSuggestedInterventions
                        });

                StatusMessage =
                    $"Kinder {SelectedTermText} record saved for " +
                    $"{SelectedKindergartenLearner.FullName}.";

                _dialogService.ShowInformation(
                    "The Kinder E-Class Record was saved successfully.",
                    "Kinder E-Class Record");
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    "TeachFlex could not save the Kinder " +
                    "E-Class Record.\n\n" +
                    exception.Message,
                    "Kinder E-Class Record Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private void ClearKindergartenRemarks()
        {
            KindergartenTeacherComment =
                string.Empty;

            KindergartenLearnerStrengths =
                string.Empty;

            KindergartenSuggestedInterventions =
                string.Empty;
        }

        private static bool
            IsMaleKindergartenLearner(
                string sex)
        {
            string normalized =
                sex?
                    .Trim()
                    .ToLowerInvariant()
                ?? string.Empty;

            return normalized == "male" ||
                   normalized == "m" ||
                   normalized == "lalaki";
        }
    }
}