using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public class BackupRestoreViewModel :
        ViewModelBase
    {
        private readonly IBackupRestoreService
            _backupRestoreService;

        private readonly IDialogService
            _dialogService;

        private string _lastBackupPath =
            "No backup created during this session.";

        public BackupRestoreViewModel(
            IBackupRestoreService backupRestoreService,
            IDialogService dialogService)
        {
            _backupRestoreService =
                backupRestoreService;

            _dialogService =
                dialogService;

            CreateBackupCommand =
                new AsyncRelayCommand(
                    CreateBackupAsync,
                    CanRunCommand);

            RestoreBackupCommand =
                new AsyncRelayCommand(
                    RestoreBackupAsync,
                    CanRunCommand);

            StatusMessage =
                "The TeachFlex database is ready for backup.";
        }

        public IAsyncRelayCommand CreateBackupCommand
        {
            get;
        }

        public IAsyncRelayCommand RestoreBackupCommand
        {
            get;
        }

        public string DatabaseLocation =>
            _backupRestoreService.DatabasePath;

        public string LastBackupPath
        {
            get => _lastBackupPath;

            private set => SetProperty(
                ref _lastBackupPath,
                value);
        }

        private bool CanRunCommand()
        {
            return !IsBusy;
        }

        private async Task CreateBackupAsync()
        {
            Directory.CreateDirectory(
                _backupRestoreService.DefaultBackupFolder);

            SaveFileDialog dialog =
                new SaveFileDialog
                {
                    Title = "Save TeachFlex Backup",
                    InitialDirectory =
                        _backupRestoreService.DefaultBackupFolder,
                    FileName =
                        $"TeachFlex_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    DefaultExt = ".db",
                    Filter =
                        "TeachFlex Database Backup (*.db)|*.db|All files (*.*)|*.*",
                    AddExtension = true,
                    OverwritePrompt = true
                };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                SetBusy(
                    true,
                    "Creating a safe database backup...");

                await _backupRestoreService
                    .CreateBackupAsync(
                        dialog.FileName);

                LastBackupPath =
                    dialog.FileName;

                StatusMessage =
                    "Backup completed successfully.";

                _dialogService.ShowInformation(
                    $"TeachFlex backup created successfully.\n\n{dialog.FileName}",
                    "Backup Complete");
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "The backup could not be created.";

                _dialogService.ShowError(
                    $"TeachFlex could not create the backup.\n\n{exception.Message}",
                    "Backup Error");
            }
            finally
            {
                SetBusy(
                    false,
                    StatusMessage);
            }
        }

        private async Task RestoreBackupAsync()
        {
            OpenFileDialog dialog =
                new OpenFileDialog
                {
                    Title = "Select TeachFlex Backup",
                    InitialDirectory =
                        Directory.Exists(
                            _backupRestoreService.DefaultBackupFolder)
                            ? _backupRestoreService.DefaultBackupFolder
                            : Environment.GetFolderPath(
                                Environment.SpecialFolder.MyDocuments),
                    DefaultExt = ".db",
                    Filter =
                        "TeachFlex Database Backup (*.db)|*.db|All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool confirmed =
                _dialogService.Confirm(
                    "Restore the selected backup?\n\n" +
                    "The current database will first be saved automatically. " +
                    "TeachFlex will restart after the restore.",
                    "Confirm Database Restore");

            if (!confirmed)
            {
                return;
            }

            try
            {
                SetBusy(
                    true,
                    "Validating and restoring the database...");

                string safetyBackupPath =
                    await _backupRestoreService
                        .RestoreBackupAsync(
                            dialog.FileName);

                StatusMessage =
                    "Database restored successfully.";

                _dialogService.ShowInformation(
                    "The backup was restored successfully.\n\n" +
                    "A safety copy of the previous database was saved to:\n" +
                    $"{safetyBackupPath}\n\n" +
                    "TeachFlex will now restart.",
                    "Restore Complete");

                RestartApplication();
            }
            catch (Exception exception)
            {
                StatusMessage =
                    "The database could not be restored.";

                _dialogService.ShowError(
                    $"TeachFlex could not restore the selected backup.\n\n{exception.Message}",
                    "Restore Error");
            }
            finally
            {
                SetBusy(
                    false,
                    StatusMessage);
            }
        }

        private void SetBusy(
            bool isBusy,
            string message)
        {
            IsBusy =
                isBusy;

            StatusMessage =
                message;

            CreateBackupCommand
                .NotifyCanExecuteChanged();

            RestoreBackupCommand
                .NotifyCanExecuteChanged();
        }

        private static void RestartApplication()
        {
            string? executablePath =
                Environment.ProcessPath;

            if (!string.IsNullOrWhiteSpace(
                    executablePath))
            {
                Process.Start(
                    new ProcessStartInfo(
                        executablePath)
                    {
                        UseShellExecute = true
                    });
            }

            Application.Current.Shutdown();
        }
    }
}
