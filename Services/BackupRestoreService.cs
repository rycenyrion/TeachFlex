using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace TeachFlex.Services
{
    public interface IBackupRestoreService
    {
        string DatabasePath { get; }

        string DefaultBackupFolder { get; }

        Task CreateBackupAsync(
            string destinationPath,
            CancellationToken cancellationToken = default);

        Task<string> RestoreBackupAsync(
            string backupPath,
            CancellationToken cancellationToken = default);
    }

    public sealed class BackupRestoreService :
        IBackupRestoreService
    {
        private readonly IDatabasePathService
            _databasePathService;

        public BackupRestoreService(
            IDatabasePathService databasePathService)
        {
            _databasePathService =
                databasePathService;

            DefaultBackupFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "TeachFlex Backups");
        }

        public string DatabasePath =>
            _databasePathService.DatabasePath;

        public string DefaultBackupFolder
        {
            get;
        }

        public Task CreateBackupAsync(
            string destinationPath,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () => CreateConsistentBackup(
                    destinationPath,
                    cancellationToken),
                cancellationToken);
        }

        public Task<string> RestoreBackupAsync(
            string backupPath,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () => RestoreBackup(
                    backupPath,
                    cancellationToken),
                cancellationToken);
        }

        private void CreateConsistentBackup(
            string destinationPath,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(DatabasePath))
            {
                throw new FileNotFoundException(
                    "The TeachFlex database could not be found.",
                    DatabasePath);
            }

            string? destinationFolder =
                Path.GetDirectoryName(
                    destinationPath);

            if (string.IsNullOrWhiteSpace(
                    destinationFolder))
            {
                throw new InvalidOperationException(
                    "Select a valid backup location.");
            }

            Directory.CreateDirectory(
                destinationFolder);

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            using SqliteConnection source =
                new SqliteConnection(
                    CreateConnectionString(
                        DatabasePath,
                        SqliteOpenMode.ReadOnly));

            using SqliteConnection destination =
                new SqliteConnection(
                    CreateConnectionString(
                        destinationPath,
                        SqliteOpenMode.ReadWriteCreate));

            source.Open();
            destination.Open();

            source.BackupDatabase(
                destination);
        }

        private string RestoreBackup(
            string backupPath,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidateBackup(
                backupPath);

            _databasePathService
                .EnsureDatabaseFolderExists();

            string safetyFolder =
                Path.Combine(
                    _databasePathService.DatabaseFolder,
                    "SafetyBackups");

            Directory.CreateDirectory(
                safetyFolder);

            string safetyBackupPath =
                Path.Combine(
                    safetyFolder,
                    $"TeachFlex_BeforeRestore_{DateTime.Now:yyyyMMdd_HHmmss}.db");

            if (File.Exists(DatabasePath))
            {
                CreateConsistentBackup(
                    safetyBackupPath,
                    cancellationToken);
            }

            string stagingPath =
                Path.Combine(
                    _databasePathService.DatabaseFolder,
                    $"restore_{Guid.NewGuid():N}.tmp");

            try
            {
                File.Copy(
                    backupPath,
                    stagingPath,
                    true);

                SqliteConnection.ClearAllPools();

                DeleteIfPresent(
                    $"{DatabasePath}-wal");

                DeleteIfPresent(
                    $"{DatabasePath}-shm");

                File.Copy(
                    stagingPath,
                    DatabasePath,
                    true);

                SqliteConnection.ClearAllPools();
            }
            finally
            {
                if (File.Exists(stagingPath))
                {
                    File.Delete(stagingPath);
                }
            }

            return safetyBackupPath;
        }

        private static void ValidateBackup(
            string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException(
                    "The selected backup file could not be found.",
                    backupPath);
            }

            using SqliteConnection connection =
                new SqliteConnection(
                    CreateConnectionString(
                        backupPath,
                        SqliteOpenMode.ReadOnly));

            connection.Open();

            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                "PRAGMA quick_check;";

            string result =
                Convert.ToString(
                    command.ExecuteScalar())
                ?? string.Empty;

            if (!result.Equals(
                    "ok",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The selected file is not a valid TeachFlex database backup.");
            }

            command.CommandText =
                "SELECT COUNT(*) FROM sqlite_master " +
                "WHERE type = 'table' " +
                "AND name IN ('Schools', 'AcademicYears');";

            long requiredTableCount =
                Convert.ToInt64(
                    command.ExecuteScalar());

            if (requiredTableCount != 2)
            {
                throw new InvalidDataException(
                    "The selected database does not contain the required TeachFlex records.");
            }
        }

        private static string CreateConnectionString(
            string databasePath,
            SqliteOpenMode mode)
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = mode
            }.ToString();
        }

        private static void DeleteIfPresent(
            string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
