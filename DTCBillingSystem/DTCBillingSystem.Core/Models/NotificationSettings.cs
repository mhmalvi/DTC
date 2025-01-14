using System;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationSettings : BaseEntity
    {
        public int CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public bool BillDueReminder { get; set; }
        public bool PaymentConfirmation { get; set; }
        public bool ServiceUpdates { get; set; }
        public bool PromotionalMessages { get; set; }
        public int ReminderDays { get; set; }
        public Customer Customer { get; set; }
    }
} 