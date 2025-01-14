using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IRepository<User>
    {
        public UserRepository(DbContext context) : base(context) { }
    }

    public class CustomerRepository : BaseRepository<Customer>, IRepository<Customer>
    {
        public CustomerRepository(DbContext context) : base(context) { }
    }

    public class BillRepository : BaseRepository<MonthlyBill>, IRepository<MonthlyBill>
    {
        public BillRepository(DbContext context) : base(context) { }
    }

    public class PaymentRepository : BaseRepository<PaymentRecord>, IRepository<PaymentRecord>
    {
        public PaymentRepository(DbContext context) : base(context) { }
    }

    public class AuditLogRepository : BaseRepository<AuditLog>, IRepository<AuditLog>
    {
        public AuditLogRepository(DbContext context) : base(context) { }
    }

    public class BillingRateRepository : BaseRepository<BillingRate>, IRepository<BillingRate>
    {
        public BillingRateRepository(DbContext context) : base(context) { }
    }

    public class MeterReadingRepository : BaseRepository<MeterReading>, IRepository<MeterReading>
    {
        public MeterReadingRepository(DbContext context) : base(context) { }
    }

    public class NotificationHistoryRepository : BaseRepository<NotificationHistory>, IRepository<NotificationHistory>
    {
        public NotificationHistoryRepository(DbContext context) : base(context) { }
    }

    public class NotificationSettingsRepository : BaseRepository<NotificationSettings>, IRepository<NotificationSettings>
    {
        public NotificationSettingsRepository(DbContext context) : base(context) { }
    }

    public class NotificationMessageRepository : BaseRepository<NotificationMessage>, IRepository<NotificationMessage>
    {
        public NotificationMessageRepository(DbContext context) : base(context) { }
    }

    public class PrintJobRepository : BaseRepository<PrintJob>, IRepository<PrintJob>
    {
        public PrintJobRepository(DbContext context) : base(context) { }
    }

    public class BackupInfoRepository : BaseRepository<BackupInfo>, IRepository<BackupInfo>
    {
        public BackupInfoRepository(DbContext context) : base(context) { }
    }

    public class BackupScheduleRepository : BaseRepository<BackupSchedule>, IRepository<BackupSchedule>
    {
        public BackupScheduleRepository(DbContext context) : base(context) { }
    }
} 