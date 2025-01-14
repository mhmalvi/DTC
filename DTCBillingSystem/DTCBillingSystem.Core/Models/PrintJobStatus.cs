using System;

namespace DTCBillingSystem.Core.Models
{
    public class PrintJobStatus
    {
        public string JobId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public string ErrorMessage { get; set; }
        public int Progress { get; set; }
        public int TotalPages { get; set; }
        public int PrintedPages { get; set; }
        public string CurrentOperation { get; set; }
    }
} 