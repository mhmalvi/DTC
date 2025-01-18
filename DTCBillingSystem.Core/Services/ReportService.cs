using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Reports;

namespace DTCBillingSystem.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var bills = await _unitOfWork.Invoices.FindAsync(b => 
                b.BillingMonth >= startDate && 
                b.BillingMonth <= endDate);

            var payments = await _unitOfWork.Payments.FindAsync(p => 
                p.PaymentDate >= startDate && 
                p.PaymentDate <= endDate &&
                !p.IsVoid);

            var billsList = bills.ToList();
            var paymentsList = payments.ToList();

            var summary = new BillingSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBillsGenerated = billsList.Count(),
                TotalBillAmount = billsList.Sum(b => b.Amount),
                TotalPaymentsReceived = paymentsList.Sum(p => p.Amount),
                TotalOutstandingAmount = billsList.Sum(b => b.Amount) - paymentsList.Sum(p => p.Amount),

                BillsByStatus = billsList
                    .GroupBy(b => b.Status)
                    .Select(g => new BillStatusSummary
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(b => b.Amount)
                    })
                    .ToList(),

                PaymentsByMethod = paymentsList
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodSummary
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToList()
            };

            return summary;
        }

        public async Task<CustomerStatement> GetCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new InvalidOperationException($"Customer with ID {customerId} not found.");

            var bills = await _unitOfWork.Invoices
                .FindAsync(b => b.CustomerId == customerId && 
                               b.BillingMonth >= startDate && 
                               b.BillingMonth <= endDate);

            var payments = await _unitOfWork.Payments
                .FindAsync(p => p.CustomerId == customerId && 
                               p.PaymentDate >= startDate && 
                               p.PaymentDate <= endDate &&
                               !p.IsVoid);

            var billsList = bills.ToList();
            var paymentsList = payments.ToList();

            var statement = new CustomerStatement
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                StartDate = startDate,
                EndDate = endDate,
                OpeningBalance = await GetOpeningBalanceAsync(customerId, startDate),
                ClosingBalance = await GetClosingBalanceAsync(customerId, endDate),
                Transactions = await GetTransactionsAsync(customerId, startDate, endDate),
                OverdueBills = await GetOverdueBillsAsync(customerId, endDate)
            };

            return statement;
        }

        private async Task<decimal> GetOpeningBalanceAsync(int customerId, DateTime startDate)
        {
            var previousBills = await _unitOfWork.Invoices
                .FindAsync(b => b.CustomerId == customerId && 
                               b.BillingMonth < startDate);

            var previousPayments = await _unitOfWork.Payments
                .FindAsync(p => p.CustomerId == customerId && 
                               p.PaymentDate < startDate &&
                               !p.IsVoid);

            return previousBills.Sum(b => b.Amount) - previousPayments.Sum(p => p.Amount);
        }

        private async Task<decimal> GetClosingBalanceAsync(int customerId, DateTime endDate)
        {
            var allBills = await _unitOfWork.Invoices
                .FindAsync(b => b.CustomerId == customerId && 
                               b.BillingMonth <= endDate);

            var allPayments = await _unitOfWork.Payments
                .FindAsync(p => p.CustomerId == customerId && 
                               p.PaymentDate <= endDate &&
                               !p.IsVoid);

            return allBills.Sum(b => b.Amount) - allPayments.Sum(p => p.Amount);
        }

        private async Task<List<StatementTransaction>> GetTransactionsAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<StatementTransaction>();

            var bills = await _unitOfWork.Invoices
                .FindAsync(b => b.CustomerId == customerId && 
                               b.BillingMonth >= startDate && 
                               b.BillingMonth <= endDate);

            var payments = await _unitOfWork.Payments
                .FindAsync(p => p.CustomerId == customerId && 
                               p.PaymentDate >= startDate && 
                               p.PaymentDate <= endDate &&
                               !p.IsVoid);

            foreach (var bill in bills)
            {
                transactions.Add(new StatementTransaction
                {
                    Date = bill.BillingMonth,
                    Description = $"Bill #{bill.InvoiceNumber}",
                    ReferenceNumber = bill.InvoiceNumber,
                    TransactionType = StatementTransactionType.Bill,
                    Amount = bill.Amount,
                    Balance = 0 // Will be calculated later
                });
            }

            foreach (var payment in payments)
            {
                transactions.Add(new StatementTransaction
                {
                    Date = payment.PaymentDate,
                    Description = $"Payment - {payment.PaymentMethod}",
                    ReferenceNumber = payment.ReferenceNumber,
                    TransactionType = StatementTransactionType.Payment,
                    Amount = -payment.Amount,
                    Balance = 0 // Will be calculated later
                });
            }

            transactions = transactions.OrderBy(t => t.Date).ToList();
            decimal runningBalance = await GetOpeningBalanceAsync(customerId, startDate);

            foreach (var transaction in transactions)
            {
                runningBalance += transaction.Amount;
                transaction.Balance = runningBalance;
            }

            return transactions;
        }

        private async Task<List<OverdueBill>> GetOverdueBillsAsync(int customerId, DateTime asOfDate)
        {
            var overdueBills = await _unitOfWork.Invoices
                .FindAsync(b => b.CustomerId == customerId && 
                               b.DueDate < asOfDate && 
                               b.Status != BillStatus.Paid);

            return overdueBills
                .Select(b => new OverdueBill
                {
                    BillNumber = b.InvoiceNumber,
                    BillDate = b.BillingMonth,
                    DueDate = b.DueDate,
                    Amount = b.Amount,
                    DaysOverdue = (int)(asOfDate - b.DueDate).TotalDays
                })
                .OrderBy(b => b.DueDate)
                .ToList();
        }
    }
} 