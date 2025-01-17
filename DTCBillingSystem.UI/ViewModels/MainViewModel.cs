using System;
using System.Windows;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Commands;
using System.Diagnostics;

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
            NavigateToCustomersCommand = new RelayCommand(ExecuteNavigateToCustomers);
        }

        public ICommand LogoutCommand { get; }
        public ICommand NavigateToCustomersCommand { get; }

        public string WelcomeMessage => $"Welcome, {_currentUserService.CurrentUser?.Username ?? "Guest"}!";

        private void ExecuteLogout()
        {
            _currentUserService.ClearCurrentUser();
            _navigationService.NavigateToMain();
        }

        private void ExecuteNavigateToCustomers()
        {
            _navigationService.NavigateToCustomers();
        }

        public void OnClosing()
        {
            // Cleanup resources and save any pending changes
            try
            {
                // Add any cleanup logic here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex}");
            }
        }

        public void OnContentRendered()
        {
            // Initialize any resources needed after the window is rendered
            try
            {
                // Add any initialization logic here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during initialization: {ex}");
            }
        }
    }
} 