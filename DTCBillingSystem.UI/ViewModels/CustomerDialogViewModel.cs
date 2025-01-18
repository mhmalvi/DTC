using System;
using System.Collections.Generic;
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
        private readonly INavigationService _navigationService;
        private readonly IAuditService _auditService;

        private Customer _customer;
        private string _dialogTitle = string.Empty;
        private string _shopNoError = string.Empty;
        private string _nameError = string.Empty;
        private string _floorError = string.Empty;
        private string _emailError = string.Empty;
        private bool _isEditMode;

        public CustomerDialogViewModel(
            ICustomerService customerService,
            IDialogService dialogService,
            INavigationService navigationService,
            IAuditService auditService,
            Customer? existingCustomer = null)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));

            _customer = existingCustomer ?? new Customer
            {
                IsActive = true,
                CustomerType = CustomerType.Regular
            };

            _isEditMode = existingCustomer != null;
            _dialogTitle = _isEditMode ? "Edit Customer" : "New Customer";

            SaveCommand = new RelayCommand<object>(_ => { ExecuteSaveAsync(); }, _ => CanExecuteSave());
            CancelCommand = new RelayCommand<object>(_ => ExecuteCancel());

            CustomerTypes = Enum.GetValues<CustomerType>();
        }

        public Customer Customer
        {
            get => _customer;
            set => SetProperty(ref _customer, value);
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }

        public string ShopNoError
        {
            get => _shopNoError;
            set => SetProperty(ref _shopNoError, value);
        }

        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public string FloorError
        {
            get => _floorError;
            set => SetProperty(ref _floorError, value);
        }

        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public IEnumerable<CustomerType> CustomerTypes { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool ValidateCustomer()
        {
            var isValid = true;

            // Reset errors
            ShopNoError = string.Empty;
            NameError = string.Empty;
            FloorError = string.Empty;
            EmailError = string.Empty;

            // Validate Shop No
            if (string.IsNullOrWhiteSpace(Customer.ShopNo))
            {
                ShopNoError = "Shop No is required";
                isValid = false;
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(Customer.Name))
            {
                NameError = "Name is required";
                isValid = false;
            }

            // Validate Floor
            if (string.IsNullOrWhiteSpace(Customer.Floor))
            {
                FloorError = "Floor is required";
                isValid = false;
            }

            // Validate Email (if provided)
            if (!string.IsNullOrWhiteSpace(Customer.Email) && !IsValidEmail(Customer.Email))
            {
                EmailError = "Invalid email format";
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async void ExecuteSaveAsync()
        {
            if (!ValidateCustomer())
                return;

            try
            {
                if (IsEditMode)
                {
                    await _customerService.UpdateCustomerAsync(Customer);
                    await _auditService.LogActivityAsync(
                        "Customer",
                        "Update",
                        Customer.LastModifiedBy,
                        $"Updated customer {Customer.ShopNo}");
                }
                else
                {
                    await _customerService.AddCustomerAsync(Customer);
                    await _auditService.LogActivityAsync(
                        "Customer",
                        "Create",
                        Customer.CreatedBy,
                        $"Created new customer {Customer.ShopNo}");
                }

                await _dialogService.ShowInformationAsync(
                    "Success",
                    $"Customer {(IsEditMode ? "updated" : "created")} successfully.");

                _navigationService.NavigateBack();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(
                    "Error",
                    $"Failed to {(IsEditMode ? "update" : "create")} customer: {ex.Message}");
            }
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(Customer.ShopNo) &&
                   !string.IsNullOrWhiteSpace(Customer.Name) &&
                   !string.IsNullOrWhiteSpace(Customer.Floor);
        }

        private void ExecuteCancel()
        {
            _navigationService.NavigateBack();
        }
    }
} 