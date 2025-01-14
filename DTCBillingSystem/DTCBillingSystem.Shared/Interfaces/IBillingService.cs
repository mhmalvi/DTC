using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBillingService
    {
        Task<IEnumerable<MonthlyBill>> GenerateMonthlyBillsAsync(DateTime billingMonth);
        Task<MonthlyBill> GenerateBillForCustomerAsync(int customerId, DateTime billingMonth);
        Task<PaymentRecord> ProcessPaymentAsync(int billId, decimal amount, PaymentMethod paymentMethod, string referenceNumber);
        Task<decimal> CalculateLatePaymentChargeAsync(int billId);
    }
} 