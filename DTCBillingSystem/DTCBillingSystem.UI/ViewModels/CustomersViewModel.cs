using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Customer> _customers;
        private string _searchText;
        private CustomerType _selectedCustomerType;
        private bool _isActive;
        private string _sortBy;
        private int _currentPage;
        private int _itemsPerPage;
        private int _totalItems;

        public CustomersViewModel()
        {
            // Initialize collections
            Customers = new ObservableCollection<Customer>();
            
            // Initialize commands
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer);
            EditCustomerCommand = new RelayCommand<Customer>(ExecuteEditCustomer);
            ViewBillsCommand = new RelayCommand<Customer>(ExecuteViewBills);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            // Set default values
            ItemsPerPage = 10;
            CurrentPage = 1;
            
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
            }
        }

        public int TotalPages => (TotalItems + ItemsPerPage - 1) / ItemsPerPage;

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void LoadCustomers()
        {
            try
            {
                // TODO: Implement actual data loading from service
                // var customers = await _customerService.GetCustomersAsync(
                //     CurrentPage, 
                //     ItemsPerPage, 
                //     SearchText, 
                //     SelectedCustomerType, 
                //     IsActive, 
                //     SortBy);
                
                // Customers = new ObservableCollection<Customer>(customers);
                // TotalItems = await _customerService.GetTotalCustomersCountAsync();
            }
            catch (Exception ex)
            {
                // TODO: Handle error and show message to user
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        private void FilterCustomers()
        {
            LoadCustomers(); // Reload with new filters
        }

        private void SortCustomers()
        {
            LoadCustomers(); // Reload with new sort
        }

        private void ExecuteAddCustomer()
        {
            // TODO: Implement navigation to Add Customer view
        }

        private void ExecuteEditCustomer(Customer customer)
        {
            if (customer == null) return;
            // TODO: Implement navigation to Edit Customer view
        }

        private void ExecuteViewBills(Customer customer)
        {
            if (customer == null) return;
            // TODO: Implement navigation to Customer Bills view
        }

        private void ExecuteRefresh()
        {
            LoadCustomers();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => 
            parameter is T typedParameter && (_canExecute?.Invoke(typedParameter) ?? true);

        public void Execute(object parameter)
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
        }
    }
} 