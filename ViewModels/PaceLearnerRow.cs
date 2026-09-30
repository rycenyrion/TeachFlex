using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeachFlex.ViewModels
{
    public class PaceLearnerRow :
        ObservableObject
    {
        private string
            _whatLearnerCanDo =
                string.Empty;

        private string
            _whatLearnerNeedsToImprove =
                string.Empty;

        private string _teacherRemarks =
            string.Empty;

        public int Number
        {
            get;
            set;
        }

        public int LearnerId
        {
            get;
            set;
        }

        public string Lrn
        {
            get;
            set;
        } = string.Empty;

        public string LearnerName
        {
            get;
            set;
        } = string.Empty;

        public string Sex
        {
            get;
            set;
        } = string.Empty;

        public ObservableCollection<PaceRatingRow>
            Ratings
        {
            get;
        } = new ObservableCollection<
            PaceRatingRow>();

        public string WhatLearnerCanDo
        {
            get => _whatLearnerCanDo;

            set => SetProperty(
                ref _whatLearnerCanDo,
                value ?? string.Empty);
        }

        public string WhatLearnerNeedsToImprove
        {
            get =>
                _whatLearnerNeedsToImprove;

            set => SetProperty(
                ref _whatLearnerNeedsToImprove,
                value ?? string.Empty);
        }

        public string TeacherRemarks
        {
            get => _teacherRemarks;

            set => SetProperty(
                ref _teacherRemarks,
                value ?? string.Empty);
        }

        public int RatedCompetencyCount
        {
            get
            {
                int count =
                    0;

                foreach (PaceRatingRow rating
                         in Ratings)
                {
                    if (!string.IsNullOrWhiteSpace(
                            rating.Rating))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int TotalCompetencyCount =>
            Ratings.Count;

        public string CompletionText =>
            $"{RatedCompetencyCount} of " +
            $"{TotalCompetencyCount} rated";

        public bool HasInvalidRating
        {
            get
            {
                foreach (PaceRatingRow rating
                         in Ratings)
                {
                    if (!rating.IsRatingValid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void RefreshCompletion()
        {
            OnPropertyChanged(
                nameof(
                    RatedCompetencyCount));

            OnPropertyChanged(
                nameof(
                    TotalCompetencyCount));

            OnPropertyChanged(
                nameof(
                    CompletionText));
        }
    }
}