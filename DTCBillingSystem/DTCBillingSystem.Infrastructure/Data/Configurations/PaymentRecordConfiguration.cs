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
            builder.Property(p => p.AmountPaid).IsRequired().HasPrecision(10, 2);
            builder.Property(p => p.PaymentDate).IsRequired();
            builder.Property(p => p.PaymentMethod).IsRequired().HasMaxLength(50);
            builder.Property(p => p.LatePaymentCharges).HasPrecision(10, 2);
            builder.Property(p => p.TransactionReference).HasMaxLength(50);

            builder.HasOne(p => p.MonthlyBill)
                .WithMany(b => b.PaymentRecords)
                .HasForeignKey(p => p.MonthlyBillId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 