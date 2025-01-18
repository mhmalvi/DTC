using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class BillGenerationViewModel : ViewModelBase
    {
        private readonly IBillingService _billingService;
        private readonly ICustomerService _customerService;
        private ObservableCollection<Customer> _customers;
        private string _searchText = string.Empty;
        private DateTime _selectedDate = DateTime.Now;

        public BillGenerationViewModel(
            IBillingService billingService,
            ICustomerService customerService)
        {
            _billingService = billingService;
            _customerService = customerService;
            _customers = new ObservableCollection<Customer>();

            GenerateBillsCommand = new RelayCommand<object>(ExecuteGenerateBills);
            SearchCommand = new RelayCommand<object>(ExecuteSearch);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public ICommand GenerateBillsCommand { get; }
        public ICommand SearchCommand { get; }

        private async void ExecuteSearch(object? parameter)
        {
            try
            {
                var customers = await _customerService.GetCustomersAsync(1, 100);
                var filteredCustomers = customers.Where(c => 
                    string.IsNullOrEmpty(SearchText) ||
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.AccountNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                
                Customers = new ObservableCollection<Customer>(filteredCustomers);
            }
            catch (Exception)
            {
                // Handle error
            }
        }

        private async void ExecuteGenerateBills(object? parameter)
        {
            try
            {
                if (!Customers.Any())
                {
                    // Show error message - no customers selected
                    return;
                }

                foreach (var customer in Customers)
                {
                    var bill = new MonthlyBill
                    {
                        CustomerId = customer.Id,
                        BillNumber = $"BILL-{customer.Id}-{SelectedDate:yyyyMM}",
                        BillingDate = SelectedDate,
                        BillingMonth = SelectedDate,
                        DueDate = SelectedDate.AddDays(15),
                        PreviousBalance = 0,
                        Amount = 0,
                        TaxAmount = 0,
                        TotalAmount = 0,
                        PresentReading = 0,
                        PreviousReading = 0,
                        ACPresentReading = 0,
                        ACPreviousReading = 0,
                        Consumption = 0,
                        BlowerFanCharge = 0,
                        GeneratorCharge = 0,
                        ServiceCharge = 0,
                        Status = BillStatus.Pending,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        LastModifiedBy = "System",
                        LastModifiedAt = DateTime.UtcNow
                    };

                    await _billingService.GenerateBillAsync(bill);
                }

                // Show success message
            }
            catch (Exception)
            {
                // Handle error
            }
        }
    }
} 