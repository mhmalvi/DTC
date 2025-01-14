using System.ComponentModel.DataAnnotations;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        
        public string? LastModifiedBy { get; set; }
        
        public DateTime? LastModifiedAt { get; set; }
        
        public bool IsDeleted { get; set; }
    }
} 