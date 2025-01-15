using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    public class PrinterInfo
    {
        public required string Name { get; set; }
        public required string Model { get; set; }
        public required string Status { get; set; }
        public required string Location { get; set; }
        public required string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsOnline { get; set; }
        public List<string> SupportedPaperSizes { get; set; } = new();
        public List<string> SupportedOrientations { get; set; } = new();
    }
} 