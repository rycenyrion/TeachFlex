namespace TeachFlex.Models
{
    public class LearnerAttendance :
        BaseEntity
    {
        public int AttendanceDayId
        {
            get;
            set;
        }

        public int LearnerId
        {
            get;
            set;
        }

        public string AttendanceStatus
        {
            get;
            set;
        } = "Present";

        public string Remarks
        {
            get;
            set;
        } = string.Empty;

        public AttendanceDay? AttendanceDay
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