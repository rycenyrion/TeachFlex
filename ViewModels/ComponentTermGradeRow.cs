using System;

namespace TeachFlex.ViewModels
{
    public class ComponentTermGradeRow
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

        public double? ComponentOneGrade
        {
            get;
            set;
        }

        public double? ComponentTwoGrade
        {
            get;
            set;
        }

        public double? CombinedTermGrade
        {
            get
            {
                if (!ComponentOneGrade.HasValue &&
                    !ComponentTwoGrade.HasValue)
                {
                    return null;
                }

                if (ComponentOneGrade.HasValue &&
                    !ComponentTwoGrade.HasValue)
                {
                    return ComponentOneGrade.Value;
                }

                if (!ComponentOneGrade.HasValue &&
                    ComponentTwoGrade.HasValue)
                {
                    return ComponentTwoGrade.Value;
                }

                return Math.Round(
                    (
                        ComponentOneGrade!.Value +
                        ComponentTwoGrade!.Value
                    ) / 2.0,
                    0,
                    MidpointRounding.AwayFromZero);
            }
        }

        public string ComponentOneGradeText =>
            ComponentOneGrade.HasValue
                ? ComponentOneGrade.Value.ToString(
                    "0")
                : "—";

        public string ComponentTwoGradeText =>
            ComponentTwoGrade.HasValue
                ? ComponentTwoGrade.Value.ToString(
                    "0")
                : "—";

        public string CombinedTermGradeText =>
            CombinedTermGrade.HasValue
                ? CombinedTermGrade.Value.ToString(
                    "0")
                : "—";
    }
}