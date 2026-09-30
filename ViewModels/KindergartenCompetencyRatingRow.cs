using CommunityToolkit.Mvvm.ComponentModel;

namespace TeachFlex.ViewModels
{
    public class KindergartenCompetencyRatingRow :
        ObservableObject
    {
        private string _rating =
            string.Empty;

        private string _observation =
            string.Empty;

        public int KindergartenCompetencyId
        {
            get;
            set;
        }

        public string CompetencyCode
        {
            get;
            set;
        } = string.Empty;

        public string DevelopmentArea
        {
            get;
            set;
        } = string.Empty;

        public string SubDomain
        {
            get;
            set;
        } = string.Empty;

        public string Description
        {
            get;
            set;
        } = string.Empty;

        public int DisplayOrder
        {
            get;
            set;
        }

        public string Rating
        {
            get => _rating;

            set
            {
                if (SetProperty(
                        ref _rating,
                        value))
                {
                    OnPropertyChanged(
                        nameof(RatingDescription));

                    OnPropertyChanged(
                        nameof(IsRated));
                }
            }
        }

        public string Observation
        {
            get => _observation;

            set => SetProperty(
                ref _observation,
                value);
        }

        public bool IsRated =>
            !string.IsNullOrWhiteSpace(
                Rating);

        public string RatingDescription =>
            Rating switch
            {
                "BG" => "Beginning",
                "DV" => "Developing",
                "CO" => "Consistent",
                _ => "Not Rated"
            };

        public string DisplayDescription =>
            $"{CompetencyCode} — {Description}";
    }
}