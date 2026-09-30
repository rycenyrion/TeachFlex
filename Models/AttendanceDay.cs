using System;

namespace TeachFlex.Models
{
    public class AttendanceDay :
        BaseEntity
    {
        public int SchoolClassId
        {
            get;
            set;
        }

        public DateTime AttendanceDate
        {
            get;
            set;
        } = DateTime.Today;

        public string DayStatus
        {
            get;
            set;
        } = "Regular Class Day";

        public string Description
        {
            get;
            set;
        } = string.Empty;

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }

        public bool RequiresLearnerAttendance =>
            DayStatus == "Regular Class Day" ||
            DayStatus == "Make-up Class";
    }
}