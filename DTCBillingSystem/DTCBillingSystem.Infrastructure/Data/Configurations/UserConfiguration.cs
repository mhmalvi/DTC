using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(u => u.PasswordSalt)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(15);

            builder.Property(u => u.CreatedBy)
                .HasMaxLength(50);

            builder.Property(u => u.LastModifiedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.PhoneNumber);
            builder.HasIndex(u => u.IsActive);
            builder.HasIndex(u => u.Role);

            // Default admin user
            builder.HasData(new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "AQAAAAIAAYagAAAAELbHJwQUZr2qONxGKR5aTXXqgzGqcQJTz0MJyQ5G5GJHW6QQJQugrqGhHyeyQZE0Yw==", // Admin@123
                PasswordSalt = "10000.6ckWDlxOJKqPn5JsGEYK2g==.QfeeiZOqHtxsgjp+yDHQk4jVjw0t7Z2DaRfUvw+cMYE=",
                FullName = "System Administrator",
                Email = "admin@dtc.com",
                IsActive = true,
                Role = UserRole.Administrator,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }
    }
} 