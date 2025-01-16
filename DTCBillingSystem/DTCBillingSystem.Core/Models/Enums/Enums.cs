namespace DTCBillingSystem.Core.Models.Enums
{
    public enum BillStatus
    {
        Draft,
        Pending,
        Unpaid,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled,
        Void
    }

    public enum PaymentMethod
    {
        Cash,
        Check,
        BankTransfer,
        OnlineBanking,
        CreditCard,
        DebitCard,
        MobileWallet,
        Other
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Refunded,
        Void
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended,
        Terminated,
        Pending
    }

    public enum CustomerType
    {
        Residential,
        Commercial,
        Industrial,
        Government,
        Special
    }

    public enum UserRole
    {
        Administrator,
        Manager,
        Cashier,
        Accountant,
        Auditor,
        Support,
        Guest
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Locked,
        PendingActivation,
        Deleted
    }

    public enum ReportType
    {
        BillingSummary,
        CustomerStatement,
        DailyCollection,
        MonthlyBilling,
        OutstandingPayments,
        RevenueSummary
    }

    public enum ReportStatus
    {
        Pending,
        Generating,
        Completed,
        Failed
    }

    public enum BackupStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Verified,
        Restored
    }

    public enum BackupType
    {
        Full,
        Differential,
        Incremental,
        Transaction,
        System
    }
} 