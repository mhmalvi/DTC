using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IBackupService _backupService;
        private readonly ICustomerService _customerService;
        private readonly IBillingService _billingService;
        private readonly IAuditService _auditService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private int _totalCustomers;
        private decimal _monthlyRevenue;
        private decimal _outstandingAmount;
        private DateTime? _lastBackupTime;
        private DateTime? _lastSyncTime;
        private ObservableCollection<RecentActivity> _recentActivities;
        private bool _isLoading;
        private string? _errorMessage;

        public DashboardViewModel(
            IBackupService backupService,
            ICustomerService customerService,
            IBillingService billingService,
            IAuditService auditService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _backupService = backupService;
            _customerService = customerService;
            _billingService = billingService;
            _auditService = auditService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            _recentActivities = new ObservableCollection<RecentActivity>();

            NewBillCommand = new RelayCommand<object>(ExecuteNewBill);
            RecordPaymentCommand = new RelayCommand<object>(ExecuteRecordPayment);
            GenerateReportCommand = new RelayCommand<object>(ExecuteGenerateReport);

            _ = LoadDashboardDataAsync();
        }

        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        public decimal MonthlyRevenue
        {
            get => _monthlyRevenue;
            set => SetProperty(ref _monthlyRevenue, value);
        }

        public decimal OutstandingAmount
        {
            get => _outstandingAmount;
            set => SetProperty(ref _outstandingAmount, value);
        }

        public DateTime? LastBackupTime
        {
            get => _lastBackupTime;
            set => SetProperty(ref _lastBackupTime, value);
        }

        public DateTime? LastSyncTime
        {
            get => _lastSyncTime;
            set => SetProperty(ref _lastSyncTime, value);
        }

        public ObservableCollection<RecentActivity> RecentActivities
        {
            get => _recentActivities;
            set => SetProperty(ref _recentActivities, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand NewBillCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand GenerateReportCommand { get; }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // Load data in parallel
                var customerTask = _customerService.GetTotalCustomersCountAsync();
                var revenueTask = _billingService.GetMonthlyRevenueAsync(DateTime.Now);
                var outstandingTask = _billingService.GetTotalOutstandingAmountAsync();
                var backupTask = _backupService.GetBackupListAsync();

                await Task.WhenAll(customerTask, revenueTask, outstandingTask, backupTask);

                TotalCustomers = await customerTask;
                MonthlyRevenue = await revenueTask;
                OutstandingAmount = await outstandingTask;

                var backups = await backupTask;
                var latestBackup = backups.MaxBy(b => b.CreatedAt);
                LastBackupTime = latestBackup?.CreatedAt;

                // Update sync time after successful data load
                LastSyncTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard data: {ex.Message}";
                await _dialogService.ShowErrorAsync("Dashboard Error", 
                    "Failed to load some dashboard data. The dashboard may show incomplete information.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ExecuteNewBill(object? parameter)
        {
            try
            {
                _navigationService.NavigateToAsync("BillView");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Navigation Error", 
                    $"Failed to navigate to billing page: {ex.Message}");
            }
        }

        private async void ExecuteRecordPayment(object? parameter)
        {
            try
            {
                _navigationService.NavigateToAsync("PaymentView");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Navigation Error", 
                    $"Failed to navigate to payment page: {ex.Message}");
            }
        }

        private async void ExecuteGenerateReport(object? parameter)
        {
            try
            {
                _navigationService.NavigateToAsync("ReportView");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Navigation Error", 
                    $"Failed to navigate to report page: {ex.Message}");
            }
        }
    }

    public class RecentActivity
    {
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
} 