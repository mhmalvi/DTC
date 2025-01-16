using System;
using System.Windows;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ICurrentUserService _currentUserService;

        public MainViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            ICurrentUserService currentUserService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        public ICommand LogoutCommand { get; }

        public string WelcomeMessage => $"Welcome, {_currentUserService.CurrentUser?.Username ?? "Guest"}!";

        private void ExecuteLogout()
        {
            _currentUserService.ClearCurrentUser();
            _navigationService.NavigateToMain();
        }

        public void OnContentRendered()
        {
            try
            {
                // Initialize any main window specific state here
                System.Diagnostics.Debug.WriteLine("MainViewModel: Content rendered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainViewModel.OnContentRendered: {ex.Message}");
                MessageBox.Show(
                    $"Error initializing main window: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 