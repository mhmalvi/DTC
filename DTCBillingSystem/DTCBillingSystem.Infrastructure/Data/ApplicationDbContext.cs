using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IAuditService _auditService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IAuditService auditService = null) : base(options)
        {
            _auditService = auditService;
        }

        // Core entities
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BillingRate> BillingRates { get; set; }
        public DbSet<MonthlyBill> Bills { get; set; }
        public DbSet<PaymentRecord> Payments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        // Meter Reading
        public DbSet<MeterReading> MeterReadings { get; set; }
        
        // Notification related entities
        public DbSet<NotificationHistory> NotificationHistory { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        
        // Print and Backup related entities
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<BackupInfo> BackupInfo { get; set; }
        public DbSet<BackupSchedule> BackupSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CustomerCode)
                .IsUnique();

            // Configure MonthlyBill relationships
            modelBuilder.Entity<MonthlyBill>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bills)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PaymentRecord relationships
            modelBuilder.Entity<PaymentRecord>()
                .HasOne(p => p.Bill)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BillId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure MeterReading
            modelBuilder.Entity<MeterReading>()
                .HasOne(m => m.Customer)
                .WithMany(c => c.MeterReadings)
                .HasForeignKey(m => m.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure NotificationHistory
            modelBuilder.Entity<NotificationHistory>()
                .HasOne(n => n.Customer)
                .WithMany(c => c.NotificationHistory)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();
            var timestamp = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditable auditable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditable.CreatedAt = timestamp;
                            auditEntries.Add(new AuditLog
                            {
                                EntityName = entry.Entity.GetType().Name,
                                Action = AuditAction.Create,
                                EntityId = GetEntityId(entry),
                                Changes = GetChanges(entry),
                                Timestamp = timestamp
                            });
                            break;

                        case EntityState.Modified:
                            auditable.LastModifiedAt = timestamp;
                            auditEntries.Add(new AuditLog
                            {
                                EntityName = entry.Entity.GetType().Name,
                                Action = AuditAction.Update,
                                EntityId = GetEntityId(entry),
                                Changes = GetChanges(entry),
                                Timestamp = timestamp
                            });
                            break;

                        case EntityState.Deleted:
                            auditEntries.Add(new AuditLog
                            {
                                EntityName = entry.Entity.GetType().Name,
                                Action = AuditAction.Delete,
                                EntityId = GetEntityId(entry),
                                Changes = GetChanges(entry),
                                Timestamp = timestamp
                            });
                            break;
                    }
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            if (_auditService != null && auditEntries.Any())
            {
                foreach (var auditEntry in auditEntries)
                {
                    await _auditService.LogAuditAsync(auditEntry);
                }
            }

            return result;
        }

        private string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyName = entry.Metadata.FindPrimaryKey()?.Properties
                .Select(p => p.Name).FirstOrDefault();

            return keyName != null ? entry.Property(keyName).CurrentValue?.ToString() : null;
        }

        private string GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var changes = new List<string>();

            foreach (var property in entry.Properties)
            {
                if (entry.State == EntityState.Added)
                {
                    if (property.CurrentValue != null)
                    {
                        changes.Add($"{property.Metadata.Name}: {property.CurrentValue}");
                    }
                }
                else if (entry.State == EntityState.Modified && property.IsModified)
                {
                    changes.Add($"{property.Metadata.Name}: {property.OriginalValue} -> {property.CurrentValue}");
                }
            }

            return string.Join(", ", changes);
        }
    }
} 