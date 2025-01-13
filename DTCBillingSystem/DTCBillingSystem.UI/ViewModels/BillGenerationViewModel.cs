using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class BillGenerationViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly Customer _customer;
        private readonly Action<MonthlyBill> _onBillGenerated;
        private readonly Action _closeDialog;

        private DateTime _billingMonth;
        private decimal _previousReading;
        private decimal _presentReading;
        private decimal _acPreviousReading;
        private decimal _acPresentReading;
        private decimal _blowerFanCharge;
        private decimal _generatorCharge;
        private decimal _serviceCharge;
        private DateTime _dueDate;
        private string _notes;
        private string _errorMessage;

        public BillGenerationViewModel(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            Customer customer,
            Action<MonthlyBill> onBillGenerated,
            Action closeDialog)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _customer = customer;
            _onBillGenerated = onBillGenerated;
            _closeDialog = closeDialog;

            // Initialize dates
            BillingMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DueDate = DateTime.Now.AddDays(30);

            // Initialize commands
            SaveCommand = new RelayCommand(_ => ExecuteSaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());

            // Load previous readings
            LoadPreviousReadingsAsync();
        }

        public DateTime BillingMonth
        {
            get => _billingMonth;
            set => SetProperty(ref _billingMonth, value);
        }

        public decimal PreviousReading
        {
            get => _previousReading;
            set => SetProperty(ref _previousReading, value);
        }

        public decimal PresentReading
        {
            get => _presentReading;
            set => SetProperty(ref _presentReading, value);
        }

        public decimal ACPreviousReading
        {
            get => _acPreviousReading;
            set => SetProperty(ref _acPreviousReading, value);
        }

        public decimal ACPresentReading
        {
            get => _acPresentReading;
            set => SetProperty(ref _acPresentReading, value);
        }

        public decimal BlowerFanCharge
        {
            get => _blowerFanCharge;
            set => SetProperty(ref _blowerFanCharge, value);
        }

        public decimal GeneratorCharge
        {
            get => _generatorCharge;
            set => SetProperty(ref _generatorCharge, value);
        }

        public decimal ServiceCharge
        {
            get => _serviceCharge;
            set => SetProperty(ref _serviceCharge, value);
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set => SetProperty(ref _dueDate, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private async void LoadPreviousReadingsAsync()
        {
            try
            {
                var lastBill = await _unitOfWork.MonthlyBills.GetCustomerLatestBillAsync(_customer.Id);
                if (lastBill != null)
                {
                    PreviousReading = lastBill.PresentReading;
                    ACPreviousReading = lastBill.ACPresentReading;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load previous readings.";
            }
        }

        private bool CanSave()
        {
            if (BillingMonth == DateTime.MinValue || DueDate == DateTime.MinValue)
                return false;

            if (PresentReading < PreviousReading)
                return false;

            if (ACPresentReading < ACPreviousReading)
                return false;

            return true;
        }

        private async void ExecuteSaveAsync()
        {
            try
            {
                var bill = new MonthlyBill
                {
                    CustomerId = _customer.Id,
                    BillingMonth = BillingMonth,
                    PreviousReading = PreviousReading,
                    PresentReading = PresentReading,
                    ACPreviousReading = ACPreviousReading,
                    ACPresentReading = ACPresentReading,
                    BlowerFanCharge = BlowerFanCharge,
                    GeneratorCharge = GeneratorCharge,
                    ServiceCharge = ServiceCharge,
                    DueDate = DueDate,
                    Notes = Notes,
                    Status = BillStatus.Pending
                };

                await _unitOfWork.MonthlyBills.AddAsync(bill);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "Bill",
                    bill.Id,
                    AuditAction.Created,
                    $"Bill generated for customer {_customer.ShopNo} for {BillingMonth:MMM yyyy}");

                _onBillGenerated?.Invoke(bill);
                _closeDialog?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to generate bill. Please try again.";
            }
        }

        private void ExecuteCancel()
        {
            _closeDialog?.Invoke();
        }
    }
} 