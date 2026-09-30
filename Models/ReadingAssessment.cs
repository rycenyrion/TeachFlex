using System;

namespace TeachFlex.Models
{
    public sealed class ReadingAssessment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int SchoolId { get; set; }
        public int AcademicYearId { get; set; }
        public int SchoolClassId { get; set; }
        public int LearnerId { get; set; }
        public string GradeLevel { get; set; } = "";
        public DateTime AssessmentDate { get; set; } = DateTime.Today;
        public string AssessmentTool { get; set; } = "";
        public string PassageOrMaterial { get; set; } = "";
        public int? WordsCorrect { get; set; }
        public int? WordsAttempted { get; set; }
        public int? ComprehensionCorrect { get; set; }
        public int? ComprehensionTotal { get; set; }
        public string ReadingLevel { get; set; } = "";
        public string Intervention { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime UpdatedAtUtc { get; set; }
        public double? Accuracy => WordsAttempted > 0 && WordsCorrect.HasValue
            ? Math.Round(100d * WordsCorrect.Value / WordsAttempted.Value, 1) : null;
        public string AccuracyText => Accuracy.HasValue ? $"{Accuracy:0.0}%" : "—";
        public string ComprehensionText => ComprehensionCorrect.HasValue && ComprehensionTotal.HasValue
            ? $"{ComprehensionCorrect}/{ComprehensionTotal}" : "—";
    }
}
