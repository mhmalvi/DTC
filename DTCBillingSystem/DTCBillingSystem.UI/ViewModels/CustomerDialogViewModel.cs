using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerDialogViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly Action _closeDialog;
        private Customer _customer;
        private bool _isEditMode;
        private string _dialogTitle;
        private ObservableCollection<string> _floors;
        private string _shopNoError;
        private string _nameError;
        private string _floorError;
        private string _emailError;
        private bool _hasErrors;

        public CustomerDialogViewModel(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            Customer customer = null,
            Action closeDialog = null)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _closeDialog = closeDialog;
            _floors = new ObservableCollection<string>();

            IsEditMode = customer != null;
            DialogTitle = IsEditMode ? "Edit Customer" : "Add Customer";
            Customer = customer ?? new Customer { IsActive = true };

            SaveCommand = new RelayCommand(_ => ExecuteSaveAsync(), _ => !HasErrors && CanSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());

            LoadFloorsAsync();
            ValidateAll();
        }

        public Customer Customer
        {
            get => _customer;
            set
            {
                if (SetProperty(ref _customer, value))
                {
                    ValidateAll();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }

        public ObservableCollection<string> Floors
        {
            get => _floors;
            set => SetProperty(ref _floors, value);
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

        public bool HasErrors
        {
            get => _hasErrors;
            set => SetProperty(ref _hasErrors, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

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

        private void ValidateAll()
        {
            ValidateShopNo();
            ValidateName();
            ValidateFloor();
            ValidateEmail();
            UpdateHasErrors();
        }

        private void ValidateShopNo()
        {
            if (string.IsNullOrWhiteSpace(Customer.ShopNo))
            {
                ShopNoError = "Shop No is required";
            }
            else if (Customer.ShopNo.Length > 20)
            {
                ShopNoError = "Shop No cannot exceed 20 characters";
            }
            else
            {
                ShopNoError = null;
            }
            UpdateHasErrors();
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Customer.Name))
            {
                NameError = "Customer Name is required";
            }
            else if (Customer.Name.Length > 100)
            {
                NameError = "Name cannot exceed 100 characters";
            }
            else
            {
                NameError = null;
            }
            UpdateHasErrors();
        }

        private void ValidateFloor()
        {
            if (string.IsNullOrWhiteSpace(Customer.Floor))
            {
                FloorError = "Floor is required";
            }
            else
            {
                FloorError = null;
            }
            UpdateHasErrors();
        }

        private void ValidateEmail()
        {
            if (!string.IsNullOrWhiteSpace(Customer.Email))
            {
                if (!IsValidEmail(Customer.Email))
                {
                    EmailError = "Invalid email format";
                }
                else if (Customer.Email.Length > 100)
                {
                    EmailError = "Email cannot exceed 100 characters";
                }
                else
                {
                    EmailError = null;
                }
            }
            else
            {
                EmailError = null;
            }
            UpdateHasErrors();
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

        private void UpdateHasErrors()
        {
            HasErrors = !string.IsNullOrEmpty(ShopNoError) ||
                       !string.IsNullOrEmpty(NameError) ||
                       !string.IsNullOrEmpty(FloorError) ||
                       !string.IsNullOrEmpty(EmailError);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Customer.ShopNo) &&
                   !string.IsNullOrWhiteSpace(Customer.Name) &&
                   !string.IsNullOrWhiteSpace(Customer.Floor);
        }

        private async void ExecuteSaveAsync()
        {
            try
            {
                ValidateAll();
                if (HasErrors)
                {
                    return;
                }

                // Validate shop number uniqueness
                if (!IsEditMode || Customer.ShopNo != Customer.ShopNo)
                {
                    var isUnique = await _unitOfWork.Customers.IsShopNoUniqueAsync(
                        Customer.ShopNo,
                        IsEditMode ? Customer.Id : null);

                    if (!isUnique)
                    {
                        ShopNoError = "This Shop No is already in use";
                        UpdateHasErrors();
                        return;
                    }
                }

                if (IsEditMode)
                {
                    await _unitOfWork.Customers.UpdateAsync(Customer);
                    await _auditService.LogActionAsync(
                        "Customer",
                        Customer.Id,
                        AuditAction.Updated,
                        $"Customer {Customer.ShopNo} updated");
                }
                else
                {
                    await _unitOfWork.Customers.AddAsync(Customer);
                    await _auditService.LogActionAsync(
                        "Customer",
                        Customer.Id,
                        AuditAction.Created,
                        $"Customer {Customer.ShopNo} created");
                }

                await _unitOfWork.SaveChangesAsync();
                _closeDialog?.Invoke();
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private void ExecuteCancel()
        {
            _closeDialog?.Invoke();
        }
    }
} 