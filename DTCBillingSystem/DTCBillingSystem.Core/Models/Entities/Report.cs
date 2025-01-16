using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Report : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public ReportType Type { get; set; }
        public string Parameters { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string GeneratedBy { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 