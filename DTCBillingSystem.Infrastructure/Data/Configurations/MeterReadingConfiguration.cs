using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
    {
        public void Configure(EntityTypeBuilder<MeterReading> builder)
        {
            builder.ToTable("MeterReadings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MeterNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Reading).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.ReadingDate).IsRequired();
            builder.Property(x => x.ReadBy).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Source).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.ImageUrl).HasMaxLength(500);
            builder.Property(x => x.PreviousReading).HasPrecision(18, 2);
            builder.Property(x => x.Consumption).HasPrecision(18, 2);
            builder.Property(x => x.ValidationNotes).HasMaxLength(500);

            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 