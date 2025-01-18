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
        private readonly IAuditService _auditService;
        private ObservableCollection<Customer> _customers;
        private int _totalCustomers;
        private int _pageSize = 10;
        private int _currentPage = 1;
        private string _searchTerm = string.Empty;
        private bool _isLoading;

        public CustomersViewModel(ICustomerService customerService, IAuditService auditService)
        {
            _customerService = customerService;
            _auditService = auditService;
            _customers = new ObservableCollection<Customer>();
            LoadCustomersCommand = new RelayCommand(async () => await LoadCustomersAsync());
            SearchCommand = new RelayCommand(async () => await SearchCustomersAsync());
            NextPageCommand = new RelayCommand(async () => await LoadNextPageAsync(), CanLoadNextPage);
            PreviousPageCommand = new RelayCommand(async () => await LoadPreviousPageAsync(), CanLoadPreviousPage);
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

        public int TotalCustomers
        {
            get => _totalCustomers;
            set
            {
                _totalCustomers = value;
                OnPropertyChanged();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
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

        public ICommand LoadCustomersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                TotalCustomers = await _customerService.GetTotalCustomersCountAsync();
                var customers = await _customerService.GetCustomersAsync(CurrentPage, PageSize);
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchCustomersAsync()
        {
            try
            {
                IsLoading = true;
                var customers = await _customerService.SearchCustomersAsync(SearchTerm);
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }

        private async Task LoadPreviousPageAsync()
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }

        private bool CanLoadNextPage()
        {
            return CurrentPage * PageSize < TotalCustomers;
        }

        private bool CanLoadPreviousPage()
        {
            return CurrentPage > 1;
        }
    }
} 