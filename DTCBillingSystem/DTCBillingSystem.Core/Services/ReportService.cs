using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(ILogger<ReportService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DailyCollectionReport> GenerateDailyCollectionReportAsync(DateTime date)
        {
            try
            {
                var payments = await _unitOfWork.PaymentRecords.FindAsync(p => p.CreatedAt.Date == date.Date);
                var report = new DailyCollectionReport
                {
                    Date = date,
                    TotalCollections = 0,
                    PaymentsByMethod = new Dictionary<PaymentMethod, decimal>(),
                    TotalTransactions = payments.Count
                };

                foreach (var payment in payments)
                {
                    report.TotalCollections += payment.Amount;
                    if (!report.PaymentsByMethod.ContainsKey(payment.Method))
                    {
                        report.PaymentsByMethod[payment.Method] = 0;
                    }
                    report.PaymentsByMethod[payment.Method] += payment.Amount;
                }

                report.AverageTransactionAmount = report.TotalTransactions > 0 
                    ? report.TotalCollections / report.TotalTransactions 
                    : 0;

                var pendingBills = await _unitOfWork.MonthlyBills.FindAsync(b => 
                    b.DueDate.Date == date.Date && 
                    b.Status == BillStatus.Pending);

                report.PendingCollections = 0;
                foreach (var bill in pendingBills)
                {
                    report.PendingCollections += bill.Amount;
                }

                var overdueBills = await _unitOfWork.MonthlyBills.FindAsync(b => 
                    b.DueDate.Date < date.Date && 
                    b.Status == BillStatus.Overdue);

                report.OverdueCollections = 0;
                foreach (var bill in overdueBills)
                {
                    report.OverdueCollections += bill.Amount;
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily collection report for {Date}", date);
                throw;
            }
        }

        public async Task<MonthlyBillingReport> GenerateMonthlyBillingReportAsync(DateTime month)
        {
            try
            {
                var startDate = new DateTime(month.Year, month.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var bills = await _unitOfWork.MonthlyBills.FindAsync(b => 
                    b.BillDate >= startDate && 
                    b.BillDate <= endDate);

                var report = new MonthlyBillingReport
                {
                    Month = startDate,
                    TotalBilledAmount = 0,
                    TotalBills = bills.Count,
                    BillsByStatus = new Dictionary<BillStatus, int>(),
                    TotalCollected = 0,
                    TotalOutstanding = 0,
                    TopCustomers = new List<CustomerBillingSummary>()
                };

                var customerSummaries = new Dictionary<int, CustomerBillingSummary>();

                foreach (var bill in bills)
                {
                    report.TotalBilledAmount += bill.Amount;

                    if (!report.BillsByStatus.ContainsKey(bill.Status))
                    {
                        report.BillsByStatus[bill.Status] = 0;
                    }
                    report.BillsByStatus[bill.Status]++;

                    if (bill.Status == BillStatus.Paid)
                    {
                        report.TotalCollected += bill.Amount;
                    }
                    else
                    {
                        report.TotalOutstanding += bill.Amount;
                    }

                    if (!customerSummaries.ContainsKey(bill.CustomerId))
                    {
                        var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId);
                        customerSummaries[bill.CustomerId] = new CustomerBillingSummary
                        {
                            CustomerId = bill.CustomerId,
                            CustomerName = customer.Name,
                            TotalBilled = 0,
                            TotalPaid = 0,
                            TotalBills = 0,
                            LastBillingDate = bill.BillDate
                        };
                    }

                    var summary = customerSummaries[bill.CustomerId];
                    summary.TotalBilled += bill.Amount;
                    summary.TotalBills++;
                    if (bill.Status == BillStatus.Paid)
                    {
                        summary.TotalPaid += bill.Amount;
                    }
                    if (bill.BillDate > summary.LastBillingDate)
                    {
                        summary.LastBillingDate = bill.BillDate;
                    }
                }

                report.CollectionRate = report.TotalBilledAmount > 0 
                    ? (report.TotalCollected / report.TotalBilledAmount) * 100 
                    : 0;

                report.TopCustomers = customerSummaries.Values
                    .OrderByDescending(c => c.TotalBilled)
                    .Take(10)
                    .ToList();

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly billing report for {Month}", month);
                throw;
            }
        }

        public async Task<OutstandingPaymentsReport> GenerateOutstandingPaymentsReportAsync(DateTime? asOfDate = null)
        {
            try
            {
                var effectiveDate = asOfDate ?? DateTime.Now.Date;
                var outstandingBills = await _unitOfWork.MonthlyBills.FindAsync(b => 
                    b.Status != BillStatus.Paid && 
                    b.BillDate <= effectiveDate);

                var report = new OutstandingPaymentsReport
                {
                    GeneratedAt = effectiveDate,
                    TotalOutstandingAmount = 0,
                    TotalOutstandingBills = outstandingBills.Count,
                    OutstandingByAgeGroup = new Dictionary<string, decimal>(),
                    CustomerDetails = new List<CustomerOutstandingBills>()
                };

                var customerBills = new Dictionary<int, CustomerOutstandingBills>();

                foreach (var bill in outstandingBills)
                {
                    report.TotalOutstandingAmount += bill.OutstandingAmount;

                    var ageGroup = GetAgeGroup((effectiveDate - bill.DueDate).Days);
                    if (!report.OutstandingByAgeGroup.ContainsKey(ageGroup))
                    {
                        report.OutstandingByAgeGroup[ageGroup] = 0;
                    }
                    report.OutstandingByAgeGroup[ageGroup] += bill.OutstandingAmount;

                    if (!customerBills.ContainsKey(bill.CustomerId))
                    {
                        var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId);
                        customerBills[bill.CustomerId] = new CustomerOutstandingBills
                        {
                            CustomerId = bill.CustomerId,
                            CustomerName = customer.Name,
                            TotalOutstanding = 0,
                            NumberOfBills = 0,
                            OldestBillDate = bill.BillDate,
                            Bills = new List<BillSummary>()
                        };
                    }

                    var customerDetail = customerBills[bill.CustomerId];
                    customerDetail.TotalOutstanding += bill.OutstandingAmount;
                    customerDetail.NumberOfBills++;
                    if (bill.BillDate < customerDetail.OldestBillDate)
                    {
                        customerDetail.OldestBillDate = bill.BillDate;
                    }

                    customerDetail.Bills.Add(new BillSummary
                    {
                        BillId = bill.Id,
                        BillDate = bill.BillDate,
                        DueDate = bill.DueDate,
                        Amount = bill.Amount,
                        AmountPaid = bill.Amount - bill.OutstandingAmount,
                        OutstandingAmount = bill.OutstandingAmount,
                        DaysOverdue = (effectiveDate - bill.DueDate).Days,
                        Status = bill.Status
                    });
                }

                report.CustomerDetails = customerBills.Values
                    .OrderByDescending(c => c.TotalOutstanding)
                    .ToList();

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outstanding payments report for {Date}", asOfDate);
                throw;
            }
        }

        public async Task<byte[]> ExportToPdfAsync(object report)
        {
            try
            {
                // TODO: Implement PDF export logic
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to PDF");
                throw;
            }
        }

        public async Task<byte[]> ExportToExcelAsync(object report)
        {
            try
            {
                // TODO: Implement Excel export logic
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to Excel");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GenerateCustomReportAsync(string reportType, Dictionary<string, object> parameters)
        {
            try
            {
                // TODO: Implement custom report generation logic
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report of type {ReportType}", reportType);
                throw;
            }
        }

        private string GetAgeGroup(int daysOverdue)
        {
            if (daysOverdue <= 30) return "0-30 days";
            if (daysOverdue <= 60) return "31-60 days";
            if (daysOverdue <= 90) return "61-90 days";
            if (daysOverdue <= 180) return "91-180 days";
            return "Over 180 days";
        }
    }
} 