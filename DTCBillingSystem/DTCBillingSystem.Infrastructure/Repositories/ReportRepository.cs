using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Reports;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportRepository(DbContext context, IUnitOfWork unitOfWork) : base(context)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var bills = await _unitOfWork.MonthlyBills
                .GetBillsByDateRangeAsync(startDate, endDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetPaymentsByDateRangeAsync(startDate, endDate);

            var billsList = bills.ToList();
            var paymentsList = payments.ToList();

            var summary = new BillingSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBillsGenerated = billsList.Count,
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
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            var bills = await _unitOfWork.MonthlyBills
                .GetCustomerBillsByDateRangeAsync(customerId, startDate, endDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetCustomerPaymentsByDateRangeAsync(customerId, startDate, endDate);

            var statement = new CustomerStatement
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                AccountNumber = customer.AccountNumber,
                StartDate = startDate,
                EndDate = endDate,
                OpeningBalance = await GetOpeningBalanceAsync(customerId, startDate),
                ClosingBalance = await GetClosingBalanceAsync(customerId, endDate),
                Transactions = await GetTransactionsAsync(customerId, startDate, endDate),
                OverdueBills = await GetOverdueBillsAsync(customerId, endDate)
            };

            return statement;
        }

        public async Task<DailyCollectionReport> GetDailyCollectionReportAsync(DateTime date)
        {
            var payments = await _unitOfWork.PaymentRecords
                .GetPaymentsByDateAsync(date);

            var paymentsList = payments.ToList();

            var report = new DailyCollectionReport
            {
                Date = date,
                TotalCollection = paymentsList.Sum(p => p.Amount),
                NumberOfPayments = paymentsList.Count,
                PaymentsByMethod = paymentsList
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentSummary
                    {
                        PaymentMethod = g.Key.ToString(),
                        Amount = g.Sum(p => p.Amount),
                        Count = g.Count()
                    })
                    .ToList(),
                Payments = paymentsList
                    .Select(p => new PaymentDetail
                    {
                        PaymentDate = p.PaymentDate,
                        CustomerName = p.Customer.Name,
                        AccountNumber = p.Customer.AccountNumber,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod.ToString(),
                        ReferenceNumber = p.ReferenceNumber
                    })
                    .ToList()
            };

            return report;
        }

        public async Task<MonthlyBillingReport> GetMonthlyBillingReportAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bills = await _unitOfWork.MonthlyBills
                .GetBillsByDateRangeAsync(startDate, endDate);

            var billsList = bills.ToList();

            var report = new MonthlyBillingReport
            {
                Month = month,
                Year = year,
                TotalBilled = billsList.Sum(b => b.TotalAmount),
                NumberOfBills = billsList.Count,
                AverageBillAmount = billsList.Any() ? billsList.Average(b => b.TotalAmount) : 0,
                BillingsByZone = billsList
                    .GroupBy(b => b.Customer.ZoneCode ?? "Unknown")
                    .Select(g => new BillingSummaryByZone
                    {
                        Zone = g.Key,
                        NumberOfCustomers = g.Select(b => b.CustomerId).Distinct().Count(),
                        TotalBilled = g.Sum(b => b.TotalAmount),
                        AverageBill = g.Average(b => b.TotalAmount)
                    })
                    .ToList(),
                Bills = billsList
                    .Select(b => new BillDetail
                    {
                        BillNumber = b.BillNumber,
                        CustomerName = b.Customer.Name,
                        AccountNumber = b.Customer.AccountNumber,
                        Amount = b.TotalAmount,
                        DueDate = b.DueDate,
                        Status = b.Status.ToString()
                    })
                    .ToList()
            };

            return report;
        }

        public async Task<OutstandingPaymentsReport> GetOutstandingPaymentsReportAsync()
        {
            var outstandingBills = await _unitOfWork.MonthlyBills
                .FindAsync(b => b.Status == BillStatus.Unpaid);

            var billsList = outstandingBills.ToList();

            var customerOutstandings = billsList
                .GroupBy(b => b.CustomerId)
                .Select(g => new CustomerOutstanding
                {
                    CustomerName = g.First().Customer.Name,
                    AccountNumber = g.First().Customer.AccountNumber,
                    OutstandingAmount = g.Sum(b => b.TotalAmount),
                    NumberOfUnpaidBills = g.Count(),
                    OldestUnpaidBillDate = g.Min(b => b.BillingDate)
                })
                .ToList();

            var report = new OutstandingPaymentsReport
            {
                GeneratedDate = DateTime.Now,
                TotalOutstanding = customerOutstandings.Sum(c => c.OutstandingAmount),
                NumberOfCustomers = customerOutstandings.Count,
                CustomerOutstandings = customerOutstandings
            };

            return report;
        }

        public async Task<RevenueSummaryReport> GetRevenueSummaryReportAsync(DateTime startDate, DateTime endDate)
        {
            var bills = await _unitOfWork.MonthlyBills
                .GetBillsByDateRangeAsync(startDate, endDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetPaymentsByDateRangeAsync(startDate, endDate);

            var billsList = bills.ToList();
            var paymentsList = payments.ToList();

            var monthlyRevenues = billsList
                .GroupBy(b => new { b.BillingMonth.Year, b.BillingMonth.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Billed = g.Sum(b => b.TotalAmount),
                    Collected = paymentsList
                        .Where(p => p.PaymentDate.Year == g.Key.Year && p.PaymentDate.Month == g.Key.Month)
                        .Sum(p => p.Amount),
                    CollectionRate = g.Sum(b => b.TotalAmount) > 0
                        ? (paymentsList
                            .Where(p => p.PaymentDate.Year == g.Key.Year && p.PaymentDate.Month == g.Key.Month)
                            .Sum(p => p.Amount) / g.Sum(b => b.TotalAmount)) * 100
                        : 0
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            var totalBilled = billsList.Sum(b => b.TotalAmount);
            var totalCollected = paymentsList.Sum(p => p.Amount);

            var report = new RevenueSummaryReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalCollected,
                TotalBilled = totalBilled,
                TotalCollected = totalCollected,
                CollectionRate = totalBilled > 0 ? (totalCollected / totalBilled) * 100 : 0,
                MonthlyRevenues = monthlyRevenues
            };

            return report;
        }

        private async Task<decimal> GetOpeningBalanceAsync(int customerId, DateTime startDate)
        {
            var bills = await _unitOfWork.MonthlyBills
                .GetCustomerBillsBeforeDateAsync(customerId, startDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetCustomerPaymentsBeforeDateAsync(customerId, startDate);

            var totalBilled = bills.Sum(b => b.TotalAmount);
            var totalPaid = payments.Sum(p => p.Amount);

            return totalBilled - totalPaid;
        }

        private async Task<decimal> GetClosingBalanceAsync(int customerId, DateTime endDate)
        {
            var bills = await _unitOfWork.MonthlyBills
                .GetCustomerBillsBeforeDateAsync(customerId, endDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetCustomerPaymentsBeforeDateAsync(customerId, endDate);

            var totalBilled = bills.Sum(b => b.TotalAmount);
            var totalPaid = payments.Sum(p => p.Amount);

            return totalBilled - totalPaid;
        }

        private async Task<List<StatementTransaction>> GetTransactionsAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<StatementTransaction>();

            var bills = await _unitOfWork.MonthlyBills
                .GetCustomerBillsByDateRangeAsync(customerId, startDate, endDate);

            var payments = await _unitOfWork.PaymentRecords
                .GetCustomerPaymentsByDateRangeAsync(customerId, startDate, endDate);

            foreach (var bill in bills)
            {
                transactions.Add(new StatementTransaction
                {
                    Date = bill.BillingMonth,
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
            var overdueBills = await _unitOfWork.MonthlyBills
                .FindAsync(b => b.CustomerId == customerId && 
                               b.DueDate < asOfDate && 
                               b.Status == BillStatus.Unpaid);

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
    }
} 