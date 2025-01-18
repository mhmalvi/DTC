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
        private ObservableCollection<MonthlyBill> _bills;
        private Customer? _selectedCustomer;
        private bool _isLoading;

        public CustomerBillsViewModel(
            IBillingService billingService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _billingService = billingService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _bills = new ObservableCollection<MonthlyBill>();

            GenerateBillCommand = new RelayCommand(async () => await GenerateBillAsync(), () => SelectedCustomer != null);
            PayBillCommand = new RelayCommand(async () => await PayBillAsync(), () => SelectedBill != null && !SelectedBill.IsPaid);
        }

        public ObservableCollection<MonthlyBill> Bills
        {
            get => _bills;
            set
            {
                _bills = value;
                OnPropertyChanged();
            }
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                LoadBillsAsync().ConfigureAwait(false);
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

        public ICommand GenerateBillCommand { get; }
        public ICommand PayBillCommand { get; }

        private MonthlyBill? _selectedBill;
        public MonthlyBill? SelectedBill
        {
            get => _selectedBill;
            set
            {
                _selectedBill = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadBillsAsync()
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                IsLoading = true;
                var bills = await _billingService.GetCustomerBillsAsync(SelectedCustomer.Id);
                Bills.Clear();
                foreach (var bill in bills)
                {
                    Bills.Add(bill);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Error", $"Failed to load bills: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateBillAsync()
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                IsLoading = true;
                await _billingService.GenerateBillsAsync(SelectedCustomer.Id, SelectedCustomer.Id);
                await LoadBillsAsync();
                _dialogService.ShowInformation("Success", "Bill generated successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Error", $"Failed to generate bill: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PayBillAsync()
        {
            if (SelectedBill == null)
                return;

            try
            {
                IsLoading = true;
                SelectedBill.IsPaid = true;
                await _billingService.GenerateBillAsync(SelectedBill);
                await LoadBillsAsync();
                _dialogService.ShowInformation("Success", "Payment recorded successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Error", $"Failed to record payment: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 