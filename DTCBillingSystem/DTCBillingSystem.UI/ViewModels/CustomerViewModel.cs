using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;
using MaterialDesignThemes.Wpf;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private string _searchText;
        private string _selectedFloor;
        private bool _showInactiveCustomers;
        private Customer _selectedCustomer;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<string> _floors;

        public CustomerViewModel(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _customers = new ObservableCollection<Customer>();
            _floors = new ObservableCollection<string>();

            // Initialize commands
            AddCustomerCommand = new RelayCommand(_ => ExecuteAddCustomer());
            EditCustomerCommand = new RelayCommand(c => ExecuteEditCustomer((Customer)c));
            ViewBillsCommand = new RelayCommand(c => ExecuteViewBills((Customer)c));
            ToggleCustomerStatusCommand = new RelayCommand(c => ExecuteToggleCustomerStatus((Customer)c));
            SearchCommand = new RelayCommand(_ => ExecuteSearch());

            LoadCustomersAsync();
            LoadFloorsAsync();
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

        public bool ShowInactiveCustomers
        {
            get => _showInactiveCustomers;
            set
            {
                if (SetProperty(ref _showInactiveCustomers, value))
                {
                    ExecuteSearch();
                }
            }
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<string> Floors
        {
            get => _floors;
            set => SetProperty(ref _floors, value);
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand ToggleCustomerStatusCommand { get; }
        public ICommand SearchCommand { get; }

        private async void LoadCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.GetAllAsync();
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private async void LoadFloorsAsync()
        {
            try
            {
                // TODO: Get unique floors from customers
                Floors.Clear();
                Floors.Add("Ground Floor");
                Floors.Add("1st Floor");
                Floors.Add("2nd Floor");
                Floors.Add("3rd Floor");
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private async void ExecuteAddCustomer()
        {
            try
            {
                var viewModel = new CustomerDialogViewModel(_unitOfWork, _auditService);
                var view = new Views.CustomerDialog { DataContext = viewModel };
                var result = await DialogHost.Show(view, "RootDialog");

                if (result != null)
                {
                    LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private async void ExecuteEditCustomer(Customer customer)
        {
            try
            {
                var viewModel = new CustomerDialogViewModel(_unitOfWork, _auditService, customer);
                var view = new Views.CustomerDialog { DataContext = viewModel };
                var result = await DialogHost.Show(view, "RootDialog");

                if (result != null)
                {
                    LoadCustomersAsync();
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private void ExecuteViewBills(Customer customer)
        {
            // TODO: Navigate to customer bills view
        }

        private async void ExecuteToggleCustomerStatus(Customer customer)
        {
            try
            {
                customer.IsActive = !customer.IsActive;
                await _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "Customer",
                    customer.Id,
                    customer.IsActive ? AuditAction.Activated : AuditAction.Deactivated,
                    $"Customer status changed to {(customer.IsActive ? "active" : "inactive")}");
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
                customer.IsActive = !customer.IsActive; // Revert the change
            }
        }

        private async void ExecuteSearch()
        {
            try
            {
                var customers = await _unitOfWork.Customers.SearchCustomersAsync(_searchText ?? string.Empty);
                Customers.Clear();
                foreach (var customer in customers)
                {
                    if (_selectedFloor != null && customer.Floor != _selectedFloor)
                        continue;

                    if (!_showInactiveCustomers && !customer.IsActive)
                        continue;

                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }
    }
} 