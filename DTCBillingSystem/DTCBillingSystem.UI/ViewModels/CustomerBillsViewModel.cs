using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerBillsViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IPrintService _printService;
        private readonly Customer _customer;
        private readonly Action _navigateBack;

        private DateTime _startDate;
        private DateTime _endDate;
        private BillStatus? _selectedStatus;
        private MonthlyBill _selectedBill;
        private ObservableCollection<MonthlyBill> _bills;
        private ObservableCollection<BillStatus> _billStatuses;

        public CustomerBillsViewModel(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IPrintService printService,
            Customer customer,
            Action navigateBack)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _printService = printService;
            _customer = customer;
            _navigateBack = navigateBack;

            // Initialize dates to current month
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);

            // Initialize collections
            Bills = new ObservableCollection<MonthlyBill>();
            BillStatuses = new ObservableCollection<BillStatus>();

            // Initialize commands
            GenerateBillCommand = new RelayCommand(_ => ExecuteGenerateBillAsync());
            ViewBillCommand = new RelayCommand(bill => ExecuteViewBill((MonthlyBill)bill));
            PrintBillCommand = new RelayCommand(bill => ExecutePrintBill((MonthlyBill)bill));
            RecordPaymentCommand = new RelayCommand(bill => ExecuteRecordPayment((MonthlyBill)bill));
            BackCommand = new RelayCommand(_ => ExecuteBack());

            // Load data
            LoadBillStatusesAsync();
            LoadBillsAsync();
        }

        public Customer Customer => _customer;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    LoadBillsAsync();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    LoadBillsAsync();
            }
        }

        public BillStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    LoadBillsAsync();
            }
        }

        public MonthlyBill SelectedBill
        {
            get => _selectedBill;
            set => SetProperty(ref _selectedBill, value);
        }

        public ObservableCollection<MonthlyBill> Bills
        {
            get => _bills;
            set => SetProperty(ref _bills, value);
        }

        public ObservableCollection<BillStatus> BillStatuses
        {
            get => _billStatuses;
            set => SetProperty(ref _billStatuses, value);
        }

        public ICommand GenerateBillCommand { get; }
        public ICommand ViewBillCommand { get; }
        public ICommand PrintBillCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand BackCommand { get; }

        private async void LoadBillStatusesAsync()
        {
            BillStatuses.Clear();
            foreach (BillStatus status in Enum.GetValues(typeof(BillStatus)))
            {
                BillStatuses.Add(status);
            }
        }

        private async void LoadBillsAsync()
        {
            try
            {
                var bills = await _unitOfWork.MonthlyBills.GetCustomerBillsAsync(_customer.Id);
                Bills.Clear();

                foreach (var bill in bills)
                {
                    if (bill.BillingMonth >= StartDate && bill.BillingMonth <= EndDate)
                    {
                        if (!SelectedStatus.HasValue || bill.Status == SelectedStatus.Value)
                        {
                            Bills.Add(bill);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private async void ExecuteGenerateBillAsync()
        {
            try
            {
                var viewModel = new BillGenerationViewModel(
                    _unitOfWork,
                    _auditService,
                    _customer,
                    OnBillGenerated,
                    () => DialogHost.Close("RootDialog")
                );

                var view = new BillGenerationDialog
                {
                    DataContext = viewModel
                };

                await DialogHost.Show(view, "RootDialog");
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private void OnBillGenerated(MonthlyBill bill)
        {
            LoadBillsAsync();
            // TODO: Show success notification
        }

        private void ExecuteViewBill(MonthlyBill bill)
        {
            // TODO: Show bill details dialog
        }

        private async void ExecutePrintBill(MonthlyBill bill)
        {
            try
            {
                await _printService.PrintBillAsync(bill);
                await _auditService.LogActionAsync(
                    "Bill",
                    bill.Id,
                    AuditAction.Printed,
                    $"Bill printed for customer {_customer.ShopNo} for {bill.BillingMonth:MMM yyyy}"
                );
            }
            catch (Exception ex)
            {
                // TODO: Show error notification
            }
        }

        private void ExecuteRecordPayment(MonthlyBill bill)
        {
            // TODO: Show payment recording dialog
        }

        private void ExecuteBack()
        {
            _navigateBack?.Invoke();
        }
    }
} 