using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class PrintJob : BaseEntity
    {
        public string DocumentType { get; set; }
        public string DocumentId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PrintedDate { get; set; }
        public string PrintedBy { get; set; }
        public int Copies { get; set; }
        public string PrinterName { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public bool IsCompleted { get; set; }
    }
} 