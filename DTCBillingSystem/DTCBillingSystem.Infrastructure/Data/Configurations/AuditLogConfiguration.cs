using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
            builder.Property(a => a.EntityName).IsRequired().HasMaxLength(50);
            builder.Property(a => a.EntityId).IsRequired();
            builder.Property(a => a.Changes).HasMaxLength(4000);
            builder.Property(a => a.Timestamp).IsRequired();
            builder.Property(a => a.UserId).IsRequired();

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.EntityId);
            builder.HasIndex(a => a.Timestamp);
            builder.HasIndex(a => a.UserId);
        }
    }
} 