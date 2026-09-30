using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class GradesViewModel
    {
        [RelayCommand]
        private void ExportConsolidatedGrades()
        {
            if (_currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Complete School Setup and select a class first.",
                    "Export Consolidated Grades");

                return;
            }

            if (ConsolidatedGradeRows.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Load the Class Consolidated Grades first.",
                    "Export Consolidated Grades");

                return;
            }

            List<Subject> exportSubjects =
                Subjects
                    .Where(
                        subject =>
                            ConsolidatedGradeRows.Any(
                                row =>
                                    row.SubjectFinalGrades
                                        .ContainsKey(
                                            subject.Id)))
                    .OrderBy(
                        subject =>
                            subject.DisplayOrder)
                    .ThenBy(
                        subject =>
                            subject.SubjectName)
                    .ToList();

            if (exportSubjects.Count == 0)
            {
                _dialogService.ShowWarning(
                    "No numerical subjects are available for export.",
                    "Export Consolidated Grades");

                return;
            }

            string suggestedFileName =
                CreateSafeGradesFileName(
                    $"TeachFlex-Consolidated-Grades-" +
                    $"{SelectedClass.GradeLevel}-" +
                    $"{SelectedClass.SectionName}-" +
                    $"{SchoolYearText}.pdf");

            SaveFileDialog saveDialog =
                new SaveFileDialog
                {
                    Title =
                        "Export Class Consolidated Grades to PDF",

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
                return;
            }

            try
            {
                StatusMessage =
                    "Creating the printable PDF document...";

                GradesExportRequest request =
                    new GradesExportRequest
                    {
                        SchoolName =
                            _currentSchool.SchoolName,

                        SchoolId =
                            _currentSchool.SchoolId,

                        Region =
    _currentSchool.Region,

                        Division =
    _currentSchool.Division,

                        District =
    _currentSchool.District,

                        SchoolAddress =
    _currentSchool.SchoolAddress,

                        SchoolHeadName =
    _currentSchool.SchoolHead,

                        DepEdLogoPath =
    _currentSchool.DepEdLogoPath,

                        SchoolLogoPath =
    _currentSchool.SchoolLogoPath,

                        SchoolYear =
                            SchoolYearText,

                        GradeLevel =
                            SelectedClass.GradeLevel,

                        SectionName =
                            SelectedClass.SectionName,

                        AdviserName =
                            SelectedClass.Adviser?
                                .FullName
                            ?? string.Empty,

                        DatePrepared =
                            DateTime.Now
                    };

                foreach (Subject subject
                         in exportSubjects)
                {
                    request.Subjects.Add(
                        new GradesExportSubject
                        {
                            SubjectId =
                                subject.Id,

                            SubjectName =
                                subject.SubjectName
                        });
                }

                foreach (
                    ConsolidatedLearnerGradeRow row
                    in ConsolidatedGradeRows)
                {
                    request.Learners.Add(
                        new GradesExportLearner
                        {
                            Number =
                                row.Number,

                            Lrn =
                                row.Lrn,

                            LearnerName =
                                row.LearnerName,

                            Sex =
                                row.Sex,

                            SubjectFinalGrades =
                                new Dictionary<int, int?>(
                                    row.SubjectFinalGrades),

                            GeneralAverage =
                                row.GeneralAverage,

                            Remarks =
                                row.Remarks
                        });
                }

                string exportedPath =
                    _gradesPdfExportService
                        .ExportConsolidatedGrades(
                            request,
                            saveDialog.FileName);

                StatusMessage =
                    "Consolidated grades PDF exported successfully.";

                _dialogService.ShowInformation(
                    $"The printable PDF document was " +
                    $"created successfully.\n\n" +
                    $"{exportedPath}",
                    "Export Complete");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The PDF export could not be completed.";

                _dialogService.ShowError(
                    $"TeachFlex could not export the " +
                    $"consolidated grades.\n\n" +
                    $"{errorMessage}",
                    "Grades PDF Export Error");
            }
        }

        private static string
            CreateSafeGradesFileName(
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