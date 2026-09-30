using System;

namespace TeachFlex.Models
{
    public class AssessmentItem :
        BaseEntity
    {
        public int SchoolClassId
        {
            get;
            set;
        }

        public int SubjectId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        } = 1;

        public string ComponentName
        {
            get;
            set;
        } = "General";

        public string Category
        {
            get;
            set;
        } = "Written Work";

        public string AssessmentDomain
        {
            get;
            set;
        } = "General";

        public string AssessmentName
        {
            get;
            set;
        } = string.Empty;

        public decimal HighestPossibleScore
        {
            get;
            set;
        }

        public DateTime? AssessmentDate
        {
            get;
            set;
        }

        public int DisplayOrder
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        } = true;

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }

        public Subject? Subject
        {
            get;
            set;
        }
    }
}