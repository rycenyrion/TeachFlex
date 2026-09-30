using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class GradesViewModel
    {
        private DataView?
            _consolidatedGradesView;

        public ObservableCollection<
            ConsolidatedLearnerGradeRow>
                ConsolidatedGradeRows
        {
            get;
        } = new ObservableCollection<
            ConsolidatedLearnerGradeRow>();

        public DataView? ConsolidatedGradesView
        {
            get => _consolidatedGradesView;

            private set => SetProperty(
                ref _consolidatedGradesView,
                value);
        }

        public int ConsolidatedSubjectCount =>
            Subjects.Count;

        public int ConsolidatedCompleteLearners =>
            ConsolidatedGradeRows.Count(
                row =>
                    row.IsComplete);

        public int ConsolidatedPassedLearners =>
            ConsolidatedGradeRows.Count(
                row =>
                    row.Remarks == "Passed");

        public int ConsolidatedNeedsSupport =>
            ConsolidatedGradeRows.Count(
                row =>
                    row.Remarks == "Failed");

        public string ConsolidatedClassAverageText
        {
            get
            {
                List<int> averages =
                    ConsolidatedGradeRows
                        .Where(
                            row =>
                                row.GeneralAverage.HasValue)
                        .Select(
                            row =>
                                row.GeneralAverage!.Value)
                        .ToList();

                if (averages.Count == 0)
                {
                    return "—";
                }

                return Math.Round(
                        averages.Average(),
                        1,
                        MidpointRounding.AwayFromZero)
                    .ToString("0.0");
            }
        }

        [RelayCommand]
        private async Task
            LoadConsolidatedGradesAsync()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Class Consolidated Grades");

                return;
            }

            if (_currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Create or select the current School Year first.",
                    "Class Consolidated Grades");

                return;
            }

            if (IsKindergartenClass)
            {
                _dialogService.ShowInformation(
                    "Kindergarten uses the prescribed ECD " +
                    "checklist and progress records.",
                    "Kindergarten Grades");

                return;
            }

            List<Subject> subjects =
                Subjects
                    .Where(
                        subject =>
                            subject.IsActive)
                    .OrderBy(
                        subject =>
                            subject.DisplayOrder)
                    .ThenBy(
                        subject =>
                            subject.SubjectName)
                    .ToList();

            if (subjects.Count == 0)
            {
                _dialogService.ShowWarning(
                    "No active subjects are assigned to this class.",
                    "Class Consolidated Grades");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Preparing consolidated grades for all subjects...";

                IReadOnlyList<Learner>
                    learners =
                        await _learnerRepository
                            .GetByClassAsync(
                                SelectedClass.Id);

                List<Subject> numericalSubjects =
                    subjects
                        .Where(
                            subject =>
                                GradingPolicy.Create(
                                    SelectedClass.GradeLevel,
                                    _currentAcademicYear.StartYear,
                                    subject.SubjectName)
                                .UsesNumericalGrades)
                        .ToList();

                if (numericalSubjects.Count == 0)
                {
                    _dialogService.ShowInformation(
                        "This class uses descriptive grading. " +
                        "Use the prescribed PACE or ECD module.",
                        "Descriptive Grading");

                    ClearConsolidatedGrades();

                    return;
                }

                Dictionary<
                    int,
                    Dictionary<int, int?>>
                        subjectGrades =
                            new Dictionary<
                                int,
                                Dictionary<int, int?>>();

                foreach (Subject subject
                         in numericalSubjects)
                {
                    GradingPolicy policy =
                        GradingPolicy.Create(
                            SelectedClass.GradeLevel,
                            _currentAcademicYear.StartYear,
                            subject.SubjectName);

                    Dictionary<int, int?>
                        termOneGrades =
                            await LoadTermGradesAsync(
                                1,
                                learners,
                                policy,
                                subject);

                    Dictionary<int, int?>
                        termTwoGrades =
                            await LoadTermGradesAsync(
                                2,
                                learners,
                                policy,
                                subject);

                    Dictionary<int, int?>
                        termThreeGrades =
                            await LoadTermGradesAsync(
                                3,
                                learners,
                                policy,
                                subject);

                    Dictionary<int, int?>
                        finalGrades =
                            new Dictionary<int, int?>();

                    foreach (Learner learner
                             in learners)
                    {
                        termOneGrades.TryGetValue(
                            learner.Id,
                            out int? termOneGrade);

                        termTwoGrades.TryGetValue(
                            learner.Id,
                            out int? termTwoGrade);

                        termThreeGrades.TryGetValue(
                            learner.Id,
                            out int? termThreeGrade);

                        if (!termOneGrade.HasValue ||
                            !termTwoGrade.HasValue ||
                            !termThreeGrade.HasValue)
                        {
                            finalGrades[learner.Id] =
                                null;

                            continue;
                        }

                        finalGrades[learner.Id] =
                            (int)Math.Round(
                                (
                                    termOneGrade.Value +
                                    termTwoGrade.Value +
                                    termThreeGrade.Value
                                ) / 3.0,
                                0,
                                MidpointRounding.AwayFromZero);
                    }

                    subjectGrades[subject.Id] =
                        finalGrades;
                }

                ConsolidatedGradeRows.Clear();

                int learnerNumber =
                    1;

                foreach (Learner learner
                         in learners)
                {
                    ConsolidatedLearnerGradeRow row =
                        new ConsolidatedLearnerGradeRow
                        {
                            Number =
                                learnerNumber++,

                            LearnerId =
                                learner.Id,

                            Lrn =
                                learner.Lrn,

                            LearnerName =
                                learner.OfficialName,

                            Sex =
                                learner.Sex
                        };

                    foreach (Subject subject
                             in numericalSubjects)
                    {
                        subjectGrades[
                            subject.Id]
                            .TryGetValue(
                                learner.Id,
                                out int? finalGrade);

                        row.SubjectFinalGrades[
                            subject.Id] =
                                finalGrade;
                    }

                    ConsolidatedGradeRows.Add(
                        row);
                }

                BuildConsolidatedDataTable(
                    numericalSubjects);

                NotifyConsolidatedSummary();

                StatusMessage =
                    "Class consolidated grades prepared successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not prepare the " +
                    $"class consolidated grades.\n\n" +
                    $"{errorMessage}",
                    "Class Consolidated Grades Error");

                StatusMessage =
                    "Consolidated grades could not be prepared.";
            }
            finally
            {
                IsBusy =
                    false;
            }
        }

        private void BuildConsolidatedDataTable(
            IReadOnlyList<Subject> subjects)
        {
            DataTable table =
                new DataTable(
                    "ClassConsolidatedGrades");

            table.Columns.Add(
                "No",
                typeof(int));

            table.Columns.Add(
                "LRN",
                typeof(string));

            table.Columns.Add(
                "Learner Name",
                typeof(string));

            table.Columns.Add(
                "Sex",
                typeof(string));

            Dictionary<int, string>
                subjectColumnNames =
                    new Dictionary<int, string>();

            foreach (Subject subject
                     in subjects)
            {
                string columnName =
                    CreateUniqueColumnName(
                        table,
                        subject.SubjectName);

                table.Columns.Add(
                    columnName,
                    typeof(string));

                subjectColumnNames[
                    subject.Id] =
                        columnName;
            }

            table.Columns.Add(
                "General Average",
                typeof(string));

            table.Columns.Add(
                "Remarks",
                typeof(string));

            foreach (
                ConsolidatedLearnerGradeRow row
                in ConsolidatedGradeRows)
            {
                DataRow dataRow =
                    table.NewRow();

                dataRow["No"] =
                    row.Number;

                dataRow["LRN"] =
                    row.Lrn;

                dataRow["Learner Name"] =
                    row.LearnerName;

                dataRow["Sex"] =
                    row.Sex;

                foreach (Subject subject
                         in subjects)
                {
                    row.SubjectFinalGrades
                        .TryGetValue(
                            subject.Id,
                            out int? finalGrade);

                    dataRow[
                        subjectColumnNames[
                            subject.Id]] =
                                finalGrade?.ToString()
                                ?? "—";
                }

                dataRow["General Average"] =
                    row.GeneralAverageText;

                dataRow["Remarks"] =
                    row.Remarks;

                table.Rows.Add(
                    dataRow);
            }

            ConsolidatedGradesView =
                table.DefaultView;
        }

        private static string CreateUniqueColumnName(
            DataTable table,
            string requestedName)
        {
            string baseName =
                string.IsNullOrWhiteSpace(
                    requestedName)
                    ? "Subject"
                    : requestedName.Trim();

            string columnName =
                baseName;

            int duplicateNumber =
                2;

            while (table.Columns.Contains(
                       columnName))
            {
                columnName =
                    $"{baseName} {duplicateNumber++}";
            }

            return columnName;
        }

        private void ClearConsolidatedGrades()
        {
            ConsolidatedGradeRows.Clear();

            ConsolidatedGradesView =
                null;

            NotifyConsolidatedSummary();
        }

        private void NotifyConsolidatedSummary()
        {
            OnPropertyChanged(
                nameof(ConsolidatedSubjectCount));

            OnPropertyChanged(
                nameof(ConsolidatedCompleteLearners));

            OnPropertyChanged(
                nameof(ConsolidatedPassedLearners));

            OnPropertyChanged(
                nameof(ConsolidatedNeedsSupport));

            OnPropertyChanged(
                nameof(
                    ConsolidatedClassAverageText));
        }
    }
}