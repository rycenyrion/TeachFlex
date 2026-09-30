using CommunityToolkit.Mvvm.ComponentModel;

namespace TeachFlex.ViewModels
{
    public class AttendanceLearnerRow :
        ObservableObject
    {
        private string _attendanceStatus =
            "Present";

        private string _remarks =
            string.Empty;

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

        public string AttendanceStatus
        {
            get => _attendanceStatus;

            set => SetProperty(
                ref _attendanceStatus,
                value);
        }

        public string Remarks
        {
            get => _remarks;

            set => SetProperty(
                ref _remarks,
                value);
        }
    }
}