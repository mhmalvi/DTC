using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<(IEnumerable<MonthlyBill> RecentBills, IEnumerable<PaymentRecord> RecentPayments, 
            int TotalCustomers, decimal TotalRevenue, int PendingBills)> GetDashboardDataAsync();
    }
} 