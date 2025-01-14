namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a bill
    /// </summary>
    public enum BillStatus
    {
        Pending,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled
    }
} 