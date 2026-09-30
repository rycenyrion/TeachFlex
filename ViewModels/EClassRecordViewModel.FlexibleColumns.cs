using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class EClassRecordViewModel
    {
        private const int MaximumFlexibleItems =
            10;

        [RelayCommand]
        private async Task AddWrittenWorkAsync()
        {
            await AddFlexibleAssessmentAsync(
                "Written Work");
        }

        [RelayCommand]
        private async Task AddPerformanceTaskAsync()
        {
            await AddFlexibleAssessmentAsync(
                "Performance Task");
        }

        private async Task AddFlexibleAssessmentAsync(
            string category)
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Flexible Assessment Columns");

                return;
            }

            if (SelectedSubject == null)
            {
                _dialogService.ShowWarning(
                    "Select a subject first.",
                    "Flexible Assessment Columns");

                return;
            }

            if (CurrentPolicy == null)
            {
                _dialogService.ShowWarning(
                    "The grading policy is not available.",
                    "Flexible Assessment Columns");

                return;
            }

            if (CurrentPolicy.UsesDescriptiveGrades)
            {
                _dialogService.ShowWarning(
                    "This class uses descriptive grading.",
                    "Flexible Assessment Columns");

                return;
            }

            try
            {
                IsBusy =
                    true;

                NotifyCommandStates();

                StatusMessage =
                    category == "Written Work"
                        ? "Adding another written work..."
                        : "Adding another performance task...";

                IReadOnlyList<AssessmentItem>
                    allExistingItems =
                        await _assessmentRepository
                            .GetItemsAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                SelectedTerm);

                List<AssessmentItem>
                    componentItems =
                        allExistingItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        ActiveComponentName))
                            .ToList();

                if (componentItems.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "Create the official assessment " +
                        "columns first.",
                        "Flexible Assessment Columns");

                    return;
                }

                List<AssessmentItem>
                    categoryItems =
                        componentItems
                            .Where(
                                item =>
                                    item.Category.Equals(
                                        category,
                                        StringComparison
                                            .OrdinalIgnoreCase))
                            .ToList();

                int nextItemNumber =
                    categoryItems.Count == 0
                        ? 1
                        : categoryItems
                            .Select(
                                item =>
                                    GetTrailingNumber(
                                        item.AssessmentName))
                            .DefaultIfEmpty(
                                0)
                            .Max() + 1;

                if (nextItemNumber >
                    MaximumFlexibleItems)
                {
                    string shortCategory =
                        category == "Written Work"
                            ? "WW"
                            : "PT";

                    _dialogService.ShowWarning(
                        $"The maximum allowed number is " +
                        $"{shortCategory}" +
                        $"{MaximumFlexibleItems}.",
                        "Maximum Columns Reached");

                    return;
                }

                IReadOnlyList<string>
                    requiredDomains =
                        GetRequiredDomains(
                            category);

                List<AssessmentItem>
                    newItems =
                        new List<AssessmentItem>();

                foreach (string domain
                         in requiredDomains)
                {
                    string assessmentName =
                        CreateFlexibleAssessmentName(
                            category,
                            domain,
                            nextItemNumber);

                    bool alreadyExists =
                        componentItems.Any(
                            item =>
                                item.AssessmentName.Equals(
                                    assessmentName,
                                    StringComparison
                                        .OrdinalIgnoreCase));

                    if (alreadyExists)
                    {
                        continue;
                    }

                    AssessmentItem newItem =
                        CreateAssessmentItem(
                            category,
                            domain,
                            assessmentName,
                            0);

                    await _assessmentRepository
                        .SaveItemAsync(
                            newItem);

                    newItems.Add(
                        newItem);

                    componentItems.Add(
                        newItem);
                }

                if (newItems.Count == 0)
                {
                    _dialogService.ShowWarning(
                        "The requested assessment columns " +
                        "already exist.",
                        "Flexible Assessment Columns");

                    return;
                }

                await NormalizeAssessmentOrderAsync(
                    componentItems);

                string addedDescription =
                    category == "Written Work"
                        ? CurrentPolicy
                            .UsesDomainBasedAssessment
                            ? $"WW-C{nextItemNumber} and " +
                              $"WW-A{nextItemNumber}"
                            : $"WW{nextItemNumber}"
                        : CurrentPolicy
                            .UsesDomainBasedAssessment
                            ? $"PT-C{nextItemNumber}, " +
                              $"PT-A{nextItemNumber}, and " +
                              $"PT-B{nextItemNumber}"
                            : $"PT{nextItemNumber}";

                StatusMessage =
                    $"{addedDescription} added to " +
                    $"{ActiveComponentName}, " +
                    $"{SelectedTermText}.";

                await LoadRecordAsync();
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not add the " +
                    $"assessment columns.\n\n" +
                    $"{errorMessage}",
                    "Flexible Assessment Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private IReadOnlyList<string>
            GetRequiredDomains(
                string category)
        {
            if (CurrentPolicy?
                .UsesDomainBasedAssessment != true)
            {
                return new[]
                {
                    "General"
                };
            }

            if (category.Equals(
                    "Written Work",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    "Cognitive",
                    "Affective"
                };
            }

            return new[]
            {
                "Cognitive",
                "Affective",
                "Behavioral"
            };
        }

        private static string
            CreateFlexibleAssessmentName(
                string category,
                string domain,
                int itemNumber)
        {
            bool isWrittenWork =
                category.Equals(
                    "Written Work",
                    StringComparison.OrdinalIgnoreCase);

            string prefix =
                isWrittenWork
                    ? "WW"
                    : "PT";

            if (domain.Equals(
                    "Cognitive",
                    StringComparison.OrdinalIgnoreCase))
            {
                return $"{prefix}-C{itemNumber}";
            }

            if (domain.Equals(
                    "Affective",
                    StringComparison.OrdinalIgnoreCase))
            {
                return $"{prefix}-A{itemNumber}";
            }

            if (domain.Equals(
                    "Behavioral",
                    StringComparison.OrdinalIgnoreCase))
            {
                return $"{prefix}-B{itemNumber}";
            }

            return $"{prefix}{itemNumber}";
        }

        private async Task
            NormalizeAssessmentOrderAsync(
                IReadOnlyList<AssessmentItem>
                    assessmentItems)
        {
            List<AssessmentItem>
                orderedItems =
                    assessmentItems
                        .OrderBy(
                            item =>
                                GetCategoryOrder(
                                    item.Category))
                        .ThenBy(
                            item =>
                                GetDomainOrder(
                                    item.Category,
                                    item.AssessmentDomain))
                        .ThenBy(
                            item =>
                                GetTrailingNumber(
                                    item.AssessmentName))
                        .ThenBy(
                            item =>
                                item.AssessmentName,
                            StringComparer
                                .OrdinalIgnoreCase)
                        .ToList();

            int displayOrder =
                1;

            foreach (AssessmentItem item
                     in orderedItems)
            {
                item.DisplayOrder =
                    displayOrder++;

                item.ComponentName =
                    ActiveComponentName;

                await _assessmentRepository
                    .SaveItemAsync(
                        item);
            }
        }

        private static int GetCategoryOrder(
            string category)
        {
            if (category.Equals(
                    "Written Work",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (category.Equals(
                    "Performance Task",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (category.Equals(
                    "Summative Test 1",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (category.Equals(
                    "Summative Test 2",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            if (category.Equals(
                    "Term Examination",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 5;
            }

            return 99;
        }

        private static int GetDomainOrder(
            string category,
            string assessmentDomain)
        {
            if (category.Equals(
                    "Written Work",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (assessmentDomain.Equals(
                        "Cognitive",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }

                if (assessmentDomain.Equals(
                        "Affective",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 2;
                }

                return 0;
            }

            if (category.Equals(
                    "Performance Task",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (assessmentDomain.Equals(
                        "Cognitive",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }

                if (assessmentDomain.Equals(
                        "Affective",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 2;
                }

                if (assessmentDomain.Equals(
                        "Behavioral",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }

                return 0;
            }

            return 0;
        }

        private static int GetTrailingNumber(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return 0;
            }

            string numberText =
                new string(
                    value
                        .Reverse()
                        .TakeWhile(
                            character =>
                                char.IsDigit(
                                    character))
                        .Reverse()
                        .ToArray());

            return int.TryParse(
                numberText,
                out int number)
                    ? number
                    : 0;
        }
    }
}