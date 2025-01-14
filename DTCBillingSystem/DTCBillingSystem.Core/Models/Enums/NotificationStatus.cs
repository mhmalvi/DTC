namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a notification message
    /// </summary>
    public enum NotificationStatus
    {
        Pending,
        Scheduled,
        Sent,
        Failed,
        Cancelled,
        Read
    }
} 