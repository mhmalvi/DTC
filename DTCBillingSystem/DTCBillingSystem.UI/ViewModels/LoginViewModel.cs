using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;
        private readonly IAuditService _auditService;
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(
            IAuthenticationService authService,
            INavigationService navigationService,
            IAuditService auditService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _auditService = auditService;

            LoginCommand = new AsyncRelayCommand<PasswordBox>(LoginAsync);
            ExitCommand = new RelayCommand(Exit);
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        private async Task LoginAsync(PasswordBox? passwordBox)
        {
            if (passwordBox == null) return;

            try
            {
                ErrorMessage = string.Empty;
                var success = await _authService.LoginAsync(Username, passwordBox.Password);

                if (success)
                {
                    await _auditService.LogActionAsync("Authentication", null, "Login", $"User '{Username}' logged in successfully");
                    await _navigationService.NavigateToMainWindow();
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    await _auditService.LogActionAsync("Authentication", null, "Login", $"Failed login attempt for user '{Username}'");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login";
                await _auditService.LogActionAsync("Authentication", null, "Login", $"Login error for user '{Username}': {ex.Message}");
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
} 