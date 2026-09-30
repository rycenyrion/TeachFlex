using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TeachFlex.Services
{
    public sealed class BowCurriculumEntry
    {
        public string Grade { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public List<string> Content { get; set; } = new();
        public List<string> Performance { get; set; } = new();
        public List<string> Competencies { get; set; } = new();
        public List<List<string>> TermCompetencies { get; set; } = new();
    }

    public static class BowCurriculumCatalog
    {
        private static readonly Lazy<IReadOnlyList<BowCurriculumEntry>> Entries = new(() =>
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Resources", "BowCatalog.json");
            if (!File.Exists(path)) return Array.Empty<BowCurriculumEntry>();
            return JsonSerializer.Deserialize<List<BowCurriculumEntry>>(
                File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<BowCurriculumEntry>();
        });

        public static BowCurriculumEntry? Find(string? grade, string? file) => Entries.Value
            .FirstOrDefault(entry => string.Equals(entry.Grade, grade, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.File, file, StringComparison.OrdinalIgnoreCase));
    }
}
