namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationSettings : BaseEntity
    {
        public string UserId { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SMSNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool BillingReminders { get; set; }
        public bool PaymentConfirmations { get; set; }
        public bool SystemUpdates { get; set; }
    }
} 