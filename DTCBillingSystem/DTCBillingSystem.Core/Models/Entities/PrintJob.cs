using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class PrintJob : BaseEntity
    {
        public string JobNumber { get; set; } = string.Empty;
        public int BillId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public PrintJobStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }

        // Navigation properties
        public MonthlyBill Bill { get; set; } = null!;
    }
} 