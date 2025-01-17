using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Frame _mainFrame;
        private Window _mainWindow;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool CanNavigateBack => _mainFrame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public void SetFrame(Frame frame)
        {
            _mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void NavigateTo<T>() where T : class
        {
            var view = _serviceProvider.GetRequiredService<T>();
            _mainFrame?.Navigate(view);
        }

        public void NavigateTo(Type viewModelType)
        {
            var view = _serviceProvider.GetService(viewModelType);
            if (view != null)
            {
                _mainFrame?.Navigate(view);
            }
        }

        public void NavigateTo<T>(object parameter) where T : class
        {
            var view = _serviceProvider.GetRequiredService<T>();
            _mainFrame?.Navigate(view, parameter);
        }

        public void NavigateTo(Type viewModelType, object parameter)
        {
            var view = _serviceProvider.GetService(viewModelType);
            if (view != null)
            {
                _mainFrame?.Navigate(view, parameter);
            }
        }

        public async Task NavigateToAsync(string viewName)
        {
            try
            {
                await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_mainFrame == null)
                        throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

                    switch (viewName?.ToLower())
                    {
                        case "dashboardview":
                            var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                            _mainFrame.Navigate(new DashboardView(dashboardViewModel));
                            break;
                        case "customersview":
                            var customersViewModel = _serviceProvider.GetRequiredService<CustomersViewModel>();
                            _mainFrame.Navigate(new CustomersView(customersViewModel));
                            break;
                        case "settingsview":
                            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                            _mainFrame.Navigate(new SettingsView(settingsViewModel));
                            break;
                        default:
                            throw new ArgumentException($"Unknown view: {viewName}");
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public void NavigateBack()
        {
            if (CanNavigateBack)
            {
                _mainFrame?.GoBack();
            }
        }

        public void NavigateToMain()
        {            try
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                // Close the login window
                if (_mainWindow != null && _mainWindow.GetType() == typeof(LoginWindow))
                {
                    _mainWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to main window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task NavigateToMainWindow()
        {
            await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
            {
                NavigateToMain();
            }));
        }

        public void NavigateToDashboard()
        {
            try
            {
                // First navigate to main window if we're not already there
                if (_mainWindow == null || _mainWindow.GetType() == typeof(LoginWindow))
                {
                    NavigateToMain();
                }

                // Then navigate to dashboard
                var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                var dashboardView = new DashboardView(dashboardViewModel);
                _mainFrame?.Navigate(dashboardView);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task NavigateToDashboardAsync()
        {
            await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
            {
                NavigateToDashboard();
            }));
        }
    }
} 