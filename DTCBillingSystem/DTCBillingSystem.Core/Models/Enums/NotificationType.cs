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
        PaymentDueReminder,
        OverduePayment,
        SystemAlert,
        General,
        Error
    }
} 