namespace TeachFlex.Models
{
    public class LearnerPaceRating :
        BaseEntity
    {
        public int LearnerId
        {
            get;
            set;
        }

        public int PaceCompetencyId
        {
            get;
            set;
        }

        public string Rating
        {
            get;
            set;
        } = string.Empty;

        public string Remarks
        {
            get;
            set;
        } = string.Empty;

        public Learner? Learner
        {
            get;
            set;
        }

        public PaceCompetency? PaceCompetency
        {
            get;
            set;
        }
    }
}