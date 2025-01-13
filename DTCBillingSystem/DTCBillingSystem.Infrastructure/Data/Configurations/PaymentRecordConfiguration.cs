using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
    {
        public void Configure(EntityTypeBuilder<PaymentRecord> builder)
        {
            builder.ToTable("PaymentRecords");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.AmountPaid)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(p => p.LatePaymentCharges)
                .HasPrecision(10, 2);

            builder.Property(p => p.TransactionReference)
                .HasMaxLength(50);

            builder.Property(p => p.Notes)
                .HasMaxLength(500);

            builder.Property(p => p.ReceivedBy)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.CreatedBy)
                .HasMaxLength(50);

            builder.Property(p => p.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(p => p.BillId);
            builder.HasIndex(p => p.PaymentDate);
            builder.HasIndex(p => p.PaymentMethod);
            builder.HasIndex(p => p.TransactionReference)
                .IsUnique()
                .HasFilter("[TransactionReference] IS NOT NULL");
        }
    }
} 