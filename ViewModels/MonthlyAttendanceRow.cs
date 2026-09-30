using System.Collections.Generic;

namespace TeachFlex.ViewModels
{
    public class MonthlyAttendanceRow
    {
        public int Number
        {
            get;
            set;
        }

        public int LearnerId
        {
            get;
            set;
        }

        public string Lrn
        {
            get;
            set;
        } = string.Empty;

        public string LearnerName
        {
            get;
            set;
        } = string.Empty;

        public string Sex
        {
            get;
            set;
        } = string.Empty;

        public int TotalClassDays
        {
            get;
            set;
        }

        public int PresentDays
        {
            get;
            set;
        }

        public int AbsentDays
        {
            get;
            set;
        }

        public int LateDays
        {
            get;
            set;
        }

        public int ExcusedDays
        {
            get;
            set;
        }

        public Dictionary<int, string>
            DailyStatuses
        {
            get;
        } = new Dictionary<int, string>();

        public double AttendanceRate =>
            TotalClassDays == 0
                ? 0
                : (
                    (
                        PresentDays +
                        LateDays
                    ) *
                    100.0
                  ) /
                  TotalClassDays;

        public string AttendanceRateText =>
            $"{AttendanceRate:0.0}%";
    }
}