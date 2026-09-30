namespace TeachFlex.Models
{
    public enum GradingSystemType
    {
        Descriptive,
        NumericalAdjusted,
        NumericalZeroBased
    }

    public enum SubjectAssessmentType
    {
        Regular,
        GmrcValuesEducation,
        MapehComponents,
        EppTleComponents
    }

    public class GradingPolicy
    {
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

        public double WrittenWorkWeight
        {
            get;
            set;
        }

        public double PerformanceTaskWeight
        {
            get;
            set;
        }

        public double ExaminationWeight
        {
            get;
            set;
        }

        public double SummativeTest1Weight
        {
            get;
            set;
        }

        public double SummativeTest2Weight
        {
            get;
            set;
        }

        public double TermExaminationWeight
        {
            get;
            set;
        }

        public bool UsesNumericalGrades =>
            GradingSystem !=
                GradingSystemType.Descriptive;

        public bool UsesDescriptiveGrades =>
            GradingSystem ==
                GradingSystemType.Descriptive;

        public bool UsesDomainBasedAssessment =>
            AssessmentType ==
                SubjectAssessmentType
                    .GmrcValuesEducation;

        public bool UsesMapehComponents =>
            AssessmentType ==
                SubjectAssessmentType
                    .MapehComponents;

        public bool SupportsEppTleComponents =>
            AssessmentType ==
                SubjectAssessmentType
                    .EppTleComponents;

        public bool UsesPerformanceBasedWeights =>
            UsesMapehComponents ||
            SupportsEppTleComponents;

        public static GradingPolicy Create(
            string gradeLevel,
            int schoolYearStart,
            string subjectName)
        {
            GradingSystemType gradingSystem =
                DetermineGradingSystem(
                    gradeLevel,
                    schoolYearStart);

            SubjectAssessmentType assessmentType =
                DetermineAssessmentType(
                    subjectName);

            if (gradingSystem ==
                GradingSystemType.Descriptive)
            {
                return new GradingPolicy
                {
                    GradingSystem =
                        gradingSystem,

                    AssessmentType =
                        assessmentType,

                    WrittenWorkWeight =
                        0,

                    PerformanceTaskWeight =
                        0,

                    ExaminationWeight =
                        0,

                    SummativeTest1Weight =
                        0,

                    SummativeTest2Weight =
                        0,

                    TermExaminationWeight =
                        0
                };
            }

            bool usesPerformanceBasedWeights =
                assessmentType ==
                    SubjectAssessmentType
                        .MapehComponents ||
                assessmentType ==
                    SubjectAssessmentType
                        .EppTleComponents;

            return new GradingPolicy
            {
                GradingSystem =
                    gradingSystem,

                AssessmentType =
                    assessmentType,

                WrittenWorkWeight =
                    20,

                PerformanceTaskWeight =
                    usesPerformanceBasedWeights
                        ? 60
                        : 50,

                ExaminationWeight =
                    usesPerformanceBasedWeights
                        ? 20
                        : 30,

                SummativeTest1Weight =
                    30,

                SummativeTest2Weight =
                    30,

                TermExaminationWeight =
                    40
            };
        }

        private static SubjectAssessmentType
            DetermineAssessmentType(
                string subjectName)
        {
            if (IsGmrcOrValuesEducation(
                    subjectName))
            {
                return SubjectAssessmentType
                    .GmrcValuesEducation;
            }

            if (IsMapeh(
                    subjectName))
            {
                return SubjectAssessmentType
                    .MapehComponents;
            }

            if (IsEppOrTle(
                    subjectName))
            {
                return SubjectAssessmentType
                    .EppTleComponents;
            }

            return SubjectAssessmentType.Regular;
        }

        private static GradingSystemType
            DetermineGradingSystem(
                string gradeLevel,
                int schoolYearStart)
        {
            int gradeNumber =
                GetGradeNumber(
                    gradeLevel);

            if (gradeNumber == 1)
            {
                return GradingSystemType
                    .Descriptive;
            }

            if (gradeNumber == 2)
            {
                return schoolYearStart >= 2027
                    ? GradingSystemType.Descriptive
                    : GradingSystemType
                        .NumericalAdjusted;
            }

            if (gradeNumber == 3)
            {
                if (schoolYearStart >= 2028)
                {
                    return GradingSystemType
                        .Descriptive;
                }

                return schoolYearStart == 2027
                    ? GradingSystemType
                        .NumericalZeroBased
                    : GradingSystemType
                        .NumericalAdjusted;
            }

            return GradingSystemType
                .NumericalAdjusted;
        }

        private static int GetGradeNumber(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return 0;
            }

            string normalizedGradeLevel =
                gradeLevel
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "grade",
                        string.Empty)
                    .Trim();

            return int.TryParse(
                normalizedGradeLevel,
                out int gradeNumber)
                    ? gradeNumber
                    : 0;
        }

        private static bool
            IsGmrcOrValuesEducation(
                string subjectName)
        {
            if (string.IsNullOrWhiteSpace(
                    subjectName))
            {
                return false;
            }

            string normalizedSubject =
                subjectName
                    .Trim()
                    .ToLowerInvariant();

            return normalizedSubject == "gmrc" ||
                   normalizedSubject.Contains(
                       "good manners") ||
                   normalizedSubject.Contains(
                       "values education") ||
                   normalizedSubject == "values";
        }

        private static bool IsMapeh(
            string subjectName)
        {
            if (string.IsNullOrWhiteSpace(
                    subjectName))
            {
                return false;
            }

            return subjectName
                .Trim()
                .Equals(
                    "MAPEH",
                    System.StringComparison
                        .OrdinalIgnoreCase);
        }

        private static bool IsEppOrTle(
            string subjectName)
        {
            if (string.IsNullOrWhiteSpace(
                    subjectName))
            {
                return false;
            }

            string normalizedSubject =
                subjectName
                    .Trim()
                    .ToLowerInvariant();

            return normalizedSubject == "epp" ||
                   normalizedSubject == "tle" ||
                   normalizedSubject.Contains(
                       "epp/tle") ||
                   normalizedSubject.Contains(
                       "epp-tle") ||
                   normalizedSubject.Contains(
                       "edukasyong pantahanan") ||
                   normalizedSubject.Contains(
                       "technology and livelihood");
        }
    }
}