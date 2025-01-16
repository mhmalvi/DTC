using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using DTCBillingSystem.UI.Views;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IViewLocator _viewLocator;
        private readonly IServiceProvider _serviceProvider;
        private Frame? _mainFrame;
        private Window? _mainWindow;

        public NavigationService(IViewLocator viewLocator, IServiceProvider serviceProvider)
        {
            Debug.WriteLine("NavigationService constructor started");
            _viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Debug.WriteLine("NavigationService constructor completed");
        }

        public bool CanNavigateBack => _mainFrame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            Debug.WriteLine("NavigationService.Initialize started");
            try
            {
                if (mainFrame == null)
                {
                    Debug.WriteLine("ERROR: mainFrame is null in Initialize");
                    throw new ArgumentNullException(nameof(mainFrame));
                }
                if (mainWindow == null)
                {
                    Debug.WriteLine("ERROR: mainWindow is null in Initialize");
                    throw new ArgumentNullException(nameof(mainWindow));
                }

                _mainFrame = mainFrame;
                _mainWindow = mainWindow;
                Debug.WriteLine("NavigationService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NavigationService.Initialize: {ex.Message}");
                throw;
            }
        }

        public void SetFrame(Frame frame)
        {
            _mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void NavigateTo<T>() where T : class
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                var view = _viewLocator.GetView<T>();
                _mainFrame.Navigate(view);
            }
            catch (Exception ex)
            {
                HandleNavigationError($"Failed to navigate to {typeof(T).Name}", ex);
            }
        }

        public void NavigateTo(Type viewModelType)
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                var view = _viewLocator.GetView(viewModelType);
                _mainFrame.Navigate(view);
            }
            catch (Exception ex)
            {
                HandleNavigationError($"Failed to navigate to {viewModelType.Name}", ex);
            }
        }

        public void NavigateTo<T>(object parameter) where T : class
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                var view = _viewLocator.GetView<T>();
                _mainFrame.Navigate(view, parameter);
            }
            catch (Exception ex)
            {
                HandleNavigationError($"Failed to navigate to {typeof(T).Name}", ex);
            }
        }

        public void NavigateTo(Type viewModelType, object parameter)
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                var view = _viewLocator.GetView(viewModelType);
                _mainFrame.Navigate(view, parameter);
            }
            catch (Exception ex)
            {
                HandleNavigationError($"Failed to navigate to {viewModelType.Name}", ex);
            }
        }

        public async void NavigateToAsync(string viewName)
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                // Execute navigation on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Attempting to navigate to {viewName}");
                        var view = _viewLocator.GetViewByName(viewName);
                        
                        if (view == null)
                            throw new InvalidOperationException($"View {viewName} could not be created");

                        _mainFrame.Navigate(view);
                        System.Diagnostics.Debug.WriteLine($"Successfully navigated to {viewName}");
                    }
                    catch (Exception ex)
                    {
                        var error = $"Failed to navigate to {viewName}: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine(error);
                        MessageBox.Show(
                            error,
                            "Navigation Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                var error = $"Failed to navigate to {viewName}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(error);
                MessageBox.Show(
                    error,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void NavigateBack()
        {
            try
            {
                if (_mainFrame == null)
                    throw new InvalidOperationException("Navigation frame is not initialized");

                if (CanNavigateBack)
                {
                    _mainFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                HandleNavigationError("Failed to navigate back", ex);
            }
        }

        public void NavigateToMain()
        {
            Debug.WriteLine("NavigateToMain started");
            MainWindow? mainWindow = null;
            LoginWindow? loginWindow = null;

            try
            {
                // Ensure we're on the UI thread
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Debug.WriteLine("Not on UI thread, invoking on dispatcher");
                    Application.Current.Dispatcher.Invoke(() => NavigateToMain());
                    return;
                }

                if (Application.Current == null)
                {
                    Debug.WriteLine("ERROR: Application.Current is null");
                    throw new InvalidOperationException("Application.Current is null");
                }

                // Find the login window
                loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                if (loginWindow == null)
                {
                    Debug.WriteLine("ERROR: Login window not found");
                    throw new InvalidOperationException("Login window not found");
                }
                Debug.WriteLine("Login window found");

                try
                {
                    Debug.WriteLine("Creating main window");
                    mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.ShowInTaskbar = true;
                    Debug.WriteLine("Main window created and configured");

                    // Get and validate the frame
                    var frame = mainWindow.FindName("MainFrame") as Frame;
                    if (frame == null)
                    {
                        Debug.WriteLine("ERROR: MainFrame not found in MainWindow");
                        throw new InvalidOperationException("MainFrame not found in MainWindow");
                    }
                    Debug.WriteLine("MainFrame found in main window");

                    // Initialize navigation
                    Initialize(frame, mainWindow);

                    // Create dashboard view before showing window
                    Debug.WriteLine("Creating dashboard view");
                    var dashboardView = _viewLocator.GetViewByName("DashboardView");
                    if (dashboardView == null)
                    {
                        Debug.WriteLine("ERROR: Failed to create DashboardView");
                        throw new InvalidOperationException("Failed to create DashboardView");
                    }
                    Debug.WriteLine("Dashboard view created successfully");

                    // Set up window transition
                    mainWindow.ContentRendered += (s, e) =>
                    {
                        try
                        {
                            Debug.WriteLine("Main window ContentRendered - navigating to dashboard");
                            frame.Navigate(dashboardView);
                            Debug.WriteLine("Navigation to dashboard complete");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error navigating to dashboard: {ex.Message}");
                            HandleNavigationError("Failed to navigate to dashboard", ex);
                        }
                    };

                    // Set as main window
                    Application.Current.MainWindow = mainWindow;
                    Debug.WriteLine("Main window set as Application.Current.MainWindow");

                    // Show main window
                    mainWindow.Show();
                    Debug.WriteLine("Main window shown");

                    // Hide login window
                    loginWindow.Hide();
                    Debug.WriteLine("Login window hidden");

                    // Wait a bit to ensure window transition is smooth
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (mainWindow.IsVisible && mainWindow.IsActive)
                            {
                                loginWindow?.Close();
                                Debug.WriteLine("Login window closed");
                            }
                            else
                            {
                                Debug.WriteLine("WARNING: Main window not visible/active, keeping login window");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error during window transition: {ex.Message}");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating/showing main window: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                var error = $"Failed to navigate to main window: {ex.Message}";
                Debug.WriteLine($"CRITICAL ERROR: {error}\nStack trace: {ex.StackTrace}");
                MessageBox.Show(
                    error,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        public async Task NavigateToMainWindow()
        {
            try
            {
                // Ensure we're on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        NavigateToMain();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in NavigateToMainWindow: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NavigateToMainWindow: {ex.Message}");
                throw;
            }
        }

        private void HandleNavigationError(string message, Exception ex)
        {
            var errorMsg = $"{message}: {ex.Message}";
            Debug.WriteLine($"Navigation Error: {errorMsg}\nStack trace: {ex.StackTrace}");
            MessageBox.Show(
                errorMsg,
                "Navigation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
} 