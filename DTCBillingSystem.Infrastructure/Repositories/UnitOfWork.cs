using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IBillingPeriodRepository _billingPeriodRepository;
        private readonly IMeterReadingScheduleRepository _meterReadingScheduleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceItemRepository _invoiceItemRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IBackupRepository _backupRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMonthlyBillRepository _monthlyBillRepository;

        public UnitOfWork(
            ApplicationDbContext context,
            ICustomerRepository customerRepository,
            IMeterReadingRepository meterReadingRepository,
            IBillingPeriodRepository billingPeriodRepository,
            IMeterReadingScheduleRepository meterReadingScheduleRepository,
            IUserRepository userRepository,
            IInvoiceRepository invoiceRepository,
            IInvoiceItemRepository invoiceItemRepository,
            IPaymentRepository paymentRepository,
            INotificationRepository notificationRepository,
            IBackupRepository backupRepository,
            IAuditLogRepository auditLogRepository,
            IMonthlyBillRepository monthlyBillRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
            _meterReadingRepository = meterReadingRepository;
            _billingPeriodRepository = billingPeriodRepository;
            _meterReadingScheduleRepository = meterReadingScheduleRepository;
            _userRepository = userRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _paymentRepository = paymentRepository;
            _notificationRepository = notificationRepository;
            _backupRepository = backupRepository;
            _auditLogRepository = auditLogRepository;
            _monthlyBillRepository = monthlyBillRepository;
        }

        public ICustomerRepository Customers => _customerRepository;
        public IMeterReadingRepository MeterReadings => _meterReadingRepository;
        public IBillingPeriodRepository BillingPeriods => _billingPeriodRepository;
        public IMeterReadingScheduleRepository MeterReadingSchedules => _meterReadingScheduleRepository;
        public IUserRepository Users => _userRepository;
        public IInvoiceRepository Invoices => _invoiceRepository;
        public IInvoiceItemRepository InvoiceItems => _invoiceItemRepository;
        public IPaymentRepository Payments => _paymentRepository;
        public INotificationRepository Notifications => _notificationRepository;
        public IBackupRepository Backups => _backupRepository;
        public IAuditLogRepository AuditLogs => _auditLogRepository;
        public IMonthlyBillRepository MonthlyBills => _monthlyBillRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }
    }
} 