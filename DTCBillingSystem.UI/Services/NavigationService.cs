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

                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
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
            try
            {
                if (Application.Current == null)
                    throw new InvalidOperationException("Application.Current is null");

                // Create and configure main window first
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.ShowInTaskbar = true;

                // Find the login window
                var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();

                // Set the main window as the application's main window
                Application.Current.MainWindow = mainWindow;

                // Show the main window without setting ownership
                mainWindow.Show();
                mainWindow.Activate();

                // If login window exists, hide and close it on a separate dispatcher call
                if (loginWindow != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            loginWindow.Hide();
                            loginWindow.Close();
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't throw, as the main window is already shown
                            MessageBox.Show(
                                $"Warning: Failed to close login window properly. The application will continue to work normally.\n\nDetails: {ex.Message}",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                HandleNavigationError("Failed to navigate to main window", ex);
                throw; // Rethrow to ensure the error is properly handled up the stack
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
            var stackTrace = ex.StackTrace ?? "No stack trace available";

            MessageBox.Show(
                $"{errorMessage}\n\nStack trace:\n{stackTrace}",
                "Navigation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
} 