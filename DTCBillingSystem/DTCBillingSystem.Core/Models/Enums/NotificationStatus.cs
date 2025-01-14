namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a notification message
    /// </summary>
    public enum NotificationStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2,
        Cancelled = 3,
        Scheduled = 4
    }
} 