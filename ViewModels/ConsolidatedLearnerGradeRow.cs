using System;
using System.Collections.Generic;
using System.Linq;

namespace TeachFlex.ViewModels
{
    public class ConsolidatedLearnerGradeRow
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

        public Dictionary<int, int?>
            SubjectFinalGrades
        {
            get;
        } = new Dictionary<int, int?>();

        public bool IsComplete =>
            SubjectFinalGrades.Count > 0 &&
            SubjectFinalGrades.Values.All(
                grade =>
                    grade.HasValue);

        public int? GeneralAverage
        {
            get
            {
                if (!IsComplete)
                {
                    return null;
                }

                return (int)Math.Round(
                    SubjectFinalGrades.Values
                        .Where(
                            grade =>
                                grade.HasValue)
                        .Average(
                            grade =>
                                grade!.Value),
                    0,
                    MidpointRounding.AwayFromZero);
            }
        }

        public string GeneralAverageText =>
            GeneralAverage?.ToString() ?? "—";

        public string Remarks =>
            GeneralAverage switch
            {
                >= 75 =>
                    "Passed",

                int grade when grade >= 0 =>
                    "Failed",

                _ =>
                    "Incomplete"
            };
    }
}