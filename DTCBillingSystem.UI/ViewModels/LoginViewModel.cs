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

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required";
                    return;
                }

                // Log the attempt
                await _auditService.LogAsync("User", "System", 1, "Debug", $"Login attempt for user: {Username}");
                
                var success = await _authenticationService.LoginAsync(Username.Trim(), Password.Trim());
                await _auditService.LogAsync("User", "System", 1, "Debug", "Authentication attempt completed");
                
                if (!success)
                {
                    await _auditService.LogAsync("User", "0", 1, "LoginFailed", $"Failed login attempt for user '{Username}'");
                    ErrorMessage = "Invalid username or password";
                    return;
                }

                var user = await _authenticationService.GetCurrentUserAsync();
                await _auditService.LogAsync("User", "System", 1, "Debug", "GetCurrentUser completed");

                if (user == null)
                {
                    await _auditService.LogAsync("User", "0", 1, "LoginError", "User is null after successful login");
                    ErrorMessage = "Authentication error: User not found";
                    return;
                }

                await _auditService.LogAsync("User", user.Id.ToString(), user.Id, "Login");
                
                // Use dispatcher to ensure UI updates happen on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        _navigationService.NavigateToMain();
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Failed to navigate to main window: {ex.Message}";
                    }
                });
            }
            catch (Exception ex)
            {
                await _auditService.LogAsync("User", "0", 1, "LoginError", $"Error during login: {ex.Message}");
                ErrorMessage = $"An error occurred during login: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
} 