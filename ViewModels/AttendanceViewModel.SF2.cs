using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class AttendanceViewModel
    {
        [RelayCommand]
        private async Task PrepareOfficialSF2Async()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Official SF2");

                return;
            }

            try
            {
                StatusMessage =
                    "Loading the selected month's attendance data...";

                // Reload the latest attendance data before export.
                await LoadMonthlySummaryAsync();

                if (SelectedClass == null)
                {
                    return;
                }

                School? school =
                    await _schoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    _dialogService.ShowWarning(
                        "Complete School Setup first.",
                        "Official SF2");

                    return;
                }

                AcademicYear? academicYear =
                    await _academicYearRepository
                        .GetCurrentAsync(
                            school.Id);

                if (academicYear == null)
                {
                    _dialogService.ShowWarning(
                        "Create the current School Year first.",
                        "Official SF2");

                    return;
                }

                Teacher? activeTeacher =
                    await _teacherRepository
                        .GetActiveTeacherAsync(
                            school.Id);

                string adviserName =
                    !string.IsNullOrWhiteSpace(
                        SelectedClass.Adviser?.FullName)
                        ? SelectedClass.Adviser!.FullName
                        : activeTeacher?.FullName
                          ?? string.Empty;

                IReadOnlyList<Learner> classLearners =
                    await _learnerRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

                Dictionary<int, Learner> learnerById =
                    classLearners.ToDictionary(
                        learner => learner.Id);

                SF2ExportRequest request =
                    new SF2ExportRequest
                    {
                        SchoolId =
                            school.SchoolId,

                        SchoolName =
                            school.SchoolName,

                        SchoolYear =
                            academicYear.DisplayName,

                        AcademicYearStartYear =
                            academicYear.StartYear,

                        AcademicYearEndYear =
                            academicYear.EndYear,

                        GradeLevel =
                            SelectedClass.GradeLevel,

                        SectionName =
                            SelectedClass.SectionName,

                        AdviserName =
                            adviserName,

                        SchoolHeadName =
                            school.SchoolHead,

                        ReportMonth =
                            SelectedMonth,

                        Learners =
                            MonthlyRows
                                .Select(
                                    row =>
                                    {
                                        learnerById.TryGetValue(
                                            row.LearnerId,
                                            out Learner? learner);

                                        string officialName;

                                        if (learner == null)
                                        {
                                            officialName =
                                                row.LearnerName;
                                        }
                                        else
                                        {
                                            string givenNames =
                                                string.Join(
                                                    " ",
                                                    new[]
                                                    {
                                                        learner.FirstName,
                                                        learner.MiddleName,
                                                        learner.Suffix
                                                    }
                                                    .Where(
                                                        value =>
                                                            !string.IsNullOrWhiteSpace(
                                                                value))
                                                    .Select(
                                                        value =>
                                                            value.Trim()));

                                            officialName =
                                                $"{learner.LastName.Trim()}, " +
                                                $"{givenNames}";
                                        }

                                        return new SF2LearnerRow
                                        {
                                            LearnerId =
                                                row.LearnerId,

                                            LearnerName =
                                                officialName,

                                            Sex =
                                                row.Sex,

                                            DailyStatuses =
                                                new Dictionary<int, string>(
                                                    row.DailyStatuses),

                                            TotalAbsent =
                                                row.AbsentDays,

                                            TotalLate =
                                                row.LateDays,

                                            Remarks =
                                                row.ExcusedDays > 0
                                                    ? $"{row.ExcusedDays} " +
                                                      $"excused day(s)"
                                                    : string.Empty,

                                            EnrollmentDate =
                                                learner?.EnrollmentDate,

                                            EnrollmentType =
                                                learner?.EnrollmentType
                                                ?? "Regular",

                                            PreviousSchoolName =
                                                learner?.PreviousSchoolName
                                                ?? string.Empty,

                                            ExitDate =
                                                learner?.ExitDate,

                                            ExitReason =
                                                learner?.ExitReason
                                                ?? string.Empty,

                                            NextSchoolName =
                                                learner?.NextSchoolName
                                                ?? string.Empty
                                        };
                                    })
                                .ToList()
                    };

                string safeGradeLevel =
                    MakeSafeFileName(
                        request.GradeLevel);

                string safeSection =
                    MakeSafeFileName(
                        request.SectionName);

                string suggestedFileName =
                    $"SF2_{safeGradeLevel}_" +
                    $"{safeSection}_" +
                    $"{SelectedMonth:MMMM_yyyy}.pdf";

                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        Title =
                            "Export Official DepEd SF2 to PDF",

                        Filter =
                            "PDF Document (*.pdf)|*.pdf",

                        DefaultExt =
                            ".pdf",

                        AddExtension =
                            true,

                        FileName =
                            suggestedFileName,

                        OverwritePrompt =
                            true
                    };

                bool? result =
                    saveFileDialog.ShowDialog();

                if (result != true)
                {
                    StatusMessage =
                        "SF2 PDF export was cancelled.";

                    return;
                }

                StatusMessage =
                    "Creating the official DepEd SF2 PDF...";

                string exportedFile =
                    _sf2ExportService
                        .ExportOfficialSF2Pdf(
                            request,
                            saveFileDialog.FileName);

                StatusMessage =
                    "Official SF2 PDF exported successfully.";

                System.Windows.MessageBoxResult openResult =
                    System.Windows.MessageBox.Show(
                        $"The official DepEd SF2 PDF was " +
                        $"created successfully.\n\n" +
                        $"{exportedFile}\n\n" +
                        $"Would you like to open the PDF now?",
                        "Official SF2 PDF Ready",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Information);

                if (openResult ==
                    System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName =
                                exportedFile,

                            UseShellExecute =
                                true
                        });
                }
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The official SF2 PDF could not be created.";

                _dialogService.ShowError(
                    $"TeachFlex could not create the " +
                    $"official SF2 PDF.\n\n" +
                    $"{errorMessage}",
                    "Official SF2 PDF Error");
            }
        }

        private static string MakeSafeFileName(
            string value)
        {
            string safeValue =
                string.IsNullOrWhiteSpace(
                    value)
                    ? "Unspecified"
                    : value.Trim();

            foreach (char invalidCharacter
                     in System.IO.Path
                         .GetInvalidFileNameChars())
            {
                safeValue =
                    safeValue.Replace(
                        invalidCharacter,
                        '-');
            }

            return safeValue.Replace(
                " ",
                "_");
        }
    }
}