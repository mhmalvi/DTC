using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.PaymentDate).IsRequired();
            builder.Property(p => p.ReferenceNumber).HasMaxLength(50);
            builder.Property(p => p.Notes).HasMaxLength(500);

            builder.HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.MonthlyBill)
                .WithOne()
                .HasForeignKey<PaymentRecord>(p => p.MonthlyBillId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 