namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Types of notifications
    /// </summary>
    public enum NotificationType
    {
        BillGenerated,
        PaymentReceived,
        PaymentDue,
        PaymentOverdue,
        SystemAlert,
        ServiceUpdate,
        AccountUpdate,
        SecurityAlert,
        MaintenanceNotice
    }
} 