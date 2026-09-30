using System;

namespace TeachFlex.ViewModels
{
    public class GradeSummaryRow
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

        public int? FinalGrade
        {
            get
            {
                if (!TermOneGrade.HasValue ||
                    !TermTwoGrade.HasValue ||
                    !TermThreeGrade.HasValue)
                {
                    return null;
                }

                return (int)Math.Round(
                    (
                        TermOneGrade.Value +
                        TermTwoGrade.Value +
                        TermThreeGrade.Value
                    ) / 3.0,
                    0,
                    MidpointRounding.AwayFromZero);
            }
        }

        public string TermOneGradeText =>
            TermOneGrade?.ToString() ?? "—";

        public string TermTwoGradeText =>
            TermTwoGrade?.ToString() ?? "—";

        public string TermThreeGradeText =>
            TermThreeGrade?.ToString() ?? "—";

        public string FinalGradeText =>
            FinalGrade?.ToString() ?? "—";

        public string Descriptor =>
            FinalGrade switch
            {
                >= 90 =>
                    "Advancing",

                >= 80 =>
                    "Benchmarking",

                >= 75 =>
                    "Connecting",

                >= 65 =>
                    "Developing",

                int grade when grade >= 0 =>
                    "Emerging",

                _ =>
                    "Incomplete"
            };

        public string Remarks =>
            FinalGrade switch
            {
                >= 75 =>
                    "Passed",

                int grade when grade >= 0 =>
                    "Failed",

                _ =>
                    "Incomplete"
            };

        public bool IsComplete =>
            FinalGrade.HasValue;
    }
}