using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachFlex.Data;
using TeachFlex.Repositories;
using TeachFlex.Services;
using TeachFlex.ViewModels;
using QuestPDF.Infrastructure;

namespace TeachFlex
{
    public partial class App :
        Application
    {
        private readonly IHost
            _host;

        public App()
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            _host =
                        Host.CreateDefaultBuilder()
                    .ConfigureServices(
                        services =>
                        {
                            services.AddSingleton<
                                IDatabasePathService,
                                DatabasePathService>();

                            services.AddSingleton<
                                IBackupRestoreService,
                                BackupRestoreService>();

                            services.AddSingleton<
                                IGradingCalculationService,
                                GradingCalculationService>();
                            services.AddTransient<
                                ILearnerGradeService,
                                LearnerGradeService>();
                            services.AddSingleton<
                                IAutomatedRemarksService,
                                AutomatedRemarksService>();
                            services.AddTransient<
    ISF9ExportService,
    SF9DirectPdfExportService>();

                            services.AddSingleton<
                                IGradeOnePaceCatalogService,
                                GradeOnePaceCatalogService>();
                            services.AddSingleton<
    IKindergartenCompetencyCatalogService,
    KindergartenCompetencyCatalogService>();

                            services.AddSingleton<
                                IAssessmentRepository,
                                AssessmentRepository>();

                            services.AddSingleton<
                                IPaceRepository,
                                PaceRepository>();
                            services.AddSingleton<
                                ILearnerTermRemarkRepository,
                                LearnerTermRemarkRepository>();
                            services.AddSingleton<
    IKindergartenRecordRepository,
    KindergartenRecordRepository>();

                            services.AddDbContextFactory<
                                TeachFlexDbContext>(
                                (
                                    serviceProvider,
                                    options) =>
                                {
                                    IDatabasePathService
                                        databasePathService =
                                            serviceProvider
                                                .GetRequiredService<
                                                    IDatabasePathService>();

                                    databasePathService
                                        .EnsureDatabaseFolderExists();

                                    options.UseSqlite(
                                        databasePathService
                                            .ConnectionString);
                                });

                            services.AddSingleton<
                                IDefaultSubjectCatalogService,
                                DefaultSubjectCatalogService>();
                            services.AddSingleton<
    ISeniorHighSubjectCatalogService,
    SeniorHighSubjectCatalogService>();

                            services.AddSingleton<
                                IDatabaseInitializationService,
                                DatabaseInitializationService>();

                            services.AddSingleton<
                                ISchoolRepository,
                                SchoolRepository>();

                            services.AddSingleton<
                                IAcademicYearRepository,
                                AcademicYearRepository>();

                            services.AddSingleton<
                                ISchoolClassRepository,
                                SchoolClassRepository>();

                            services.AddSingleton<
                                ITeacherRepository,
                                TeacherRepository>();

                            services.AddSingleton<
                                ILearnerRepository,
                                LearnerRepository>();

                            services.AddSingleton<
                                ISubjectRepository,
                                SubjectRepository>();

                            services.AddSingleton<
                                IAttendanceRepository,
                                AttendanceRepository>();

                            services.AddSingleton<
                                IDialogService,
                                DialogService>();

                            services.AddSingleton<
                                ILogoService,
                                LogoService>();

                            services.AddSingleton<
                                IGradesExportService,
                                GradesExportService>();

                            services.AddSingleton<
                                IGradesPdfExportService,
                                GradesPdfExportService>();

                            services.AddSingleton<
                                ISF1PdfExportService,
                                SF1PdfExportService>();

                            services.AddSingleton<
                                ISF1ExportService,
                                SF1ExportService>();

                            services.AddSingleton<
                                ISF2ExportService,
                                SF2ExportService>();

                            services.AddSingleton<
                                IECRExportService,
                                ECRExportService>();

                            services.AddSingleton<
                                IGradeOnePaceExportService,
                                GradeOnePaceExportService>();
                            services.AddSingleton<
                                IKindergartenECRExportService,
                                KindergartenECRExportService>();

                            services.AddSingleton<
                                IKindergartenProgressReportPdfService,
                                KindergartenProgressReportPdfService>();

                            services.AddSingleton<
                                ISeniorHighECRExportService,
                                SeniorHighECRExportService>();

                            services.AddSingleton<
                                NavigationStore>();

                            services.AddSingleton<
                                IClockService,
                                ClockService>();

                            services.AddSingleton<
                                IDashboardSummaryService,
                                DashboardSummaryService>();

                            services.AddSingleton<ILessonPlanStore, LessonPlanStore>();
                            services.AddSingleton<IReadingAssessmentStore, ReadingAssessmentStore>();
                            services.AddSingleton<ILessonPlanPdfService, LessonPlanPdfService>();
                            services.AddSingleton<ActivitySheetStore>();
                            services.AddSingleton<ActivitySheetPdfService>();
                            services.AddSingleton<CommonsActivityImageService>();
                            services.AddSingleton<ActivityItemGenerator>();

                            services.AddTransient<
    SchoolSetupViewModel>();

                            services.AddTransient<
                                MyClassesViewModel>();

                            services.AddTransient<
                                LearnersViewModel>();

                            services.AddTransient<
                                SubjectSetupViewModel>();

                            services.AddTransient<
                                AttendanceViewModel>();

                            services.AddTransient<
                               SchoolFormsViewModel>();

                            services.AddTransient<
                                EClassRecordViewModel>();

                            services.AddTransient<
                                GradesViewModel>();

                            services.AddTransient<
                                DashboardViewModel>();

                            services.AddTransient<LessonPlannerViewModel>();
                            services.AddTransient<ActivitySheetViewModel>();
                            services.AddTransient<ReadingTrackerViewModel>();
                            services.AddTransient<ReportsViewModel>();

                            services.AddTransient<
                                BackupRestoreViewModel>();

                            services.AddSingleton<
                                Func<SchoolSetupViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    SchoolSetupViewModel>());

                            services.AddSingleton<
                                Func<MyClassesViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    MyClassesViewModel>());

                            services.AddSingleton<
                                Func<LearnersViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    LearnersViewModel>());

