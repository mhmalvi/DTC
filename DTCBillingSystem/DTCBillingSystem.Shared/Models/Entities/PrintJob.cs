using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class PrintJob : BaseEntity
    {
        public string JobId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int Copies { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public DateTime? PrintedAt { get; set; }
        public string PaperSize { get; set; } = string.Empty;
        public string Orientation { get; set; } = string.Empty;
        public bool IsDuplex { get; set; }
        public bool IsColor { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
} 