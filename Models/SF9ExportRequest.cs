using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class SF9ExportRequest
    {
        public School School
        {
            get;
            set;
        } = null!;

        public AcademicYear AcademicYear
        {
            get;
            set;
        } = null!;

        public SchoolClass SchoolClass
        {
            get;
            set;
        } = null!;

        public Learner Learner
        {
            get;
            set;
        } = null!;

        public IReadOnlyList<SF9SubjectGradeRow>
            SubjectGrades
        {
            get;
            set;
        } = Array.Empty<SF9SubjectGradeRow>();

        public IReadOnlyList<SF9AttendanceRow>
            AttendanceRows
        {
            get;
            set;
        } = Array.Empty<SF9AttendanceRow>();

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

        public DateTime GeneratedOn
        {
            get;
            set;
        } = DateTime.Now;
    }

    public class SF9SubjectGradeRow
    {
        public int SubjectId
        {
            get;
            set;
        }

        public string SubjectCode
        {
            get;
            set;
        } = string.Empty;

        public string SubjectName
        {
            get;
            set;
        } = string.Empty;

        public string LearningArea
        {
            get;
            set;
        } = string.Empty;

        public string SubjectCategory
        {
            get;
            set;
        } = string.Empty;

        public int DisplayOrder
        {
            get;
            set;
        }

        public int? TermOneGrade
        {
            get;
            set;
        }

        public int? TermTwoGrade
        {
            get;
            set;
        }

        public int? TermThreeGrade
        {
            get;
            set;
        }

        public decimal Units
        {
            get;
            set;
        }

        public int? FinalGrade
        {
            get;
            set;
        }

        public string Remarks =>
            !FinalGrade.HasValue
                ? string.Empty
                : FinalGrade.Value >= 75
                    ? "PASSED"
                    : "FAILED";
    }

    public class SF9AttendanceRow
    {
        public int MonthNumber
        {
            get;
            set;
        }

        public string MonthName
        {
            get;
            set;
        } = string.Empty;

        public int SchoolDays
        {
            get;
            set;
        }

        public int DaysPresent
        {
            get;
            set;
        }

        public int DaysAbsent
        {
            get;
            set;
        }
    }
}