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
        private string _searchText = string.Empty;
        private CustomerType? _selectedCustomerType;
        private string _sortBy = "Name";
        private bool _isActive = true;
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalItems;
        private IEnumerable<Customer> _allCustomers;

        public CustomersViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            try
            {
                _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
                _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
                _customers = new ObservableCollection<Customer>();
                _allCustomers = new List<Customer>();

                AddCustomerCommand = new AsyncRelayCommand<object?>(ExecuteAddCustomerAsync);
                EditCustomerCommand = new AsyncRelayCommand<object?>(ExecuteEditCustomerAsync, _ => SelectedCustomer != null);
                ViewBillsCommand = new AsyncRelayCommand<object?>(ExecuteViewBillsAsync, _ => SelectedCustomer != null);
                RefreshCommand = new AsyncRelayCommand<object?>(_ => ExecuteRefreshAsync());
                NextPageCommand = new RelayCommand(ExecuteNextPage, () => CanGoToNextPage);
                PreviousPageCommand = new RelayCommand(ExecutePreviousPage, () => CanGoToPreviousPage);
                ActivateDeactivateCommand = new AsyncRelayCommand<Customer>(ExecuteActivateDeactivateAsync);
                NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);
                NavigateToSettingsCommand = new RelayCommand(ExecuteNavigateToSettings);

                // Load customers asynchronously
                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        await ExecuteRefreshAsync();
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowErrorAsync("Error", $"Failed to load initial customer data: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing CustomersViewModel: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _ = ExecuteRefreshAsync();
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
                    _ = ExecuteRefreshAsync();
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
                    _ = ExecuteRefreshAsync();
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
                    _ = ExecuteRefreshAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                if (SetProperty(ref _itemsPerPage, value))
                {
                    _ = ExecuteRefreshAsync();
                }
            }
        }

        public int TotalPages => (_totalItems + ItemsPerPage - 1) / ItemsPerPage;

        public bool CanGoToNextPage => CurrentPage < TotalPages;

        public bool CanGoToPreviousPage => CurrentPage > 1;

        public IEnumerable<int> PageSizes => new[] { 10, 20, 50, 100 };

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ActivateDeactivateCommand { get; private set; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

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

        private void ExecuteNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
                UpdatePagedCustomers();
            }
        }

        private void ExecutePreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
                UpdatePagedCustomers();
            }
        }

        private void UpdatePagedCustomers()
        {
            var pagedCustomers = _allCustomers
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage);

            Customers = new ObservableCollection<Customer>(pagedCustomers);
        }

        private async Task ExecuteRefreshAsync()
        {
            try
            {
                IsLoading = true;
                var customers = await _customerService.GetAllCustomersAsync();

                // Apply filters
                var filteredCustomers = customers.Where(c =>
                    (string.IsNullOrEmpty(SearchText) ||
                     c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                     c.AccountNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
                    (!IsActive || c.IsActive) &&
                    (!SelectedCustomerType.HasValue || c.CustomerType == SelectedCustomerType.Value));

                // Apply sorting
                filteredCustomers = SortBy switch
                {
                    "Name" => filteredCustomers.OrderBy(c => c.Name),
                    "Account Number" => filteredCustomers.OrderBy(c => c.AccountNumber),
                    "Type" => filteredCustomers.OrderBy(c => c.CustomerType),
                    "Zone" => filteredCustomers.OrderBy(c => c.ZoneCode),
                    _ => filteredCustomers.OrderBy(c => c.Name)
                };

                _allCustomers = filteredCustomers.ToList();
                _totalItems = _allCustomers.Count();

                // Reset to first page when filters change
                CurrentPage = 1;
                UpdatePagedCustomers();

                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to load customers: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteActivateDeactivateAsync(Customer? customer)
        {
            if (customer == null) return;

            try
            {
                var action = customer.IsActive ? "deactivate" : "activate";
                var result = await _dialogService.ShowConfirmationAsync(
                    "Confirm Action",
                    $"Are you sure you want to {action} customer {customer.Name}?");

                if (result)
                {
                    customer.IsActive = !customer.IsActive;
                    await _customerService.UpdateCustomerAsync(customer);
                    await ExecuteRefreshAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to update customer status: {ex.Message}");
            }
        }

        private void ExecuteNavigateToDashboard()
        {
            try
            {
                _navigationService.NavigateToDashboard();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation to Dashboard failed: {ex}");
                _dialogService.ShowErrorAsync("Navigation Error", "Failed to navigate to Dashboard").Wait();
            }
        }

        private void ExecuteNavigateToSettings()
        {
            try
            {
                _navigationService.NavigateToAsync("SettingsView").Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation to Settings failed: {ex}");
                _dialogService.ShowErrorAsync("Navigation Error", "Failed to navigate to Settings").Wait();
            }
        }
    }
} 