using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using DTCBillingSystem.UI.Views;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

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
            _viewLocator = viewLocator;
            _serviceProvider = serviceProvider;
        }

        public bool CanNavigateBack => _mainFrame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            if (mainFrame == null)
                throw new ArgumentNullException(nameof(mainFrame));
            if (mainWindow == null)
                throw new ArgumentNullException(nameof(mainWindow));

            _mainFrame = mainFrame;
            _mainWindow = mainWindow;
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
                        var view = _viewLocator.GetViewByName(viewName);
                        _mainFrame.Navigate(view);
                    }
                    catch (Exception ex)
                    {
                        HandleNavigationError($"Failed to navigate to {viewName}", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HandleNavigationError($"Failed to navigate to {viewName}", ex);
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
            MainWindow? mainWindow = null;
            LoginWindow? loginWindow = null;

            try
            {
                // Ensure we're on the UI thread
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() => NavigateToMain());
                    return;
                }

                if (Application.Current == null)
                    throw new InvalidOperationException("Application.Current is null");

                // Find the login window
                loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                if (loginWindow == null)
                {
                    throw new InvalidOperationException("Login window not found");
                }

                try
                {
                    // Create and set up main window
                    mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.ShowInTaskbar = true;

                    // Get and validate the frame
                    var frame = mainWindow.FindName("MainFrame") as Frame;
                    if (frame == null)
                    {
                        throw new InvalidOperationException("MainFrame not found in MainWindow");
                    }

                    // Initialize navigation
                    Initialize(frame, mainWindow);

                    // Create dashboard view before showing window
                    var dashboardView = _viewLocator.GetViewByName("DashboardView");
                    if (dashboardView == null)
                    {
                        throw new InvalidOperationException("Failed to create DashboardView");
                    }

                    // Set up window transition
                    mainWindow.ContentRendered += (s, e) =>
                    {
                        try
                        {
                            // Navigate to dashboard after window is rendered
                            frame.Navigate(dashboardView);
                        }
                        catch (Exception ex)
                        {
                            HandleNavigationError("Failed to navigate to dashboard", ex);
                        }
                    };

                    // Set as main window
                    Application.Current.MainWindow = mainWindow;

                    // Show main window
                    mainWindow.Show();

                    // Hide login window (don't close it yet in case of errors)
                    loginWindow.Hide();

                    // Wait a bit to ensure window transition is smooth
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // Only close login window if main window is visible and active
                            if (mainWindow.IsVisible && mainWindow.IsActive)
                            {
                                loginWindow?.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error closing login window: {ex.Message}");
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    // Clean up on error
                    if (mainWindow != null && mainWindow.IsVisible)
                    {
                        mainWindow.Close();
                    }

                    // Show login window again
                    if (loginWindow != null && !loginWindow.IsVisible)
                    {
                        loginWindow.Show();
                        Application.Current.MainWindow = loginWindow;
                    }

                    throw new InvalidOperationException("Failed to initialize main window", ex);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in NavigateToMain: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show(
                    "Failed to open main window. Please try logging in again.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public Task NavigateToMainWindow()
        {
            NavigateToMain();
            return Task.CompletedTask;
        }

        private void HandleNavigationError(string message, Exception ex)
        {
            var errorMessage = $"{message}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Navigation Error: {errorMessage}\nStack trace: {ex.StackTrace}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    errorMessage,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }
} 