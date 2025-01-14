using System;

namespace DTCBillingSystem.Core.Models
{
    public class PrintJob : BaseEntity
    {
        public string JobId { get; set; }
        public string DocumentType { get; set; }
        public int DocumentId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public new DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PrinterName { get; set; }
        public int Copies { get; set; }
        public bool IsDuplex { get; set; }
        public string PaperSize { get; set; }
        public string Orientation { get; set; }
        public new string CreatedBy { get; set; }
    }
} 