                            services.AddSingleton<
                                Func<SubjectSetupViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    SubjectSetupViewModel>());

                            services.AddSingleton<
                                Func<AttendanceViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    AttendanceViewModel>());

                            services.AddSingleton<
    Func<SchoolFormsViewModel>>(
        serviceProvider =>
            () =>
                serviceProvider
                    .GetRequiredService<
                        SchoolFormsViewModel>());

                            services.AddSingleton<
                                Func<EClassRecordViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    EClassRecordViewModel>());
                            services.AddSingleton<
    Func<GradesViewModel>>(
        serviceProvider =>
            () =>
                serviceProvider
                    .GetRequiredService<
                        GradesViewModel>());

                            services.AddSingleton<
                                Func<DashboardViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    DashboardViewModel>());

                            services.AddSingleton<Func<LessonPlannerViewModel>>(
                                provider => () => provider.GetRequiredService<LessonPlannerViewModel>());
                            services.AddSingleton<Func<ActivitySheetViewModel>>(
                                provider => () => provider.GetRequiredService<ActivitySheetViewModel>());
                            services.AddSingleton<Func<ReadingTrackerViewModel>>(
                                provider => () => provider.GetRequiredService<ReadingTrackerViewModel>());
                            services.AddSingleton<Func<ReportsViewModel>>(
                                provider => () => provider.GetRequiredService<ReportsViewModel>());

                            services.AddSingleton<
                                Func<BackupRestoreViewModel>>(
                                    serviceProvider =>
                                        () =>
                                            serviceProvider
                                                .GetRequiredService<
                                                    BackupRestoreViewModel>());

                            services.AddSingleton<
                                MainWindowViewModel>();

                            services.AddSingleton<
                                MainWindow>();
                        })
                    .Build();
        }

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(
                e);

            try
            {
                _host.Start();

                IDatabaseInitializationService
                    databaseInitializationService =
                        _host.Services
                            .GetRequiredService<
                                IDatabaseInitializationService>();

                databaseInitializationService
                    .InitializeAsync()
                    .GetAwaiter()
                    .GetResult();

                MainWindow mainWindow =
                    _host.Services
                        .GetRequiredService<
                            MainWindow>();

                MainWindow =
                    mainWindow;

                mainWindow.Show();
                mainWindow.Activate();
            }
            catch (Exception exception)
            {
                string errorFolder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder
                                .LocalApplicationData),
                        "TeachFlex");

                Directory.CreateDirectory(
                    errorFolder);

                string errorFile =
                    Path.Combine(
                        errorFolder,
                        "startup-error.txt");

                File.WriteAllText(
                    errorFile,
                    exception.ToString());

                MessageBox.Show(
                    $"TeachFlex could not start.\n\n" +
                    $"{exception.Message}\n\n" +
                    $"Error details were saved to:\n" +
                    $"{errorFile}",
                    "TeachFlex Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(
                    -1);
            }
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            try
            {
                _host.StopAsync(
                        TimeSpan.FromSeconds(
                            5))
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                // Application is already closing.
            }
            finally
            {
                _host.Dispose();
            }

            base.OnExit(
                e);
        }
    }
}
