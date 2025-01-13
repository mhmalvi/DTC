using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IBillingService _billingService;
        private readonly IBackupService _backupService;
        private int _totalCustomers;
        private decimal _monthlyRevenue;
        private decimal _outstandingAmount;
        private DateTime? _lastBackupTime;
        private DateTime? _lastSyncTime;
        private ObservableCollection<RecentActivityItem> _recentActivities;

        public DashboardViewModel(IBillingService billingService, IBackupService backupService)
        {
            _billingService = billingService;
            _backupService = backupService;
            _recentActivities = new ObservableCollection<RecentActivityItem>();

            NewBillCommand = new RelayCommand(ExecuteNewBill);
            RecordPaymentCommand = new RelayCommand(ExecuteRecordPayment);
            GenerateReportCommand = new RelayCommand(ExecuteGenerateReport);

            LoadDashboardDataAsync();
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

        public ObservableCollection<RecentActivityItem> RecentActivities
        {
            get => _recentActivities;
            set => SetProperty(ref _recentActivities, value);
        }

        public ICommand NewBillCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand GenerateReportCommand { get; }

        private async void LoadDashboardDataAsync()
        {
            try
            {
                // Load statistics
                TotalCustomers = await _billingService.GetTotalCustomersAsync();
                MonthlyRevenue = await _billingService.GetMonthlyRevenueAsync(DateTime.Now);
                OutstandingAmount = await _billingService.GetTotalOutstandingAmountAsync();

                // Load system status
                var backupHistory = await _backupService.GetBackupHistoryAsync();
                if (backupHistory.Count > 0)
                {
                    LastBackupTime = backupHistory[0].CreatedAt;
                }

                // Load recent activities (placeholder)
                LoadRecentActivities();
            }
            catch (Exception ex)
            {
                // TODO: Handle errors and show notification
            }
        }

        private void LoadRecentActivities()
        {
            // TODO: Replace with actual data from audit log
            RecentActivities.Clear();
            RecentActivities.Add(new RecentActivityItem
            {
                Icon = "FileDocument",
                Description = "New bill generated for Shop #123",
                UserName = "John Doe",
                Timestamp = DateTime.Now.AddMinutes(-30)
            });
            RecentActivities.Add(new RecentActivityItem
            {
                Icon = "CashRegister",
                Description = "Payment recorded for Shop #456",
                UserName = "Jane Smith",
                Timestamp = DateTime.Now.AddHours(-2)
            });
        }

        private void ExecuteNewBill(object parameter)
        {
            // TODO: Navigate to new bill view
        }

        private void ExecuteRecordPayment(object parameter)
        {
            // TODO: Navigate to record payment view
        }

        private void ExecuteGenerateReport(object parameter)
        {
            // TODO: Navigate to report generation view
        }
    }

    public class RecentActivityItem
    {
        public string Icon { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 