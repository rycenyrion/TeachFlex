using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class KindergartenEcrExportRequest
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

        public string CityMunicipality
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
        } = "Kindergarten";

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

        public string SchoolLogoPath
        {
            get;
            set;
        } = string.Empty;

        public List<KindergartenEcrLearnerExportRow>
            Learners
        {
            get;
            set;
        } = new();

        public List<KindergartenEcrCompetencyExportRow>
            Competencies
        {
            get;
            set;
        } = new();

        public List<KindergartenEcrRatingExportRow>
            Ratings
        {
            get;
            set;
        } = new();

        public List<KindergartenEcrRemarkExportRow>
            Remarks
        {
            get;
            set;
        } = new();
    }

    public class KindergartenEcrLearnerExportRow
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

    public class KindergartenEcrCompetencyExportRow
    {
        public int KindergartenCompetencyId
        {
            get;
            set;
        }

        public string CompetencyCode
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

    public class KindergartenEcrRatingExportRow
    {
        public int LearnerId
        {
            get;
            set;
        }

        public int KindergartenCompetencyId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        }

        public string Rating
        {
            get;
            set;
        } = string.Empty;

        public string Observation
        {
            get;
            set;
        } = string.Empty;
    }

    public class KindergartenEcrRemarkExportRow
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

        public string TeacherComment
        {
            get;
            set;
        } = string.Empty;

        public string LearnerStrengths
        {
            get;
            set;
        } = string.Empty;

        public string SuggestedInterventions
        {
            get;
            set;
        } = string.Empty;
    }
}