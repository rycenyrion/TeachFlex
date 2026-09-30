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
        [RelayCommand]
        private async Task
            CreateOfficialAssessmentItemsAsync()
        {
            if (SelectedClass == null)
            {
                _dialogService.ShowWarning(
                    "Select a class first.",
                    "Assessment Setup");

                return;
            }

            if (SelectedSubject == null)
            {
                _dialogService.ShowWarning(
                    "Select a subject first.",
                    "Assessment Setup");

                return;
            }

            if (CurrentPolicy == null)
            {
                _dialogService.ShowWarning(
                    "The grading policy is not available.",
                    "Assessment Setup");

                return;
            }

            if (CurrentPolicy.UsesDescriptiveGrades)
            {
                _dialogService.ShowWarning(
                    "This class uses the descriptive " +
                    "PACE record instead of numerical " +
                    "assessment columns.",
                    "Descriptive Grading");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Preparing official assessment columns...";

                IReadOnlyList<AssessmentItem>
                    allExistingItems =
                        await _assessmentRepository
                            .GetItemsAsync(
                                SelectedClass.Id,
                                SelectedSubject.Id,
                                SelectedTerm);

                List<AssessmentItem>
                    existingComponentItems =
                        allExistingItems
                            .Where(
                                item =>
                                    IsSameComponent(
                                        item.ComponentName,
                                        ActiveComponentName))
                            .ToList();

                bool replaceLegacyDomainLayout =
                    CurrentPolicy
                        .UsesDomainBasedAssessment &&
                    IsLegacyDomainLayout(
                        existingComponentItems);

                if (replaceLegacyDomainLayout)
                {
                    foreach (
                        AssessmentItem oldItem
                        in existingComponentItems)
                    {
                        await _assessmentRepository
                            .ArchiveItemAsync(
                                oldItem.Id);
                    }

                    existingComponentItems.Clear();
                }

                if (existingComponentItems.Count > 0)
                {
                    _dialogService.ShowWarning(
                        $"Assessment columns have already " +
                        $"been prepared for " +
                        $"{ActiveComponentName}, " +
                        $"{SelectedTermText}.",
                        "Assessment Setup");

                    return;
                }

                List<AssessmentItem>
                    assessmentItems =
                        CurrentPolicy
                            .UsesDomainBasedAssessment
                            ? CreateDomainBasedItems()
                            : CreateRegularItems();

                foreach (AssessmentItem assessmentItem
                         in assessmentItems)
                {
                    await _assessmentRepository
                        .SaveItemAsync(
                            assessmentItem);
                }

                StatusMessage =
                    replaceLegacyDomainLayout
                        ? "The old domain totals were replaced " +
                          "with flexible official columns."
                        : $"{ActiveComponentName} assessment " +
                          $"columns created.";

                await LoadRecordAsync();
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not prepare the " +
                    $"assessment columns.\n\n" +
                    $"{errorMessage}",
                    "Assessment Setup Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        [RelayCommand]
        private async Task
            SaveAssessmentSetupAsync()
        {
            if (AssessmentItems.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Create the assessment columns first.",
                    "Assessment Setup");

                return;
            }

            AssessmentItem?
                invalidAssessment =
                    AssessmentItems.FirstOrDefault(
                        item =>
                            item.HighestPossibleScore <
                                0);

            if (invalidAssessment != null)
            {
                _dialogService.ShowWarning(
                    $"The HPS of " +
                    $"{invalidAssessment.AssessmentName} " +
                    $"cannot be negative.",
                    "Invalid Highest Possible Score");

                return;
            }

            bool hasActiveAssessment =
                AssessmentItems.Any(
                    item =>
                        item.HighestPossibleScore >
                            0);

            if (!hasActiveAssessment)
            {
                _dialogService.ShowWarning(
                    "Enter an HPS greater than zero for " +
                    "at least one assessment item.",
                    "Assessment HPS Required");

                return;
            }

            try
            {
                IsBusy =
                    true;

                StatusMessage =
                    "Saving assessment setup...";

                foreach (AssessmentItem assessmentItem
                         in AssessmentItems)
                {
                    assessmentItem.ComponentName =
                        ActiveComponentName;

                    await _assessmentRepository
                        .SaveItemAsync(
                            assessmentItem);
                }

                StatusMessage =
                    $"{ActiveComponentName} assessment " +
                    $"setup saved successfully.";

                await LoadRecordAsync();
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not save the " +
                    $"assessment setup.\n\n" +
                    $"{errorMessage}",
                    "Assessment Setup Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }

        private List<AssessmentItem>
            CreateRegularItems()
        {
            List<AssessmentItem>
                assessmentItems =
                    new List<AssessmentItem>();

            int displayOrder =
                1;

            for (int itemNumber = 1;
                 itemNumber <= 5;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Written Work",
                        "General",
                        $"WW{itemNumber}",
                        displayOrder++));
            }

            for (int itemNumber = 1;
                 itemNumber <= 3;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Performance Task",
                        "General",
                        $"PT{itemNumber}",
                        displayOrder++));
            }

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Summative Test 1",
                    "General",
                    "ST1",
                    displayOrder++));

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Summative Test 2",
                    "General",
                    "ST2",
                    displayOrder++));

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Term Examination",
                    "General",
                    "Term Exam",
                    displayOrder));

            return assessmentItems;
        }

        private List<AssessmentItem>
            CreateDomainBasedItems()
        {
            List<AssessmentItem>
                assessmentItems =
                    new List<AssessmentItem>();

            int displayOrder =
                1;

            for (int itemNumber = 1;
                 itemNumber <= 5;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Written Work",
                        "Cognitive",
                        $"WW-C{itemNumber}",
                        displayOrder++));
            }

            for (int itemNumber = 1;
                 itemNumber <= 5;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Written Work",
                        "Affective",
                        $"WW-A{itemNumber}",
                        displayOrder++));
            }

            for (int itemNumber = 1;
                 itemNumber <= 3;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Performance Task",
                        "Cognitive",
                        $"PT-C{itemNumber}",
                        displayOrder++));
            }

            for (int itemNumber = 1;
                 itemNumber <= 3;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Performance Task",
                        "Affective",
                        $"PT-A{itemNumber}",
                        displayOrder++));
            }

            for (int itemNumber = 1;
                 itemNumber <= 3;
                 itemNumber++)
            {
                assessmentItems.Add(
                    CreateAssessmentItem(
                        "Performance Task",
                        "Behavioral",
                        $"PT-B{itemNumber}",
                        displayOrder++));
            }

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Summative Test 1",
                    "General",
                    "ST1",
                    displayOrder++));

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Summative Test 2",
                    "General",
                    "ST2",
                    displayOrder++));

            assessmentItems.Add(
                CreateAssessmentItem(
                    "Term Examination",
                    "General",
                    "Term Exam",
                    displayOrder));

            return assessmentItems;
        }

        private AssessmentItem
            CreateAssessmentItem(
                string category,
                string assessmentDomain,
                string assessmentName,
                int displayOrder)
        {
            return new AssessmentItem
            {
                SchoolClassId =
                    SelectedClass!.Id,

                SubjectId =
                    SelectedSubject!.Id,

                TermNumber =
                    SelectedTerm,

                ComponentName =
                    ActiveComponentName,

                Category =
                    category,

                AssessmentDomain =
                    assessmentDomain,

                AssessmentName =
                    assessmentName,

                HighestPossibleScore =
                    0,

                DisplayOrder =
                    displayOrder,

                IsActive =
                    true
            };
        }

        private static bool
            IsLegacyDomainLayout(
                IReadOnlyList<AssessmentItem>
                    existingItems)
        {
            if (existingItems.Count == 0)
            {
                return false;
            }

            string[] legacyNames =
            {
                "WW Cognitive",
                "WW Affective",
                "PT Cognitive",
                "PT Affective",
                "PT Behavioral"
            };

            return existingItems.Any(
                item =>
                    legacyNames.Contains(
                        item.AssessmentName,
                        StringComparer
                            .OrdinalIgnoreCase));
        }

        private static bool IsSameComponent(
            string savedComponent,
            string selectedComponent)
        {
            string normalizedSavedComponent =
                string.IsNullOrWhiteSpace(
                    savedComponent)
                    ? "General"
                    : savedComponent.Trim();

            return normalizedSavedComponent.Equals(
                selectedComponent,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}