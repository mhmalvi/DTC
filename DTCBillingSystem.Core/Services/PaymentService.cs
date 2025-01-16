using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private const int SYSTEM_USER_ID = 1;

        public PaymentService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(payment.MonthlyBillId);
            if (bill == null)
                throw new InvalidOperationException("Bill not found");

            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = PaymentStatus.Completed;

            await _unitOfWork.PaymentRecords.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            int userId = int.TryParse(payment.CreatedBy, out int id) ? id : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Payment",
                payment.Id.ToString(),
                userId,
                AuditAction.Create.ToString(),
                $"Payment recorded for Bill {payment.MonthlyBillId}");

            return payment;
        }

        public async Task<PaymentRecord?> GetPaymentByIdAsync(int id)
        {
            return await _unitOfWork.PaymentRecords.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerAsync(int customerId)
        {
            return await _unitOfWork.PaymentRecords.GetPaymentsByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByBillAsync(int billId)
        {
            return await _unitOfWork.PaymentRecords.GetPaymentsByBillIdAsync(billId);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.PaymentRecords.GetPaymentsByDateRangeAsync(startDate, endDate);
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _unitOfWork.PaymentRecords.GetTotalPaymentsForBillAsync(billId);
        }

        public async Task<bool> CancelPaymentAsync(int paymentId)
        {
            var payment = await _unitOfWork.PaymentRecords.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Cancelled;
            payment.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            int userId = payment.LastModifiedBy != null && int.TryParse(payment.LastModifiedBy, out int id) ? id : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Payment",
                paymentId.ToString(),
                userId,
                AuditAction.Update.ToString(),
                "Payment cancelled");

            return true;
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason)
        {
            var payment = await _unitOfWork.PaymentRecords.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (amount > payment.Amount)
                throw new InvalidOperationException("Refund amount cannot be greater than payment amount");

            payment.Status = PaymentStatus.Refunded;
            payment.LastModifiedAt = DateTime.UtcNow;
            payment.Notes = $"Refunded: {reason}";

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Payment",
                paymentId.ToString(),
                int.TryParse(payment.LastModifiedBy, out int userId) ? userId : SYSTEM_USER_ID,
                AuditAction.Update.ToString(),
                $"Payment refunded. Amount: {amount}, Reason: {reason}");

            return true;
        }
    }
} 