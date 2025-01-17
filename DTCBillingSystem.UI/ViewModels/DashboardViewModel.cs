using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.UI.Commands;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly INavigationService _navigationService;
        private ObservableCollection<MonthlyBill> _recentBills;
        private ObservableCollection<PaymentRecord> _recentPayments;
        private int _totalCustomers;
        private decimal _totalRevenue;
        private int _pendingBills;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public DashboardViewModel(
            IDashboardService dashboardService,
            INavigationService navigationService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            _recentBills = new ObservableCollection<MonthlyBill>();
            _recentPayments = new ObservableCollection<PaymentRecord>();
            
            LoadDashboardDataCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            NavigateToCustomersCommand = new RelayCommand(() => _navigationService.NavigateToCustomers());
            
            _ = LoadDashboardDataAsync(); // Load data when the view model is created
        }

        public ObservableCollection<MonthlyBill> RecentBills
        {
            get => _recentBills;
            set
            {
                _recentBills = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PaymentRecord> RecentPayments
        {
            get => _recentPayments;
            set
            {
                _recentPayments = value;
                OnPropertyChanged();
            }
        }

        public int TotalCustomers
        {
            get => _totalCustomers;
            set
            {
                _totalCustomers = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        public int PendingBills
        {
            get => _pendingBills;
            set
            {
                _pendingBills = value;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDashboardDataCommand { get; }
        public ICommand NavigateToCustomersCommand { get; }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var (recentBills, recentPayments, totalCustomers, totalRevenue, pendingBills) = 
                    await _dashboardService.GetDashboardDataAsync();

                RecentBills = new ObservableCollection<MonthlyBill>(recentBills);
                RecentPayments = new ObservableCollection<PaymentRecord>(recentPayments);
                TotalCustomers = totalCustomers;
                TotalRevenue = totalRevenue;
                PendingBills = pendingBills;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load dashboard data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 