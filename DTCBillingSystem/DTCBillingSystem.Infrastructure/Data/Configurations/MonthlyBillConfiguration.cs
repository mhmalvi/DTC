using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class MonthlyBillConfiguration : IEntityTypeConfiguration<MonthlyBill>
    {
        public void Configure(EntityTypeBuilder<MonthlyBill> builder)
        {
            builder.ToTable("MonthlyBills");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.PresentReading)
                .HasPrecision(10, 2);

            builder.Property(b => b.PreviousReading)
                .HasPrecision(10, 2);

            builder.Property(b => b.ACPresentReading)
                .HasPrecision(10, 2);

            builder.Property(b => b.ACPreviousReading)
                .HasPrecision(10, 2);

            builder.Property(b => b.BlowerFanCharge)
                .HasPrecision(10, 2);

            builder.Property(b => b.GeneratorCharge)
                .HasPrecision(10, 2);

            builder.Property(b => b.ServiceCharge)
                .HasPrecision(10, 2);

            builder.Property(b => b.TotalAmount)
                .HasPrecision(10, 2);

            builder.Property(b => b.AdditionalCharges)
                .HasPrecision(10, 2);

            builder.Property(b => b.Notes)
                .HasMaxLength(500);

            builder.Property(b => b.CreatedBy)
                .HasMaxLength(50);

            builder.Property(b => b.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(b => new { b.CustomerId, b.BillingMonth })
                .IsUnique();

            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.DueDate);
            builder.HasIndex(b => b.BillingMonth);
        }
    }
} 