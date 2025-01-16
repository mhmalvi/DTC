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

                try
                {
                    // Log the attempt
                    await _auditService.LogAsync("User", "System", 0, "Debug", $"Login attempt for user: {Username}");
                    
                    var success = await _authenticationService.LoginAsync(Username.Trim(), Password.Trim());
                    await _auditService.LogAsync("User", "System", 0, "Debug", "Authentication attempt completed");
                    
                    if (!success)
                    {
                        await _auditService.LogAsync("User", "0", 0, "LoginFailed", $"Failed login attempt for user '{Username}'");
                        ErrorMessage = "Invalid username or password";
                        return;
                    }

                    var user = await _authenticationService.GetCurrentUserAsync();
                    await _auditService.LogAsync("User", "System", 0, "Debug", "GetCurrentUser completed");

                    if (user == null)
                    {
                        await _auditService.LogAsync("User", "0", 0, "LoginError", "User is null after successful login");
                        ErrorMessage = "Authentication error: User not found";
                        return;
                    }

                    await _auditService.LogAsync("User", user.Id.ToString(), user.Id, "Login");
                    
                    try
                    {
                        // Create a TaskCompletionSource to track the navigation
                        var navigationComplete = new TaskCompletionSource<bool>();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                // Attempt to navigate
                                _navigationService.NavigateToMain();
                                navigationComplete.SetResult(true);
                            }
                            catch (Exception ex)
                            {
                                navigationComplete.SetException(ex);
                            }
                        });

                        // Wait for navigation to complete with a timeout
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                        var completedTask = await Task.WhenAny(navigationComplete.Task, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException("Navigation to main window timed out");
                        }

                        // If we got here, navigation was successful
                        await _auditService.LogAsync("User", user.Id.ToString(), user.Id, "Navigation", "Successfully navigated to main window");
                    }
                    catch (Exception ex)
                    {
                        var errorDetails = $"Navigation Error: {ex.Message}\nStack Trace: {ex.StackTrace}";
                        await _auditService.LogAsync("User", user.Id.ToString(), user.Id, "NavigationError", errorDetails);
                        
                        // Force logout since navigation failed
                        await _authenticationService.LogoutAsync();

                        // Show error message
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ErrorMessage = "Failed to open main window. Please try logging in again.";
                            MessageBox.Show(
                                $"Failed to open main window. Error: {ex.Message}",
                                "Navigation Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        });
                    }
                }
                catch (Exception ex)
                {
                    await _auditService.LogAsync("User", "0", 0, "AuthenticationError", $"Authentication process failed: {ex.Message}");
                    ErrorMessage = "Authentication service error. Please try again.";
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Authentication Error: {ex.Message}",
                            "Login Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                }
            }
            catch (Exception ex)
            {
                await _auditService.LogAsync("User", "0", 0, "LoginError", $"Critical error during login: {ex.Message}");
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = "A critical error occurred. Please contact support.";
                    MessageBox.Show(
                        $"Critical Error: {ex.Message}\nStack Trace: {ex.StackTrace}",
                        "Critical Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
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