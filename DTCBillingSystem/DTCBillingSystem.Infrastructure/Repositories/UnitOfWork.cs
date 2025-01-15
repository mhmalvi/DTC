using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed;

    public ICustomerRepository Customers { get; }
    public IRepository<BillingRate> BillingRates { get; }
    public IMonthlyBillRepository MonthlyBills { get; }
    public IPaymentRecordRepository PaymentRecords { get; }
    public IUserRepository Users { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IMeterReadingRepository MeterReadings { get; }
    public INotificationRepository Notifications { get; }
    public INotificationSettingsRepository NotificationSettings { get; }
    public IPrintJobRepository PrintJobs { get; }
    public IBackupInfoRepository BackupInfos { get; }
    public IScheduledNotificationRepository ScheduledNotifications { get; }
    public IBackupScheduleRepository BackupSchedules { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Customers = new CustomerRepository(_context);
        BillingRates = new BillingRateRepository(_context);
        MonthlyBills = new MonthlyBillRepository(_context);
        PaymentRecords = new PaymentRecordRepository(_context);
        Users = new UserRepository(_context);
        AuditLogs = new AuditLogRepository(_context);
        MeterReadings = new MeterReadingRepository(_context);
        Notifications = new NotificationMessageRepository(_context);
        NotificationSettings = new NotificationSettingsRepository(_context);
        PrintJobs = new PrintJobRepository(_context);
        BackupInfos = new BackupInfoRepository(_context);
        ScheduledNotifications = new ScheduledNotificationRepository(_context);
        BackupSchedules = new BackupScheduleRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.Database.CommitTransactionAsync();
        }
        catch
        {
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
} 