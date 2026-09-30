namespace TeachFlex.Models
{
    public class Subject :
        BaseEntity
    {
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

        public string GradeLevel
        {
            get;
            set;
        } = string.Empty;

        public string LearningArea
        {
            get;
            set;
        } = string.Empty;

        public int DisplayOrder
        {
            get;
            set;
        }

        public bool IsCoreSubject
        {
            get;
            set;
        } = true;

        public string SubjectCategory
        {
            get;
            set;
        } = "Core";

      
        public string TrackStrand
        {
            get;
            set;
        } = "All";
        public string SubjectCluster
        {
            get;
            set;
        } = "Core";

        public int TotalHours
        {
            get;
            set;
        }

        public int TermsTaught
        {
            get;
            set;
        } = 3;

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

        public bool IsActive
        {
            get;
            set;
        } = true;
    }
}