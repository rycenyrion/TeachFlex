using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class DashboardSummary
    {
        public string TeacherName
        {
            get;
            set;
        } = "Teacher";

        public string SchoolName
        {
            get;
            set;
        } = "Complete the School Setup";

        public string SchoolId
        {
            get;
            set;
        } = "School ID not set";

        public string SchoolLogoPath
        {
            get;
            set;
        } = string.Empty;

        public string SchoolYear
        {
            get;
            set;
        } = "School Year not set";

        public string GradeAndSection
        {
            get;
            set;
        } = "No class selected";

        public int? DefaultSchoolClassId
        {
            get;
            set;
        }

        public List<DashboardClassOption>
            AvailableClasses
        {
            get;
            set;
        } = new List<DashboardClassOption>();

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

        public int PresentToday
        {
            get;
            set;
        }

        public int AbsentToday
        {
            get;
            set;
        }

        public string AverageClassPerformance
        {
            get;
            set;
        } = "—";

        public string ReadingPerformance
        {
            get;
            set;
        } = "No records yet";
    }
}