using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DTCBillingSystem.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private readonly Dictionary<string, Type> _pageTypes;

        public NavigationService(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _pageTypes = new Dictionary<string, Type>();

            // Register pages
            RegisterPage("Customers", typeof(Views.CustomersView));
            RegisterPage("CustomerDialog", typeof(Views.CustomerDialog));
            RegisterPage("CustomerBills", typeof(Views.CustomerBillsView));
        }

        public void RegisterPage(string key, Type pageType)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (pageType == null)
                throw new ArgumentNullException(nameof(pageType));

            _pageTypes[key] = pageType;
        }

        public Task NavigateToAsync(string viewName, object parameter = null)
        {
            if (!_pageTypes.TryGetValue(viewName, out Type pageType))
                throw new ArgumentException($"Page {viewName} is not registered");

            return _frame.Dispatcher.InvokeAsync(() =>
            {
                return _frame.Navigate(pageType, parameter);
            }).Task;
        }

        public Task GoBackAsync()
        {
            if (!CanGoBack)
                return Task.CompletedTask;

            return _frame.Dispatcher.InvokeAsync(() =>
            {
                _frame.GoBack();
                return true;
            }).Task;
        }

        public bool CanGoBack => _frame.CanGoBack;
    }
} 