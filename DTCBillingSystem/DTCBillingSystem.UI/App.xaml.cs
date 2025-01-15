using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var services = new ServiceCollection();

                // Register DbContext
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=dtcbilling.db"));

                // Register DbContext as base type
                services.AddScoped<DbContext>(provider => 
                    provider.GetRequiredService<ApplicationDbContext>());

                // Configure TokenService
                services.AddScoped<ITokenService>(provider => new TokenService(
                    secretKey: "your-256-bit-secret-key-here-make-it-long-and-secure-12345",
                    issuer: "DTCBillingSystem",
                    audience: "DTCBillingSystem.UI",
                    expirationMinutes: 60
                ));

                // Register repositories
                services.AddScoped<ICustomerRepository, CustomerRepository>();
                services.AddScoped<IMonthlyBillRepository, MonthlyBillRepository>();
                services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
                services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
                services.AddScoped<IBackupInfoRepository, BackupInfoRepository>();
                services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                services.AddScoped<IBillingRateRepository, BillingRateRepository>();
                services.AddScoped<IPrintJobRepository, PrintJobRepository>();

                // Register UnitOfWork
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Register core services
                services.AddScoped<IPasswordHasher, PasswordHasher>();
                services.AddSingleton<ICurrentUserService, CurrentUserService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IAuditService, AuditService>();
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<ICustomerService, CustomerService>();
                services.AddScoped<IBillingService, BillingService>();
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddScoped<IMeterReadingService, MeterReadingService>();
                services.AddScoped<IPrintService, PrintService>();
                services.AddScoped<IBackupService, BackupService>();

                // Register UI services
                services.AddScoped<INavigationService, NavigationService>();
                services.AddScoped<IViewLocator, ViewLocator>();
                services.AddScoped<IDialogService, DialogService>();
                services.AddScoped<IWindowFactory, WindowFactory>();

                // Register ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<CustomerDialogViewModel>();
                services.AddTransient<CustomerBillsViewModel>();

                _serviceProvider = services.BuildServiceProvider();

                // Initialize database
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                }

                // Show login window first
                var viewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
                var loginWindow = viewLocator.CreateLoginWindow();
                loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                loginWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start: {ex.Message}\n\nDetails: {ex.StackTrace}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }
}
