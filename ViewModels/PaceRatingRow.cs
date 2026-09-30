using CommunityToolkit.Mvvm.ComponentModel;

namespace TeachFlex.ViewModels
{
    public class PaceRatingRow :
        ObservableObject
    {
        private string _rating =
            string.Empty;

        private string _remarks =
            string.Empty;

        public int PaceCompetencyId
        {
            get;
            set;
        }

        public string LearningArea
        {
            get;
            set;
        } = string.Empty;

        public string DomainName
        {
            get;
            set;
        } = string.Empty;

        public string CompetencyCode
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
                string safeRating =
                    value?
                        .Trim()
                        .ToUpperInvariant()
                    ?? string.Empty;

                if (SetProperty(
                        ref _rating,
                        safeRating))
                {
                    OnPropertyChanged(
                        nameof(IsRatingValid));

                    OnPropertyChanged(
                        nameof(RatingDescription));
                }
            }
        }

        public string Remarks
        {
            get => _remarks;

            set => SetProperty(
                ref _remarks,
                value?.Trim()
                ?? string.Empty);
        }

        public bool IsRatingValid =>
            string.IsNullOrWhiteSpace(
                Rating) ||
            Rating == "A" ||
            Rating == "B" ||
            Rating == "C" ||
            Rating == "D" ||
            Rating == "E";

        public string RatingDescription =>
            Rating switch
            {
                "A" =>
                    "Advancing (Namumukod-tangi)",

                "B" =>
                    "Benchmarking (Naipamamalas)",

                "C" =>
                    "Connecting (Natutungo)",

                "D" =>
                    "Developing (Nagpapaunlad)",

                "E" =>
                    "Emerging (Nagsisimula)",

                _ =>
                    "Not yet rated"
            };
    }
}