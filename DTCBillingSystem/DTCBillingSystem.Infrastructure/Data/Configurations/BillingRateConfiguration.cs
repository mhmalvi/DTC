using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class BillingRateConfiguration : IEntityTypeConfiguration<BillingRate>
    {
        public void Configure(EntityTypeBuilder<BillingRate> builder)
        {
            builder.ToTable("BillingRates");

            builder.Property(r => r.Rate)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.TaxRate)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.LatePaymentRate)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.CustomerType)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .IsRequired();

            builder.HasIndex(r => new { r.CustomerType, r.IsActive });
        }
    }
} 