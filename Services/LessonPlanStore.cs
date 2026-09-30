using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ILessonPlanStore
    {
        Task<IReadOnlyList<LessonPlan>> GetByYearAsync(int schoolId, int academicYearId);
        Task SaveAsync(LessonPlan plan);
        Task DeleteAsync(string id);
    }

    public sealed class LessonPlanStore : ILessonPlanStore
    {
        private readonly IDatabasePathService _pathService;
        public LessonPlanStore(IDatabasePathService pathService) => _pathService = pathService;

        private async Task<SqliteConnection> OpenAsync()
        {
            _pathService.EnsureDatabaseFolderExists();
            var connection = new SqliteConnection(_pathService.ConnectionString);
            await connection.OpenAsync();
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS LessonPlans (
                    Id TEXT NOT NULL PRIMARY KEY,
                    SchoolId INTEGER NOT NULL,
                    AcademicYearId INTEGER NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL,
                    ContentJson TEXT NOT NULL
                );";
                await command.ExecuteNonQueryAsync();
                return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<LessonPlan>> GetByYearAsync(
            int schoolId, int academicYearId)
        {
            await using var connection = await OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT ContentJson FROM LessonPlans
                WHERE SchoolId = $school AND AcademicYearId = $year
                ORDER BY UpdatedAtUtc DESC";
            command.Parameters.AddWithValue("$school", schoolId);
            command.Parameters.AddWithValue("$year", academicYearId);
            var plans = new List<LessonPlan>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                LessonPlan? plan = JsonSerializer.Deserialize<LessonPlan>(reader.GetString(0));
                if (plan != null) plans.Add(plan);
            }
            return plans;
        }

        public async Task SaveAsync(LessonPlan plan)
        {
            plan.UpdatedAtUtc = DateTime.UtcNow;
            await using var connection = await OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO LessonPlans
                (Id, SchoolId, AcademicYearId, UpdatedAtUtc, ContentJson)
                VALUES ($id, $school, $year, $updated, $content)
                ON CONFLICT(Id) DO UPDATE SET
                UpdatedAtUtc = excluded.UpdatedAtUtc,
                ContentJson = excluded.ContentJson";
            command.Parameters.AddWithValue("$id", plan.Id);
            command.Parameters.AddWithValue("$school", plan.SchoolId);
            command.Parameters.AddWithValue("$year", plan.AcademicYearId);
            command.Parameters.AddWithValue("$updated", plan.UpdatedAtUtc.ToString("O"));
            command.Parameters.AddWithValue("$content", JsonSerializer.Serialize(plan));
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(string id)
        {
            await using var connection = await OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM LessonPlans WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            await command.ExecuteNonQueryAsync();
        }
    }
}
