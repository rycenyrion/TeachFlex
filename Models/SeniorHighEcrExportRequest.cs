namespace TeachFlex.Models
{
    public class SeniorHighEcrExportRequest :
        ECRExportRequest
    {
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
        } = string.Empty;

        public string SchoolLogoPath
        {
            get;
            set;
        } = string.Empty;

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
        public int StartingTermNumber
        {
            get;
            set;
        } = 1;

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

        public string OtherSubjectName
        {
            get;
            set;
        } = string.Empty;
    }
}