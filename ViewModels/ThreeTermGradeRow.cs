using System;
using System.Collections.Generic;
using System.Linq;

namespace TeachFlex.ViewModels
{
    public class TermSubjectGradeCell
    {
        public int? Grade { get; set; }
        public string GradeText => Grade?.ToString() ?? "—";
    }

    public class TermSubjectSummaryRow
    {
        public int Number { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public List<TermSubjectGradeCell> Grades { get; set; } = new();
        public int? Average =>
            Grades.Count > 0 && Grades.All(cell => cell.Grade.HasValue)
                ? (int)Math.Round(Grades.Average(cell => cell.Grade!.Value),
                    0, MidpointRounding.AwayFromZero)
                : null;
        public string AverageText => Average?.ToString() ?? "—";
        public string Status => Average.HasValue
            ? Average.Value >= 75 ? "Passed" : "Failed"
            : "Incomplete";
    }

    public class TermSubjectSummarySection
    {
        public int TermNumber { get; set; }
        public string Title => TermNumber == 4
            ? "FINAL RATING"
            : $"TERM {TermNumber}";
        public List<string> Subjects { get; set; } = new();
        public List<TermSubjectSummaryRow> Rows { get; set; } = new();
    }

    public class ThreeTermGradeRow
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

        public string SubjectName
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
            get
            {
                if (!TermOneGrade.HasValue ||
                    !TermTwoGrade.HasValue ||
                    !TermThreeGrade.HasValue)
                {
                    return null;
                }

                return Math.Round(
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
            TermOneGrade.HasValue
                ? TermOneGrade.Value.ToString(
                    "0")
                : "—";

        public string TermTwoGradeText =>
            TermTwoGrade.HasValue
                ? TermTwoGrade.Value.ToString(
                    "0")
                : "—";

        public string TermThreeGradeText =>
            TermThreeGrade.HasValue
                ? TermThreeGrade.Value.ToString(
                    "0")
                : "—";

        public string FinalGradeText =>
            FinalGrade.HasValue
                ? FinalGrade.Value.ToString(
                    "0")
                : "—";

        public string CompletionStatus =>
            FinalGrade.HasValue
                ? "Complete"
                : "Incomplete";
    }
}
