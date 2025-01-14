using System.Threading.Tasks;

namespace DTCBillingSystem.UI.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string viewName, object parameter = null);
        Task GoBackAsync();
        bool CanGoBack { get; }
    }
} 