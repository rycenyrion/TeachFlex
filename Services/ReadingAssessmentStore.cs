using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IReadingAssessmentStore
    {
        Task<IReadOnlyList<ReadingAssessment>> GetForLearnerAsync(int schoolId, int academicYearId, int learnerId);
        Task SaveAsync(ReadingAssessment assessment);
        Task DeleteAsync(string id, int schoolId, int academicYearId);
    }

    public sealed class ReadingAssessmentStore : IReadingAssessmentStore
    {
        private readonly IDatabasePathService _paths;
        public ReadingAssessmentStore(IDatabasePathService paths) => _paths = paths;

        private async Task<SqliteConnection> OpenAsync()
        {
            _paths.EnsureDatabaseFolderExists();
            var db = new SqliteConnection(_paths.ConnectionString);
            await db.OpenAsync();
            try
            {
                using var cmd = db.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ReadingAssessments (
                    Id TEXT NOT NULL PRIMARY KEY,
                    SchoolId INTEGER NOT NULL,
                    AcademicYearId INTEGER NOT NULL,
                    LearnerId INTEGER NOT NULL,
                    AssessmentDate TEXT NOT NULL,
                    ContentJson TEXT NOT NULL);
                    CREATE INDEX IF NOT EXISTS IX_ReadingAssessments_Learner
                    ON ReadingAssessments (SchoolId, AcademicYearId, LearnerId, AssessmentDate);";
                await cmd.ExecuteNonQueryAsync();
                return db;
            }
            catch { await db.DisposeAsync(); throw; }
        }

        public async Task<IReadOnlyList<ReadingAssessment>> GetForLearnerAsync(int schoolId, int academicYearId, int learnerId)
        {
            await using var db = await OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"SELECT ContentJson FROM ReadingAssessments
                WHERE SchoolId=$school AND AcademicYearId=$year AND LearnerId=$learner
                ORDER BY AssessmentDate DESC, Id DESC";
            cmd.Parameters.AddWithValue("$school", schoolId);
            cmd.Parameters.AddWithValue("$year", academicYearId);
            cmd.Parameters.AddWithValue("$learner", learnerId);
            var list = new List<ReadingAssessment>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = JsonSerializer.Deserialize<ReadingAssessment>(reader.GetString(0));
                if (item != null) list.Add(item);
            }
            return list;
        }

        public async Task SaveAsync(ReadingAssessment item)
        {
            item.UpdatedAtUtc = DateTime.UtcNow;
            await using var db = await OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = @"INSERT INTO ReadingAssessments
                (Id,SchoolId,AcademicYearId,LearnerId,AssessmentDate,ContentJson)
                VALUES ($id,$school,$year,$learner,$date,$json)
                ON CONFLICT(Id) DO UPDATE SET
                    SchoolId=excluded.SchoolId,AcademicYearId=excluded.AcademicYearId,
                    LearnerId=excluded.LearnerId,AssessmentDate=excluded.AssessmentDate,
                    ContentJson=excluded.ContentJson";
            cmd.Parameters.AddWithValue("$id", item.Id);
            cmd.Parameters.AddWithValue("$school", item.SchoolId);
            cmd.Parameters.AddWithValue("$year", item.AcademicYearId);
            cmd.Parameters.AddWithValue("$learner", item.LearnerId);
            cmd.Parameters.AddWithValue("$date", item.AssessmentDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(item));
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(string id, int schoolId, int academicYearId)
        {
            await using var db = await OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = "DELETE FROM ReadingAssessments WHERE Id=$id AND SchoolId=$school AND AcademicYearId=$year";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$school", schoolId);
            cmd.Parameters.AddWithValue("$year", academicYearId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
