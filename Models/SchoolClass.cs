namespace TeachFlex.Models
{
    public class SchoolClass :
        BaseEntity
    {
        public int SchoolId
        {
            get;
            set;
        }

        public int AcademicYearId
        {
            get;
            set;
        }

        public int? AdviserId
        {
            get;
            set;
        }

        public string GradeLevel
        {
            get;
            set;
        } = string.Empty;

        public string SectionName
        {
            get;
            set;
        } = string.Empty;

        public string KeyStage
        {
            get;
            set;
        } = string.Empty;

        public string TrackStrand
        {
            get;
            set;
        } = "Not Applicable";

        public string Schedule
        {
            get;
            set;
        } = string.Empty;

        public string Room
        {
            get;
            set;
        } = string.Empty;

        public bool IsActive
        {
            get;
            set;
        } = true;

        public School? School
        {
            get;
            set;
        }

        public AcademicYear? AcademicYear
        {
            get;
            set;
        }

        public Teacher? Adviser
        {
            get;
            set;
        }

        public string DisplayName =>
            string.IsNullOrWhiteSpace(
                SectionName)
                ? GradeLevel
                : $"{GradeLevel} - {SectionName}";
    }
}