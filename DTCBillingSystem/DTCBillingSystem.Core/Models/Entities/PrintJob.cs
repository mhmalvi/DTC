using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class PrintJob : BaseEntity
    {
        public string JobId { get; set; } = string.Empty;
        public int BillId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public PrintJobStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        // Navigation properties
        public MonthlyBill Bill { get; set; } = null!;
    }
} 