using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    public class PrintModels
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public List<string> SupportedPaperSizes { get; set; } = new();

        public PrintModels(string name, string model, string status, string location, string description)
        {
            Name = name;
            Model = model;
            Status = status;
            Location = location;
            Description = description;
        }
    }
} 