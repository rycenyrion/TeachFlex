using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        public ObservableCollection<ThreeTermGradeRow> ThreeTermSummaryRows
            { get; } = new ObservableCollection<ThreeTermGradeRow>();

        public ObservableCollection<TermSubjectSummarySection>
            TermSubjectSections { get; } = new();

        public bool HasThreeTermSummary => TermSubjectSections.Count > 0;

        [RelayCommand]
        private async Task LoadThreeTermSummaryAsync()
        {
            if (SelectedClass == null || _currentAcademicYear == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.", "Three-Term Grade Summary");
                return;
            }

            SchoolClass schoolClass = SelectedClass;
            AcademicYear academicYear = _currentAcademicYear;
            try
            {
                IsBusy = true;
                NotifyCommandStates();
                StatusMessage = "Preparing all-subject three-term summary...";
                ThreeTermSummaryRows.Clear();
                TermSubjectSections.Clear();
                OnPropertyChanged(nameof(HasThreeTermSummary));

                IReadOnlyList<Subject> assignedSubjects =
                    await _subjectRepository.GetByClassAsync(schoolClass.Id);
                List<Subject> subjects = assignedSubjects
                    .Where(subject => GradingPolicy.Create(
                        schoolClass.GradeLevel, academicYear.StartYear,
                        subject.SubjectName).UsesNumericalGrades)
                    .OrderBy(subject => subject.DisplayOrder)
                    .ThenBy(subject => subject.SubjectName)
                    .ToList();

                if (subjects.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "No numerical subjects are assigned to this class.",
                        "Three-Term Grade Summary");
                    return;
                }

                IReadOnlyList<Learner> loadedLearners =
                    await _learnerRepository.GetByClassAsync(schoolClass.Id);
                List<Learner> learners = loadedLearners
                    .OrderBy(learner =>
                        string.Equals(learner.Sex?.Trim(), "Male",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(learner.Sex?.Trim(), "M",
                            StringComparison.OrdinalIgnoreCase)
                            ? 0
                            : string.Equals(learner.Sex?.Trim(), "Female",
                                StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(learner.Sex?.Trim(), "F",
                                StringComparison.OrdinalIgnoreCase)
                                ? 1 : 2)
                    .ThenBy(learner => learner.LastName,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(learner => learner.FirstName,
                        StringComparer.OrdinalIgnoreCase)
                    .ThenBy(learner => learner.MiddleName,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var rows = new List<ThreeTermGradeRow>();
                var gradeLookup =
                    new Dictionary<(int Term, int SubjectId),
                        Dictionary<int, int?>>();
                foreach (Subject subject in subjects)
                {
                    Dictionary<int, int?> termOne =
                        await _learnerGradeService.GetTermGradesAsync(
                            schoolClass, academicYear, subject, learners, 1);
                    Dictionary<int, int?> termTwo =
                        await _learnerGradeService.GetTermGradesAsync(
                            schoolClass, academicYear, subject, learners, 2);
                    Dictionary<int, int?> termThree =
                        await _learnerGradeService.GetTermGradesAsync(
                            schoolClass, academicYear, subject, learners, 3);
                    gradeLookup[(1, subject.Id)] = termOne;
                    gradeLookup[(2, subject.Id)] = termTwo;
                    gradeLookup[(3, subject.Id)] = termThree;

                    int learnerNumber = 1;
                    foreach (Learner learner in learners)
                    {
                        termOne.TryGetValue(learner.Id, out int? gradeOne);
                        termTwo.TryGetValue(learner.Id, out int? gradeTwo);
                        termThree.TryGetValue(learner.Id, out int? gradeThree);
                        rows.Add(new ThreeTermGradeRow
                        {
                            Number = learnerNumber++,
                            LearnerId = learner.Id,
                            LearnerName = learner.OfficialName,
                            Sex = learner.Sex,
                            SubjectName = subject.SubjectName,
                            TermOneGrade = gradeOne,
                            TermTwoGrade = gradeTwo,
                            TermThreeGrade = gradeThree
                        });
                    }
                }

                foreach (ThreeTermGradeRow row in rows)
                    ThreeTermSummaryRows.Add(row);

                for (int term = 1; term <= 3; term++)
                {
                    var section = new TermSubjectSummarySection
                    {
                        TermNumber = term,
                        Subjects = subjects.Select(subject =>
                            subject.SubjectName).ToList()
                    };
                    int number = 1;
                    foreach (Learner learner in learners)
                    {
                        var summaryRow = new TermSubjectSummaryRow
                        {
                            Number = number++,
                            LearnerName = learner.OfficialName,
                            Sex = learner.Sex
                        };
                        foreach (Subject subject in subjects)
                        {
                            gradeLookup[(term, subject.Id)].TryGetValue(
                                learner.Id, out int? grade);
                            summaryRow.Grades.Add(
                                new TermSubjectGradeCell { Grade = grade });
                        }
                        section.Rows.Add(summaryRow);
                    }
                    TermSubjectSections.Add(section);
                }

                var finalSection = new TermSubjectSummarySection
                {
                    TermNumber = 4,
                    Subjects = subjects.Select(subject =>
                        subject.SubjectName).ToList()
                };
                int finalNumber = 1;
                foreach (Learner learner in learners)
                {
                    var finalRow = new TermSubjectSummaryRow
                    {
                        Number = finalNumber++,
                        LearnerName = learner.OfficialName,
                        Sex = learner.Sex
                    };
                    foreach (Subject subject in subjects)
                    {
                        gradeLookup[(1, subject.Id)].TryGetValue(
                            learner.Id, out int? first);
                        gradeLookup[(2, subject.Id)].TryGetValue(
                            learner.Id, out int? second);
                        gradeLookup[(3, subject.Id)].TryGetValue(
                            learner.Id, out int? third);
                        int? finalGrade =
                            first.HasValue && second.HasValue && third.HasValue
                                ? (int)Math.Round(
                                    (first.Value + second.Value + third.Value)
                                        / 3.0,
                                    0, MidpointRounding.AwayFromZero)
                                : null;
                        finalRow.Grades.Add(new TermSubjectGradeCell
                        {
                            Grade = finalGrade
                        });
                    }
                    finalSection.Rows.Add(finalRow);
                }
                TermSubjectSections.Add(finalSection);

                OnPropertyChanged(nameof(HasThreeTermSummary));
                StatusMessage =
                    $"Summary prepared for {subjects.Count} subjects.";
            }
            catch (Exception exception)
            {
                ThreeTermSummaryRows.Clear();
                TermSubjectSections.Clear();
                OnPropertyChanged(nameof(HasThreeTermSummary));
                _dialogService.ShowError(
                    "TeachFlex could not prepare the all-subject summary.\n\n" +
                    (exception.InnerException?.Message ?? exception.Message),
                    "Three-Term Grade Summary Error");
            }
            finally
            {
                IsBusy = false;
                NotifyCommandStates();
            }
        }
        private async Task<
            Dictionary<int, double?>>
                LoadTermGradesAsync(
                    int termNumber,
                    IReadOnlyList<Learner>
                        learners,
                    IReadOnlyList<string>
                        componentNames)
        {
            if (SelectedClass == null ||
                SelectedSubject == null)
            {
                return new Dictionary<
                    int,
                    double?>();
            }

            IReadOnlyList<AssessmentItem>
                assessmentItems =
                    await _assessmentRepository
                        .GetItemsAsync(
                            SelectedClass.Id,
                            SelectedSubject.Id,
                            termNumber);

            IReadOnlyList<
                LearnerAssessmentScore>
                    savedScores =
                        await _assessmentRepository
                            .GetScoresAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                termNumber);

            Dictionary<
                (int AssessmentItemId,
                 int LearnerId),
                LearnerAssessmentScore>
                    savedScoreLookup =
                        savedScores
                            .GroupBy(
                                score =>
                                    (
                                        score.AssessmentItemId,
                                        score.LearnerId
                                    ))
                            .ToDictionary(
                                group =>
                                    group.Key,
                                group =>
                                    group
                                        .OrderByDescending(
                                            score =>
                                                score.UpdatedAtUtc)
                                        .First());

            Dictionary<int, double?>
                learnerGrades =
                    new Dictionary<
                        int,
                        double?>();

            if (UsesComponentRecords)
            {
                string componentOneName =
                    componentNames[0];

                string componentTwoName =
                    componentNames[1];

                List<AssessmentItem>
                    componentOneItems =
                        assessmentItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        componentOneName))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                List<AssessmentItem>
                    componentTwoItems =
                        assessmentItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        componentTwoName))
                            .OrderBy(
                                item =>
                                    item.DisplayOrder)
                            .ToList();

                foreach (Learner learner
                         in learners)
                {
                    double? componentOneGrade =
                        CalculateComponentGrade(
                            learner,
                            componentOneItems,
                            savedScoreLookup);

                    double? componentTwoGrade =
                        CalculateComponentGrade(
                            learner,
                            componentTwoItems,
                            savedScoreLookup);

                    double? combinedTermGrade =
                        null;

                    if (componentOneGrade.HasValue &&
                        componentTwoGrade.HasValue)
                    {
                        combinedTermGrade =
                            Math.Round(
                                (
                                    componentOneGrade.Value +
                                    componentTwoGrade.Value
                                ) / 2.0,
                                0,
                                MidpointRounding
                                    .AwayFromZero);
                    }

                    learnerGrades[learner.Id] =
                        combinedTermGrade;
                }

                return learnerGrades;
            }

            List<AssessmentItem>
                generalItems =
                    assessmentItems
                        .Where(
                            item =>
                                IsSameComponent(
                                    item.ComponentName,
                                    "General"))
                        .OrderBy(
                            item =>
                                item.DisplayOrder)
                        .ToList();

            foreach (Learner learner
                     in learners)
            {
                double? termGrade =
                    CalculateComponentGrade(
                        learner,
                        generalItems,
                        savedScoreLookup);

                learnerGrades[learner.Id] =
                    termGrade;
            }

            return learnerGrades;
        }
    }
}
