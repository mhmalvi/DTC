using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Initialize database
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.EnsureCreated();
                }

                // Create and show the login window
                var viewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
                var loginWindow = viewLocator.CreateLoginWindow();
                
                if (loginWindow == null)
                {
                    MessageBox.Show("Failed to create login window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown(-1);
                    return;
                }

                MainWindow = loginWindow;
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start: {ex.Message}\n\nDetails: {ex}", 
                              "Critical Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(baseDir, "dtcbilling.db");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IMonthlyBillRepository, MonthlyBillRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            services.AddScoped<IBackupInfoRepository, BackupInfoRepository>();
            services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IBillingRateRepository, BillingRateRepository>();
            services.AddScoped<IPrintJobRepository, PrintJobRepository>();

            // Register services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IViewLocator, ViewLocator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IMeterReadingService, MeterReadingService>();
            services.AddScoped<IPrintService, PrintService>();
            services.AddScoped<IBackupService, BackupService>();

            // Register view models
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomerDialogViewModel>();
            services.AddTransient<CustomerBillsViewModel>();
            services.AddTransient<BillGenerationViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ReportViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<PaymentViewModel>();
            services.AddTransient<BillViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
