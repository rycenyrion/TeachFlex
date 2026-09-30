using System;

namespace TeachFlex.Models
{
    public class LearnerTermRemark :
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
        }

        public int GradeLevel
        {
            get;
            set;
        }

        public AutomatedRemarkLevel PerformanceLevel
        {
            get;
            set;
        }

        public decimal? SourceGeneralAverage
        {
            get;
            set;
        }

        public string SuggestedRemark
        {
            get;
            set;
        } = string.Empty;

        public string FinalRemark
        {
            get;
            set;
        } = string.Empty;

        public int SuggestionVariation
        {
            get;
            set;
        }

        public bool IsTeacherApproved
        {
            get;
            set;
        }

        public bool NeedsReview
        {
            get;
            set;
        }

        public DateTime? ApprovedAtUtc
        {
            get;
            set;
        }

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
