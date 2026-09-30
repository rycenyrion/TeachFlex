using System.Windows;
using TeachFlex.ViewModels;

namespace TeachFlex
{
    public partial class MainWindow :
        Window
    {
        public MainWindow(
            MainWindowViewModel viewModel)
        {
            InitializeComponent();

            DataContext =
                viewModel;
        }
    }
}