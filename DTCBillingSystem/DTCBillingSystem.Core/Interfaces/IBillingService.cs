using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for billing-related operations
    /// </summary>
    public interface IBillingService
    {
        /// <summary>
        /// Generate monthly bills for all active customers
        /// </summary>
        Task<IEnumerable<MonthlyBill>> GenerateMonthlyBillsAsync(DateTime billingMonth);

        /// <summary>
        /// Generate bill for a specific customer
        /// </summary>
        Task<MonthlyBill> GenerateBillForCustomerAsync(int customerId, DateTime billingMonth);

        /// <summary>
        /// Calculate bill amount based on readings and rates
        /// </summary>
        Task<decimal> CalculateBillAmountAsync(
            decimal presentReading,
            decimal previousReading,
            decimal acPresentReading,
            decimal acPreviousReading,
            decimal blowerFanCharge,
            DateTime billingMonth);

        /// <summary>
        /// Record a payment for a bill
        /// </summary>
        Task<PaymentRecord> RecordPaymentAsync(
            int billId,
            decimal amount,
            PaymentMethod paymentMethod,
            string transactionReference,
            string notes = null);

        /// <summary>
        /// Get outstanding bills for a customer
        /// </summary>
        Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(int customerId);

        /// <summary>
        /// Get payment history for a customer
        /// </summary>
        Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(
            int customerId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Get bills due for payment
        /// </summary>
        Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime? dueDate = null);

        /// <summary>
        /// Calculate late payment charges for a bill
        /// </summary>
        Task<decimal> CalculateLatePaymentChargesAsync(int billId);

        /// <summary>
        /// Update bill status
        /// </summary>
        Task UpdateBillStatusAsync(int billId, BillStatus newStatus, string notes = null);

        /// <summary>
        /// Get billing summary for a period
        /// </summary>
        Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate);
    }
} 