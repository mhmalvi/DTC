using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.OldValues)
                .HasMaxLength(4000);

            builder.Property(a => a.NewValues)
                .HasMaxLength(4000);

            builder.Property(a => a.Notes)
                .HasMaxLength(500);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(50);

            builder.Property(a => a.CreatedBy)
                .HasMaxLength(50);

            builder.Property(a => a.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(a => a.EntityType);
            builder.HasIndex(a => a.EntityId);
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.Timestamp);

            // Composite indexes
            builder.HasIndex(a => new { a.EntityType, a.EntityId });
            builder.HasIndex(a => new { a.UserId, a.Timestamp });
        }
    }
} 