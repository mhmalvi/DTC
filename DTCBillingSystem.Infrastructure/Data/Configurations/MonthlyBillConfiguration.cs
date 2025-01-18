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
            builder.Property(b => b.BillingDate).IsRequired();
            builder.Property(b => b.Amount).HasPrecision(18, 2);
            builder.Property(b => b.PreviousReading).HasPrecision(18, 2);
            builder.Property(b => b.CurrentReading).HasPrecision(18, 2);
            builder.Property(b => b.Consumption).HasPrecision(18, 2);
            builder.Property(b => b.Notes).HasMaxLength(500);

            builder.HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 