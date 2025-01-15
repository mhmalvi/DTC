namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a notification message
    /// </summary>
    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed,
        Cancelled,
        Retrying
    }
} 