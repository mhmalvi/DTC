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

                        // Ensure the frame is ready
                        _mainFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                        _mainFrame.NavigationService?.RemoveBackEntry();

                        // Perform the navigation
                        _mainFrame.Navigate(view);
                        System.Diagnostics.Debug.WriteLine($"Successfully navigated to {viewName}");
                        
                        // Ensure the frame is visible and focused
                        _mainFrame.Visibility = System.Windows.Visibility.Visible;
                        _mainFrame.Focus();
                        
                        // Force layout update
                        _mainFrame.UpdateLayout();
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
                        throw;
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);
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
            Debug.WriteLine("NavigateToMain started");
            MainWindow? mainWindow = null;
            LoginWindow? loginWindow = null;

            try
            {
                // Force execution on UI thread
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Debug.WriteLine("Not on UI thread, invoking on dispatcher");
                    Application.Current.Dispatcher.Invoke(() => NavigateToMain());
                    return;
                }

                Debug.WriteLine("Checking Application.Current");
                if (Application.Current == null)
                {
                    Debug.WriteLine("ERROR: Application.Current is null");
                    throw new InvalidOperationException("Application.Current is null");
                }

                // Find and hide the login window first
                Debug.WriteLine("Looking for login window");
                loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                if (loginWindow == null)
                {
                    Debug.WriteLine("ERROR: Login window not found");
                    throw new InvalidOperationException("Login window not found");
                }
                Debug.WriteLine("Login window found");

                // Create main window
                Debug.WriteLine("Creating main window using ViewLocator");
                var viewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
                mainWindow = viewLocator.CreateMainWindow();
                
                if (mainWindow == null)
                {
                    Debug.WriteLine("ERROR: Failed to create MainWindow");
                    throw new InvalidOperationException("Failed to create MainWindow");
                }
                Debug.WriteLine("Main window created successfully");

                // Configure main window
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.ShowInTaskbar = true;
                mainWindow.Title = "DTC Billing System";
                mainWindow.Topmost = false;
                Debug.WriteLine("Main window configured");

                // Set as main window immediately
                Application.Current.MainWindow = mainWindow;
                Debug.WriteLine("Main window set as Application.Current.MainWindow");

                // Get and validate the frame before showing the window
                Debug.WriteLine("Looking for MainFrame in main window");
                var frame = mainWindow.FindName("MainFrame") as Frame;
                if (frame == null)
                {
                    Debug.WriteLine("ERROR: MainFrame not found in MainWindow");
                    throw new InvalidOperationException("MainFrame not found in MainWindow");
                }
                Debug.WriteLine("MainFrame found in main window");

                // Initialize navigation
                Debug.WriteLine("Initializing navigation service with frame");
                Initialize(frame, mainWindow);
                Debug.WriteLine("Navigation service initialized");

                // Show main window first
                Debug.WriteLine("Showing main window");
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
                mainWindow.Focus();
                mainWindow.UpdateLayout();
                Debug.WriteLine("Main window shown and activated");

                // Create and navigate to dashboard view
                Debug.WriteLine("Creating dashboard view");
                var dashboardView = _viewLocator.GetViewByName("DashboardView");
                if (dashboardView == null)
                {
                    Debug.WriteLine("ERROR: Failed to create DashboardView");
                    throw new InvalidOperationException("Failed to create DashboardView");
                }
                Debug.WriteLine("Dashboard view created successfully");

                // Navigate to dashboard
                Debug.WriteLine("Navigating to dashboard");
                frame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                frame.NavigationService?.RemoveBackEntry();
                frame.Navigate(dashboardView);
                frame.UpdateLayout();
                Debug.WriteLine("Navigation to dashboard complete");

                // Hide login window after dashboard is loaded
                Debug.WriteLine("Hiding login window");
                loginWindow.Hide();
                Debug.WriteLine("Login window hidden");

                // Final window activation
                mainWindow.Activate();
                mainWindow.Focus();
                frame.Focus();
                Debug.WriteLine("Final window activation complete");

                // Add a delayed verification
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Debug.WriteLine("Performing delayed verification");
                        
                        if (!mainWindow.IsActive || !mainWindow.IsVisible)
                        {
                            Debug.WriteLine("Window not properly shown, forcing activation");
                            mainWindow.Show();
                            mainWindow.WindowState = WindowState.Normal;
                            mainWindow.Activate();
                            mainWindow.Focus();
                            frame.Focus();
                            mainWindow.UpdateLayout();
                            frame.UpdateLayout();
                        }

                        // Close login window
                        loginWindow?.Close();
                        Debug.WriteLine("Login window closed");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error during delayed verification: {ex.Message}");
                        // Final fallback attempt
                        mainWindow.Show();
                        mainWindow.Activate();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
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

                // Recovery attempt
                if (mainWindow != null)
                {
                    Debug.WriteLine("Attempting to recover by showing main window");
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    mainWindow.Focus();
                }
                else if (loginWindow != null)
                {
                    Debug.WriteLine("Falling back to login window");
                    loginWindow.Show();
                    loginWindow.Activate();
                }
                throw; // Rethrow to be handled by caller
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