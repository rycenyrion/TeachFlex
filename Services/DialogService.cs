using System.Windows;

namespace TeachFlex.Services
{
    public interface IDialogService
    {
        void ShowInformation(
            string message,
            string title);

        void ShowWarning(
            string message,
            string title);

        void ShowError(
            string message,
            string title);

        bool Confirm(
            string message,
            string title);
    }

    public class DialogService :
        IDialogService
    {
        public void ShowInformation(
            string message,
            string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowWarning(
            string message,
            string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public void ShowError(
            string message,
            string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public bool Confirm(
            string message,
            string title)
        {
            MessageBoxResult result =
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            return result ==
                MessageBoxResult.Yes;
        }
    }
}