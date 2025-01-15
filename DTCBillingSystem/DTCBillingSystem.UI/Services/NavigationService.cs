using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IViewLocator _viewLocator;
        private Frame? _mainFrame;
        private Window? _mainWindow;

        public NavigationService(IViewLocator viewLocator)
        {
            _viewLocator = viewLocator;
        }

        public bool CanNavigateBack => _mainFrame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            _mainFrame = mainFrame;
            _mainWindow = mainWindow;
        }

        public void SetFrame(Frame frame)
        {
            _mainFrame = frame;
        }

        public void NavigateTo<T>() where T : class
        {
            var view = _viewLocator.GetView<T>();
            _mainFrame?.Navigate(view);
        }

        public void NavigateTo(Type viewModelType)
        {
            var view = _viewLocator.GetView(viewModelType);
            _mainFrame?.Navigate(view);
        }

        public async void NavigateToAsync(string viewName)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var view = _viewLocator.GetViewByName(viewName);
                    _mainFrame?.Navigate(view);
                });
            });
        }

        public void NavigateBack()
        {
            if (CanNavigateBack)
            {
                _mainFrame?.GoBack();
            }
        }

        public void NavigateToMain()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = _viewLocator.CreateMainWindow();
                mainWindow.Show();

                // Close other windows
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != mainWindow)
                    {
                        window.Close();
                    }
                }
            });
        }

        public Task NavigateToMainWindow()
        {
            NavigateToMain();
            return Task.CompletedTask;
        }
    }
} 