using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class BillingRateConfiguration : IEntityTypeConfiguration<BillingRate>
    {
        public void Configure(EntityTypeBuilder<BillingRate> builder)
        {
            builder.ToTable("BillingRates");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RateType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Rate)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(r => r.Description)
                .HasMaxLength(200);

            builder.Property(r => r.Unit)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.CreatedBy)
                .HasMaxLength(50);

            builder.Property(r => r.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(r => new { r.RateType, r.EffectiveDate })
                .IsUnique();

            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => r.EffectiveDate);
            builder.HasIndex(r => r.EndDate);
        }
    }
} 