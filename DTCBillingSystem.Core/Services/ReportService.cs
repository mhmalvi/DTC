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
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.BillingDate >= startDate && b.BillingDate <= endDate,
                null,
                false);

            var payments = await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.CreatedAt >= startDate && p.CreatedAt <= endDate,
                null,
                false);

            var billsList = bills.ToList();
            var paymentsList = payments.ToList();

            var summary = new BillingSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBillsGenerated = billsList.Count(),
                TotalBillAmount = billsList.Sum(b => b.TotalAmount),
                TotalPaymentsReceived = paymentsList.Sum(p => p.Amount),
                TotalOutstandingAmount = billsList.Sum(b => b.TotalAmount) - paymentsList.Sum(p => p.Amount),

                BillsByStatus = billsList
                    .GroupBy(b => b.Status)
                    .Select(g => new BillStatusSummary
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(b => b.TotalAmount)
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

            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId && b.BillingDate >= startDate && b.BillingDate <= endDate,
                null,
                false);

            var payments = await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && p.CreatedAt >= startDate && p.CreatedAt <= endDate,
                "MonthlyBill",
                false);

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
            var previousBills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId && b.BillingDate < startDate,
                null,
                false);

            var previousPayments = await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && p.CreatedAt < startDate,
                "MonthlyBill",
                false);

            return previousBills.Sum(b => b.TotalAmount) - previousPayments.Sum(p => p.Amount);
        }

        private async Task<decimal> GetClosingBalanceAsync(int customerId, DateTime endDate)
        {
            var allBills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId && b.BillingDate <= endDate,
                null,
                false);

            var allPayments = await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && p.CreatedAt <= endDate,
                "MonthlyBill",
                false);

            return allBills.Sum(b => b.TotalAmount) - allPayments.Sum(p => p.Amount);
        }

        private async Task<List<StatementTransaction>> GetTransactionsAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<StatementTransaction>();

            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId && b.BillingDate >= startDate && b.BillingDate <= endDate,
                null,
                false);

            var payments = await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && p.CreatedAt >= startDate && p.CreatedAt <= endDate,
                "MonthlyBill",
                false);

            foreach (var bill in bills)
            {
                transactions.Add(new StatementTransaction
                {
                    Date = bill.BillingDate,
                    Description = $"Bill #{bill.BillNumber}",
                    ReferenceNumber = bill.BillNumber,
                    TransactionType = StatementTransactionType.Bill,
                    Amount = bill.TotalAmount,
                    Balance = 0 // Will be calculated later
                });
            }

            foreach (var payment in payments)
            {
                transactions.Add(new StatementTransaction
                {
                    Date = payment.CreatedAt,
                    Description = $"Payment - {payment.PaymentMethod}",
                    ReferenceNumber = payment.TransactionId,
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
            var overdueBills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId && b.DueDate < asOfDate && b.Status != BillStatus.Paid,
                null,
                false);

            return overdueBills
                .Select(b => new OverdueBill
                {
                    BillNumber = b.BillNumber,
                    BillDate = b.BillingDate,
                    DueDate = b.DueDate,
                    Amount = b.TotalAmount,
                    DaysOverdue = (int)(asOfDate - b.DueDate).TotalDays
                })
                .OrderBy(b => b.DueDate)
                .ToList();
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.BillingDate >= startDate && b.BillingDate <= endDate,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetUnpaidBillsAsync()
        {
            return await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.Status == BillStatus.Unpaid,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetOverdueBillsAsync()
        {
            var today = DateTime.Today;
            return await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.Status == BillStatus.Unpaid && b.DueDate < today,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<decimal> GetTotalCollectionsForMonthAsync(DateTime date)
        {
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.BillingDate.Month == date.Month && b.BillingDate.Year == date.Year,
                "PaymentRecords",
                false);
            
            return bills.SelectMany(b => b.PaymentRecords).Sum(p => p.Amount);
        }

        public async Task<decimal> GetTotalBilledAmountForMonthAsync(DateTime date)
        {
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.BillingDate.Month == date.Month && b.BillingDate.Year == date.Year,
                null,
                false);
            
            return bills.Sum(b => b.TotalAmount);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByCustomerAsync(int customerId)
        {
            return await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.CustomerId == customerId,
                "PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByStatusAsync(BillStatus status)
        {
            return await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.Status == status,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.PaymentDate >= startDate && p.PaymentDate <= endDate,
                "MonthlyBill",
                false);
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerAsync(int customerId)
        {
            return await _unitOfWork.PaymentRecords.GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId,
                "MonthlyBill",
                false);
        }

        public async Task<decimal> GetTotalCollectionsAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Sum(p => p.Amount);
        }

        public async Task<decimal> GetTotalReceivablesAsync()
        {
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.Status != BillStatus.Paid,
                null,
                false);
            return bills.Sum(b => b.TotalAmount);
        }

        public async Task<IEnumerable<Customer>> GetDelinquentCustomersAsync()
        {
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync(
                b => b.Status == BillStatus.Overdue,
                "Customer",
                false);
            var customerIds = bills.Select(b => b.CustomerId).Distinct();
            
            return await _unitOfWork.Customers.GetAllAsync(
                c => customerIds.Contains(c.Id),
                null,
                false);
        }
    }
} 