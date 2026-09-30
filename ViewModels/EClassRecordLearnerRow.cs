using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeachFlex.ViewModels
{
    public class AssessmentScoreCell :
        ObservableObject
    {
        private decimal? _score;

        public int AssessmentItemId
        {
            get;
            set;
        }

        public string AssessmentName
        {
            get;
            set;
        } = string.Empty;

        public string Category
        {
            get;
            set;
        } = string.Empty;

        public string AssessmentDomain
        {
            get;
            set;
        } = "General";

        public decimal HighestPossibleScore
        {
            get;
            set;
        }

        public decimal? Score
        {
            get => _score;

            set
            {
                if (SetProperty(
                        ref _score,
                        value))
                {
                    OnPropertyChanged(
                        nameof(IsScoreValid));
                }
            }
        }

        public bool IsScoreValid =>
            !Score.HasValue ||
            (Score.Value >= 0 &&
             Score.Value <=
                HighestPossibleScore);
    }

    public class EClassRecordLearnerRow :
        ObservableObject
    {
        private decimal _writtenWorkPercentage;

        private decimal
            _performanceTaskPercentage;

        private decimal _examinationPercentage;

        private decimal _initialGrade;

        private int _termGrade;

        private string _descriptor =
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

        public ObservableCollection<
            AssessmentScoreCell>
                ScoreCells
        {
            get;
        } = new ObservableCollection<
            AssessmentScoreCell>();

        public decimal WrittenWorkPercentage
        {
            get => _writtenWorkPercentage;

            set => SetProperty(
                ref _writtenWorkPercentage,
                value);
        }

        public decimal PerformanceTaskPercentage
        {
            get =>
                _performanceTaskPercentage;

            set => SetProperty(
                ref _performanceTaskPercentage,
                value);
        }

        public decimal ExaminationPercentage
        {
            get => _examinationPercentage;

            set => SetProperty(
                ref _examinationPercentage,
                value);
        }

        public decimal InitialGrade
        {
            get => _initialGrade;

            set => SetProperty(
                ref _initialGrade,
                value);
        }

        public int TermGrade
        {
            get => _termGrade;

            set => SetProperty(
                ref _termGrade,
                value);
        }

        public string Descriptor
        {
            get => _descriptor;

            set => SetProperty(
                ref _descriptor,
                value);
        }

        public bool HasInvalidScore
        {
            get
            {
                foreach (
                    AssessmentScoreCell cell
                    in ScoreCells)
                {
                    if (!cell.IsScoreValid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}