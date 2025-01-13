using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.ShopNo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Floor)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.PhoneNumber)
                .HasMaxLength(15);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            builder.Property(c => c.CreatedBy)
                .HasMaxLength(50);

            builder.Property(c => c.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(c => c.ShopNo)
                .IsUnique();

            builder.HasIndex(c => c.PhoneNumber);
            builder.HasIndex(c => c.Email);
            builder.HasIndex(c => c.IsActive);
        }
    }
} 