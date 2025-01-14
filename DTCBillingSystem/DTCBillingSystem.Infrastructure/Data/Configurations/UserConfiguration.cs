using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Role).IsRequired().HasMaxLength(20);
            builder.Property(u => u.LastLoginDate);
            builder.Property(u => u.IsActive).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(u => u.LastModifiedAt).IsRequired();

            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
} 