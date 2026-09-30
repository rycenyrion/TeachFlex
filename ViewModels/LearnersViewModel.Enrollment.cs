using System;
using System.Collections.Generic;

namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        private DateTime?
            _enrollmentDate;

        private string
            _enrollmentType =
                "Regular";

        private string
            _previousSchoolName =
                string.Empty;

        private DateTime?
            _exitDate;

        private string
            _exitReason =
                string.Empty;

        private string
            _nextSchoolName =
                string.Empty;

        public IReadOnlyList<string>
            EnrollmentTypeOptions
        {
            get;
        } = new[]
        {
            "Regular",
            "Late Enrollee",
            "Transferred In"
        };

        public IReadOnlyList<string>
            ExitReasonOptions
        {
            get;
        } = new[]
        {
            "",
            "Transferred Out",
            "Dropped Out",
            "Completed",
            "Other"
        };

        public DateTime? EnrollmentDate
        {
            get =>
                _enrollmentDate;

            set => SetProperty(
                ref _enrollmentDate,
                value);
        }

        public string EnrollmentType
        {
            get =>
                _enrollmentType;

            set => SetProperty(
                ref _enrollmentType,
                value);
        }

        public string PreviousSchoolName
        {
            get =>
                _previousSchoolName;

            set => SetProperty(
                ref _previousSchoolName,
                value);
        }

        public DateTime? ExitDate
        {
            get =>
                _exitDate;

            set => SetProperty(
                ref _exitDate,
                value);
        }

        public string ExitReason
        {
            get =>
                _exitReason;

            set => SetProperty(
                ref _exitReason,
                value);
        }

        public string NextSchoolName
        {
            get =>
                _nextSchoolName;

            set => SetProperty(
                ref _nextSchoolName,
                value);
        }

        public bool IsTransferredIn =>
            EnrollmentType ==
                "Transferred In";

        public bool HasExitedClass =>
            !string.IsNullOrWhiteSpace(
                ExitReason);
    }
}