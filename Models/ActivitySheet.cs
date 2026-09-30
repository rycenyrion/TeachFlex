using System;

namespace TeachFlex.Models
{
    public sealed class ActivitySheet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int SchoolId { get; set; }
        public int AcademicYearId { get; set; }
        public string LessonPlanId { get; set; } = "";
        public int Session { get; set; } = 1;
        public string Title { get; set; } = "";
        public string Instructions { get; set; } = "";
        public string Questions { get; set; } = "";
        public string TeacherGuide { get; set; } = "";
        public string ImageBase64 { get; set; } = "";
        public string ImageTitle { get; set; } = "";
        public string ImageSourceUrl { get; set; } = "";
        public string ImageLicense { get; set; } = "";
        public string ImageLicenseUrl { get; set; } = "";
        public string ImageAuthor { get; set; } = "";
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public string DisplayName => $"Session {Session} · {Title}";
    }
}
