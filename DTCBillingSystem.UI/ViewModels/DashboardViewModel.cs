using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.DTOs;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private DashboardStatisticsDto? _statistics;
        private bool _isLoading;

        public DashboardViewModel(
            IDashboardService dashboardService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _dashboardService = dashboardService;
            _dialogService = dialogService;
            _navigationService = navigationService;

            RefreshCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            NavigateToCustomersCommand = new RelayCommand(() => _navigationService.NavigateToCustomers());
            NavigateToBillGenerationCommand = new RelayCommand(async () => await _navigationService.NavigateToAsync("BillGeneration"));
            NavigateToSettingsCommand = new RelayCommand(async () => await _navigationService.NavigateToAsync("Settings"));

            _ = LoadDashboardDataAsync();
        }

        public DashboardStatisticsDto? Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
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

        public ICommand RefreshCommand { get; }
        public ICommand NavigateToCustomersCommand { get; }
        public ICommand NavigateToBillGenerationCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                Statistics = await _dashboardService.GetDashboardStatisticsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Error", $"Failed to load dashboard data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 