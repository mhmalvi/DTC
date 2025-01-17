using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class MonthlyBillConfiguration : IEntityTypeConfiguration<MonthlyBill>
    {
        public void Configure(EntityTypeBuilder<MonthlyBill> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.BillNumber).IsRequired().HasMaxLength(20);
            builder.Property(b => b.BillingMonth).IsRequired();
            builder.Property(b => b.DueDate).IsRequired();
            builder.Property(b => b.Amount).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.TaxAmount).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.TotalAmount).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.Status).IsRequired();
            builder.Property(b => b.PaymentReference).HasMaxLength(50);
            
            // Meter readings
            builder.Property(b => b.PresentReading).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.PreviousReading).IsRequired().HasPrecision(18, 2);
            builder.Property(b => b.ACPresentReading).HasPrecision(18, 2);
            builder.Property(b => b.ACPreviousReading).HasPrecision(18, 2);
            
            // Additional charges
            builder.Property(b => b.BlowerFanCharge).HasPrecision(18, 2);
            builder.Property(b => b.GeneratorCharge).HasPrecision(18, 2);
            builder.Property(b => b.ServiceCharge).HasPrecision(18, 2);

            builder.HasOne(b => b.Customer)
                .WithMany(c => c.MonthlyBills)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 