using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.UI.Commands;
using System.Windows;
using System.Threading.Tasks;

namespace DTCBillingSystem.UI.ViewModels
{
    public class BillGenerationViewModel : ViewModelBase
    {
        private readonly IBillingService _billingService;
        private string _startCustomerId = string.Empty;
        private string _endCustomerId = string.Empty;
        private bool _isLoading;

        public BillGenerationViewModel(IBillingService billingService)
        {
            _billingService = billingService;
            GenerateBillsCommand = new RelayCommand(async () => await GenerateBillsAsync());
        }

        public string StartCustomerId
        {
            get => _startCustomerId;
            set
            {
                _startCustomerId = value;
                OnPropertyChanged();
            }
        }

        public string EndCustomerId
        {
            get => _endCustomerId;
            set
            {
                _endCustomerId = value;
                OnPropertyChanged();
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

        public ICommand GenerateBillsCommand { get; }

        private async Task GenerateBillsAsync()
        {
            try
            {
                IsLoading = true;
                var startId = int.Parse(StartCustomerId);
                var endId = int.Parse(EndCustomerId);
                await _billingService.GenerateBillsAsync(startId, endId);
                MessageBox.Show("Bills generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating bills: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 