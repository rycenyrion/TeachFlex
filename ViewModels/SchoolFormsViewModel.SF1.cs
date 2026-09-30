using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class SchoolFormsViewModel
    {
        [RelayCommand]
        private void ExportSf1()
        {
            if (!CanExportSf1() ||
                _currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class containing 1 to 50 learners.",
                    "Export SF1 to PDF");

                return;
            }

            string suggestedFileName =
                CreateSafeSf1FileName(
                    $"SF1-" +
                    $"{SelectedClass.GradeLevel}-" +
                    $"{SelectedClass.SectionName}-" +
                    $"{SchoolYearText}.pdf");

            SaveFileDialog saveDialog =
                new SaveFileDialog
                {
                    Title =
                        "Export Official DepEd SF1 to PDF",

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
                IsBusy =
                    true;

                StatusMessage =
                    "Creating the official SF1 PDF...";

                SF1ExportRequest request =
                    CreateSf1ExportRequest();

                string exportedPath =
                    _sf1ExportService
                    .ExportOfficialSF1Pdf(
                        request,
                        saveDialog.FileName);
    
                StatusMessage =
                    "Official SF1 PDF created successfully.";

                _dialogService.ShowInformation(
                    $"The official SF1 PDF was " +
                    $"created successfully.\n\n" +
                    $"{exportedPath}",
                    "SF1 PDF Export Complete");
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                StatusMessage =
                    "The SF1 PDF could not be created.";

                _dialogService.ShowError(
                    $"TeachFlex could not create the " +
                    $"official SF1 PDF.\n\n" +
                    $"{errorMessage}",
                    "SF1 PDF Export Error");
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private bool CanExportSf1()
        {
            return !IsBusy &&
                   CanPrepareSf1;
        }

        private SF1ExportRequest
            CreateSf1ExportRequest()
        {
            if (_currentSchool == null ||
                _currentAcademicYear == null ||
                SelectedClass == null)
            {
                throw new InvalidOperationException(
                    "School, School Year, and class " +
                    "information are required.");
            }

            SF1ExportRequest request =
                new SF1ExportRequest
                {
                    SchoolId =
                        _currentSchool.SchoolId,

                    SchoolName =
                        _currentSchool.SchoolName,

                    Division =
                        _currentSchool.Division,

                    District =
                        _currentSchool.District,

                    SchoolYear =
                        _currentAcademicYear.DisplayName,

                    SchoolYearStartYear =
                        _currentAcademicYear.StartYear,

                    GradeLevel =
                        SelectedClass.GradeLevel,

                    SectionName =
                        SelectedClass.SectionName,

                    AdviserName =
                        SelectedClass.Adviser?
                            .FullName
                        ?? string.Empty,

                    SchoolHeadName =
                        _currentSchool.SchoolHead,

                    DatePrepared =
                        DateTime.Today
                };

            DateTime firstFriday =
                GetSf1FirstFridayOfJune(
                    _currentAcademicYear.StartYear);

            foreach (Learner learner
                     in CurrentLearners)
            {
                request.Learners.Add(
                    new SF1LearnerRow
                    {
                        LearnerId =
                            learner.Id,

                        Lrn =
                            learner.Lrn,

                        LearnerName =
                            learner.OfficialName,

                        Sex =
                            learner.Sex,

                        BirthDate =
                            learner.BirthDate,

                        AgeAsOfFirstFriday =
                            CalculateAgeOnDate(
                                learner.BirthDate,
                                firstFriday),

                        MotherTongue =
                            learner.MotherTongue,

                        IndigenousPeopleEthnicGroup =
                            learner
                                .IndigenousPeopleEthnicGroup,

                        Religion =
                            learner.Religion,

                        HouseStreetSitioPurok =
                            learner.HouseStreetSitioPurok,

                        Barangay =
                            learner.Barangay,

                        MunicipalityCity =
                            learner.MunicipalityCity,

                        Province =
                            learner.Province,

                        FatherName =
                            learner.FatherName,

                        MotherMaidenName =
                            learner.MotherMaidenName,

                        GuardianName =
                            learner.GuardianName,

                        GuardianRelationship =
                            learner.GuardianRelationship,

                        ContactNumber =
                            learner
                                .ParentGuardianContactNumber,

                        LearningModality =
                            learner.LearningModality,

                        EnrollmentDate =
                            learner.EnrollmentDate,

                        EnrollmentType =
                            learner.EnrollmentType,

                        PreviousSchoolName =
                            learner.PreviousSchoolName,

                        ExitDate =
                            learner.ExitDate,

                        ExitReason =
                            learner.ExitReason,

                        NextSchoolName =
                            learner.NextSchoolName,

                        IsCctRecipient =
                            learner.IsCctRecipient,

                        CctReferenceNumber =
                            learner.CctReferenceNumber,

                        IsBalikAral =
                            learner.IsBalikAral,

                        LastSchoolAttended =
                            learner.LastSchoolAttended,

                        LastSchoolYearAttended =
                            learner.LastSchoolYearAttended,

                        IsSpecialNeedsEducation =
                            learner.IsSpecialNeedsEducation,

                        SpecialNeedsDetails =
                            learner.SpecialNeedsDetails,

                        IsAccelerated =
                            learner.IsAccelerated,

                        AcceleratedLevel =
                            learner.AcceleratedLevel,

                        AdditionalRemarks =
                            learner.Sf1Remarks
                    });
            }

            return request;
        }

        private static int? CalculateAgeOnDate(
            DateTime? birthDate,
            DateTime referenceDate)
        {
            if (!birthDate.HasValue)
            {
                return null;
            }

            int age =
                referenceDate.Year -
                birthDate.Value.Year;

            if (birthDate.Value.Date >
                referenceDate.AddYears(
                    -age))
            {
                age--;
            }

            return age;
        }

        private static DateTime
            GetSf1FirstFridayOfJune(
                int year)
        {
            DateTime date =
                new DateTime(
                    year,
                    6,
                    1);

            while (date.DayOfWeek !=
                   DayOfWeek.Friday)
            {
                date =
                    date.AddDays(
                        1);
            }

            return date;
        }

        private static string
            CreateSafeSf1FileName(
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