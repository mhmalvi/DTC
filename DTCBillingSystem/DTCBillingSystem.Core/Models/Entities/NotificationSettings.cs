using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class NotificationSettings : BaseEntity
    {
        public int CustomerId { get; set; }
        public NotificationPreference Preference { get; set; }
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; }
        public bool InAppEnabled { get; set; } = true;
        public bool IsPushEnabled { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DeviceToken { get; set; } = string.Empty;
        public string EnabledNotifications { get; set; } = string.Empty;
        public bool QuietHoursEnabled { get; set; }
        public TimeSpan QuietHoursStart { get; set; }
        public TimeSpan QuietHoursEnd { get; set; }

        // Navigation property
        public virtual Customer Customer { get; set; } = null!;
    }
} 