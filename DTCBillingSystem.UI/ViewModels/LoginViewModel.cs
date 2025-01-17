using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;
using System.Windows;

namespace DTCBillingSystem.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;
        private readonly IAuditService _auditService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private AsyncRelayCommand? _loginCommand;

        public LoginViewModel(IAuthenticationService authenticationService, INavigationService navigationService, IAuditService auditService)
        {
            MessageBox.Show("LoginViewModel constructor called", "Debug Info");
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            _auditService = auditService;
            ExitCommand = new RelayCommand(Exit);
        }

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                OnPropertyChanged(nameof(CanLogin));
                (_loginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                OnPropertyChanged(nameof(CanLogin));
                (_loginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(CanLogin));
                (_loginCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool CanLogin => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        public ICommand LoginCommand => _loginCommand ??= new AsyncRelayCommand(LoginAsync, () => CanLogin);
        public ICommand ExitCommand { get; }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        private async Task LoginAsync()
        {
            try
            {
                MessageBox.Show("Login attempt started", "Debug Info");
                IsLoading = true;
                ClearError();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required";
                    return;
                }

                MessageBox.Show($"Attempting login with username: {Username}", "Debug Info");
                
                // Log the attempt
                await _auditService.LogAsync("User", "System", 1, "Debug", $"Login attempt for user: {Username}");
                
                var success = await _authenticationService.LoginAsync(Username.Trim(), Password.Trim());
                await _auditService.LogAsync("User", "System", 1, "Debug", "Authentication attempt completed");

                if (success)
                {
                    MessageBox.Show("Login successful", "Debug Info");
                    await _navigationService.NavigateToDashboardAsync();
                }
                else
                {
                    MessageBox.Show("Login failed", "Debug Info");
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorMessage = "An error occurred during login. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 