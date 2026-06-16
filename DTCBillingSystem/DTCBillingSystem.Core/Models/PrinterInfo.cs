namespace DTCBillingSystem.Core.Models
{
    public class PrinterInfo
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public bool SupportsColor { get; set; }
        public bool SupportsDuplex { get; set; }
        public string[] SupportedPaperSizes { get; set; }
        public string[] SupportedOrientations { get; set; }
        public int MaxCopies { get; set; }
    }
} 