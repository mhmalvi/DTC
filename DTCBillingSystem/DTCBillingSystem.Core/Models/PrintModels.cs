using System;

namespace DTCBillingSystem.Core.Models
{
    public class PrintOptions
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public bool IsDuplex { get; set; }
        public string PaperSize { get; set; } = "A4";
        public string Orientation { get; set; } = "Portrait";
        public bool IsColor { get; set; }
        public int Quality { get; set; } = 300; // DPI
        public string Tray { get; set; } = "Auto";
        public bool Collate { get; set; } = true;
    }

    public class PrintResult
    {
        public bool Success { get; set; }
        public string JobId { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
        public string PrinterName { get; set; }
        public int Pages { get; set; }
        public int Copies { get; set; }
        public string DocumentType { get; set; }
        public int DocumentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
    }
} 