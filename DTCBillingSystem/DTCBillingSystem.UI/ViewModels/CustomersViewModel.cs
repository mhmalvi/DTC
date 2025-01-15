using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomersViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private ObservableCollection<Customer> _customers;
        private bool _isLoading;
        private Customer? _selectedCustomer;

        public CustomersViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _customers = new ObservableCollection<Customer>();

            AddCustomerCommand = new AsyncRelayCommand<object?>(ExecuteAddCustomerAsync);
            EditCustomerCommand = new AsyncRelayCommand<object?>(ExecuteEditCustomerAsync, _ => SelectedCustomer != null);
            ViewBillsCommand = new AsyncRelayCommand<object?>(ExecuteViewBillsAsync, _ => SelectedCustomer != null);
            RefreshCommand = new AsyncRelayCommand<object?>(_ => ExecuteRefreshAsync());

            // Load customers when view model is created
            _ = ExecuteRefreshAsync();
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            private set => SetProperty(ref _customers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    // Refresh command can-execute state
                    (EditCustomerCommand as AsyncRelayCommand<object?>)?.RaiseCanExecuteChanged();
                    (ViewBillsCommand as AsyncRelayCommand<object?>)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task ExecuteAddCustomerAsync(object? _)
        {
            try
            {
                _navigationService.NavigateTo(typeof(CustomerDialog));
                await ExecuteRefreshAsync(); // Refresh list after adding
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to open add customer dialog: {ex.Message}");
            }
        }

        private async Task ExecuteEditCustomerAsync(object? _)
        {
            try
            {
                if (SelectedCustomer == null) return;
                
                _navigationService.NavigateTo(typeof(CustomerDialog), SelectedCustomer);
                await ExecuteRefreshAsync(); // Refresh list after editing
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to open edit customer dialog: {ex.Message}");
            }
        }

        private async Task ExecuteViewBillsAsync(object? _)
        {
            try
            {
                if (SelectedCustomer == null) return;

                _navigationService.NavigateTo(typeof(CustomerBillsView), SelectedCustomer);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to open customer bills view: {ex.Message}");
            }
        }

        private async Task ExecuteRefreshAsync()
        {
            try
            {
                IsLoading = true;
                var customers = await _customerService.GetAllCustomersAsync();
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to refresh customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 