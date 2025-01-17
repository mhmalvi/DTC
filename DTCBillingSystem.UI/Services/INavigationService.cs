using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DTCBillingSystem.UI.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : class;
        void NavigateTo(Type viewModelType);
        void NavigateTo<T>(object parameter) where T : class;
        void NavigateTo(Type viewModelType, object parameter);
        Task NavigateToAsync(string viewName);
        void NavigateBack();
        void NavigateToMain();
        Task NavigateToMainWindow();
        bool CanNavigateBack { get; }
        void Initialize(Frame mainFrame, Window mainWindow);
        void SetFrame(Frame frame);
        void NavigateToDashboard();
        Task NavigateToDashboardAsync();
        void NavigateToCustomers();
    }
} 