using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Core.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private bool _disposed;

        public IRepository<User> Users { get; }
        public IRepository<Customer> Customers { get; }
        public IRepository<MonthlyBill> MonthlyBills { get; }
        public IRepository<PaymentRecord> PaymentRecords { get; }
        public IRepository<AuditLog> AuditLogs { get; }
        public IRepository<BillingRate> BillingRates { get; }
        public IRepository<MeterReading> MeterReadings { get; }
        public IRepository<NotificationHistory> NotificationHistory { get; }
        public IRepository<NotificationSettings> NotificationSettings { get; }
        public IRepository<NotificationMessage> NotificationMessages { get; }
        public IRepository<PrintJob> PrintJobs { get; }
        public IRepository<BackupInfo> BackupInfo { get; }
        public IRepository<BackupSchedule> BackupSchedules { get; }

        public UnitOfWork(
            DbContext context,
            IRepository<User> users,
            IRepository<Customer> customers,
            IRepository<MonthlyBill> monthlyBills,
            IRepository<PaymentRecord> paymentRecords,
            IRepository<AuditLog> auditLogs,
            IRepository<BillingRate> billingRates,
            IRepository<MeterReading> meterReadings,
            IRepository<NotificationHistory> notificationHistory,
            IRepository<NotificationSettings> notificationSettings,
            IRepository<NotificationMessage> notificationMessages,
            IRepository<PrintJob> printJobs,
            IRepository<BackupInfo> backupInfo,
            IRepository<BackupSchedule> backupSchedules)
        {
            _context = context;
            Users = users;
            Customers = customers;
            MonthlyBills = monthlyBills;
            PaymentRecords = paymentRecords;
            AuditLogs = auditLogs;
            BillingRates = billingRates;
            MeterReadings = meterReadings;
            NotificationHistory = notificationHistory;
            NotificationSettings = notificationSettings;
            NotificationMessages = notificationMessages;
            PrintJobs = printJobs;
            BackupInfo = backupInfo;
            BackupSchedules = backupSchedules;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error and throw
                throw new Exception("Error saving changes to the database", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_context.Database.CurrentTransaction == null)
            {
                await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await RollbackTransactionAsync();
                throw new Exception("Error committing transaction", ex);
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.Database.RollbackTransactionAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
} 