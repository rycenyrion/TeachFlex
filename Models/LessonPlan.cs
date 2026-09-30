using System;

namespace TeachFlex.Models
{
    public sealed class LessonPlan
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int SchoolId { get; set; }
        public int AcademicYearId { get; set; }
        public int SchoolClassId { get; set; }
        public int SubjectId { get; set; }
        public DateTime LessonDate { get; set; } = DateTime.Today;
        public string Title { get; set; } = string.Empty;
        public string LearningCompetency { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Materials { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
        public string Activities { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Reflection { get; set; } = string.Empty;
        public int TermNumber { get; set; } = 1;
        public int WeekNumber { get; set; } = 1;
        public string ContentStandard { get; set; } = string.Empty;
        public string PerformanceStandard { get; set; } = string.Empty;
        public string BowSource { get; set; } = string.Empty;
        public string PreLesson { get; set; } = string.Empty;
        public string Session1Title { get; set; } = string.Empty;
        public string Session1Competency { get; set; } = string.Empty;
        public string Session1Objectives { get; set; } = string.Empty;
        public string Session2Title { get; set; } = string.Empty;
        public string Session2Competency { get; set; } = string.Empty;
        public string Session2Objectives { get; set; } = string.Empty;
        public string Session3Title { get; set; } = string.Empty;
        public string Session3Competency { get; set; } = string.Empty;
        public string Session3Objectives { get; set; } = string.Empty;
        public string Session4Title { get; set; } = string.Empty;
        public string Session4Competency { get; set; } = string.Empty;
        public string Session4Objectives { get; set; } = string.Empty;
        public string Session1Motivation { get; set; } = string.Empty;
        public string Session2Motivation { get; set; } = string.Empty;
        public string Session3Motivation { get; set; } = string.Empty;
        public string Session4Motivation { get; set; } = string.Empty;
        public string Day1 { get; set; } = string.Empty;
        public string Day2 { get; set; } = string.Empty;
        public string Day3 { get; set; } = string.Empty;
        public string Day4 { get; set; } = string.Empty;
        public string Day1Assessment { get; set; } = string.Empty;
        public string Day2Assessment { get; set; } = string.Empty;
        public string Day3Assessment { get; set; } = string.Empty;
        public string Day4Assessment { get; set; } = string.Empty;
        public string WeeklyTest { get; set; } = string.Empty;
        public string Session1ExtendLearning { get; set; } = string.Empty;
        public string Session1Reflection { get; set; } = string.Empty;
        public string Session2ExtendLearning { get; set; } = string.Empty;
        public string Session2Reflection { get; set; } = string.Empty;
        public string Session3ExtendLearning { get; set; } = string.Empty;
        public string Session3Reflection { get; set; } = string.Empty;
        public string Session4ExtendLearning { get; set; } = string.Empty;
        public string Session4Reflection { get; set; } = string.Empty;
        public string Session5ExtendLearning { get; set; } = string.Empty;
        public string Session5Reflection { get; set; } = string.Empty;
        public string ExtendLearning { get; set; } = string.Empty;
        public string LearnerNeeds { get; set; } = string.Empty;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public string DisplayName => $"Term {TermNumber} · Week {WeekNumber} · {(string.IsNullOrWhiteSpace(Session1Title) ? Title : Session1Title)}";
    }
}
