using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TeachFlex.ViewModels;

namespace TeachFlex.Services
{
    public class NavigationStore :
        ObservableObject
    {
        private ViewModelBase?
            _currentViewModel;

        public ViewModelBase?
            CurrentViewModel
        {
            get => _currentViewModel;

            private set => SetProperty(
                ref _currentViewModel,
                value);
        }

        public void NavigateTo(
            ViewModelBase viewModel)
        {
            ArgumentNullException.ThrowIfNull(
                viewModel);

            if (ReferenceEquals(
                    CurrentViewModel,
                    viewModel))
            {
                return;
            }

            if (CurrentViewModel
                is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }

            CurrentViewModel =
                viewModel;
        }
    }
}