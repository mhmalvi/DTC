using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillingService
    {
        Task<MonthlyBill> GenerateBillAsync(MonthlyBill bill);
        Task<MonthlyBill?> GetBillByIdAsync(int id);
        Task<IEnumerable<MonthlyBill>> GetBillsByCustomerAsync(int customerId);
        Task<MonthlyBill> UpdateBillAsync(MonthlyBill bill);
        Task<bool> DeleteBillAsync(int id);
        Task<bool> MarkBillAsPaidAsync(int id, string paymentReference);
        Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId);
        Task<MonthlyBill?> GetBillDetailsAsync(int billId);
        Task<bool> PrintBillAsync(int billId);
        Task<int> GetTotalCustomersAsync();
        Task<decimal> GetMonthlyRevenueAsync(DateTime month);
        Task<decimal> GetTotalOutstandingAmountAsync();
    }
} 