using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRecordRepository _paymentRepository;
        private readonly IMonthlyBillRepository _billRepository;
        private readonly IAuditService _auditService;

        public PaymentService(
            IPaymentRecordRepository paymentRepository,
            IMonthlyBillRepository billRepository,
            IAuditService auditService)
        {
            _paymentRepository = paymentRepository;
            _billRepository = billRepository;
            _auditService = auditService;
        }

        public async Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment)
        {
            var bill = await _billRepository.GetByIdAsync(payment.MonthlyBillId);
            if (bill == null)
                throw new ArgumentException("Bill not found", nameof(payment.MonthlyBillId));

            if (bill.Status == BillStatus.Paid)
                throw new InvalidOperationException("Bill is already paid");

            payment.PaymentDate = DateTime.UtcNow;
            payment.Status = PaymentStatus.Completed;
            payment.CreatedAt = DateTime.UtcNow;
            payment.LastModifiedAt = DateTime.UtcNow;

            await _paymentRepository.AddAsync(payment);
            await _auditService.LogActionAsync("Payment", payment.Id, "Record", $"Recorded payment for bill {payment.MonthlyBillId}");

            bill.Status = BillStatus.Paid;
            bill.PaidDate = payment.PaymentDate;
            bill.LastModifiedAt = DateTime.UtcNow;
            await _billRepository.UpdateAsync(bill);

            return payment;
        }

        public async Task<PaymentRecord?> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerAsync(int customerId)
        {
            return await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByBillAsync(int billId)
        {
            return await _paymentRepository.GetPaymentsByBillIdAsync(billId);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _paymentRepository.GetTotalPaymentsForBillAsync(billId);
        }

        public async Task<bool> CancelPaymentAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (payment.Status != PaymentStatus.Completed)
                return false;

            payment.Status = PaymentStatus.Cancelled;
            payment.LastModifiedAt = DateTime.UtcNow;

            var bill = await _billRepository.GetByIdAsync(payment.MonthlyBillId);
            if (bill != null)
            {
                bill.Status = BillStatus.Pending;
                bill.LastModifiedAt = DateTime.UtcNow;
                await _billRepository.UpdateAsync(bill);
            }

            await _paymentRepository.UpdateAsync(payment);
            await _auditService.LogActionAsync("Payment", paymentId, "Cancel", $"Cancelled payment {paymentId}");

            return true;
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (payment.Status != PaymentStatus.Completed || amount > payment.Amount)
                return false;

            payment.Status = PaymentStatus.Refunded;
            payment.LastModifiedAt = DateTime.UtcNow;
            payment.Notes = $"Refunded: {reason}";

            await _paymentRepository.UpdateAsync(payment);
            await _auditService.LogActionAsync("Payment", paymentId, "Refund", $"Refunded payment {paymentId} - Amount: {amount}, Reason: {reason}");

            return true;
        }
    }
} 