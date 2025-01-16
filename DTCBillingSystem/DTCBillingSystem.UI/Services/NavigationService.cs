using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Frame? _frame;
        private Window? _mainWindow;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _serviceProvider = serviceProvider;
        }

        public bool CanNavigateBack => _frame?.CanGoBack ?? false;

        public void Initialize(Frame mainFrame, Window mainWindow)
        {
            _frame = mainFrame;
            _mainWindow = mainWindow;
        }

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo<T>() where T : class
        {
            var viewType = typeof(T).Name.Replace("ViewModel", "View");
            NavigateToAsync(viewType);
        }

        public void NavigateTo(Type viewModelType)
        {
            var viewType = viewModelType.Name.Replace("ViewModel", "View");
            NavigateToAsync(viewType);
        }

        public void NavigateTo<T>(object parameter) where T : class
        {
            var viewType = typeof(T).Name.Replace("ViewModel", "View");
            using var scope = _serviceScopeFactory.CreateScope();
            var view = scope.ServiceProvider.GetRequiredService(Type.GetType($"DTCBillingSystem.UI.Views.{viewType}")!);
            if (view is FrameworkElement fe)
            {
                fe.DataContext = parameter;
            }
            _frame?.Navigate(view);
        }

        public void NavigateTo(Type viewModelType, object parameter)
        {
            var viewType = viewModelType.Name.Replace("ViewModel", "View");
            using var scope = _serviceScopeFactory.CreateScope();
            var view = scope.ServiceProvider.GetRequiredService(Type.GetType($"DTCBillingSystem.UI.Views.{viewType}")!);
            if (view is FrameworkElement fe)
            {
                fe.DataContext = parameter;
            }
            _frame?.Navigate(view);
        }

        public void NavigateToAsync(string viewName)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            UserControl view = viewName switch
            {
                "DashboardView" => scope.ServiceProvider.GetRequiredService<DashboardView>(),
                "CustomerView" => scope.ServiceProvider.GetRequiredService<CustomerView>(),
                "BillView" => scope.ServiceProvider.GetRequiredService<BillView>(),
                "PaymentView" => scope.ServiceProvider.GetRequiredService<PaymentView>(),
                "ReportView" => scope.ServiceProvider.GetRequiredService<ReportView>(),
                "SettingsView" => scope.ServiceProvider.GetRequiredService<SettingsView>(),
                _ => throw new ArgumentException($"View {viewName} not found", nameof(viewName))
            };
            _frame?.Navigate(view);
        }

        public void NavigateBack()
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
            }
        }

        public void NavigateToMain()
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            _mainWindow?.Close();
            _mainWindow = mainWindow;
        }

        public async Task NavigateToMainWindow()
        {
            await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                _mainWindow?.Close();
                _mainWindow = mainWindow;
            }));
        }
    }
} 