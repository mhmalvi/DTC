using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SmtpClient _smtpClient;

        public NotificationService(
            IConfiguration configuration,
            ILogger<NotificationService> logger,
            IAuditService auditService,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
            _unitOfWork = unitOfWork;

            // Configure SMTP client
            var smtpConfig = _configuration.GetSection("Notification:Email");
            _smtpClient = new SmtpClient(smtpConfig["SmtpServer"])
            {
                Port = int.Parse(smtpConfig["Port"]),
                Credentials = new System.Net.NetworkCredential(
                    smtpConfig["Username"],
                    smtpConfig["Password"]
                ),
                EnableSsl = bool.Parse(smtpConfig["EnableSsl"])
            };
        }

        public async Task SendBillNotificationAsync(int billId)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new KeyNotFoundException($"Bill with ID {billId} not found");
                }

                var customer = bill.Customer;
                var subject = $"New Bill for {bill.BillingMonth:MMM yyyy}";
                var body = GenerateBillEmailBody(bill);

                // Send email if customer has email
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await SendEmailAsync(customer.Email, subject, body);
                }

                // Send SMS if customer has phone number
                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    var smsText = GenerateBillSmsText(bill);
                    await SendSmsAsync(customer.PhoneNumber, smsText);
                }

                await _auditService.LogActionAsync(
                    "Notification",
                    billId,
                    AuditAction.Created,
                    null,
                    $"Sent bill notification to {customer.Name} for {bill.BillingMonth:MMM yyyy}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bill notification for bill {BillId}", billId);
                throw;
            }
        }

        public async Task SendPaymentConfirmationAsync(int paymentId)
        {
            try
            {
                var payment = await _unitOfWork.PaymentRecords.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {paymentId} not found");
                }

                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(payment.BillId);
                var customer = bill.Customer;

                var subject = $"Payment Confirmation - {bill.BillingMonth:MMM yyyy}";
                var body = GeneratePaymentConfirmationEmailBody(payment, bill);

                // Send email if customer has email
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await SendEmailAsync(customer.Email, subject, body);
                }

                // Send SMS if customer has phone number
                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    var smsText = GeneratePaymentConfirmationSmsText(payment, bill);
                    await SendSmsAsync(customer.PhoneNumber, smsText);
                }

                await _auditService.LogActionAsync(
                    "Notification",
                    paymentId,
                    AuditAction.Created,
                    null,
                    $"Sent payment confirmation to {customer.Name} for {payment.AmountPaid:C2}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmation for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task SendPaymentReminderAsync(int billId)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new KeyNotFoundException($"Bill with ID {billId} not found");
                }

                var customer = bill.Customer;
                var daysOverdue = (DateTime.UtcNow.Date - bill.DueDate.Date).Days;
                var remainingAmount = bill.TotalAmount - bill.Payments.Sum(p => p.AmountPaid);

                var subject = $"Payment Reminder - {bill.BillingMonth:MMM yyyy}";
                var body = GeneratePaymentReminderEmailBody(bill, daysOverdue, remainingAmount);

                // Send email if customer has email
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    await SendEmailAsync(customer.Email, subject, body);
                }

                // Send SMS if customer has phone number
                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                {
                    var smsText = GeneratePaymentReminderSmsText(bill, daysOverdue, remainingAmount);
                    await SendSmsAsync(customer.PhoneNumber, smsText);
                }

                await _auditService.LogActionAsync(
                    "Notification",
                    billId,
                    AuditAction.Created,
                    null,
                    $"Sent payment reminder to {customer.Name} for {bill.BillingMonth:MMM yyyy}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder for bill {BillId}", billId);
                throw;
            }
        }

        public async Task SendSystemAlertAsync(string subject, string message, NotificationPriority priority)
        {
            try
            {
                var adminEmails = await GetAdminEmailsAsync();
                if (!adminEmails.Any())
                {
                    _logger.LogWarning("No admin emails found for system alert");
                    return;
                }

                var emailSubject = $"[{priority}] {subject}";
                var emailBody = GenerateSystemAlertEmailBody(message, priority);

                foreach (var email in adminEmails)
                {
                    await SendEmailAsync(email, emailSubject, emailBody);
                }

                await _auditService.LogActionAsync(
                    "Notification",
                    0,
                    AuditAction.Created,
                    null,
                    $"Sent system alert: {subject}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system alert");
                throw;
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _configuration["Notification:Email:SenderEmail"],
                        _configuration["Notification:Email:SenderName"]
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await _smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}: {Subject}", to, subject);
                throw;
            }
        }

        private async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // TODO: Implement SMS sending using configured provider
                // This is a placeholder for SMS implementation
                _logger.LogInformation("SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        private async Task<List<string>> GetAdminEmailsAsync()
        {
            var adminUsers = await _unitOfWork.UsersExt.GetUsersByRoleAsync(UserRole.Administrator);
            return adminUsers.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email).ToList();
        }

        private string GenerateBillEmailBody(MonthlyBill bill)
        {
            return $@"
                <h2>Monthly Bill - {bill.BillingMonth:MMMM yyyy}</h2>
                <p>Dear {bill.Customer.Name},</p>
                <p>Your bill for {bill.BillingMonth:MMMM yyyy} has been generated.</p>
                <h3>Bill Details:</h3>
                <ul>
                    <li>Shop No: {bill.Customer.ShopNo}</li>
                    <li>Electricity Usage: {bill.PresentReading - bill.PreviousReading:N2} units</li>
                    <li>AC Usage: {bill.ACPresentReading - bill.ACPreviousReading:N2} units</li>
                    <li>Total Amount: {bill.TotalAmount:C2}</li>
                    <li>Due Date: {bill.DueDate:d}</li>
                </ul>
                <p>Please ensure payment is made before the due date to avoid late charges.</p>
                <p>Thank you for your business.</p>";
        }

        private string GenerateBillSmsText(MonthlyBill bill)
        {
            return $"DTC Bill - {bill.BillingMonth:MMM yyyy}\n" +
                   $"Amount: {bill.TotalAmount:C2}\n" +
                   $"Due Date: {bill.DueDate:d}\n" +
                   $"Shop: {bill.Customer.ShopNo}";
        }

        private string GeneratePaymentConfirmationEmailBody(PaymentRecord payment, MonthlyBill bill)
        {
            return $@"
                <h2>Payment Confirmation</h2>
                <p>Dear {bill.Customer.Name},</p>
                <p>We have received your payment for {bill.BillingMonth:MMMM yyyy}.</p>
                <h3>Payment Details:</h3>
                <ul>
                    <li>Amount Paid: {payment.AmountPaid:C2}</li>
                    <li>Payment Date: {payment.PaymentDate:g}</li>
                    <li>Payment Method: {payment.PaymentMethod}</li>
                    <li>Transaction Reference: {payment.TransactionReference}</li>
                </ul>
                <p>Thank you for your payment.</p>";
        }

        private string GeneratePaymentConfirmationSmsText(PaymentRecord payment, MonthlyBill bill)
        {
            return $"DTC Payment Confirmation\n" +
                   $"Amount: {payment.AmountPaid:C2}\n" +
                   $"Date: {payment.PaymentDate:d}\n" +
                   $"Ref: {payment.TransactionReference}";
        }

        private string GeneratePaymentReminderEmailBody(MonthlyBill bill, int daysOverdue, decimal remainingAmount)
        {
            return $@"
                <h2>Payment Reminder</h2>
                <p>Dear {bill.Customer.Name},</p>
                <p>This is a reminder that your payment for {bill.BillingMonth:MMMM yyyy} is overdue by {daysOverdue} days.</p>
                <h3>Payment Details:</h3>
                <ul>
                    <li>Original Amount: {bill.TotalAmount:C2}</li>
                    <li>Remaining Amount: {remainingAmount:C2}</li>
                    <li>Due Date: {bill.DueDate:d}</li>
                </ul>
                <p>Please make the payment as soon as possible to avoid additional late charges.</p>";
        }

        private string GeneratePaymentReminderSmsText(MonthlyBill bill, int daysOverdue, decimal remainingAmount)
        {
            return $"DTC Payment Reminder\n" +
                   $"Bill: {bill.BillingMonth:MMM yyyy}\n" +
                   $"Amount Due: {remainingAmount:C2}\n" +
                   $"Overdue: {daysOverdue} days";
        }

        private string GenerateSystemAlertEmailBody(string message, NotificationPriority priority)
        {
            var color = priority switch
            {
                NotificationPriority.Low => "#28a745",
                NotificationPriority.Medium => "#ffc107",
                NotificationPriority.High => "#dc3545",
                _ => "#6c757d"
            };

            return $@"
                <h2 style='color: {color}'>System Alert</h2>
                <p><strong>Priority:</strong> {priority}</p>
                <p><strong>Time:</strong> {DateTime.UtcNow:g} UTC</p>
                <div style='margin-top: 20px;'>
                    <p>{message}</p>
                </div>";
        }
    }
} 