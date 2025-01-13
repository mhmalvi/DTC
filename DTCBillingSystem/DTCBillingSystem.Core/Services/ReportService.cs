using Microsoft.Extensions.Logging;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;
        private readonly IAuditService _auditService;

        public ReportService(
            IUnitOfWork unitOfWork,
            ILogger<ReportService> logger,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<DailyCollectionReport> GenerateDailyCollectionReportAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                // Get all payments for the specified date
                var payments = await _unitOfWork.PaymentRecords.FindAsync(p =>
                    p.PaymentDate >= startDate && p.PaymentDate < endDate);

                var paymentsList = payments.ToList();

                var report = new DailyCollectionReport
                {
                    Date = date,
                    TotalCollection = paymentsList.Sum(p => p.AmountPaid),
                    TotalLateCharges = paymentsList.Sum(p => p.LatePaymentCharges),
                    PaymentsByMethod = paymentsList
                        .GroupBy(p => p.PaymentMethod)
                        .ToDictionary(
                            g => g.Key,
                            g => new PaymentMethodSummary
                            {
                                Count = g.Count(),
                                Amount = g.Sum(p => p.AmountPaid)
                            }
                        ),
                    CollectionsByFloor = await GetCollectionsByFloorAsync(paymentsList),
                    DetailedTransactions = await GetDetailedTransactionsAsync(paymentsList)
                };

                await _auditService.LogActionAsync(
                    "Report",
                    0,
                    AuditAction.Generated,
                    null,
                    $"Generated daily collection report for {date:d}"
                );

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
                var bills = await _unitOfWork.MonthlyBillsExt.GetBillsByMonthAsync(month);
                var billsList = bills.ToList();

                var report = new MonthlyBillingReport
                {
                    Month = new DateTime(month.Year, month.Month, 1),
                    TotalBilled = billsList.Sum(b => b.TotalAmount),
                    TotalPaid = billsList.Sum(b => b.Payments.Sum(p => p.AmountPaid)),
                    TotalOutstanding = billsList.Sum(b => 
                        b.TotalAmount - b.Payments.Sum(p => p.AmountPaid)),
                    BillingsByFloor = await GetBillingsByFloorAsync(billsList),
                    BillingsByStatus = billsList
                        .GroupBy(b => b.Status)
                        .ToDictionary(
                            g => g.Key,
                            g => new BillingStatusSummary
                            {
                                Count = g.Count(),
                                Amount = g.Sum(b => b.TotalAmount)
                            }
                        ),
                    DetailedBills = billsList.Select(b => new DetailedBillInfo
                    {
                        BillId = b.Id,
                        CustomerName = b.Customer.Name,
                        ShopNo = b.Customer.ShopNo,
                        Floor = b.Customer.Floor,
                        Amount = b.TotalAmount,
                        Status = b.Status,
                        DueDate = b.DueDate,
                        PaidAmount = b.Payments.Sum(p => p.AmountPaid)
                    }).ToList()
                };

                await _auditService.LogActionAsync(
                    "Report",
                    0,
                    AuditAction.Generated,
                    null,
                    $"Generated monthly billing report for {month:MMM yyyy}"
                );

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly billing report for {Month}", month);
                throw;
            }
        }

        public async Task<CustomerStatement> GenerateCustomerStatementAsync(
            int customerId,
            DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                var customer = await _unitOfWork.CustomersExt.GetCustomerWithBillsAsync(customerId);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                var bills = await _unitOfWork.MonthlyBillsExt.GetCustomerBillsAsync(customerId);
                var billsInPeriod = bills.Where(b => 
                    b.BillingMonth >= fromDate && b.BillingMonth <= toDate).ToList();

                var statement = new CustomerStatement
                {
                    CustomerId = customerId,
                    CustomerName = customer.Name,
                    ShopNo = customer.ShopNo,
                    Floor = customer.Floor,
                    FromDate = fromDate,
                    ToDate = toDate,
                    OpeningBalance = await CalculateOpeningBalanceAsync(customerId, fromDate),
                    ClosingBalance = await CalculateClosingBalanceAsync(customerId, toDate),
                    Transactions = await GetStatementTransactionsAsync(billsInPeriod)
                };

                await _auditService.LogActionAsync(
                    "Report",
                    customerId,
                    AuditAction.Generated,
                    null,
                    $"Generated statement for {customer.Name} ({fromDate:d} - {toDate:d})"
                );

                return statement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error generating customer statement for {CustomerId} ({FromDate} - {ToDate})", 
                    customerId, fromDate, toDate);
                throw;
            }
        }

        public async Task<OutstandingPaymentsReport> GenerateOutstandingPaymentsReportAsync()
        {
            try
            {
                var overdueBills = await _unitOfWork.MonthlyBillsExt.GetOverdueBillsAsync();
                var billsList = overdueBills.ToList();

                var report = new OutstandingPaymentsReport
                {
                    GeneratedDate = DateTime.UtcNow,
                    TotalOutstanding = billsList.Sum(b => 
                        b.TotalAmount - b.Payments.Sum(p => p.AmountPaid)),
                    OutstandingByFloor = await GetOutstandingByFloorAsync(billsList),
                    DetailedOutstandings = billsList.Select(b => new DetailedOutstanding
                    {
                        BillId = b.Id,
                        CustomerName = b.Customer.Name,
                        ShopNo = b.Customer.ShopNo,
                        Floor = b.Customer.Floor,
                        BillingMonth = b.BillingMonth,
                        Amount = b.TotalAmount,
                        PaidAmount = b.Payments.Sum(p => p.AmountPaid),
                        DueDate = b.DueDate,
                        DaysOverdue = (DateTime.UtcNow.Date - b.DueDate.Date).Days
                    }).OrderByDescending(d => d.DaysOverdue).ToList()
                };

                await _auditService.LogActionAsync(
                    "Report",
                    0,
                    AuditAction.Generated,
                    null,
                    "Generated outstanding payments report"
                );

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outstanding payments report");
                throw;
            }
        }

        public async Task<RevenueSummaryReport> GenerateRevenueSummaryReportAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                var bills = await _unitOfWork.MonthlyBills.FindAsync(b =>
                    b.BillingMonth >= fromDate && b.BillingMonth <= toDate);
                var billsList = bills.ToList();

                var report = new RevenueSummaryReport
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalRevenue = billsList.Sum(b => b.Payments.Sum(p => p.AmountPaid)),
                    TotalBilled = billsList.Sum(b => b.TotalAmount),
                    TotalOutstanding = billsList.Sum(b => 
                        b.TotalAmount - b.Payments.Sum(p => p.AmountPaid)),
                    RevenueByMonth = GetRevenueByMonth(billsList),
                    RevenueByCategory = GetRevenueByCategory(billsList),
                    CollectionEfficiency = CalculateCollectionEfficiency(billsList)
                };

                await _auditService.LogActionAsync(
                    "Report",
                    0,
                    AuditAction.Generated,
                    null,
                    $"Generated revenue summary report ({fromDate:d} - {toDate:d})"
                );

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error generating revenue summary report ({FromDate} - {ToDate})", 
                    fromDate, toDate);
                throw;
            }
        }

        private async Task<Dictionary<string, decimal>> GetCollectionsByFloorAsync(
            List<PaymentRecord> payments)
        {
            var result = new Dictionary<string, decimal>();
            foreach (var payment in payments)
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(payment.BillId);
                var floor = bill.Customer.Floor;
                
                if (!result.ContainsKey(floor))
                    result[floor] = 0;
                
                result[floor] += payment.AmountPaid;
            }
            return result;
        }

        private async Task<List<DetailedTransaction>> GetDetailedTransactionsAsync(
            List<PaymentRecord> payments)
        {
            var transactions = new List<DetailedTransaction>();
            foreach (var payment in payments)
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(payment.BillId);
                transactions.Add(new DetailedTransaction
                {
                    TransactionId = payment.Id,
                    CustomerName = bill.Customer.Name,
                    ShopNo = bill.Customer.ShopNo,
                    Amount = payment.AmountPaid,
                    PaymentMethod = payment.PaymentMethod,
                    TransactionReference = payment.TransactionReference,
                    ReceivedBy = payment.ReceivedBy,
                    Timestamp = payment.PaymentDate
                });
            }
            return transactions;
        }

        private async Task<Dictionary<string, decimal>> GetBillingsByFloorAsync(
            List<MonthlyBill> bills)
        {
            return bills
                .GroupBy(b => b.Customer.Floor)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(b => b.TotalAmount)
                );
        }

        private async Task<decimal> CalculateOpeningBalanceAsync(
            int customerId,
            DateTime fromDate)
        {
            var previousBills = await _unitOfWork.MonthlyBills.FindAsync(b =>
                b.CustomerId == customerId && b.BillingMonth < fromDate);

            return previousBills.Sum(b => 
                b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
        }

        private async Task<decimal> CalculateClosingBalanceAsync(
            int customerId,
            DateTime toDate)
        {
            var bills = await _unitOfWork.MonthlyBills.FindAsync(b =>
                b.CustomerId == customerId && b.BillingMonth <= toDate);

            return bills.Sum(b => 
                b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
        }

        private async Task<List<StatementTransaction>> GetStatementTransactionsAsync(
            List<MonthlyBill> bills)
        {
            var transactions = new List<StatementTransaction>();

            foreach (var bill in bills)
            {
                // Add bill
                transactions.Add(new StatementTransaction
                {
                    Date = bill.BillingMonth,
                    Description = $"Bill for {bill.BillingMonth:MMM yyyy}",
                    Debit = bill.TotalAmount,
                    Credit = 0,
                    Balance = bill.TotalAmount
                });

                // Add payments
                foreach (var payment in bill.Payments.OrderBy(p => p.PaymentDate))
                {
                    transactions.Add(new StatementTransaction
                    {
                        Date = payment.PaymentDate,
                        Description = $"Payment ({payment.PaymentMethod})" +
                            (!string.IsNullOrEmpty(payment.TransactionReference) 
                                ? $" - Ref: {payment.TransactionReference}" 
                                : ""),
                        Debit = 0,
                        Credit = payment.AmountPaid,
                        Balance = bill.TotalAmount - 
                            bill.Payments
                                .Where(p => p.PaymentDate <= payment.PaymentDate)
                                .Sum(p => p.AmountPaid)
                    });
                }
            }

            return transactions.OrderBy(t => t.Date).ToList();
        }

        private async Task<Dictionary<string, decimal>> GetOutstandingByFloorAsync(
            List<MonthlyBill> bills)
        {
            return bills
                .GroupBy(b => b.Customer.Floor)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(b => b.TotalAmount - b.Payments.Sum(p => p.AmountPaid))
                );
        }

        private Dictionary<string, decimal> GetRevenueByMonth(List<MonthlyBill> bills)
        {
            return bills
                .SelectMany(b => b.Payments)
                .GroupBy(p => p.PaymentDate.ToString("MMM yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.AmountPaid)
                );
        }

        private Dictionary<string, decimal> GetRevenueByCategory(List<MonthlyBill> bills)
        {
            var categories = new Dictionary<string, decimal>
            {
                { "Electricity", bills.Sum(b => (b.PresentReading - b.PreviousReading) * GetCurrentRate("Electricity")) },
                { "AC", bills.Sum(b => (b.ACPresentReading - b.ACPreviousReading) * GetCurrentRate("AC")) },
                { "Blower Fan", bills.Sum(b => b.BlowerFanCharge) },
                { "Generator", bills.Sum(b => b.GeneratorCharge) },
                { "Service", bills.Sum(b => b.ServiceCharge) }
            };

            return categories;
        }

        private decimal GetCurrentRate(string rateType)
        {
            // TODO: Implement proper rate retrieval
            return rateType == "Electricity" ? 12.0m : 10.0m;
        }

        private decimal CalculateCollectionEfficiency(List<MonthlyBill> bills)
        {
            var totalBilled = bills.Sum(b => b.TotalAmount);
            if (totalBilled == 0) return 0;

            var totalCollected = bills.Sum(b => b.Payments.Sum(p => p.AmountPaid));
            return (totalCollected / totalBilled) * 100;
        }
    }
} 