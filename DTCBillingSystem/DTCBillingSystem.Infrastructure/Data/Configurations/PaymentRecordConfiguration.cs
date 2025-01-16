using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> builder)
        {
            builder.ToTable("PaymentRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentDate).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.PaymentMethod).IsRequired();
            builder.Property(x => x.ReferenceNumber).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(500);

            // Configure relationships
            builder.HasOne(x => x.MonthlyBill)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.MonthlyBillId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 