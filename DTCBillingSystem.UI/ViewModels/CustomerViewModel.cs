using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IAuditService _auditService;
        private string _searchText = string.Empty;
        private string _selectedFloor = string.Empty;
        private Customer? _selectedCustomer;
        private ObservableCollection<Customer> _customers = new();
        private const int SYSTEM_USER_ID = 1;

        public CustomerViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAuditService auditService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _auditService = auditService;

            AddCustomerCommand = new RelayCommand<object>(_ => ExecuteAddCustomer());
            EditCustomerCommand = new RelayCommand<object>(_ => ExecuteEditCustomer(_selectedCustomer));
            ViewBillsCommand = new RelayCommand<object>(_ => ExecuteViewBills(_selectedCustomer));
            DeactivateCustomerCommand = new RelayCommand<object>(_ => ExecuteDeactivateCustomer());
            ActivateCustomerCommand = new RelayCommand<object>(_ => ExecuteActivateCustomer());
            DeleteCustomerCommand = new RelayCommand<object>(_ => ExecuteDeleteCustomer());
            SearchCommand = new RelayCommand<object>(_ => ExecuteSearch());
            RefreshCommand = new RelayCommand<object>(_ => ExecuteRefresh());

            _ = LoadCustomersAsync(); // Fire and forget intentionally for constructor
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public string SelectedFloor
        {
            get => _selectedFloor;
            set
            {
                if (SetProperty(ref _selectedFloor, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand DeactivateCustomerCommand { get; }
        public ICommand ActivateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerService.GetCustomersAsync(1, 100);
                Customers = new ObservableCollection<Customer>(customers);
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to load customers");
            }
        }

        private async void ExecuteSearch()
        {
            try
            {
                var customers = await _customerService.GetCustomersAsync(1, 100);
                var filteredCustomers = customers.Where(c =>
                    string.IsNullOrEmpty(SearchText) ||
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.AccountNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(SelectedFloor))
                {
                    filteredCustomers = filteredCustomers.Where(c => 
                        c.Floor.Equals(SelectedFloor, StringComparison.OrdinalIgnoreCase));
                }

                Customers = new ObservableCollection<Customer>(filteredCustomers);
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to search customers");
            }
        }

        private async void ExecuteRefresh()
        {
            await LoadCustomersAsync();
        }

        private void ExecuteAddCustomer()
        {
            try
            {
                _navigationService.NavigateTo(typeof(CustomerDialog));
            }
            catch (Exception)
            {
                _dialogService.ShowErrorAsync("Error", "Failed to open add customer dialog").Wait();
            }
        }

        private void ExecuteEditCustomer(Customer? customer)
        {
            if (customer == null)
            {
                _dialogService.ShowWarningAsync("Warning", "Please select a customer to edit").Wait();
                return;
            }

            try
            {
                _navigationService.NavigateTo(typeof(CustomerDialog));
            }
            catch (Exception)
            {
                _dialogService.ShowErrorAsync("Error", "Failed to open edit customer dialog").Wait();
            }
        }

        private void ExecuteViewBills(Customer? customer)
        {
            if (customer == null)
            {
                _dialogService.ShowWarningAsync("Warning", "Please select a customer to view bills").Wait();
                return;
            }

            try
            {
                _navigationService.NavigateTo(typeof(CustomerBillsView));
            }
            catch (Exception)
            {
                _dialogService.ShowErrorAsync("Error", "Failed to open customer bills view").Wait();
            }
        }

        private async void ExecuteDeactivateCustomer()
        {
            if (_selectedCustomer == null)
            {
                await _dialogService.ShowWarningAsync("Warning", "Please select a customer to deactivate");
                return;
            }

            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Confirm Deactivation",
                    $"Are you sure you want to deactivate {_selectedCustomer.Name}?");

                if (result)
                {
                    await _customerService.DeactivateCustomerAsync(_selectedCustomer.Id, SYSTEM_USER_ID);
                    await _auditService.LogActivityAsync(
                        "Customer",
                        "Deactivate",
                        SYSTEM_USER_ID,
                        $"Deactivated customer {_selectedCustomer.AccountNumber}");
                    await LoadCustomersAsync();
                }
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to deactivate customer");
            }
        }

        private async void ExecuteActivateCustomer()
        {
            if (_selectedCustomer == null)
            {
                await _dialogService.ShowWarningAsync("Warning", "Please select a customer to activate");
                return;
            }

            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Confirm Activation",
                    $"Are you sure you want to activate {_selectedCustomer.Name}?");

                if (result)
                {
                    await _customerService.ActivateCustomerAsync(_selectedCustomer.Id, SYSTEM_USER_ID);
                    await _auditService.LogActivityAsync(
                        "Customer",
                        "Activate",
                        SYSTEM_USER_ID,
                        $"Activated customer {_selectedCustomer.AccountNumber}");
                    await LoadCustomersAsync();
                }
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to activate customer");
            }
        }

        private async void ExecuteDeleteCustomer()
        {
            if (_selectedCustomer == null)
            {
                await _dialogService.ShowWarningAsync("Warning", "Please select a customer to delete");
                return;
            }

            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Confirm Deletion",
                    $"Are you sure you want to delete {_selectedCustomer.Name}? This action cannot be undone.");

                if (result)
                {
                    await _customerService.DeleteCustomerAsync(_selectedCustomer.Id, SYSTEM_USER_ID);
                    await _auditService.LogActivityAsync(
                        "Customer",
                        "Delete",
                        SYSTEM_USER_ID,
                        $"Deleted customer {_selectedCustomer.AccountNumber}");
                    await LoadCustomersAsync();
                }
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to delete customer");
            }
        }
    }
} 