using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomersViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly ICustomerService _customerService;
        private ObservableCollection<Customer> _customers;
        private bool _isLoading;
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private CustomerType? _selectedCustomerType;
        private string _sortBy = "Name";
        private bool _isActive = true;
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalItems;
        private IEnumerable<Customer> _allCustomers;

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand DeactivateCustomerCommand { get; }
        public ICommand ActivateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public CustomersViewModel(
            IServiceProvider serviceProvider,
            IDialogService dialogService,
            INavigationService navigationService,
            ICustomerService customerService)
        {
            try
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
                _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
                _customers = new ObservableCollection<Customer>();
                _allCustomers = new List<Customer>();

                AddCustomerCommand = new AsyncRelayCommand<object?>(ExecuteAddCustomerAsync);
                EditCustomerCommand = new AsyncRelayCommand<object?>(ExecuteEditCustomerAsync, _ => SelectedCustomer != null);
                ViewBillsCommand = new AsyncRelayCommand<object?>(ExecuteViewBillsAsync, _ => SelectedCustomer != null);
                DeactivateCustomerCommand = new AsyncRelayCommand<object?>(ExecuteDeactivateCustomerAsync, _ => SelectedCustomer?.IsActive == true);
                ActivateCustomerCommand = new AsyncRelayCommand<object?>(ExecuteActivateCustomerAsync, _ => SelectedCustomer?.IsActive == false);
                DeleteCustomerCommand = new AsyncRelayCommand<object?>(ExecuteDeleteCustomerAsync, _ => SelectedCustomer != null);
                SearchCommand = new AsyncRelayCommand<object?>(ExecuteSearchAsync);
                RefreshCommand = new AsyncRelayCommand<object?>(ExecuteRefreshAsync);
                NextPageCommand = new AsyncRelayCommand<object?>(ExecuteNextPageAsync, _ => HasNextPage);
                PreviousPageCommand = new AsyncRelayCommand<object?>(ExecutePreviousPageAsync, _ => HasPreviousPage);

                _ = LoadCustomersAsync(); // Fire and forget intentionally for constructor
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CustomersViewModel constructor: {ex}");
                throw;
            }
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            private set
            {
                if (SetProperty(ref _customers, value))
                {
                    OnPropertyChanged(nameof(HasCustomers));
                }
            }
        }

        public bool HasCustomers => Customers.Any();

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = LoadCustomersAsync();
                }
            }
        }

        public CustomerType? SelectedCustomerType
        {
            get => _selectedCustomerType;
            set
            {
                if (SetProperty(ref _selectedCustomerType, value))
                {
                    _ = LoadCustomersAsync();
                }
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                {
                    _ = LoadCustomersAsync();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    _ = LoadCustomersAsync();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    OnPropertyChanged(nameof(HasPreviousPage));
                    OnPropertyChanged(nameof(HasNextPage));
                }
            }
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (SetProperty(ref _itemsPerPage, value))
                {
                    _ = LoadCustomersAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage * ItemsPerPage < _totalItems;

        private async Task LoadCustomersAsync()
        {
            IsLoading = true;

            try
            {
                _allCustomers = await _customerService.GetCustomersAsync(
                    CurrentPage,
                    ItemsPerPage,
                    SearchText,
                    SelectedCustomerType,
                    IsActive,
                    SortBy);

                _totalItems = await _customerService.GetTotalCustomersCountAsync(
                    SearchText,
                    SelectedCustomerType,
                    IsActive);

                UpdatePagedCustomers();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to load customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteAddCustomerAsync(object? parameter)
        {
            try
            {
                var customerDialogViewModel = _serviceProvider.GetRequiredService<CustomerDialogViewModel>();
                var customerDialog = new CustomerDialog { DataContext = customerDialogViewModel };

                var result = await _dialogService.ShowDialogAsync(customerDialog);
                if (result == true)
                {
                    await LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to add customer: {ex.Message}");
            }
        }

        private async Task ExecuteEditCustomerAsync(object? parameter)
        {
            if (SelectedCustomer == null) return;

            try
            {
                var customerDialogViewModel = _serviceProvider.GetRequiredService<CustomerDialogViewModel>();
                customerDialogViewModel.Initialize(SelectedCustomer);
                var customerDialog = new CustomerDialog { DataContext = customerDialogViewModel };

                var result = await _dialogService.ShowDialogAsync(customerDialog);
                if (result == true)
                {
                    await LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to edit customer: {ex.Message}");
            }
        }

        private async Task ExecuteDeactivateCustomerAsync(object? parameter)
        {
            if (SelectedCustomer == null) return;

            try
            {
                await _customerService.DeactivateCustomerAsync(SelectedCustomer.Id);
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to deactivate customer: {ex.Message}");
            }
        }

        private async Task ExecuteActivateCustomerAsync(object? parameter)
        {
            if (SelectedCustomer == null) return;

            try
            {
                await _customerService.ActivateCustomerAsync(SelectedCustomer.Id);
                await LoadCustomersAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to activate customer: {ex.Message}");
            }
        }

        private async Task ExecuteDeleteCustomerAsync(object? parameter)
        {
            if (SelectedCustomer == null) return;

            try
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Confirm Delete",
                    $"Are you sure you want to delete customer {SelectedCustomer.Name}?");

                if (result)
                {
                    await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
                    await LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to delete customer: {ex.Message}");
            }
        }

        private async Task ExecuteSearchAsync(object? parameter)
        {
            await LoadCustomersAsync();
        }

        private async Task ExecuteRefreshAsync(object? parameter)
        {
            await LoadCustomersAsync();
        }

        private async Task ExecuteNextPageAsync(object? parameter)
        {
            if (HasNextPage)
            {
                CurrentPage++;
                await LoadCustomersAsync();
            }
        }

        private async Task ExecutePreviousPageAsync(object? parameter)
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                await LoadCustomersAsync();
            }
        }

        private void UpdatePagedCustomers()
        {
            Customers = new ObservableCollection<Customer>(_allCustomers);
        }

        private async Task ExecuteViewBillsAsync(object? parameter)
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
    }
} 