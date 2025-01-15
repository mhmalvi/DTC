using System;

namespace DTCBillingSystem.Core.Models
{
    public class PrintJobInfo
    {
        public required string JobId { get; set; }
        public required string Message { get; set; }
        public required string ErrorDetails { get; set; }
        public required string PrinterName { get; set; }
        public int Copies { get; set; }
        public bool IsDuplex { get; set; }
        public required string DocumentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
} 