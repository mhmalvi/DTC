using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomersViewModel : INotifyPropertyChanged
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private ObservableCollection<Customer> _customers;
        private string _searchText;
        private CustomerType _selectedCustomerType;
        private bool _isActive = true;
        private string _sortBy = "Name";
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalItems;
        private bool _isLoading;
        private string _errorMessage;
        private readonly List<int> _pageSizes = new() { 10, 25, 50, 100 };

        public CustomersViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Initialize collections
            Customers = new ObservableCollection<Customer>();
            
            // Initialize commands
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer);
            EditCustomerCommand = new RelayCommand<Customer>(ExecuteEditCustomer);
            ViewBillsCommand = new RelayCommand<Customer>(ExecuteViewBills);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            DeactivateCustomerCommand = new RelayCommand<Customer>(ExecuteDeactivateCustomer);
            ActivateCustomerCommand = new RelayCommand<Customer>(ExecuteActivateCustomer);
            DeleteCustomerCommand = new RelayCommand<Customer>(ExecuteDeleteCustomer);
            NextPageCommand = new RelayCommand(ExecuteNextPage, CanExecuteNextPage);
            PreviousPageCommand = new RelayCommand(ExecutePreviousPage, CanExecutePreviousPage);
            
            // Load initial data
            LoadCustomers();
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }

        public CustomerType SelectedCustomerType
        {
            get => _selectedCustomerType;
            set
            {
                _selectedCustomerType = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set
            {
                _sortBy = value;
                OnPropertyChanged();
                SortCustomers();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                LoadCustomers();
            }
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                _itemsPerPage = value;
                OnPropertyChanged();
                CurrentPage = 1; // Reset to first page when changing page size
                LoadCustomers();
            }
        }

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public int TotalPages => (TotalItems + ItemsPerPage - 1) / ItemsPerPage;
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;
        public List<int> PageSizes => _pageSizes;

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeactivateCustomerCommand { get; }
        public ICommand ActivateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        private async Task LoadCustomers()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var customers = await _customerService.GetCustomersAsync(
                    CurrentPage,
                    ItemsPerPage,
                    SearchText,
                    SelectedCustomerType,
                    IsActive,
                    SortBy);

                Customers = new ObservableCollection<Customer>(customers);
                TotalItems = await _customerService.GetTotalCustomersCountAsync(
                    SearchText,
                    SelectedCustomerType,
                    IsActive);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading customers. Please try again.";
                await _dialogService.ShowErrorAsync("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterCustomers()
        {
            CurrentPage = 1; // Reset to first page when filtering
            LoadCustomers();
        }

        private void SortCustomers()
        {
            LoadCustomers();
        }

        private async void ExecuteAddCustomer()
        {
            await _navigationService.NavigateToAsync("CustomerDialog", null);
            await LoadCustomers(); // Refresh after navigation returns
        }

        private async void ExecuteEditCustomer(Customer customer)
        {
            if (customer == null) return;
            await _navigationService.NavigateToAsync("CustomerDialog", customer);
            await LoadCustomers(); // Refresh after navigation returns
        }

        private async void ExecuteViewBills(Customer customer)
        {
            if (customer == null) return;
            await _navigationService.NavigateToAsync("CustomerBills", customer);
        }

        private async void ExecuteRefresh()
        {
            await LoadCustomers();
        }

        private async void ExecuteDeactivateCustomer(Customer customer)
        {
            if (customer == null) return;

            var result = await _dialogService.ShowConfirmationAsync(
                "Deactivate Customer",
                $"Are you sure you want to deactivate {customer.Name}?");

            if (result)
            {
                try
                {
                    await _customerService.DeactivateCustomerAsync(customer.Id);
                    await LoadCustomers();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
            }
        }

        private async void ExecuteActivateCustomer(Customer customer)
        {
            if (customer == null) return;

            var result = await _dialogService.ShowConfirmationAsync(
                "Activate Customer",
                $"Are you sure you want to activate {customer.Name}?");

            if (result)
            {
                try
                {
                    await _customerService.ActivateCustomerAsync(customer.Id);
                    await LoadCustomers();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
            }
        }

        private async void ExecuteDeleteCustomer(Customer customer)
        {
            if (customer == null) return;

            var result = await _dialogService.ShowConfirmationAsync(
                "Delete Customer",
                $"Are you sure you want to delete {customer.Name}? This action cannot be undone.");

            if (result)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(customer.Id);
                    await LoadCustomers();
                }
                catch (InvalidOperationException ex)
                {
                    await _dialogService.ShowErrorAsync(
                        "Cannot Delete",
                        "This customer has existing bills or payments and cannot be deleted.");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error", ex.Message);
                }
            }
        }

        private void ExecuteNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
            }
        }

        private void ExecutePreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
            }
        }

        private bool CanExecuteNextPage()
        {
            return CanGoToNextPage;
        }

        private bool CanExecutePreviousPage()
        {
            return CanGoToPreviousPage;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 