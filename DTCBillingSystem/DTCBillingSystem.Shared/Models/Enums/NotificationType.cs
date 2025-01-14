namespace DTCBillingSystem.Shared.Models.Enums
{
    public enum NotificationType
    {
        BillGenerated,
        PaymentReceived,
        PaymentOverdue,
        MeterReadingDue,
        SystemMaintenance,
        AccountUpdate,
        ServiceInterruption,
        GeneralAnnouncement,
        RateChange,
        BackupCompleted,
        BackupFailed,
        SecurityAlert
    }
} 