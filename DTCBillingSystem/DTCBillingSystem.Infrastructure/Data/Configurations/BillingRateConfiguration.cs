using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class BillingRateConfiguration : IEntityTypeConfiguration<BillingRate>
    {
        public void Configure(EntityTypeBuilder<BillingRate> builder)
        {
            builder.ToTable("BillingRates");

            builder.Property(r => r.BaseRate)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.ExcessRate)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.Threshold)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.FixedCharges)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.CustomerType)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .IsRequired();

            builder.Property(r => r.EffectiveFrom)
                .IsRequired();

            builder.Property(r => r.Notes)
                .HasMaxLength(500);

            builder.HasIndex(r => new { r.CustomerType, r.IsActive });
        }
    }
} 