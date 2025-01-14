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
            builder.Property(b => b.BillingMonth).IsRequired();
            builder.Property(b => b.PresentReading).IsRequired().HasPrecision(10, 2);
            builder.Property(b => b.PreviousReading).IsRequired().HasPrecision(10, 2);
            builder.Property(b => b.ACPresentReading).HasPrecision(10, 2);
            builder.Property(b => b.ACPreviousReading).HasPrecision(10, 2);
            builder.Property(b => b.BlowerFanCharge).HasPrecision(10, 2);
            builder.Property(b => b.GeneratorCharge).HasPrecision(10, 2);
            builder.Property(b => b.ServiceCharge).HasPrecision(10, 2);
            builder.Property(b => b.DueDate).IsRequired();
            builder.Property(b => b.Status).IsRequired().HasMaxLength(20);
            builder.Property(b => b.CreatedAt).IsRequired();
            builder.Property(b => b.LastModifiedAt).IsRequired();

            builder.HasOne(b => b.Customer)
                .WithMany(c => c.MonthlyBills)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 