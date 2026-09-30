namespace TeachFlex.Models
{
    public class SeniorHighSubjectDefinition
    {
        public string GradeLevel
        {
            get;
            set;
        } = string.Empty;

        public string SubjectCode
        {
            get;
            set;
        } = string.Empty;

        public string SubjectName
        {
            get;
            set;
        } = string.Empty;

        public string SubjectCategory
        {
            get;
            set;
        } = string.Empty;

        public string SubjectCluster
        {
            get;
            set;
        } = string.Empty;

        public string TrackStrand
        {
            get;
            set;
        } = "All";

        public int DisplayOrder
        {
            get;
            set;
        }

        public bool IsCoreSubject
        {
            get;
            set;
        }

        public int TotalHours
        {
            get;
            set;
        }

        public int TermsTaught
        {
            get;
            set;
        }

        public decimal UnitsPerTerm
        {
            get;
            set;
        }

        public decimal UnitsPerYear
        {
            get;
            set;
        }

        public int WrittenOralWorksWeight
        {
            get;
            set;
        }

        public int PerformanceTasksWeight
        {
            get;
            set;
        }

        public int SummativeTermExamWeight
        {
            get;
            set;
        }

        public int SummativeTestOneShare
        {
            get;
            set;
        }

        public int SummativeTestTwoShare
        {
            get;
            set;
        }

        public int TermExamShare
        {
            get;
            set;
        }
    }
}