using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace TeachFlex.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void RepositoryLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Unable to open the TeachFlex repository. Please visit https://github.com/rycenyrion/TeachFlex in your browser.",
                    "TeachFlex",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            e.Handled = true;
        }
    }
}