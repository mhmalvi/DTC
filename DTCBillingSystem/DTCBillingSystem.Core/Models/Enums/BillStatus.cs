namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a bill
    /// </summary>
    public enum BillStatus
    {
        Draft = 0,
        Pending = 1,
        Unpaid = 2,
        Paid = 3,
        PartiallyPaid = 4,
        Overdue = 5,
        Cancelled = 6,
        Disputed = 7
    }
} 