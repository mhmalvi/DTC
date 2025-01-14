using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class NotificationSettings : BaseEntity
    {
        public int CustomerId { get; set; }
        public bool PushEnabled { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DeviceToken { get; set; }
        public NotificationPreference BillingNotifications { get; set; }
        public NotificationPreference PaymentNotifications { get; set; }
        public NotificationPreference ServiceNotifications { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
} 