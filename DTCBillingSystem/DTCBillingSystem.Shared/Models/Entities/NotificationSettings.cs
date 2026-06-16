using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationSettings : BaseEntity
    {
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? SmsApiKey { get; set; }
        public string? SmsApiSecret { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public int MaxRetries { get; set; }
        public int RetryIntervalMinutes { get; set; }
    }
} 