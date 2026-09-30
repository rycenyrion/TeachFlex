namespace TeachFlex.Models
{
    public class AcademicYear :
        BaseEntity
    {
        public int SchoolId
        {
            get;
            set;
        }

        public int StartYear
        {
            get;
            set;
        }

        public int EndYear
        {
            get;
            set;
        }

        public bool IsCurrent
        {
            get;
            set;
        }

        public School? School
        {
            get;
            set;
        }

        public string DisplayName =>
            $"{StartYear}-{EndYear}";
    }
}