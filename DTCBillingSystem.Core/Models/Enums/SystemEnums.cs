namespace DTCBillingSystem.Core.Models.Enums
{
    public enum NotificationType
    {
        System,
        BillDue,
        PaymentOverdue,
        PrintJob,
        BackupFailed,
        SystemMaintenance,
        AccountUpdate,
        Other
    }

    public enum NotificationStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public enum BillStatus
    {
        Pending,
        Paid,
        Overdue,
        Cancelled,
        Void
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        BankTransfer,
        OnlineBanking,
        Check,
        Other
    }
} 