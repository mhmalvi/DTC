using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerBillsViewModel : ViewModelBase
    {
        private readonly IBillingService _billingService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly Customer _customer;
        private ObservableCollection<MonthlyBill> _bills = new();
        private MonthlyBill? _selectedBill;
        private DateTime _startDate = DateTime.Now.AddMonths(-6);
        private DateTime _endDate = DateTime.Now;

        public CustomerBillsViewModel(
            IBillingService billingService,
            IDialogService dialogService,
            INavigationService navigationService,
            Customer customer)
        {
            _billingService = billingService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _customer = customer;

            ViewBillDetailsCommand = new RelayCommand<object>(_ => ExecuteViewBillDetails(_selectedBill));
            BackCommand = new RelayCommand<object>(_ => ExecuteBack());
            RefreshCommand = new RelayCommand<object>(_ => ExecuteRefresh());

            _ = LoadBillsAsync(); // Fire and forget intentionally for constructor
        }

        public ObservableCollection<MonthlyBill> Bills
        {
            get => _bills;
            set => SetProperty(ref _bills, value);
        }

        public MonthlyBill? SelectedBill
        {
            get => _selectedBill;
            set => SetProperty(ref _selectedBill, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    _ = LoadBillsAsync();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    _ = LoadBillsAsync();
                }
            }
        }

        public ICommand ViewBillDetailsCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadBillsAsync()
        {
            try
            {
                var bills = await _billingService.GetCustomerBillsAsync(_customer.Id);
                var filteredBills = bills.Where(b => 
                    b.BillingMonth >= StartDate && 
                    b.BillingMonth <= EndDate);
                Bills = new ObservableCollection<MonthlyBill>(filteredBills);
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to load bills");
            }
        }

        private async void ExecuteViewBillDetails(MonthlyBill? bill)
        {
            if (bill == null)
            {
                await _dialogService.ShowWarningAsync("Warning", "Please select a bill to view");
                return;
            }

            try
            {
                await _dialogService.ShowBillDetailsAsync(bill);
            }
            catch (Exception)
            {
                await _dialogService.ShowErrorAsync("Error", "Failed to show bill details");
            }
        }

        private void ExecuteBack()
        {
            _navigationService.NavigateBack();
        }

        private async void ExecuteRefresh()
        {
            await LoadBillsAsync();
        }
    }
} 