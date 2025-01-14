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
        Retrying = 3,
        Cancelled = 4
    }
} 