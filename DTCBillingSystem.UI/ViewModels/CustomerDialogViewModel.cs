using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerDialogViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;
        private Customer _customer;
        private bool _isEditMode;
        private string _title;

        public CustomerDialogViewModel(ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            _customer = new Customer
            {
                Name = string.Empty,
                AccountNumber = string.Empty,
                MeterNumber = string.Empty,
                ContactNumber = string.Empty,
                ZoneCode = string.Empty,
                ShopNo = string.Empty,
                IsActive = true,
                RegistrationDate = DateTime.Today,
                CustomerType = CustomerType.Residential,
                CreatedAt = DateTime.UtcNow
            };

            _title = "Add Customer";
            _isEditMode = false;

            SaveCommand = new AsyncRelayCommand<object?>(ExecuteSaveAsync, CanExecuteSave);
            CancelCommand = new RelayCommand<object?>(_ => ExecuteCancel());
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            private set => SetProperty(ref _isEditMode, value);
        }

        public Customer Customer
        {
            get => _customer;
            private set => SetProperty(ref _customer, value);
        }

        public void Initialize(Customer customer)
        {
            Customer = new Customer
            {
                Id = customer.Id,
                Name = customer.Name,
                AccountNumber = customer.AccountNumber,
                MeterNumber = customer.MeterNumber,
                ContactNumber = customer.ContactNumber,
                Email = customer.Email,
                ZoneCode = customer.ZoneCode,
                ShopNo = customer.ShopNo,
                Floor = customer.Floor,
                CustomerType = customer.CustomerType,
                Status = customer.Status,
                SecurityDeposit = customer.SecurityDeposit,
                CurrentBalance = customer.CurrentBalance,
                RegistrationDate = customer.RegistrationDate,
                LastBillingDate = customer.LastBillingDate,
                LastPaymentDate = customer.LastPaymentDate,
                CreatedBy = customer.CreatedBy,
                CreatedAt = customer.CreatedAt,
                LastModifiedBy = customer.LastModifiedBy,
                LastModifiedAt = DateTime.UtcNow,
                IsActive = customer.IsActive,
                Notes = customer.Notes
            };

            IsEditMode = true;
            Title = "Edit Customer";
        }

        private bool CanExecuteSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Customer.Name) &&
                   !string.IsNullOrWhiteSpace(Customer.AccountNumber) &&
                   !string.IsNullOrWhiteSpace(Customer.MeterNumber);
        }

        private async Task ExecuteSaveAsync(object? parameter)
        {
            try
            {
                if (IsEditMode)
                {
                    await _customerService.UpdateCustomerAsync(Customer);
                }
                else
                {
                    await _customerService.CreateCustomerAsync(Customer);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error", $"Failed to save customer: {ex.Message}");
            }
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
        }

        public bool? DialogResult { get; private set; }
    }
} 