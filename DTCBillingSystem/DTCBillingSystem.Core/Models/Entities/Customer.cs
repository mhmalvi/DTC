using System.ComponentModel.DataAnnotations;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string MeterNumber { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
        public virtual ICollection<MonthlyBill> Bills { get; set; } = new List<MonthlyBill>();
    }
} 