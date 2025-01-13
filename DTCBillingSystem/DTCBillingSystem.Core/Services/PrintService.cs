using System;
using System.IO;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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

        public async Task<string> PrintBillAsync(int billId, PrintOptions options)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new ArgumentException($"Bill with ID {billId} not found.");
                }

                var reportData = await GenerateBillReportDataAsync(bill);
                var outputPath = Path.Combine(_reportPath, $"Bill_{billId}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                await GeneratePdfReportAsync(reportData, outputPath);

                if (!options.Preview)
                {
                    await PrintPdfAsync(outputPath);
                }

                await _auditService.LogActionAsync(
                    "Print Bill",
                    bill.CustomerId,
                    AuditAction.Print,
                    billId,
                    "MonthlyBill",
                    $"Bill {bill.BillNo} printed successfully",
                    outputPath,
                    null);

                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing bill {BillId}", billId);
                throw;
            }
        }

        public async Task<string> PrintBillsAsync(IEnumerable<int> billIds, PrintOptions options)
        {
            try
            {
                var outputPath = Path.Combine(_reportPath, $"Bills_Batch_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                var document = Document.Create(container =>
                {
                    foreach (var billId in billIds)
                    {
                        container.Page(page =>
                        {
                            var bill = _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId).Result;
                            if (bill != null)
                            {
                                var reportData = GenerateBillReportDataAsync(bill).Result;
                                ConfigureBillPage(page, reportData);
                            }
                        });
                    }
                });

                document.GeneratePdf(outputPath);

                if (!options.Preview)
                {
                    await PrintPdfAsync(outputPath);
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error printing bills batch");
                throw;
            }
        }

        private void ConfigureBillPage(IContainer container, BillReportData billData)
        {
            container.Column(column =>
            {
                // Header
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(billData.CompanyName).Bold().FontSize(20);
                        c.Item().Text(billData.CompanyAddress);
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Bill No: {billData.BillNo}");
                        c.Item().Text($"Date: {billData.BillDate:d}");
                        c.Item().Text($"Due Date: {billData.DueDate:d}");
                    });
                });

                // Customer Info
                column.Item().PaddingVertical(10).Column(c =>
                {
                    c.Item().Text("Customer Information").Bold();
                    c.Item().Text($"Name: {billData.Customer.Name}");
                    c.Item().Text($"Shop No: {billData.Customer.ShopNo}");
                    c.Item().Text($"Floor: {billData.Customer.Floor}");
                    c.Item().Text($"Phone: {billData.Customer.PhoneNumber}");
                    c.Item().Text($"Email: {billData.Customer.Email}");
                });

                // Bill Items
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Description").Bold();
                        header.Cell().Text("Previous").Bold();
                        header.Cell().Text("Current").Bold();
                        header.Cell().Text("Units").Bold();
                        header.Cell().Text("Rate").Bold();
                        header.Cell().Text("Amount").Bold();
                    });

                    foreach (var item in billData.LineItems)
                    {
                        table.Cell().Text(item.Description);
                        table.Cell().Text($"{item.PreviousReading:N2}");
                        table.Cell().Text($"{item.CurrentReading:N2}");
                        table.Cell().Text($"{item.Units:N2}");
                        table.Cell().Text($"{item.Rate:C2}");
                        table.Cell().Text($"{item.Amount:C2}");
                    }
                });

                // Summary
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem();
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Subtotal: {billData.SubTotal:C2}");
                        c.Item().Text($"Tax: {billData.Tax:C2}");
                        c.Item().Text($"Total: {billData.Total:C2}").Bold();
                    });
                });

                // Notes
                if (!string.IsNullOrEmpty(billData.Notes))
                {
                    column.Item().PaddingTop(10).Text(billData.Notes);
                }

                // Payment Instructions
                if (!string.IsNullOrEmpty(billData.PaymentInstructions))
                {
                    column.Item().PaddingTop(10).Text(billData.PaymentInstructions);
                }
            });
        }

        private async Task<BillReportData> GenerateBillReportDataAsync(MonthlyBill bill)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId);
            var lineItems = new List<BillLineItem>();

            // Add electricity charges
            if (bill.ElectricityCharges > 0)
            {
                lineItems.Add(new BillLineItem
                {
                    Description = "Electricity",
                    PreviousReading = bill.PreviousReading,
                    CurrentReading = bill.CurrentReading,
                    Units = bill.UnitsConsumed,
                    Rate = bill.ElectricityRate,
                    Amount = bill.ElectricityCharges
                });
            }

            // Add AC charges if applicable
            if (bill.ACCharges > 0)
            {
                lineItems.Add(new BillLineItem
                {
                    Description = "Air Conditioning",
                    Amount = bill.ACCharges
                });
            }

            // Add other charges if any
            if (bill.OtherCharges > 0)
            {
                lineItems.Add(new BillLineItem
                {
                    Description = "Other Charges",
                    Amount = bill.OtherCharges
                });
            }

            return new BillReportData
            {
                CompanyName = _configuration["AppSettings:CompanyName"] ?? "DTC Billing System",
                CompanyAddress = _configuration["AppSettings:CompanyAddress"] ?? "123 Main Street",
                BillNo = bill.BillNo,
                BillDate = bill.BillDate,
                DueDate = bill.DueDate,
                Customer = new CustomerInfo
                {
                    Name = customer.Name,
                    ShopNo = customer.ShopNo,
                    Floor = customer.Floor,
                    PhoneNumber = customer.PhoneNumber,
                    Email = customer.Email
                },
                LineItems = lineItems,
                SubTotal = bill.TotalAmount - bill.Tax,
                Tax = bill.Tax,
                Total = bill.TotalAmount,
                Notes = bill.Notes,
                PaymentInstructions = _configuration["AppSettings:PaymentInstructions"]
            };
        }

        private async Task PrintPdfAsync(string filePath)
        {
            try
            {
                var printerSettings = GetDefaultPrinterSettings();
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.Verb = "print";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.Start();
                await process.WaitForExitAsync();
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
            settings.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
            return settings;
        }

        public Task<string> PrintCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate, PrintOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<string> PrintDailyCollectionReportAsync(DateTime date, PrintOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<string> PrintMonthlyBillingReportAsync(DateTime month, PrintOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<string> PrintReceiptAsync(int paymentId, PrintOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<string> PreviewDocumentAsync(object document, PrintOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAvailablePrintersAsync()
        {
            return Task.FromResult<IEnumerable<string>>(PrinterSettings.InstalledPrinters.Cast<string>());
        }

        public Task<string> GetPrintJobStatusAsync(string jobId)
        {
            throw new NotImplementedException();
        }

        public Task CancelPrintJobAsync(string jobId)
        {
            throw new NotImplementedException();
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
}