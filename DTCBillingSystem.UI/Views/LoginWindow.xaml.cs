using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DTCBillingSystem.UI.Views
{
    public partial class LoginWindow : ScopedWindow
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            try
            {
                InitializeComponent();
                _viewModel = ServiceScope.ServiceProvider.GetRequiredService<LoginViewModel>();
                DataContext = _viewModel;

                // Handle password changes
                PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
                
                // Enable window dragging
                MouseDown += Window_MouseDown;

                // Subscribe to login success
                _viewModel.LoginSuccessful += ViewModel_LoginSuccessful;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize login window: {ex.Message}\n\nDetails: {ex}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void LoginInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_viewModel.LoginCommand.CanExecute(null))
                {
                    _viewModel.LoginCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        private void ViewModel_LoginSuccessful(object? sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Login successful event triggered");
                // Don't close the login window until we're sure the main window is ready
                var navigationService = ServiceScope.ServiceProvider.GetRequiredService<INavigationService>();
                Debug.WriteLine("Retrieved navigation service");
                
                navigationService.NavigateToMainWindow().ContinueWith(task =>
                {
                    Debug.WriteLine($"Navigation task completed with status: {task.Status}");
                    if (task.IsFaulted)
                    {
                        Debug.WriteLine($"Navigation failed with error: {task.Exception}");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Failed to open main window: {task.Exception?.InnerException?.Message}",
                                          "Navigation Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                        });
                    }
                    else
                    {
                        Debug.WriteLine("Navigation successful, closing login window");
                        Application.Current.Dispatcher.Invoke(() => Close());
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ViewModel_LoginSuccessful: {ex}");
                MessageBox.Show($"Error during navigation: {ex.Message}",
                              "Navigation Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginViewModel)DataContext).Password = ((PasswordBox)sender).Password;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
} 