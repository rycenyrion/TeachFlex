namespace TeachFlex.Models
{
    public class KindergartenLearnerRating :
        BaseEntity
    {
        public int SchoolClassId
        {
            get;
            set;
        }

        public int LearnerId
        {
            get;
            set;
        }

        public int KindergartenCompetencyId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        } = 1;

        public string Rating
        {
            get;
            set;
        } = string.Empty;

        public string Observation
        {
            get;
            set;
        } = string.Empty;

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }

        public Learner? Learner
        {
            get;
            set;
        }

        public KindergartenCompetency?
            KindergartenCompetency
        {
            get;
            set;
        }

        public string RatingDescription =>
            Rating switch
            {
                "BG" => "Beginning",
                "DV" => "Developing",
                "CO" => "Consistent",
                _ => "Not Rated"
            };
    }
}