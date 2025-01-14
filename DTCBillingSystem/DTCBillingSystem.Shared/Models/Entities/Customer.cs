using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string MeterNumber { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; }
        public string CustomerType { get; set; }
        public string AccountNumber { get; set; }
    }
} 