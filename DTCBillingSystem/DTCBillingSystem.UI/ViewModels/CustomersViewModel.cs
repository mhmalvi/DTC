using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
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

        public CustomersViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            _navigationService = navigationService;

            AddCustomerCommand = new RelayCommand<object>(_ => ExecuteAddCustomer());
            EditCustomerCommand = new RelayCommand<object>(_ => ExecuteEditCustomer());
            ViewBillsCommand = new RelayCommand<object>(_ => ExecuteViewBills());
            RefreshCommand = new RelayCommand<object>(_ => ExecuteRefresh());
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand ViewBillsCommand { get; }
        public ICommand RefreshCommand { get; }

        private void ExecuteAddCustomer()
        {
            try
            {
                _navigationService.NavigateTo(typeof(CustomerDialog));
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorAsync("Error", $"Failed to open add customer dialog: {ex.Message}").Wait();
            }
        }

        private void ExecuteEditCustomer()
        {
            try
            {
                _navigationService.NavigateTo(typeof(CustomerDialog));
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorAsync("Error", $"Failed to open edit customer dialog: {ex.Message}").Wait();
            }
        }

        private void ExecuteViewBills()
        {
            try
            {
                _navigationService.NavigateTo(typeof(CustomerBillsView));
            }
            catch (Exception ex)
            {
                _dialogService.ShowErrorAsync("Error", $"Failed to open customer bills view: {ex.Message}").Wait();
            }
        }

        private async Task ExecuteRefresh()
        {
            try
            {
                // Implement refresh logic
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to refresh: {ex.Message}");
            }
        }
    }
} 