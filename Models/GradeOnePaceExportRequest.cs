using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class GradeOnePaceExportRequest
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

        public string SchoolHeadName
        {
            get;
            set;
        } = string.Empty;

        public List<GradeOnePaceLearnerExportRow>
            Learners
        {
            get;
            set;
        } = new List<
            GradeOnePaceLearnerExportRow>();

        public List<GradeOnePaceCompetencyExportRow>
            Competencies
        {
            get;
            set;
        } = new List<
            GradeOnePaceCompetencyExportRow>();

        public List<GradeOnePaceRatingExportRow>
            Ratings
        {
            get;
            set;
        } = new List<
            GradeOnePaceRatingExportRow>();

        public List<GradeOnePaceSummaryExportRow>
            Summaries
        {
            get;
            set;
        } = new List<
            GradeOnePaceSummaryExportRow>();
    }

    public class GradeOnePaceLearnerExportRow
    {
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

        public DateTime? BirthDate
        {
            get;
            set;
        }
    }

    public class GradeOnePaceCompetencyExportRow
    {
        public int PaceCompetencyId
        {
            get;
            set;
        }

        public string LearningArea
        {
            get;
            set;
        } = string.Empty;

        public int TermNumber
        {
            get;
            set;
        }

        public string DomainName
        {
            get;
            set;
        } = string.Empty;

        public string CompetencyCode
        {
            get;
            set;
        } = string.Empty;

        public string Description
        {
            get;
            set;
        } = string.Empty;

        public int DisplayOrder
        {
            get;
            set;
        }
    }

    public class GradeOnePaceRatingExportRow
    {
        public int LearnerId
        {
            get;
            set;
        }

        public int PaceCompetencyId
        {
            get;
            set;
        }

        public string Rating
        {
            get;
            set;
        } = string.Empty;

        public string Remarks
        {
            get;
            set;
        } = string.Empty;
    }

    public class GradeOnePaceSummaryExportRow
    {
        public int LearnerId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        }

        public string WhatLearnerCanDo
        {
            get;
            set;
        } = string.Empty;

        public string WhatLearnerNeedsToImprove
        {
            get;
            set;
        } = string.Empty;

        public string TeacherRemarks
        {
            get;
            set;
        } = string.Empty;
    }
}