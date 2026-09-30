namespace TeachFlex.Models
{
    public class KindergartenTermRemark :
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

        public int TermNumber
        {
            get;
            set;
        } = 1;

        public string TeacherComment
        {
            get;
            set;
        } = string.Empty;

        public string LearnerStrengths
        {
            get;
            set;
        } = string.Empty;

        public string SuggestedInterventions
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
    }
}