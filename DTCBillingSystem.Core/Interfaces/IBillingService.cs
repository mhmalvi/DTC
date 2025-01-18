using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillingService
    {
        Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId);
        Task GenerateBillsAsync(int startCustomerId, int endCustomerId);
        Task<MonthlyBill> GenerateBillAsync(MonthlyBill bill);
        Task<decimal> CalculateBillAmountAsync(int customerId, decimal currentReading, decimal previousReading);
    }
} 