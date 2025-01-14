namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the status of a bill
    /// </summary>
    public enum BillStatus
    {
        Draft = 0,
        Unpaid = 1,
        Paid = 2,
        PartiallyPaid = 3,
        Overdue = 4,
        Cancelled = 5,
        Disputed = 6
    }
} 