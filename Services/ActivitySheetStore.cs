using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public sealed class ActivitySheetStore
    {
        private readonly IDatabasePathService _paths;
        public ActivitySheetStore(IDatabasePathService paths) => _paths = paths;

        private async Task<SqliteConnection> OpenAsync()
        {
            _paths.EnsureDatabaseFolderExists();
            var db = new SqliteConnection(_paths.ConnectionString);
            await db.OpenAsync();
            try
            {
                using var cmd = db.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ActivitySheets (
                    Id TEXT PRIMARY KEY NOT NULL,
                    SchoolId INTEGER NOT NULL,
                    AcademicYearId INTEGER NOT NULL,
                    LessonPlanId TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL,
                    ContentJson TEXT NOT NULL);";
                await cmd.ExecuteNonQueryAsync();
                return db;
            }
            catch { await db.DisposeAsync(); throw; }
        }

        public async Task<IReadOnlyList<ActivitySheet>> GetAsync(int schoolId, int yearId)
        {
            await using var db = await OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"SELECT ContentJson FROM ActivitySheets WHERE SchoolId=$school AND AcademicYearId=$year
                ORDER BY UpdatedAtUtc DESC";
            cmd.Parameters.AddWithValue("$school", schoolId);
            cmd.Parameters.AddWithValue("$year", yearId);
            var sheets = new List<ActivitySheet>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var sheet = JsonSerializer.Deserialize<ActivitySheet>(reader.GetString(0));
                if (sheet != null) sheets.Add(sheet);
            }
            return sheets;
        }

        public async Task SaveAsync(ActivitySheet sheet)
        {
            sheet.UpdatedAtUtc = DateTime.UtcNow;
            await using var db = await OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"INSERT INTO ActivitySheets (Id,SchoolId,AcademicYearId,LessonPlanId,UpdatedAtUtc,ContentJson)
                VALUES ($id,$school,$year,$plan,$updated,$content)
                ON CONFLICT(Id) DO UPDATE SET LessonPlanId=excluded.LessonPlanId,
                UpdatedAtUtc=excluded.UpdatedAtUtc,ContentJson=excluded.ContentJson";
            cmd.Parameters.AddWithValue("$id", sheet.Id);
            cmd.Parameters.AddWithValue("$school", sheet.SchoolId);
            cmd.Parameters.AddWithValue("$year", sheet.AcademicYearId);
            cmd.Parameters.AddWithValue("$plan", sheet.LessonPlanId);
            cmd.Parameters.AddWithValue("$updated", sheet.UpdatedAtUtc.ToString("O"));
            cmd.Parameters.AddWithValue("$content", JsonSerializer.Serialize(sheet));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
