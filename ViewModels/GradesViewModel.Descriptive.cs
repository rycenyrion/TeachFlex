using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public sealed class DescriptiveProgressRow
    {
        public int Number { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string TermOne { get; set; } = string.Empty;
        public string TermTwo { get; set; } = string.Empty;
        public string TermThree { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string Status => IsComplete ? "Complete" : "Needs recording";
    }

    public partial class GradesViewModel
    {
        public ObservableCollection<DescriptiveProgressRow> DescriptiveRows { get; }

        private async Task LoadDescriptiveProgressAsync()
        {
            if (SelectedClass == null || (!IsKindergartenClass && SelectedSubject == null))
                return;

            SchoolClass schoolClass = SelectedClass;
            Subject? subject = SelectedSubject;
            bool kindergarten = IsKindergartenClass;
            try
            {
                IsBusy = true;
                StatusMessage = kindergarten
                    ? "Loading saved ECD ratings..."
                    : "Loading saved PACE ratings...";

                IReadOnlyList<Learner> learners = await _learnerRepository
                    .GetByClassAsync(schoolClass.Id);
                var counts = new Dictionary<int, int>[3];
                var totals = new int[3];

                if (kindergarten)
                {
                    IReadOnlyList<KindergartenCompetency> competencies =
                        await _kindergartenRecordRepository.GetCompetenciesAsync();
                    HashSet<int> ids = competencies.Select(item => item.Id).ToHashSet();
                    for (int term = 1; term <= 3; term++)
                    {
                        IReadOnlyList<KindergartenLearnerRating> ratings =
                            await _kindergartenRecordRepository.GetRatingsAsync(
                                schoolClass.Id, term);
                        totals[term - 1] = ids.Count;
                        counts[term - 1] = ratings
                            .Where(item => ids.Contains(item.KindergartenCompetencyId) &&
                                !string.IsNullOrWhiteSpace(item.Rating))
                            .GroupBy(item => item.LearnerId)
                            .ToDictionary(group => group.Key, group => group
                                .Select(item => item.KindergartenCompetencyId).Distinct().Count());
                    }
                }
                else
                {
                    for (int term = 1; term <= 3; term++)
                    {
                        IReadOnlyList<PaceCompetency> competencies =
                            await _paceRepository.GetCompetenciesAsync(subject!.Id, term);
                        HashSet<int> ids = competencies.Select(item => item.Id).ToHashSet();
                        IReadOnlyList<LearnerPaceRating> ratings =
                            await _paceRepository.GetRatingsAsync(
                                schoolClass.Id, subject.Id, term);
                        totals[term - 1] = ids.Count;
                        counts[term - 1] = ratings
                            .Where(item => ids.Contains(item.PaceCompetencyId) &&
                                !string.IsNullOrWhiteSpace(item.Rating))
                            .GroupBy(item => item.LearnerId)
                            .ToDictionary(group => group.Key, group => group
                                .Select(item => item.PaceCompetencyId).Distinct().Count());
                    }
                }

                // A selection change while records are loading must not show
                // results for a different class or subject.
                if (SelectedClass != schoolClass || SelectedSubject != subject)
                    return;

                GradeRows.Clear();
                DescriptiveRows.Clear();
                int number = 1;
                foreach (Learner learner in learners)
                {
                    int[] rated = Enumerable.Range(0, 3)
                        .Select(index => counts[index].GetValueOrDefault(learner.Id))
                        .ToArray();
                    DescriptiveRows.Add(new DescriptiveProgressRow
                    {
                        Number = number++,
                        LearnerName = learner.OfficialName,
                        Sex = learner.Sex,
                        TermOne = $"{rated[0]}/{totals[0]} rated",
                        TermTwo = $"{rated[1]}/{totals[1]} rated",
                        TermThree = $"{rated[2]}/{totals[2]} rated",
                        IsComplete = Enumerable.Range(0, 3).All(index =>
                            totals[index] > 0 && rated[index] >= totals[index])
                    });
                }
                NotifyGradeSummary();
                StatusMessage = learners.Count == 0 ? "No learners found."
                    : kindergarten ? "ECD ratings loaded from Kindergarten ECR."
                    : $"PACE ratings loaded for {subject!.SubjectName}.";
            }
            catch (Exception exception)
            {
                StatusMessage = "Descriptive ratings could not be loaded.";
                _dialogService.ShowError(exception.Message, "Grades Error");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
