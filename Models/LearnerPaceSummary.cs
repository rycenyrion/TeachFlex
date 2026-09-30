namespace TeachFlex.Models
{
    public class LearnerPaceSummary :
        BaseEntity
    {
        public int LearnerId
        {
            get;
            set;
        }

        public int SchoolClassId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        } = 1;

        public string WhatLearnerCanDo
        {
            get;
            set;
        } = string.Empty;

        public string WhatLearnerNeedsToImprove
        {
            get;
            set;
        } = string.Empty;

        public string TeacherRemarks
        {
            get;
            set;
        } = string.Empty;

        public Learner? Learner
        {
            get;
            set;
        }

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }
    }
}