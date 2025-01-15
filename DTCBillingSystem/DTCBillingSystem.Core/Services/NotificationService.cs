using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public NotificationService(
            IEmailService emailService,
            ISmsService smsService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _smsService = smsService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task SendBillGeneratedNotificationAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {bill.CustomerId} not found.");

            var message = new NotificationMessage
            {
                To = customer.Email,
                Subject = "New Bill Generated",
                Body = $"Dear {customer.Name},\n\nYour bill for {bill.BillingPeriod:MMMM yyyy} has been generated.\nAmount Due: {bill.TotalAmount:C}\nDue Date: {bill.DueDate:d}\n\nPlease log in to your account to view the details.",
                IsHtml = false,
                RecipientId = customer.Id
            };

            await _emailService.SendEmailAsync(message);
        }

        public async Task SendPaymentReceivedNotificationAsync(int paymentId)
        {
            var payment = await _unitOfWork.PaymentRecords.GetByIdAsync(paymentId)
                ?? throw new ArgumentException($"Payment with ID {paymentId} not found.");
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(payment.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {payment.CustomerId} not found.");

            var message = new NotificationMessage
            {
                To = customer.Email,
                Subject = "Payment Received",
                Body = $"Dear {customer.Name},\n\nWe have received your payment of {payment.Amount:C} on {payment.PaymentDate:d}.\nThank you for your prompt payment.\n\nReference Number: {payment.ReferenceNumber}",
                IsHtml = false,
                RecipientId = customer.Id
            };

            await _emailService.SendEmailAsync(message);
        }

        public async Task SendPaymentDueReminderAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {bill.CustomerId} not found.");

            var message = new NotificationMessage
            {
                To = customer.Email,
                Subject = "Payment Due Reminder",
                Body = $"Dear {customer.Name},\n\nThis is a reminder that your payment of {bill.TotalAmount:C} is due on {bill.DueDate:d}.\nPlease ensure timely payment to avoid any late fees.\n\nBill Number: {bill.BillNumber}",
                IsHtml = false,
                RecipientId = customer.Id
            };

            await _emailService.SendEmailAsync(message);
        }

        public async Task SendOverduePaymentNotificationAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");
            
            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {bill.CustomerId} not found.");

            var message = new NotificationMessage
            {
                To = customer.Email,
                Subject = "Overdue Payment Notice",
                Body = $"Dear {customer.Name},\n\nYour payment of {bill.TotalAmount:C} for Bill Number {bill.BillNumber} is overdue.\nPlease make the payment as soon as possible to avoid service interruption.\n\nOriginal Due Date: {bill.DueDate:d}",
                IsHtml = false,
                RecipientId = customer.Id
            };

            await _emailService.SendEmailAsync(message);
        }

        public async Task SendSystemAlertAsync(string message, NotificationType type)
        {
            var adminEmails = _configuration.GetSection("AdminNotifications:Emails").Get<string[]>();
            if (adminEmails == null || !adminEmails.Any())
                return;

            var notification = new NotificationMessage
            {
                Subject = $"System Alert: {type}",
                Body = message,
                IsHtml = false
            };

            foreach (var email in adminEmails)
            {
                notification.To = email;
                await _emailService.SendEmailAsync(notification);
            }
        }

        public async Task SendBulkNotificationsAsync(IEnumerable<NotificationMessage> messages)
        {
            await _emailService.SendBulkEmailAsync(messages);
        }

        public async Task<IEnumerable<NotificationMessage>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.RecipientId == userId);
            return notifications.Select(n => new NotificationMessage
            {
                To = n.RecipientEmail,
                Subject = n.Subject,
                Body = n.Body,
                IsHtml = n.IsHtml,
                RecipientId = n.RecipientId
            });
        }

        public async Task<UserNotificationPreferences> GetUserNotificationSettingsAsync(int userId)
        {
            var settings = await _unitOfWork.NotificationSettings.FindAsync(ns => ns.RecipientId == userId);
            var entitySettings = settings.FirstOrDefault();

            if (entitySettings == null)
            {
                return new UserNotificationPreferences { UserId = userId };
            }

            return new UserNotificationPreferences
            {
                UserId = entitySettings.RecipientId,
                EmailEnabled = entitySettings.EmailEnabled,
                SmsEnabled = entitySettings.SmsEnabled,
                InAppEnabled = entitySettings.InAppEnabled,
                EmailAddress = entitySettings.EmailAddress,
                PhoneNumber = entitySettings.PhoneNumber,
                EnabledNotifications = DeserializeEnabledNotifications(entitySettings.EnabledNotifications),
                QuietHoursEnabled = entitySettings.QuietHoursEnabled,
                QuietHoursStart = entitySettings.QuietHoursStart,
                QuietHoursEnd = entitySettings.QuietHoursEnd
            };
        }

        public async Task UpdateUserNotificationSettingsAsync(int userId, UserNotificationPreferences settings)
        {
            var existingSettings = await _unitOfWork.NotificationSettings.FindAsync(ns => ns.RecipientId == userId);
            var entitySettings = existingSettings.FirstOrDefault();

            if (entitySettings == null)
            {
                entitySettings = new NotificationSettings
                {
                    RecipientId = userId,
                    EmailEnabled = settings.EmailEnabled,
                    SmsEnabled = settings.SmsEnabled,
                    InAppEnabled = settings.InAppEnabled,
                    EmailAddress = settings.EmailAddress,
                    PhoneNumber = settings.PhoneNumber,
                    EnabledNotifications = SerializeEnabledNotifications(settings.EnabledNotifications),
                    QuietHoursEnabled = settings.QuietHoursEnabled,
                    QuietHoursStart = settings.QuietHoursStart,
                    QuietHoursEnd = settings.QuietHoursEnd
                };
                await _unitOfWork.NotificationSettings.AddAsync(entitySettings);
            }
            else
            {
                entitySettings.EmailEnabled = settings.EmailEnabled;
                entitySettings.SmsEnabled = settings.SmsEnabled;
                entitySettings.InAppEnabled = settings.InAppEnabled;
                entitySettings.EmailAddress = settings.EmailAddress;
                entitySettings.PhoneNumber = settings.PhoneNumber;
                entitySettings.EnabledNotifications = SerializeEnabledNotifications(settings.EnabledNotifications);
                entitySettings.QuietHoursEnabled = settings.QuietHoursEnabled;
                entitySettings.QuietHoursStart = settings.QuietHoursStart;
                entitySettings.QuietHoursEnd = settings.QuietHoursEnd;

                await _unitOfWork.NotificationSettings.UpdateAsync(entitySettings);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledTime)
        {
            var scheduledNotification = new DTCBillingSystem.Core.Models.Entities.ScheduledNotification
            {
                Message = message,
                ScheduledTime = scheduledTime,
                Status = NotificationStatus.Pending
            };

            await _unitOfWork.ScheduledNotifications.AddAsync(scheduledNotification);
            await _unitOfWork.SaveChangesAsync();
        }

        private Dictionary<NotificationType, bool> DeserializeEnabledNotifications(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new Dictionary<NotificationType, bool>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<NotificationType, bool>>(json) 
                    ?? new Dictionary<NotificationType, bool>();
            }
            catch
            {
                return new Dictionary<NotificationType, bool>();
            }
        }

        private string SerializeEnabledNotifications(Dictionary<NotificationType, bool> notifications)
        {
            return JsonSerializer.Serialize(notifications);
        }
    }
} 