using System;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationSettings : BaseEntity
    {
        public int CustomerId { get; set; }
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool BillGeneratedNotification { get; set; }
        public bool PaymentReceivedNotification { get; set; }
        public bool PaymentDueNotification { get; set; }
        public bool PaymentOverdueNotification { get; set; }
        public bool SystemAlertNotification { get; set; }
        public int DaysBeforeDueReminder { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
} 