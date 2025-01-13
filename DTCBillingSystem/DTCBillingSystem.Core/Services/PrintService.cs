using System.IO;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PrintService> _logger;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportService _reportService;
        private readonly string _reportPath;

        public PrintService(
            IConfiguration configuration,
            ILogger<PrintService> logger,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            IReportService reportService)
        {
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _reportService = reportService;
            
            _reportPath = _configuration["AppSettings:ReportPath"] ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                    "DTCBillingSystem", "Reports");

            if (!Directory.Exists(_reportPath))
            {
                Directory.CreateDirectory(_reportPath);
            }
        }

        public async Task<string> PrintBillAsync(int billId, bool preview = false)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new KeyNotFoundException($"Bill with ID {billId} not found");
                }

                var reportData = await GenerateBillReportDataAsync(bill);
                var fileName = $"Bill_{bill.Customer.ShopNo}_{bill.BillingMonth:yyyyMM}.pdf";
                var filePath = Path.Combine(_reportPath, fileName);

                await GeneratePdfReportAsync(reportData, filePath);

                if (!preview)
                {
                    await PrintPdfAsync(filePath);
                }

                await _auditService.LogActionAsync(
                    "Print",
                    billId,
                    AuditAction.Printed,
                    null,
                    $"Printed bill for {bill.Customer.Name} - {bill.BillingMonth:MMM yyyy}"
                );

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing bill {BillId}", billId);
                throw;
            }
        }

        public async Task<string> PrintCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate, bool preview = false)
        {
            try
            {
                var customer = await _unitOfWork.CustomersExt.GetCustomerWithBillsAsync(customerId);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                var statement = await _reportService.GenerateCustomerStatementAsync(customerId, startDate, endDate);
                var fileName = $"Statement_{customer.ShopNo}_{startDate:yyyyMM}_{endDate:yyyyMM}.pdf";
                var filePath = Path.Combine(_reportPath, fileName);

                await GeneratePdfReportAsync(statement, filePath);

                if (!preview)
                {
                    await PrintPdfAsync(filePath);
                }

                await _auditService.LogActionAsync(
                    "Print",
                    customerId,
                    AuditAction.Printed,
                    null,
                    $"Printed statement for {customer.Name} from {startDate:MMM yyyy} to {endDate:MMM yyyy}"
                );

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing statement for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<string> PrintDailyCollectionReportAsync(DateTime date, bool preview = false)
        {
            try
            {
                var report = await _reportService.GenerateDailyCollectionReportAsync(date);
                var fileName = $"DailyCollection_{date:yyyyMMdd}.pdf";
                var filePath = Path.Combine(_reportPath, fileName);

                await GeneratePdfReportAsync(report, filePath);

                if (!preview)
                {
                    await PrintPdfAsync(filePath);
                }

                await _auditService.LogActionAsync(
                    "Print",
                    0,
                    AuditAction.Printed,
                    null,
                    $"Printed daily collection report for {date:d}"
                );

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing daily collection report for {Date}", date);
                throw;
            }
        }

        public async Task<string> PrintMonthlyBillingReportAsync(DateTime month, bool preview = false)
        {
            try
            {
                var report = await _reportService.GenerateMonthlyBillingReportAsync(month);
                var fileName = $"MonthlyBilling_{month:yyyyMM}.pdf";
                var filePath = Path.Combine(_reportPath, fileName);

                await GeneratePdfReportAsync(report, filePath);

                if (!preview)
                {
                    await PrintPdfAsync(filePath);
                }

                await _auditService.LogActionAsync(
                    "Print",
                    0,
                    AuditAction.Printed,
                    null,
                    $"Printed monthly billing report for {month:MMM yyyy}"
                );

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing monthly billing report for {Month}", month);
                throw;
            }
        }

        public async Task<string> PrintOutstandingPaymentsReportAsync(bool preview = false)
        {
            try
            {
                var report = await _reportService.GenerateOutstandingPaymentsReportAsync();
                var fileName = $"OutstandingPayments_{DateTime.UtcNow:yyyyMMdd}.pdf";
                var filePath = Path.Combine(_reportPath, fileName);

                await GeneratePdfReportAsync(report, filePath);

                if (!preview)
                {
                    await PrintPdfAsync(filePath);
                }

                await _auditService.LogActionAsync(
                    "Print",
                    0,
                    AuditAction.Printed,
                    null,
                    "Printed outstanding payments report"
                );

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing outstanding payments report");
                throw;
            }
        }

        private async Task<BillReportData> GenerateBillReportDataAsync(MonthlyBill bill)
        {
            var companyName = _configuration["AppSettings:CompanyName"];
            var companyAddress = _configuration["AppSettings:Address"];
            var vatPercentage = decimal.Parse(_configuration["AppSettings:VatPercentage"]);

            // Get active billing rates
            var electricityRate = await _unitOfWork.BillingRates
                .FindAsync(r => r.RateType == "Electricity" && r.IsActive)
                .FirstOrDefaultAsync();
            var acRate = await _unitOfWork.BillingRates
                .FindAsync(r => r.RateType == "AC" && r.IsActive)
                .FirstOrDefaultAsync();

            if (electricityRate == null || acRate == null)
            {
                throw new InvalidOperationException("Active billing rates not found");
            }

            // Calculate amounts
            var electricityUnits = bill.PresentReading - bill.PreviousReading;
            var acUnits = bill.ACPresentReading - bill.ACPreviousReading;
            var electricityAmount = electricityUnits * electricityRate.Rate;
            var acAmount = acUnits * acRate.Rate;

            var billData = new BillReportData
            {
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                BillNo = $"BILL-{bill.Id:D6}",
                BillDate = bill.CreatedAt,
                DueDate = bill.DueDate,
                Customer = new CustomerInfo
                {
                    Name = bill.Customer.Name,
                    ShopNo = bill.Customer.ShopNo,
                    Floor = bill.Customer.Floor,
                    PhoneNumber = bill.Customer.PhoneNumber,
                    Email = bill.Customer.Email
                },
                LineItems = new List<BillLineItem>
                {
                    new BillLineItem
                    {
                        Description = "Electricity Charges",
                        PreviousReading = bill.PreviousReading,
                        CurrentReading = bill.PresentReading,
                        Units = electricityUnits,
                        Rate = electricityRate.Rate,
                        Amount = electricityAmount
                    },
                    new BillLineItem
                    {
                        Description = "AC Charges",
                        PreviousReading = bill.ACPreviousReading,
                        CurrentReading = bill.ACPresentReading,
                        Units = acUnits,
                        Rate = acRate.Rate,
                        Amount = acAmount
                    }
                },
                SubTotal = bill.TotalAmount / (1 + vatPercentage / 100),
                Tax = bill.TotalAmount - (bill.TotalAmount / (1 + vatPercentage / 100)),
                Total = bill.TotalAmount,
                Notes = bill.Notes,
                PaymentInstructions = "Please pay before the due date to avoid late payment charges."
            };

            // Add other charges
            if (bill.BlowerFanCharge > 0)
            {
                billData.LineItems.Add(new BillLineItem
                {
                    Description = "Blower Fan Charges",
                    Amount = bill.BlowerFanCharge
                });
            }

            if (bill.GeneratorCharge > 0)
            {
                billData.LineItems.Add(new BillLineItem
                {
                    Description = "Generator Charges",
                    Amount = bill.GeneratorCharge
                });
            }

            if (bill.ServiceCharge > 0)
            {
                billData.LineItems.Add(new BillLineItem
                {
                    Description = "Service Charges",
                    Amount = bill.ServiceCharge
                });
            }

            if (bill.AdditionalCharges > 0)
            {
                billData.LineItems.Add(new BillLineItem
                {
                    Description = "Additional Charges",
                    Amount = bill.AdditionalCharges
                });
            }

            return billData;
        }

        private async Task GeneratePdfReportAsync(object reportData, string outputPath)
        {
            using (var report = new XtraReport())
            {
                // Configure report settings
                report.Landscape = false;
                report.PaperKind = System.Drawing.Printing.PaperKind.A4;
                report.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

                // Set report data source
                report.DataSource = reportData;

                // Add report bands and controls based on report type
                if (reportData is BillReportData billData)
                {
                    ConfigureBillReport(report, billData);
                }
                else
                {
                    ConfigureGenericReport(report, reportData);
                }

                // Export to PDF
                report.ExportToPdf(outputPath);
            }
        }

        private void ConfigureBillReport(XtraReport report, BillReportData billData)
        {
            // Add watermark
            report.Watermark.Text = "DTC Billing System";
            report.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            report.Watermark.Font = new System.Drawing.Font("Arial", 36);
            report.Watermark.ForeColor = System.Drawing.Color.FromArgb(30, 128, 128, 128);
            report.Watermark.TextTransparency = 150;

            // Add report header
            var reportHeader = new ReportHeaderBand
            {
                HeightF = 250
            };

            // Add company details
            reportHeader.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = billData.CompanyName,
                    Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 0),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = billData.CompanyAddress,
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 30),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = "ELECTRICITY BILL",
                    Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 60),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                // Bill details
                new XRLabel
                {
                    Text = $"Bill No: {billData.BillNo}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 100),
                    WidthF = 200
                },
                new XRLabel
                {
                    Text = $"Date: {billData.BillDate:d}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(report.PageWidth - report.Margins.Left - report.Margins.Right - 200, 100),
                    WidthF = 200,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
                },
                // Customer details
                new XRLabel
                {
                    Text = $"Customer Name: {billData.Customer.Name}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 130),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right
                },
                new XRLabel
                {
                    Text = $"Shop No: {billData.Customer.ShopNo}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 150),
                    WidthF = 200
                },
                new XRLabel
                {
                    Text = $"Floor: {billData.Customer.Floor}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(200, 150),
                    WidthF = 200
                },
                new XRLabel
                {
                    Text = $"Due Date: {billData.DueDate:d}",
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.Red,
                    LocationF = new System.Drawing.PointF(report.PageWidth - report.Margins.Left - report.Margins.Right - 200, 150),
                    WidthF = 200,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
                }
            });

            report.Bands.Add(reportHeader);

            // Add barcode for bill tracking
            reportHeader.Controls.Add(new XRBarCode
            {
                Text = billData.BillNo,
                LocationF = new System.Drawing.PointF(report.PageWidth - report.Margins.Right - 200, 10),
                WidthF = 200,
                HeightF = 50,
                Symbology = new Code128Generator(),
                ShowText = true,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.BottomCenter
            });

            // Add QR code with payment details
            var paymentInfo = $"Bill:{billData.BillNo}|Amount:{billData.Total:N2}|Due:{billData.DueDate:yyyy-MM-dd}";
            reportHeader.Controls.Add(new XRBarCode
            {
                Text = paymentInfo,
                LocationF = new System.Drawing.PointF(report.PageWidth - report.Margins.Right - 100, 150),
                WidthF = 100,
                HeightF = 100,
                Symbology = new QRCodeGenerator(),
                ShowText = false
            });

            // Add table header
            var tableHeader = new PageHeaderBand
            {
                HeightF = 30
            };

            tableHeader.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Description", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 200 },
                new XRLabel { Text = "Previous", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(200, 0), WidthF = 100 },
                new XRLabel { Text = "Current", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { Text = "Units", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { Text = "Rate", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(500, 0), WidthF = 100 },
                new XRLabel { Text = "Amount", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(600, 0), WidthF = 100 }
            });

            report.Bands.Add(tableHeader);

            // Add detail band for line items
            var detailBand = new DetailBand
            {
                HeightF = 25
            };

            detailBand.Controls.AddRange(new XRControl[] {
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Description") }, LocationF = new System.Drawing.PointF(0, 0), WidthF = 200 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "PreviousReading", "{0:N2}") }, LocationF = new System.Drawing.PointF(200, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "CurrentReading", "{0:N2}") }, LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Units", "{0:N2}") }, LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Rate", "{0:N2}") }, LocationF = new System.Drawing.PointF(500, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Amount", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 0), WidthF = 100 }
            });

            report.Bands.Add(detailBand);

            // Add report footer
            var reportFooter = new ReportFooterBand
            {
                HeightF = 200
            };

            // Add totals
            reportFooter.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Sub Total:", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "SubTotal", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { Text = "VAT:", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 25), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Tax", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 25), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { Text = "Total:", Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 50), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Total", "{0:N2}") }, Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(600, 50), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                // Payment instructions
                new XRLabel { Text = "Payment Instructions", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 100), WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right },
                new XRLabel { Text = billData.PaymentInstructions, Font = new System.Drawing.Font("Arial", 10), LocationF = new System.Drawing.PointF(0, 120), WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right },
                // Notes
                new XRLabel { Text = "Notes:", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 150), WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right },
                new XRLabel { Text = billData.Notes, Font = new System.Drawing.Font("Arial", 10), LocationF = new System.Drawing.PointF(0, 170), WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right }
            });

            report.Bands.Add(reportFooter);

            // Add signature section in report footer
            reportFooter.Controls.AddRange(new XRControl[] {
                new XRLine
                {
                    LocationF = new System.Drawing.PointF(50, 150),
                    WidthF = 200,
                    LineStyle = LineStyle.Dash
                },
                new XRLabel
                {
                    Text = "Authorized Signature",
                    Font = new System.Drawing.Font("Arial", 8),
                    LocationF = new System.Drawing.PointF(50, 155),
                    WidthF = 200,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                }
            });
        }

        private void ConfigureGenericReport(XtraReport report, object reportData)
        {
            switch (reportData)
            {
                case DailyCollectionReport dailyReport:
                    ConfigureDailyCollectionReport(report, dailyReport);
                    break;
                case MonthlyBillingReport monthlyReport:
                    ConfigureMonthlyBillingReport(report, monthlyReport);
                    break;
                case OutstandingPaymentsReport outstandingReport:
                    ConfigureOutstandingPaymentsReport(report, outstandingReport);
                    break;
                default:
                    throw new ArgumentException("Unsupported report type", nameof(reportData));
            }
        }

        private void ConfigureDailyCollectionReport(XtraReport report, DailyCollectionReport reportData)
        {
            // Add watermark
            report.Watermark.Text = "DTC Collections";
            report.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            report.Watermark.Font = new System.Drawing.Font("Arial", 36);
            report.Watermark.ForeColor = System.Drawing.Color.FromArgb(30, 128, 128, 128);
            report.Watermark.TextTransparency = 150;

            report.Landscape = true;

            // Add report header
            var reportHeader = new ReportHeaderBand { HeightF = 150 };
            reportHeader.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = _configuration["AppSettings:CompanyName"],
                    Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 0),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = "Daily Collection Report",
                    Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 40),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = $"Date: {reportData.Date:d}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 70),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                }
            });
            report.Bands.Add(reportHeader);

            // Add summary section
            var summaryBand = new ReportHeaderBand { HeightF = 120 };
            summaryBand.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Summary", Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 200 },
                new XRLabel { Text = "Total Collections:", LocationF = new System.Drawing.PointF(0, 30), WidthF = 150 },
                new XRLabel { Text = reportData.TotalCollections.ToString("C2"), LocationF = new System.Drawing.PointF(150, 30), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { Text = "Total Late Charges:", LocationF = new System.Drawing.PointF(0, 50), WidthF = 150 },
                new XRLabel { Text = reportData.TotalLateCharges.ToString("C2"), LocationF = new System.Drawing.PointF(150, 50), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight }
            });
            report.Bands.Add(summaryBand);

            // Add chart for payment method distribution
            var chartBand = new ReportHeaderBand { HeightF = 250 };
            var chart = new XRChart
            {
                LocationF = new System.Drawing.PointF(0, 0),
                WidthF = 300,
                HeightF = 250
            };

            var series = new Series("Payment Methods", ViewType.Pie);
            foreach (var payment in reportData.CollectionsByPaymentMethod)
            {
                series.Points.Add(new SeriesPoint(payment.PaymentMethod.ToString(), payment.Amount));
            }
            chart.Series.Add(series);
            chartBand.Controls.Add(chart);
            report.Bands.Add(chartBand);

            // Add table header
            var tableHeader = new PageHeaderBand { HeightF = 30 };
            tableHeader.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Receipt No", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { Text = "Shop No", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(100, 0), WidthF = 100 },
                new XRLabel { Text = "Customer", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(200, 0), WidthF = 200 },
                new XRLabel { Text = "Bill Month", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { Text = "Amount", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(500, 0), WidthF = 100 },
                new XRLabel { Text = "Late Charges", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(600, 0), WidthF = 100 },
                new XRLabel { Text = "Payment Method", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(700, 0), WidthF = 100 }
            });
            report.Bands.Add(tableHeader);

            // Add detail band
            var detailBand = new DetailBand { HeightF = 25 };
            detailBand.Controls.AddRange(new XRControl[] {
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ReceiptNo") }, LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ShopNo") }, LocationF = new System.Drawing.PointF(100, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "CustomerName") }, LocationF = new System.Drawing.PointF(200, 0), WidthF = 200 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "BillMonth", "{0:MMM yyyy}") }, LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Amount", "{0:N2}") }, LocationF = new System.Drawing.PointF(500, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "LateCharges", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "PaymentMethod") }, LocationF = new System.Drawing.PointF(700, 0), WidthF = 100 }
            });
            report.Bands.Add(detailBand);

            // Add group band for floor-wise grouping
            var groupHeader = new GroupHeaderBand { HeightF = 30 };
            var groupField = new GroupField("Floor");
            groupHeader.GroupFields.Add(groupField);
            groupHeader.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = "Floor: [Floor]",
                    Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 5),
                    WidthF = 200
                }
            });
            report.Bands.Add(groupHeader);

            // Add group footer for subtotals
            var groupFooter = new GroupFooterBand { HeightF = 30 };
            groupFooter.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = "Floor Total:",
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(400, 5),
                    WidthF = 100
                },
                new XRLabel
                {
                    DataBindings = { new XRBinding("Text", null, "Amount", "Sum={0:N2}") },
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(500, 5),
                    WidthF = 100,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
                }
            });
            report.Bands.Add(groupFooter);

            // Add page footer with numbers
            var pageFooter = new PageFooterBand { HeightF = 30 };
            pageFooter.Controls.AddRange(new XRControl[] {
                new XRPageInfo
                {
                    LocationF = new System.Drawing.PointF(0, 5),
                    PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime,
                    TextFormatString = "Generated on {0:dd MMM yyyy HH:mm}",
                    WidthF = 200
                },
                new XRPageInfo
                {
                    LocationF = new System.Drawing.PointF(report.PageWidth - report.Margins.Right - 200, 5),
                    PageInfo = DevExpress.XtraPrinting.PageInfo.NumberOfTotal,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight,
                    TextFormatString = "Page {0} of {1}",
                    WidthF = 200
                }
            });
            report.Bands.Add(pageFooter);

            // Sort the data
            report.DataSource = reportData.Collections.OrderBy(c => c.ShopNo).ToList();
        }

        private void ConfigureMonthlyBillingReport(XtraReport report, MonthlyBillingReport reportData)
        {
            // Add watermark
            report.Watermark.Text = "DTC Monthly Billing";
            report.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            report.Watermark.Font = new System.Drawing.Font("Arial", 36);
            report.Watermark.ForeColor = System.Drawing.Color.FromArgb(30, 128, 128, 128);
            report.Watermark.TextTransparency = 150;

            report.Landscape = true;

            // Add report header
            var reportHeader = new ReportHeaderBand { HeightF = 150 };
            reportHeader.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = _configuration["AppSettings:CompanyName"],
                    Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 0),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = "Monthly Billing Report",
                    Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 40),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = $"Month: {reportData.Month:MMMM yyyy}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 70),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                }
            });
            report.Bands.Add(reportHeader);

            // Add summary section
            var summaryBand = new ReportHeaderBand { HeightF = 120 };
            summaryBand.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Summary", Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 200 },
                new XRLabel { Text = "Total Billed:", LocationF = new System.Drawing.PointF(0, 30), WidthF = 150 },
                new XRLabel { Text = reportData.TotalBilled.ToString("C2"), LocationF = new System.Drawing.PointF(150, 30), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { Text = "Total Paid:", LocationF = new System.Drawing.PointF(0, 50), WidthF = 150 },
                new XRLabel { Text = reportData.TotalPaid.ToString("C2"), LocationF = new System.Drawing.PointF(150, 50), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { Text = "Total Outstanding:", LocationF = new System.Drawing.PointF(0, 70), WidthF = 150 },
                new XRLabel { Text = reportData.TotalOutstanding.ToString("C2"), LocationF = new System.Drawing.PointF(150, 70), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight }
            });
            report.Bands.Add(summaryBand);

            // Add chart for floor-wise billing distribution
            var chartBand = new ReportHeaderBand { HeightF = 250 };
            var chart = new XRChart
            {
                LocationF = new System.Drawing.PointF(0, 0),
                WidthF = 400,
                HeightF = 250
            };

            var series = new Series("Floor-wise Billing", ViewType.Bar);
            foreach (var floor in reportData.BillingsByFloor)
            {
                series.Points.Add(new SeriesPoint(floor.Floor, floor.TotalAmount));
            }
            chart.Series.Add(series);
            chartBand.Controls.Add(chart);
            report.Bands.Add(chartBand);

            // Add table header
            var tableHeader = new PageHeaderBand { HeightF = 30 };
            tableHeader.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Shop No", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { Text = "Customer", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(100, 0), WidthF = 200 },
                new XRLabel { Text = "Floor", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { Text = "Electricity", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { Text = "AC", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(500, 0), WidthF = 100 },
                new XRLabel { Text = "Other", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(600, 0), WidthF = 100 },
                new XRLabel { Text = "Total", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(700, 0), WidthF = 100 },
                new XRLabel { Text = "Status", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(800, 0), WidthF = 100 }
            });
            report.Bands.Add(tableHeader);

            // Add detail band
            var detailBand = new DetailBand { HeightF = 25 };
            detailBand.Controls.AddRange(new XRControl[] {
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ShopNo") }, LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "CustomerName") }, LocationF = new System.Drawing.PointF(100, 0), WidthF = 200 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Floor") }, LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ElectricityCharges", "{0:N2}") }, LocationF = new System.Drawing.PointF(400, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ACCharges", "{0:N2}") }, LocationF = new System.Drawing.PointF(500, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "OtherCharges", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "TotalAmount", "{0:N2}") }, LocationF = new System.Drawing.PointF(700, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Status") }, LocationF = new System.Drawing.PointF(800, 0), WidthF = 100 }
            });
            report.Bands.Add(detailBand);

            // Add conditional formatting for status
            var statusLabel = detailBand.Controls["Status"] as XRLabel;
            statusLabel.BeforePrint += (s, e) =>
            {
                var label = s as XRLabel;
                var status = (BillStatus)label.GetCurrentColumnValue("Status");
                switch (status)
                {
                    case BillStatus.Paid:
                        label.ForeColor = System.Drawing.Color.Green;
                        break;
                    case BillStatus.PartiallyPaid:
                        label.ForeColor = System.Drawing.Color.Orange;
                        break;
                    case BillStatus.Pending:
                        label.ForeColor = System.Drawing.Color.Red;
                        break;
                }
            };

            // Add group footer for floor totals
            var groupFooter = new GroupFooterBand { HeightF = 30 };
            groupFooter.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = "Floor Total:",
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(600, 5),
                    WidthF = 100
                },
                new XRLabel
                {
                    DataBindings = { new XRBinding("Text", null, "TotalAmount", "Sum={0:N2}") },
                    Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(700, 5),
                    WidthF = 100,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight
                }
            });
            report.Bands.Add(groupFooter);

            // Sort the data
            report.DataSource = reportData.Billings.OrderBy(b => b.Floor).ThenBy(b => b.ShopNo).ToList();
        }

        private void ConfigureOutstandingPaymentsReport(XtraReport report, OutstandingPaymentsReport reportData)
        {
            // Add watermark
            report.Watermark.Text = "DTC Outstanding";
            report.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            report.Watermark.Font = new System.Drawing.Font("Arial", 36);
            report.Watermark.ForeColor = System.Drawing.Color.FromArgb(30, 128, 128, 128);
            report.Watermark.TextTransparency = 150;

            report.Landscape = true;

            // Add report header
            var reportHeader = new ReportHeaderBand { HeightF = 150 };
            reportHeader.Controls.AddRange(new XRControl[] {
                new XRLabel
                {
                    Text = _configuration["AppSettings:CompanyName"],
                    Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 0),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = "Outstanding Payments Report",
                    Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                    LocationF = new System.Drawing.PointF(0, 40),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                },
                new XRLabel
                {
                    Text = $"As of: {reportData.AsOfDate:d}",
                    Font = new System.Drawing.Font("Arial", 10),
                    LocationF = new System.Drawing.PointF(0, 70),
                    WidthF = report.PageWidth - report.Margins.Left - report.Margins.Right,
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
                }
            });
            report.Bands.Add(reportHeader);

            // Add summary section
            var summaryBand = new ReportHeaderBand { HeightF = 80 };
            summaryBand.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Summary", Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 200 },
                new XRLabel { Text = "Total Outstanding:", LocationF = new System.Drawing.PointF(0, 30), WidthF = 150 },
                new XRLabel { Text = reportData.TotalOutstanding.ToString("C2"), LocationF = new System.Drawing.PointF(150, 30), WidthF = 150, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight }
            });
            report.Bands.Add(summaryBand);

            // Add chart for aging analysis
            var chartBand = new ReportHeaderBand { HeightF = 250 };
            var chart = new XRChart
            {
                LocationF = new System.Drawing.PointF(0, 0),
                WidthF = 400,
                HeightF = 250
            };

            var agingGroups = reportData.OutstandingBills
                .GroupBy(b => b.DaysOverdue switch
                {
                    <= 30 => "0-30 days",
                    <= 60 => "31-60 days",
                    <= 90 => "61-90 days",
                    _ => "90+ days"
                })
                .Select(g => new { Range = g.Key, Amount = g.Sum(b => b.OutstandingAmount) });

            var series = new Series("Aging Analysis", ViewType.Pie);
            foreach (var group in agingGroups)
            {
                series.Points.Add(new SeriesPoint(group.Range, group.Amount));
            }
            chart.Series.Add(series);
            chartBand.Controls.Add(chart);
            report.Bands.Add(chartBand);

            // Add table header
            var tableHeader = new PageHeaderBand { HeightF = 30 };
            tableHeader.Controls.AddRange(new XRControl[] {
                new XRLabel { Text = "Shop No", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { Text = "Customer", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(100, 0), WidthF = 200 },
                new XRLabel { Text = "Floor", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { Text = "Bill Month", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { Text = "Bill Amount", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(500, 0), WidthF = 100 },
                new XRLabel { Text = "Paid Amount", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(600, 0), WidthF = 100 },
                new XRLabel { Text = "Outstanding", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(700, 0), WidthF = 100 },
                new XRLabel { Text = "Days Overdue", Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold), LocationF = new System.Drawing.PointF(800, 0), WidthF = 100 }
            });
            report.Bands.Add(tableHeader);

            // Add detail band
            var detailBand = new DetailBand { HeightF = 25 };
            detailBand.Controls.AddRange(new XRControl[] {
                new XRLabel { DataBindings = { new XRBinding("Text", null, "ShopNo") }, LocationF = new System.Drawing.PointF(0, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "CustomerName") }, LocationF = new System.Drawing.PointF(100, 0), WidthF = 200 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "Floor") }, LocationF = new System.Drawing.PointF(300, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "BillMonth", "{0:MMM yyyy}") }, LocationF = new System.Drawing.PointF(400, 0), WidthF = 100 },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "BillAmount", "{0:N2}") }, LocationF = new System.Drawing.PointF(500, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "PaidAmount", "{0:N2}") }, LocationF = new System.Drawing.PointF(600, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "OutstandingAmount", "{0:N2}") }, LocationF = new System.Drawing.PointF(700, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight },
                new XRLabel { DataBindings = { new XRBinding("Text", null, "DaysOverdue") }, LocationF = new System.Drawing.PointF(800, 0), WidthF = 100, TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight }
            });
            report.Bands.Add(detailBand);

            // Add conditional formatting for days overdue
            var daysOverdueLabel = detailBand.Controls["DaysOverdue"] as XRLabel;
            daysOverdueLabel.BeforePrint += (s, e) =>
            {
                var label = s as XRLabel;
                var days = (int)label.GetCurrentColumnValue("DaysOverdue");
                if (days > 60)
                    label.ForeColor = System.Drawing.Color.Red;
                else if (days > 30)
                    label.ForeColor = System.Drawing.Color.Orange;
                else
                    label.ForeColor = System.Drawing.Color.Black;
            };

            // Add running totals
            var runningTotal = 0m;
            var outstandingLabel = detailBand.Controls["OutstandingAmount"] as XRLabel;
            outstandingLabel.BeforePrint += (s, e) =>
            {
                var label = s as XRLabel;
                var amount = (decimal)label.GetCurrentColumnValue("OutstandingAmount");
                runningTotal += amount;
                label.Tag = runningTotal;
            };

            // Add running total display
            detailBand.Controls.Add(new XRLabel
            {
                DataBindings = { new XRBinding("Text", null, "OutstandingAmount", "Running Total: {0:N2}") },
                LocationF = new System.Drawing.PointF(900, 0),
                WidthF = 150,
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight,
                Font = new System.Drawing.Font("Arial", 10)
            });

            // Sort the data
            report.DataSource = reportData.OutstandingBills
                .OrderByDescending(b => b.DaysOverdue)
                .ThenBy(b => b.Floor)
                .ThenBy(b => b.ShopNo)
                .ToList();
        }

        private async Task PrintPdfAsync(string filePath)
        {
            try
            {
                var settings = GetDefaultPrinterSettings();
                using (var printTool = new ReportPrintTool(new XtraReport()))
                {
                    printTool.PrintingSystem.LoadDocument(filePath);
                    if (settings.PrinterName != null)
                    {
                        printTool.PrintingSystem.PageSettings.PrinterName = settings.PrinterName;
                    }
                    await Task.Run(() => printTool.Print());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing PDF file {FilePath}", filePath);
                throw;
            }
        }

        private PrinterSettings GetDefaultPrinterSettings()
        {
            var settings = new PrinterSettings();
            // Configure default printer settings
            settings.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);
            return settings;
        }
    }

    public class BillReportData
    {
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public CustomerInfo Customer { get; set; }
        public List<BillLineItem> LineItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }
        public string PaymentInstructions { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; }
        public string ShopNo { get; set; }
        public string Floor { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class BillLineItem
    {
        public string Description { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Units { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

    public class DailyCollectionReport
    {
        public DateTime Date { get; set; }
        public decimal TotalCollections { get; set; }
        public decimal TotalLateCharges { get; set; }
        public List<PaymentMethodSummary> CollectionsByPaymentMethod { get; set; }
        public List<CollectionDetail> Collections { get; set; }
    }

    public class PaymentMethodSummary
    {
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
    }

    public class CollectionDetail
    {
        public string ReceiptNo { get; set; }
        public string ShopNo { get; set; }
        public string CustomerName { get; set; }
        public DateTime BillMonth { get; set; }
        public decimal Amount { get; set; }
        public decimal LateCharges { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class MonthlyBillingReport
    {
        public DateTime Month { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public List<FloorSummary> BillingsByFloor { get; set; }
        public List<BillingDetail> Billings { get; set; }
    }

    public class FloorSummary
    {
        public string Floor { get; set; }
        public int TotalShops { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class BillingDetail
    {
        public string ShopNo { get; set; }
        public string CustomerName { get; set; }
        public string Floor { get; set; }
        public decimal ElectricityCharges { get; set; }
        public decimal ACCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public BillStatus Status { get; set; }
    }

    public class OutstandingPaymentsReport
    {
        public DateTime AsOfDate { get; set; }
        public decimal TotalOutstanding { get; set; }
        public List<FloorOutstanding> OutstandingByFloor { get; set; }
        public List<OutstandingDetail> OutstandingBills { get; set; }
    }

    public class FloorOutstanding
    {
        public string Floor { get; set; }
        public int TotalShops { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class OutstandingDetail
    {
        public string ShopNo { get; set; }
        public string CustomerName { get; set; }
        public string Floor { get; set; }
        public DateTime BillMonth { get; set; }
        public decimal BillAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int DaysOverdue { get; set; }
    }
} 