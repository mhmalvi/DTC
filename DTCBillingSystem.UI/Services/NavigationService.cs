using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Frame? _mainFrame;
        private Window? _mainWindow;
        private bool _isInitialized;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool CanNavigateBack => _mainFrame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _isInitialized = true;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized || _mainFrame == null || _mainWindow == null)
            {
                Debug.WriteLine("NavigationService not initialized");
                throw new InvalidOperationException("NavigationService must be initialized before use");
            }
        }

        public void NavigateTo<T>() where T : class
        {
            EnsureInitialized();
            var view = _serviceProvider.GetRequiredService<T>();
            _mainFrame!.Navigate(view);
        }

        public void NavigateTo(Type viewModelType)
        {
            EnsureInitialized();
            var view = _serviceProvider.GetService(viewModelType);
            if (view != null)
            {
                _mainFrame!.Navigate(view);
            }
        }

        public void NavigateTo<T>(object parameter) where T : class
        {
            EnsureInitialized();
            var view = _serviceProvider.GetRequiredService<T>();
            _mainFrame!.Navigate(view, parameter);
        }

        public void NavigateTo(Type viewModelType, object parameter)
        {
            EnsureInitialized();
            var view = _serviceProvider.GetService(viewModelType);
            if (view != null)
            {
                _mainFrame!.Navigate(view, parameter);
            }
        }

        public async Task NavigateToAsync(string viewName)
        {
            try
            {
                await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    EnsureInitialized();

                    switch (viewName?.ToLower())
                    {
                        case "dashboardview":
                            var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                            _mainFrame!.Navigate(new DashboardView(dashboardViewModel));
                            break;
                        case "customersview":
                            var customersViewModel = _serviceProvider.GetRequiredService<CustomersViewModel>();
                            _mainFrame!.Navigate(new CustomersView(customersViewModel));
                            break;
                        case "settingsview":
                            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                            _mainFrame!.Navigate(new SettingsView(settingsViewModel));
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
            EnsureInitialized();
            if (CanNavigateBack)
            {
                _mainFrame!.GoBack();
            }
        }

        public void NavigateToMain()
        {
            try
            {
                var mainWindow = new MainWindow(_serviceProvider);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                // Close the login window if it exists
                if (_mainWindow != null && _mainWindow is LoginWindow loginWindow)
                {
                    loginWindow.Close();
                }

                // Update the current window reference
                _mainWindow = mainWindow;
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
                if (_mainWindow == null || _mainWindow is LoginWindow)
                {
                    NavigateToMain();
                }

                EnsureInitialized();

                using (var scope = _serviceProvider.CreateScope())
                {
                    // Then navigate to dashboard
                    var dashboardViewModel = scope.ServiceProvider.GetRequiredService<DashboardViewModel>();
                    var dashboardView = new DashboardView(dashboardViewModel);
                    _mainFrame!.Navigate(dashboardView);
                }
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

        public void SetFrame(Frame frame)
        {
            _mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
            _isInitialized = _mainWindow != null;
        }

        public void NavigateToCustomers()
        {
            try
            {
                Debug.WriteLine("NavigateToCustomers called");
                EnsureInitialized();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var viewModel = scope.ServiceProvider.GetRequiredService<CustomersViewModel>();
                    var view = scope.ServiceProvider.GetRequiredService<CustomersView>();
                    view.DataContext = viewModel;

                    Debug.WriteLine("Navigating to CustomersView");
                    _mainFrame!.Navigate(view);
                    Debug.WriteLine("Navigation completed successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NavigateToCustomers: {ex}");
                MessageBox.Show($"Navigation failed: {ex.Message}\n\nDetails: {ex}", 
                              "Navigation Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                throw; // Rethrow to allow higher-level error handling
            }
        }
    }
} 