namespace TeachFlex.Models
{
    public class DashboardClassOption
    {
        public int SchoolClassId
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

        public bool IsAdvisoryClass
        {
            get;
            set;
        }

        public int NumberOfLearners
        {
            get;
            set;
        }

        public int MaleLearners
        {
            get;
            set;
        }

        public int FemaleLearners
        {
            get;
            set;
        }

        public int? PresentToday { get; set; }
        public int? AbsentToday { get; set; }

        public string DisplayName
        {
            get
            {
                string className =
                    string.IsNullOrWhiteSpace(
                        SectionName)
                        ? GradeLevel
                        : $"{GradeLevel} - {SectionName}";

                return IsAdvisoryClass
                    ? $"{className} (Advisory)"
                    : className;
            }
        }
    }
}
