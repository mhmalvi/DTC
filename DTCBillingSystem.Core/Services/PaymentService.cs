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

        public PaymentService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<Payment> RecordPaymentAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(payment.InvoiceId);
            if (invoice == null)
                throw new InvalidOperationException("Invoice not found");

            if (invoice.Status == BillStatus.Paid)
                throw new InvalidOperationException("Invoice is already paid");

            payment.Status = BillStatus.Paid;
            payment.CreatedAt = DateTime.UtcNow;
            payment.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Update invoice status
            invoice.Status = BillStatus.Paid;
            invoice.PaymentReference = payment.ReferenceNumber;
            invoice.PaidDate = payment.PaymentDate;
            invoice.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Invoices.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "Payment",
                "Create",
                payment.CreatedBy,
                $"Recorded payment of {payment.Amount:C} for invoice {invoice.InvoiceNumber}"
            );

            return payment;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _unitOfWork.Payments.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(int customerId)
        {
            return await _unitOfWork.Payments.FindAsync(p => p.CustomerId == customerId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId)
        {
            return await _unitOfWork.Payments.FindAsync(p => p.InvoiceId == invoiceId);
        }

        public async Task<bool> VoidPaymentAsync(int paymentId, int userId, string reason)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (payment.IsVoid)
                return false;

            payment.IsVoid = true;
            payment.VoidReason = reason;
            payment.LastModifiedBy = userId;
            payment.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "Payment",
                "Void",
                userId,
                $"Voided payment {payment.ReferenceNumber}"
            );

            return true;
        }
    }
} 