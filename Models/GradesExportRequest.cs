using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class GradesExportRequest
    {
        public string SchoolName
        {
            get;
            set;
        } = string.Empty;

        public string SchoolId
        {
            get;
            set;
        } = string.Empty;

        public string Region
        {
            get;
            set;
        } = string.Empty;

        public string Division
        {
            get;
            set;
        } = string.Empty;

        public string District
        {
            get;
            set;
        } = string.Empty;

        public string SchoolAddress
        {
            get;
            set;
        } = string.Empty;

        public string SchoolHeadName
        {
            get;
            set;
        } = string.Empty;

        public string DepEdLogoPath
        {
            get;
            set;
        } = string.Empty;

        public string SchoolLogoPath
        {
            get;
            set;
        } = string.Empty;

        public string SchoolYear
        {
            get;
            set;
        } = string.Empty;

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

        public DateTime DatePrepared
        {
            get;
            set;
        } = DateTime.Now;

        public List<GradesExportSubject>
            Subjects
        {
            get;
            set;
        } = new List<GradesExportSubject>();

        public List<GradesExportLearner>
            Learners
        {
            get;
            set;
        } = new List<GradesExportLearner>();
    }

    public class GradesExportSubject
    {
        public int SubjectId
        {
            get;
            set;
        }

        public string SubjectName
        {
            get;
            set;
        } = string.Empty;
    }

    public class GradesExportLearner
    {
        public int Number
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

        public Dictionary<int, int?>
            SubjectFinalGrades
        {
            get;
            set;
        } = new Dictionary<int, int?>();

        public int? GeneralAverage
        {
            get;
            set;
        }

        public string Remarks
        {
            get;
            set;
        } = string.Empty;
    }
}