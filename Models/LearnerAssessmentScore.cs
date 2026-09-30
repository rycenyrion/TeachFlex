namespace TeachFlex.Models
{
    public class LearnerAssessmentScore :
        BaseEntity
    {
        public int AssessmentItemId
        {
            get;
            set;
        }

        public int LearnerId
        {
            get;
            set;
        }

        public decimal? Score
        {
            get;
            set;
        }

        public string Remarks
        {
            get;
            set;
        } = string.Empty;

        public AssessmentItem? AssessmentItem
        {
            get;
            set;
        }

        public Learner? Learner
        {
            get;
            set;
        }
    }
}