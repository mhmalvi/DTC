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
                Debug.WriteLine($"NavigateToAsync called with view: {viewName}");
                await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
                {
                    EnsureInitialized();

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var serviceProvider = scope.ServiceProvider;
                        switch (viewName?.ToLower())
                        {
                            case "dashboardview":
                                var dashboardViewModel = serviceProvider.GetRequiredService<DashboardViewModel>();
                                var dashboardView = new DashboardView(dashboardViewModel);
                                _mainFrame!.Navigate(dashboardView);
                                break;

                            case "customersview":
                                var customersViewModel = serviceProvider.GetRequiredService<CustomersViewModel>();
                                var customersView = new CustomersView(customersViewModel);
                                _mainFrame!.Navigate(customersView);
                                break;

                            case "settingsview":
                                var settingsViewModel = serviceProvider.GetRequiredService<SettingsViewModel>();
                                var settingsView = new SettingsView(settingsViewModel);
                                _mainFrame!.Navigate(settingsView);
                                break;

                            default:
                                throw new ArgumentException($"Unknown view: {viewName}");
                        }
                    }
                }));
                Debug.WriteLine($"Navigation to {viewName} completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex}");
                MessageBox.Show($"Navigation failed: {ex.Message}\n\nDetails: {ex}", 
                              "Navigation Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
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
            try
            {
                Debug.WriteLine("NavigateToMainWindow called");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Create and show the main window
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                    
                    // Set as the main application window
                    Application.Current.MainWindow = mainWindow;

                    // Close the login window if it exists
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }

                    // Update the current window reference
                    _mainWindow = mainWindow;
                });
                
                Debug.WriteLine("Main window navigation completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NavigateToMainWindow: {ex}");
                MessageBox.Show($"Error navigating to main window: {ex.Message}\n\nDetails: {ex}",
                              "Navigation Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                throw;
            }
        }

        public void NavigateToDashboard()
        {
            try
            {
                Debug.WriteLine("NavigateToDashboard called");
                EnsureInitialized();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var viewModel = serviceProvider.GetRequiredService<DashboardViewModel>();
                    var view = new DashboardView(viewModel);

                    Debug.WriteLine("Navigating to DashboardView");
                    _mainFrame!.Navigate(view);
                    Debug.WriteLine("Navigation completed successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NavigateToDashboard: {ex}");
                MessageBox.Show($"Navigation failed: {ex.Message}\n\nDetails: {ex}", 
                              "Navigation Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
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
                    var serviceProvider = scope.ServiceProvider;
                    var viewModel = serviceProvider.GetRequiredService<CustomersViewModel>();
                    var view = new CustomersView(viewModel);

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
                throw;
            }
        }
    }
} 