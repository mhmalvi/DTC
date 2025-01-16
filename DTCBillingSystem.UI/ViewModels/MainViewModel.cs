using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;

        public MainViewModel(
            INavigationService navigationService,
            ICurrentUserService currentUserService)
        {
            _navigationService = navigationService;
            _currentUserService = currentUserService;
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        public ICommand LogoutCommand { get; }

        public string WelcomeMessage => $"Welcome, {_currentUserService.CurrentUser?.Username ?? "Guest"}!";

        private void ExecuteLogout()
        {
            _currentUserService.ClearCurrentUser();
            _navigationService.NavigateToMain();
        }
    }
} 