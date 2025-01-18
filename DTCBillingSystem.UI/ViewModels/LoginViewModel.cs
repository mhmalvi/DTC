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
        private readonly IUserService _userService;
        private readonly IDialogService _dialogService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private AsyncRelayCommand? _loginCommand;

        public event EventHandler? LoginSuccessful;

        public LoginViewModel(IAuthenticationService authenticationService, INavigationService navigationService, IAuditService auditService, IUserService userService, IDialogService dialogService)
        {
            _authenticationService = authenticationService;
            _navigationService = navigationService;
            _auditService = auditService;
            _userService = userService;
            _dialogService = dialogService;
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
                IsLoading = true;
                ClearError();

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required";
                    return;
                }

                // Log the attempt
                await _auditService.LogActivityAsync(
                    "Authentication",
                    "Login",
                    0,
                    $"Login attempt for user {Username}");

                var success = await _authenticationService.LoginAsync(Username.Trim(), Password.Trim());
                var user = await _userService.GetUserByUsernameAsync(Username.Trim());

                if (success && user != null)
                {
                    await _auditService.LogActivityAsync(
                        "Authentication",
                        "Login",
                        user.Id,
                        $"User {user.Username} logged in successfully");

                    // First raise the login successful event
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    
                    // Then navigate to main window
                    try
                    {
                        await _navigationService.NavigateToMainWindow();
                    }
                    catch (Exception navEx)
                    {
                        ErrorMessage = "Failed to open main window. Please try again.";
                        await _auditService.LogActivityAsync(
                            "Authentication",
                            "Navigation",
                            user.Id,
                            $"Failed to navigate to main window: {navEx.Message}");
                        throw;
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    await _auditService.LogActivityAsync(
                        "Authentication",
                        "Login",
                        0,
                        $"Failed login attempt for user {Username}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                await _auditService.LogActivityAsync(
                    "Authentication",
                    "Login",
                    0,
                    $"Login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 