using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        [RelayCommand]
        private async Task DeleteLearnerAsync()
        {
            if (SelectedLearner == null)
            {
                _dialogService.ShowWarning(
                    "Select a learner from the list first.",
                    "Select Learner");

                return;
            }

            string learnerName =
                SelectedLearner.FullName;

            bool confirmed =
                _dialogService.Confirm(
                    $"Permanently delete {learnerName}?\n\n" +
                    $"This action cannot be undone.",
                    "Delete Learner");

            if (!confirmed)
            {
                return;
            }

            try
            {
                IsBusy =
                    true;

                bool deleted =
                    await _learnerRepository
                        .DeleteAsync(
                            SelectedLearner.Id);

                if (!deleted)
                {
                    _dialogService.ShowWarning(
                        "The selected learner was not found.",
                        "Delete Learner");

                    return;
                }

                ClearForm();

                await LoadLearnersAsync();

                StatusMessage =
                    $"{learnerName} deleted successfully.";
            }
            catch (Exception exception)
            {
                string errorMessage =
                    exception.InnerException?.Message
                    ?? exception.Message;

                _dialogService.ShowError(
                    $"TeachFlex could not delete the " +
                    $"learner.\n\n" +
                    $"{errorMessage}",
                    "Delete Learner Error");
            }
            finally
            {
                IsBusy =
                    false;

                NotifyCommandStates();
            }
        }
    }
}