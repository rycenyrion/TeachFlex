using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class ECRExportRequest
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

        public string SubjectName
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

        public GradingSystemType GradingSystem
        {
            get;
            set;
        }

        public SubjectAssessmentType AssessmentType
        {
            get;
            set;
        }

        public bool UsesComponentRecords
        {
            get;
            set;
        }

        public string ComponentOneName
        {
            get;
            set;
        } = string.Empty;

        public string ComponentTwoName
        {
            get;
            set;
        } = string.Empty;

        public List<ECRTermExportData> Terms
        {
            get;
            set;
        } = new List<ECRTermExportData>();

        public List<ECRFinalGradeExportRow>
            FinalGrades
        {
            get;
            set;
        } = new List<
            ECRFinalGradeExportRow>();
    }

    public class ECRTermExportData
    {
        public int TermNumber
        {
            get;
            set;
        }

        public string ComponentName
        {
            get;
            set;
        } = "General";

        public List<ECRAssessmentExportColumn>
            AssessmentColumns
        {
            get;
            set;
        } = new List<
            ECRAssessmentExportColumn>();

        public List<ECRLearnerExportRow>
            Learners
        {
            get;
            set;
        } = new List<
            ECRLearnerExportRow>();
    }

    public class ECRAssessmentExportColumn
    {
        public int AssessmentItemId
        {
            get;
            set;
        }

        public string AssessmentName
        {
            get;
            set;
        } = string.Empty;

        public string Category
        {
            get;
            set;
        } = string.Empty;

        public string AssessmentDomain
        {
            get;
            set;
        } = "General";

        public decimal HighestPossibleScore
        {
            get;
            set;
        }

        public int DisplayOrder
        {
            get;
            set;
        }
    }

    public class ECRLearnerExportRow
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

        public Dictionary<int, decimal?>
            Scores
        {
            get;
            set;
        } = new Dictionary<
            int,
            decimal?>();

        public decimal WrittenWorkPercentage
        {
            get;
            set;
        }

        public decimal PerformanceTaskPercentage
        {
            get;
            set;
        }

        public decimal ExaminationPercentage
        {
            get;
            set;
        }

        public decimal InitialGrade
        {
            get;
            set;
        }

        public int TermGrade
        {
            get;
            set;
        }

        public string Descriptor
        {
            get;
            set;
        } = string.Empty;
    }

    public class ECRFinalGradeExportRow
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

        public double? TermOneGrade
        {
            get;
            set;
        }

        public double? TermTwoGrade
        {
            get;
            set;
        }

        public double? TermThreeGrade
        {
            get;
            set;
        }

        public double? FinalGrade
        {
            get;
            set;
        }

        public string CompletionStatus
        {
            get;
            set;
        } = string.Empty;
    }
}