using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IBackupService _backupService;
        private string? _icon;
        private string? _description;
        private string? _userName;

        public DashboardViewModel(IBackupService backupService)
        {
            _backupService = backupService;
            NewBillCommand = new RelayCommand<object>(ExecuteNewBill);
            RecordPaymentCommand = new RelayCommand<object>(ExecuteRecordPayment);
            GenerateReportCommand = new RelayCommand<object>(ExecuteGenerateReport);

            _ = LoadBackupInfoAsync(); // Fire and forget intentionally for constructor
        }

        public string? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string? UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public ICommand NewBillCommand { get; }
        public ICommand RecordPaymentCommand { get; }
        public ICommand GenerateReportCommand { get; }

        private void ExecuteNewBill(object? parameter)
        {
            // Implement new bill logic
        }

        private void ExecuteRecordPayment(object? parameter)
        {
            // Implement record payment logic
        }

        private void ExecuteGenerateReport(object? parameter)
        {
            // Implement generate report logic
        }

        private async Task LoadBackupInfoAsync()
        {
            try
            {
                var backupList = (await _backupService.GetBackupListAsync()).ToList();
                if (backupList.Count > 0)
                {
                    var latestBackup = backupList.OrderByDescending(b => b.CreatedAt).First();
                    Description = $"Last backup: {latestBackup.CreatedAt:g}";
                }
            }
            catch (Exception)
            {
                Description = "Failed to load backup information";
            }
        }
    }
} 