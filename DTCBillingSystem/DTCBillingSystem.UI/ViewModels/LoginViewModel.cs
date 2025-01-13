using System;
using System.Windows;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;
        private string _username;
        private string _errorMessage;
        private bool _isLoading;

        public LoginViewModel(IUserService userService, IAuditService auditService)
        {
            _userService = userService;
            _auditService = auditService;
            LoginCommand = new RelayCommand(ExecuteLoginAsync, CanExecuteLogin);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        private bool CanExecuteLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && !IsLoading;
        }

        private async void ExecuteLoginAsync(object parameter)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var passwordBox = parameter as System.Windows.Controls.PasswordBox;
                if (passwordBox == null)
                {
                    ErrorMessage = "Invalid password input";
                    return;
                }

                var user = await _userService.AuthenticateAsync(Username, passwordBox.Password);
                if (user != null)
                {
                    await _auditService.LogActionAsync(
                        "User",
                        user.Id,
                        AuditAction.LoginAttempt,
                        user.Id,
                        "Successful login");

                    // TODO: Store user session and navigate to main window
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Application.Current.MainWindow.Close();
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                    await _auditService.LogActionAsync(
                        "User",
                        0,
                        AuditAction.LoginAttempt,
                        0,
                        $"Failed login attempt for username: {Username}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                await _auditService.LogActionAsync(
                    "User",
                    0,
                    AuditAction.LoginAttempt,
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