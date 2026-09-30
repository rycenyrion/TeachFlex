using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        [RelayCommand]
        private async Task ExportSf2Async()
        {
            if (!CanPrepareSf2 ||
                _currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class containing enrolled learners.",
                    "Export SF2 to PDF");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    $"Loading attendance records for " +
                    $"{SelectedMonthText}...";

                IReadOnlyList<AttendanceDay>
                    monthDays =
                        await _attendanceRepository
                            .GetMonthDaysAsync(
                                SelectedClass.Id,
                                SelectedMonth.Year,
                                SelectedMonth.Month);

                List<AttendanceDay>
                    classDays =
                        monthDays
                            .Where(
                                day =>
                                    day.DayStatus ==
                                        "Regular Class Day" ||
                                    day.DayStatus ==
                                        "Make-up Class")
                            .ToList();

                HashSet<int> classDayIds =
                    classDays
                        .Select(
                            day =>
                                day.Id)
                        .ToHashSet();

                IReadOnlyList<LearnerAttendance>
                    attendanceRecords =
                        await _attendanceRepository
                            .GetMonthAttendancesAsync(
                                SelectedClass.Id,
                                SelectedMonth.Year,
                                SelectedMonth.Month);

                List<LearnerAttendance>
                    validAttendanceRecords =
                        attendanceRecords
                            .Where(
                                record =>
                                    classDayIds.Contains(
                                        record.AttendanceDayId))
                            .ToList();

                Teacher? activeTeacher =
                    await _teacherRepository
                        .GetActiveTeacherAsync(
                            _currentSchool.Id);

                string adviserName =
                    !string.IsNullOrWhiteSpace(
                        SelectedClass.Adviser?.FullName)
                        ? SelectedClass.Adviser!.FullName
                        : activeTeacher?.FullName
                          ?? string.Empty;

                SF2ExportRequest request =
                    new SF2ExportRequest
                    {
                        SchoolId =
                            _currentSchool.SchoolId,

                        SchoolName =
                            _currentSchool.SchoolName,

                        SchoolYear =
                            _currentAcademicYear.DisplayName,

                        AcademicYearStartYear =
                            _currentAcademicYear.StartYear,

                        AcademicYearEndYear =
                            _currentAcademicYear.EndYear,

                        GradeLevel =
                            SelectedClass.GradeLevel,

                        SectionName =
                            SelectedClass.SectionName,

                        AdviserName =
                            adviserName,

                        SchoolHeadName =
                            _currentSchool.SchoolHead,

                        ReportMonth =
                            SelectedMonth
                    };

                foreach (Learner learner
                         in CurrentLearners)
                {
                    List<LearnerAttendance>
                        learnerRecords =
                            validAttendanceRecords
                                .Where(
                                    record =>
                                        record.LearnerId ==
                                        learner.Id)
                                .ToList();

                    Dictionary<int, string>
                        dailyStatuses =
                            new Dictionary<int, string>();

                    foreach (LearnerAttendance record
                             in learnerRecords)
                    {
                        if (record.AttendanceDay ==
                            null)
                        {
                            continue;
                        }

                        int dayNumber =
                            record.AttendanceDay
                                .AttendanceDate.Day;

                        dailyStatuses[dayNumber] =
                            GetSf2AttendanceMark(
                                record.AttendanceStatus);
                    }

                    int absentDays =
                        learnerRecords.Count(
                            record =>
                                record.AttendanceStatus ==
                                    "Absent");

                    int lateDays =
                        learnerRecords.Count(
                            record =>
                                record.AttendanceStatus ==
                                    "Late");

                    int excusedDays =
                        learnerRecords.Count(
                            record =>
                                record.AttendanceStatus ==
                                    "Excused");

                    request.Learners.Add(
                        new SF2LearnerRow
                        {
                            LearnerId =
                                learner.Id,

                            LearnerName =
                                learner.OfficialName,

                            Sex =
                                learner.Sex,

                            DailyStatuses =
                                dailyStatuses,

                            TotalAbsent =
                                absentDays,

                            TotalLate =
                                lateDays,

                            Remarks =
                                excusedDays > 0
                                    ? $"{excusedDays} " +
                                      $"excused day(s)"
                                    : string.Empty,

                            EnrollmentDate =
                                learner.EnrollmentDate,

                            EnrollmentType =
                                learner.EnrollmentType
                                ?? "Regular",

                            PreviousSchoolName =
                                learner.PreviousSchoolName
                                ?? string.Empty,

                            ExitDate =
                                learner.ExitDate,

                            ExitReason =
                                learner.ExitReason
                                ?? string.Empty,

                            NextSchoolName =
                                learner.NextSchoolName
                                ?? string.Empty
                        });
                }

                string suggestedFileName =
                    CreateSafeSf2FileName(
                        $"SF2-" +
                        $"{SelectedClass.GradeLevel}-" +
                        $"{SelectedClass.SectionName}-" +
                        $"{SelectedMonth:MMMM-yyyy}.pdf");

                SaveFileDialog saveDialog =
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

                        OverwritePrompt =
                            true,

                        FileName =
                            suggestedFileName
                    };

                bool? result =
                    saveDialog.ShowDialog();

                if (result != true)
                {
                    StatusMessage =
                        "SF2 PDF export was cancelled.";

                    return;
                }

                StatusMessage =
                    "Creating the official SF2 PDF...";

                string exportedPath =
                    _sf2ExportService
                        .ExportOfficialSF2Pdf(
                            request,
                            saveDialog.FileName);

                StatusMessage =
                    "Official SF2 PDF created successfully.";

                _dialogService.ShowInformation(
                    $"The official SF2 PDF was " +
                    $"created successfully.\n\n" +
                    $"{exportedPath}",
                    "SF2 PDF Export Complete");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The SF2 PDF could not be created.";

                _dialogService.ShowError(
                    $"TeachFlex could not create the " +
                    $"official SF2 PDF.\n\n" +
                    $"{errorMessage}",
                    "SF2 PDF Export Error");
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private static string GetSf2AttendanceMark(
            string attendanceStatus)
        {
            return attendanceStatus switch
            {
                "Present" =>
                    "P",

                "Absent" =>
                    "A",

                "Late" =>
                    "L",

                "Excused" =>
                    "E",

                _ =>
                    string.Empty
            };
        }

        private static string CreateSafeSf2FileName(
            string fileName)
        {
            string safeFileName =
                fileName;

            foreach (char invalidCharacter
                     in Path.GetInvalidFileNameChars())
            {
                safeFileName =
                    safeFileName.Replace(
                        invalidCharacter,
                        '-');
            }

            return safeFileName;
        }
    }
}