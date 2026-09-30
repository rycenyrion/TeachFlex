using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        [RelayCommand]
        private async Task ExportSf9Async()
        {
            if (!CanPrepareSf9 ||
                _currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null ||
                SelectedSf9Learner == null)
            {
                _dialogService.ShowWarning(
                    "Select a supported Grade 1 to Grade 12 class " +
                    "and a learner first.",
                    "Export SF9");

                return;
            }

            string suggestedFileName =
                CreateSafeSf9FileName(
                    $"SF9-" +
                    $"{SelectedSf9Learner.OfficialName}-" +
                    $"{SelectedClass.GradeLevel}-" +
                    $"{SelectedClass.SectionName}-" +
                    $"{_currentAcademicYear.DisplayName}.pdf");

            SaveFileDialog saveDialog =
                new SaveFileDialog
                {
                    Title =
                        $"Export Official {SelectedClass.GradeLevel} SF9",

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

            bool? dialogResult =
                saveDialog.ShowDialog();

            if (dialogResult != true)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifySchoolFormAvailability();

                StatusMessage =
                    $"Preparing the learner's {SelectedClass.GradeLevel} SF9...";

                SF9ExportRequest request =
                    await CreateSf9ExportRequestAsync();

                StatusMessage =
                    "Creating the official SF9 PDF...";

                string exportedPath =
                    _sf9ExportService
                        .ExportOfficialSf9Pdf(
                            request,
                            saveDialog.FileName);

                StatusMessage =
                    "Official SF9 PDF created successfully.";

                _dialogService.ShowInformation(
                    $"The official {SelectedClass.GradeLevel} " +
                    $"SF9 PDF was created successfully.\n\n" +
                    $"{exportedPath}",
                    "SF9 Export Complete");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The SF9 PDF could not be created.";

                _dialogService.ShowError(
                    $"TeachFlex could not create the " +
                    $"official {SelectedClass.GradeLevel} SF9 PDF.\n\n" +
                    $"{errorMessage}",
                    "SF9 Export Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifySchoolFormAvailability();
            }
        }

        private async Task<SF9ExportRequest>
            CreateSf9ExportRequestAsync()
        {
            if (_currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null ||
                SelectedSf9Learner == null)
            {
                throw new InvalidOperationException(
                    "School, school year, class, and " +
                    "learner information are required.");
            }

            bool isGradeOne =
                SelectedClass.GradeLevel
                    .Replace(
                        "Grade",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim() == "1";

            IReadOnlyList<Subject>
                subjects =
                    await _subjectRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

            if (!isGradeOne && subjects.Count == 0)
            {
                throw new InvalidOperationException(
                    "No subjects are assigned to the " +
                    $"selected {SelectedClass.GradeLevel} class.");
            }

            List<Learner> selectedLearnerList =
                new List<Learner>
                {
                    SelectedSf9Learner
                };

            List<SF9SubjectGradeRow>
                subjectGradeRows =
                    new List<SF9SubjectGradeRow>();

            if (isGradeOne)
            {
                bool previousTermsComplete = true;
                for (int termNumber = 1;
                     termNumber <= 3;
                     termNumber++)
                {
                    IReadOnlyList<LearnerPaceSummary>
                        summaries =
                            await _paceRepository
                                .GetSummariesAsync(
                                    SelectedClass.Id,
                                    termNumber);

                    LearnerPaceSummary? summary =
                        summaries.FirstOrDefault(
                            item =>
                                item.LearnerId ==
                                    SelectedSf9Learner.Id);

                    // A previously generated summary may be stored even
                    // though this term has no completed PACE record.
                    // Only completed terms belong on the printed SF9.
                    bool termComplete =
                        await GetIncompleteGradeOneAreaAsync(termNumber)
                        == string.Empty;
                    if (!previousTermsComplete || !termComplete)
                    {
                        summary = null;
                    }
                    previousTermsComplete &= termComplete;

                    subjectGradeRows.Add(
                        new SF9SubjectGradeRow
                        {
                            SubjectName =
                                $"Grade 1 Term {termNumber} Summary",

                            LearningArea =
                                summary?.WhatLearnerCanDo
                                ?? string.Empty,

                            SubjectCategory =
                                summary?.WhatLearnerNeedsToImprove
                                ?? string.Empty,

                            SubjectCode =
                                summary?.TeacherRemarks
                                ?? string.Empty,

                            DisplayOrder =
                                termNumber
                        });
                }
            }

            foreach (Subject subject
                     in subjects
                         .OrderBy(
                             item =>
                                 item.DisplayOrder)
                         .ThenBy(
                             item =>
                                 item.SubjectName))
            {
                GradingPolicy policy =
                    GradingPolicy.Create(
                        SelectedClass.GradeLevel,
                        _currentAcademicYear.StartYear,
                        subject.SubjectName);

                if (!policy.UsesNumericalGrades)
                {
                    continue;
                }

                Dictionary<int, int?>
                    termOneGrades =
                        await _learnerGradeService
                            .GetTermGradesAsync(
                                SelectedClass,
                                _currentAcademicYear,
                                subject,
                                selectedLearnerList,
                                1);

                Dictionary<int, int?>
                    termTwoGrades =
                        await _learnerGradeService
                            .GetTermGradesAsync(
                                SelectedClass,
                                _currentAcademicYear,
                                subject,
                                selectedLearnerList,
                                2);

                Dictionary<int, int?>
                    termThreeGrades =
                        await _learnerGradeService
                            .GetTermGradesAsync(
                                SelectedClass,
                                _currentAcademicYear,
                                subject,
                                selectedLearnerList,
                                3);

                termOneGrades.TryGetValue(
                    SelectedSf9Learner.Id,
                    out int? termOneGrade);

                termTwoGrades.TryGetValue(
                    SelectedSf9Learner.Id,
                    out int? termTwoGrade);

                termThreeGrades.TryGetValue(
                    SelectedSf9Learner.Id,
                    out int? termThreeGrade);

                int? finalGrade =
                    CalculateSf9FinalGrade(
                        subject,
                        termOneGrade,
                        termTwoGrade,
                        termThreeGrade);

                decimal units =
                    GetSubjectUnits(
                        subject);

                subjectGradeRows.Add(
                    new SF9SubjectGradeRow
                    {
                        SubjectId =
                            subject.Id,

                        SubjectCode =
                            subject.SubjectCode,

                        SubjectName =
                            subject.SubjectName,

                        LearningArea =
                            subject.LearningArea,

                        SubjectCategory =
                            subject.SubjectCategory,

                        DisplayOrder =
                            subject.DisplayOrder,

                        TermOneGrade =
                            termOneGrade,

                        TermTwoGrade =
                            termTwoGrade,

                        TermThreeGrade =
                            termThreeGrade,

                        Units =
                            units,

                        FinalGrade =
                            finalGrade
                    });

                if (policy.UsesMapehComponents)
                {
                    string[] componentNames =
                    {
                        "Music and Arts",
                        "Physical Education and Health"
                    };

                    for (int componentIndex = 0;
                         componentIndex < componentNames.Length;
                         componentIndex++)
                    {
                        string componentName =
                            componentNames[componentIndex];

                        Dictionary<int, int?>
                            componentTermOneGrades =
                                await _learnerGradeService
                                    .GetComponentTermGradesAsync(
                                        SelectedClass,
                                        _currentAcademicYear,
                                        subject,
                                        selectedLearnerList,
                                        1,
                                        componentName);

                        Dictionary<int, int?>
                            componentTermTwoGrades =
                                await _learnerGradeService
                                    .GetComponentTermGradesAsync(
                                        SelectedClass,
                                        _currentAcademicYear,
                                        subject,
                                        selectedLearnerList,
                                        2,
                                        componentName);

                        Dictionary<int, int?>
                            componentTermThreeGrades =
                                await _learnerGradeService
                                    .GetComponentTermGradesAsync(
                                        SelectedClass,
                                        _currentAcademicYear,
                                        subject,
                                        selectedLearnerList,
                                        3,
                                        componentName);

                        componentTermOneGrades.TryGetValue(
                            SelectedSf9Learner.Id,
                            out int? componentTermOneGrade);

                        componentTermTwoGrades.TryGetValue(
                            SelectedSf9Learner.Id,
                            out int? componentTermTwoGrade);

                        componentTermThreeGrades.TryGetValue(
                            SelectedSf9Learner.Id,
                            out int? componentTermThreeGrade);

                        int? componentFinalGrade =
                            CalculateSf9FinalGrade(
                                subject,
                                componentTermOneGrade,
                                componentTermTwoGrade,
                                componentTermThreeGrade);

                        subjectGradeRows.Add(
                            new SF9SubjectGradeRow
                            {
                                SubjectId =
                                    subject.Id,

                                SubjectCode =
                                    subject.SubjectCode,

                                SubjectName =
                                    componentName,

                                LearningArea =
                                    "MAPEH",

                                SubjectCategory =
                                    "MAPEH Component",

                                DisplayOrder =
                                    subject.DisplayOrder * 10 +
                                    componentIndex + 1,

                                TermOneGrade =
                                    componentTermOneGrade,

                                TermTwoGrade =
                                    componentTermTwoGrade,

                                TermThreeGrade =
                                    componentTermThreeGrade,

                                Units =
                                    0,

                                FinalGrade =
                                    componentFinalGrade
                            });
                    }
                }
            }

            List<SF9AttendanceRow>
                attendanceRows =
                    await CreateSf9AttendanceRowsAsync(
                        SelectedClass.Id,
                        SelectedSf9Learner.Id,
                        _currentAcademicYear);

            SF9ExportRequest request =
                new SF9ExportRequest
                {
                    School =
                    _currentSchool,

                    AcademicYear =
                    _currentAcademicYear,

                    SchoolClass =
                    SelectedClass,

                    Learner =
                    SelectedSf9Learner,

                    SubjectGrades =
                    subjectGradeRows,

                    AttendanceRows =
                    attendanceRows,

                    AdviserName =
                    SelectedClass.Adviser?
                        .FullName
                    ?? string.Empty,

                    SchoolHeadName =
                    _currentSchool.SchoolHead,

                    GeneratedOn =
                    DateTime.Now
                };

            int selectedGradeNumber =
                GetSelectedGradeNumber();

            if (selectedGradeNumber >= 2 &&
                selectedGradeNumber <= 12)
            {
                List<SF9TeacherRemarkRow> approvedRemarks =
                    new List<SF9TeacherRemarkRow>();

                for (int termNumber = 1;
                     termNumber <= 3;
                     termNumber++)
                {
                    LearnerTermRemark? savedRemark =
                        await _learnerTermRemarkRepository.GetAsync(
                            SelectedClass.Id,
                            SelectedSf9Learner.Id,
                            termNumber);

                    if (savedRemark == null ||
                        !savedRemark.IsTeacherApproved ||
                        savedRemark.NeedsReview ||
                        string.IsNullOrWhiteSpace(
                            savedRemark.FinalRemark))
                    {
                        continue;
                    }

                    // Never print a previously approved remark after a
                    // subject grade is removed or becomes incomplete.
                    if (!HasCompleteSf9TermGrades(
                            subjectGradeRows, selectedGradeNumber, termNumber))
                        continue;

                    approvedRemarks.Add(
                        new SF9TeacherRemarkRow
                        {
                            TermNumber = termNumber,
                            Remark = savedRemark.FinalRemark.Trim()
                        });
                }

                SF9TeacherRemarksRegistry.Attach(
                    request,
                    approvedRemarks);
            }

            return request;
        }

        private static bool HasCompleteSf9TermGrades(
            IReadOnlyList<SF9SubjectGradeRow> rows, int gradeNumber, int term)
        {
            List<SF9SubjectGradeRow> subjects = rows
                .Where(row => !string.Equals(row.SubjectCategory,
                    "MAPEH Component", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (subjects.Count == 0) return false;

            if (gradeNumber >= 11)
            {
                List<SF9SubjectGradeRow> electives = subjects
                    .Where(row => row.SubjectCategory.Contains("elective",
                        StringComparison.OrdinalIgnoreCase) ||
                        row.SubjectName.Contains("elective",
                            StringComparison.OrdinalIgnoreCase) ||
                        row.SubjectName.Contains("immersion",
                            StringComparison.OrdinalIgnoreCase))
                    .OrderBy(row => row.DisplayOrder)
                    .ThenBy(row => row.SubjectName).ToList();
                int perTerm = gradeNumber == 12 && electives.Count >= 9 ? 4 : 1;
                if (electives.Count < term * perTerm) return false;
                subjects = subjects.Except(electives)
                    .Concat(electives.Skip((term - 1) * perTerm).Take(perTerm))
                    .ToList();
            }

            return subjects.All(row => (term switch
            {
                1 => row.TermOneGrade,
                2 => row.TermTwoGrade,
                3 => row.TermThreeGrade,
                _ => null
            }).HasValue);
        }

        private async Task<List<SF9AttendanceRow>>
            CreateSf9AttendanceRowsAsync(
                int schoolClassId,
                int learnerId,
                AcademicYear academicYear)
        {
            List<SF9AttendanceRow>
                attendanceRows =
                    new List<SF9AttendanceRow>();

            List<(int Year, int Month)>
                schoolYearMonths =
                    new List<(int Year, int Month)>
                    {
                        (
                            academicYear.StartYear,
                            6
                        ),
                        (
                            academicYear.StartYear,
                            7
                        ),
                        (
                            academicYear.StartYear,
                            8
                        ),
                        (
                            academicYear.StartYear,
                            9
                        ),
                        (
                            academicYear.StartYear,
                            10
                        ),
                        (
                            academicYear.StartYear,
                            11
                        ),
                        (
                            academicYear.StartYear,
                            12
                        ),
                        (
                            academicYear.EndYear,
                            1
                        ),
                        (
                            academicYear.EndYear,
                            2
                        ),
                        (
                            academicYear.EndYear,
                            3
                        ),
                        (
                            academicYear.EndYear,
                            4
                        )
                    };

            foreach ((int year, int month)
                     in schoolYearMonths)
            {
                IReadOnlyList<AttendanceDay>
                    monthDays =
                        await _attendanceRepository
                            .GetMonthDaysAsync(
                                schoolClassId,
                                year,
                                month);

                List<AttendanceDay>
                    classDays =
                        monthDays
                            .Where(
                                day =>
                                    IsSf9ClassDay(
                                        day.DayStatus))
                            .ToList();

                HashSet<int> classDayIds =
                    classDays
                        .Select(
                            day =>
                                day.Id)
                        .ToHashSet();

                IReadOnlyList<LearnerAttendance>
                    monthAttendances =
                        await _attendanceRepository
                            .GetMonthAttendancesAsync(
                                schoolClassId,
                                year,
                                month);

                List<LearnerAttendance>
                    learnerRecords =
                        monthAttendances
                            .Where(
                                record =>
                                    record.LearnerId ==
                                        learnerId &&
                                    classDayIds.Contains(
                                        record.AttendanceDayId))
                            .ToList();

                int daysPresent =
                    learnerRecords.Count(
                        record =>
                            IsPresentForSf9(
                                record.AttendanceStatus));

                int daysAbsent =
                    learnerRecords.Count(
                        record =>
                            record.AttendanceStatus.Equals(
                                "Absent",
                                StringComparison
                                    .OrdinalIgnoreCase));

                attendanceRows.Add(
                    new SF9AttendanceRow
                    {
                        MonthNumber =
                            month,

                        MonthName =
                            new DateTime(
                                year,
                                month,
                                1)
                                .ToString(
                                    "MMMM"),

                        SchoolDays =
                            classDays.Count,

                        DaysPresent =
                            daysPresent,

                        DaysAbsent =
                            daysAbsent
                    });
            }

            return attendanceRows;
        }

        private static int?
            CalculateSf9FinalGrade(
                Subject subject,
                params int?[] termGrades)
        {
            List<int> availableGrades =
                termGrades
                    .Where(
                        grade =>
                            grade.HasValue)
                    .Select(
                        grade =>
                            grade!.Value)
                    .ToList();

            if (availableGrades.Count == 0)
            {
                return null;
            }

            bool isElective =
                IsSf9ElectiveSubject(
                    subject);

            if (!isElective &&
                availableGrades.Count < 3)
            {
                return null;
            }

            int requiredTerms =
                subject.TermsTaught > 0
                    ? Math.Min(
                        subject.TermsTaught,
                        3)
                    : 3;

            if (!isElective &&
                availableGrades.Count <
                    requiredTerms)
            {
                return null;
            }

            return (int)Math.Round(
                availableGrades.Average(),
                0,
                MidpointRounding.AwayFromZero);
        }

        private static decimal GetSubjectUnits(
            Subject subject)
        {
            if (subject.UnitsPerYear > 0)
            {
                return subject.UnitsPerYear;
            }

            if (subject.UnitsPerTerm > 0)
            {
                int termsTaught =
                    subject.TermsTaught > 0
                        ? subject.TermsTaught
                        : 1;

                return subject.UnitsPerTerm *
                       termsTaught;
            }

            return IsSf9ElectiveSubject(
                    subject)
                ? 3m
                : 6m;
        }

        private static bool IsSf9ElectiveSubject(
            Subject subject)
        {
            return ContainsSf9Text(
                       subject.SubjectCategory,
                       "elective") ||
                   ContainsSf9Text(
                       subject.SubjectCluster,
                       "elective") ||
                   ContainsSf9Text(
                       subject.LearningArea,
                       "elective") ||
                   ContainsSf9Text(
                       subject.SubjectName,
                       "elective");
        }

        private static bool IsSf9ClassDay(
            string dayStatus)
        {
            return dayStatus.Equals(
                       "Regular Class Day",
                       StringComparison.OrdinalIgnoreCase) ||
                   dayStatus.Equals(
                       "Make-up Class",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPresentForSf9(
            string attendanceStatus)
        {
            return attendanceStatus.Equals(
                       "Present",
                       StringComparison.OrdinalIgnoreCase) ||
                   attendanceStatus.Equals(
                       "Late",
                       StringComparison.OrdinalIgnoreCase) ||
                   attendanceStatus.Equals(
                       "Excused",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsSf9Text(
            string value,
            string expectedText)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return false;
            }

            return value.Contains(
                expectedText,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string
            CreateSafeSf9FileName(
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

            while (safeFileName.Contains(
                       "--",
                       StringComparison.Ordinal))
            {
                safeFileName =
                    safeFileName.Replace(
                        "--",
                        "-",
                        StringComparison.Ordinal);
            }

            return safeFileName.Trim(
                ' ',
                '-');
        }
    }
}
