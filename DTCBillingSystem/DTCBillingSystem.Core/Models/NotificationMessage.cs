using System;
using System.ComponentModel.DataAnnotations;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationMessage : BaseEntity
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public int RecipientId { get; set; }

        [Required]
        public int SenderId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ReadDate { get; set; }
        
        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        
        [Required]
        public NotificationType NotificationType { get; set; }
        
        [Required]
        public string Channel { get; set; } = string.Empty;
        
        [Required]
        public string DeliveryStatus { get; set; } = "Pending";
        
        [Required]
        public string Recipient { get; set; } = string.Empty;

        public int? RelatedEntityId { get; set; }
        
        public string? RelatedEntityType { get; set; }
        
        public bool IsRead { get; set; }
        
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        public DateTime? ExpiryDate { get; set; }

        public NotificationMessage()
        {
            CreatedDate = DateTime.UtcNow;
            Status = NotificationStatus.Pending;
            DeliveryStatus = "Pending";
            Priority = 3;
            IsRead = false;
        }
    }
} 