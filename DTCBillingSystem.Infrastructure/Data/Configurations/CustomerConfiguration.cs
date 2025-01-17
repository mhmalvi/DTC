using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.ShopNo).IsRequired().HasMaxLength(20);
            builder.Property(c => c.Floor).IsRequired().HasMaxLength(10);
            builder.Property(c => c.PhoneNumber).HasMaxLength(15);
            builder.Property(c => c.Status).IsRequired();
            builder.Property(c => c.RegistrationDate).IsRequired();
            builder.Property(c => c.CurrentBalance).HasPrecision(18, 2);
            builder.Property(c => c.SecurityDeposit).HasPrecision(18, 2);

            builder.HasMany(c => c.MonthlyBills)
                .WithOne(b => b.Customer)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 