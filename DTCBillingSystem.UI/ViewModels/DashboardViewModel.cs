using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.UI.Commands;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly ApplicationDbContext _context;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private decimal _totalRevenue;
        private int _totalCustomers;
        private int _pendingBills;
        private int _overduePayments;
        private ObservableCollection<MonthlyBill> _recentBills;
        private ObservableCollection<PaymentRecord> _recentPayments;

        public DashboardViewModel(ApplicationDbContext context)
        {
            _context = context;
            _recentBills = new ObservableCollection<MonthlyBill>();
            _recentPayments = new ObservableCollection<PaymentRecord>();
            LoadDashboardDataCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            _ = LoadDashboardDataAsync(); // Load data when the view model is created
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

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
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

        public int PendingBills
        {
            get => _pendingBills;
            set
            {
                _pendingBills = value;
                OnPropertyChanged();
            }
        }

        public int OverduePayments
        {
            get => _overduePayments;
            set
            {
                _overduePayments = value;
                OnPropertyChanged();
            }
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

        public ICommand LoadDashboardDataCommand { get; }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load total revenue (sum of all paid bills)
                var paidBills = await _context.MonthlyBills
                    .Where(b => b.Status == BillStatus.Paid)
                    .Select(b => b.TotalAmount)
                    .ToListAsync();
                TotalRevenue = paidBills.Sum();

                // Load total customers
                TotalCustomers = await _context.Customers
                    .CountAsync(c => c.IsActive);

                // Load pending bills count
                PendingBills = await _context.MonthlyBills
                    .CountAsync(b => b.Status == BillStatus.Pending);

                // Load overdue payments count
                OverduePayments = await _context.MonthlyBills
                    .CountAsync(b => b.Status == BillStatus.Overdue);

                // Load recent bills
                var recentBills = await _context.MonthlyBills
                    .Include(b => b.Customer)
                    .OrderByDescending(b => b.BillingMonth)
                    .Take(5)
                    .ToListAsync();

                RecentBills.Clear();
                foreach (var bill in recentBills)
                {
                    RecentBills.Add(bill);
                }

                // Load recent payments
                var recentPayments = await _context.PaymentRecords
                    .Include(p => p.MonthlyBill)
                    .ThenInclude(b => b.Customer)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync();

                RecentPayments.Clear();
                foreach (var payment in recentPayments)
                {
                    RecentPayments.Add(payment);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading dashboard data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 