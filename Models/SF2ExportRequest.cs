using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class SF2ExportRequest
    {
        public string SchoolId
        {
            get;
            set;
        } = string.Empty;

        public string SchoolName
        {
            get;
            set;
        } = string.Empty;

        public string SchoolYear
        {
            get;
            set;
        } = string.Empty;
        public int AcademicYearStartYear
        {
            get;
            set;
        }

        public int AcademicYearEndYear
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

        public string AdviserName
        {
            get;
            set;
        } = string.Empty;

        public string SchoolHeadName
        {
            get;
            set;
        } = string.Empty;

        public DateTime ReportMonth
        {
            get;
            set;
        }

        public List<SF2LearnerRow>
            Learners
        {
            get;
            set;
        } = new();
    }

    public class SF2LearnerRow
    {
        public int LearnerId
        {
            get;
            set;
        }

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

        public Dictionary<int, string>
            DailyStatuses
        {
            get;
            set;
        } = new();

        public int TotalAbsent
        {
            get;
            set;
        }

        public int TotalLate
        {
            get;
            set;
        }

        public string Remarks
        {
            get;
            set;
        } = string.Empty;
        public DateTime? EnrollmentDate
        {
            get;
            set;
        }

        public string EnrollmentType
        {
            get;
            set;
        } = "Regular";

        public string PreviousSchoolName
        {
            get;
            set;
        } = string.Empty;

        public DateTime? ExitDate
        {
            get;
            set;
        }

        public string ExitReason
        {
            get;
            set;
        } = string.Empty;

        public string NextSchoolName
        {
            get;
            set;
        } = string.Empty;
    }
}