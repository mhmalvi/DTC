using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
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
        private IConfiguration? _configuration;

        public App()
        {
            try
            {
                LoadConfiguration();
                ConfigureServices();
                InitializeApplication();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application initialization failed:\n\n{ex.Message}", 
                    "Startup Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void LoadConfiguration()
        {
            var executablePath = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(executablePath, "appsettings.json");

            if (!File.Exists(configPath))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                configPath = Path.Combine(currentDirectory, "appsettings.json");
                
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Configuration file not found. Searched in:\n{executablePath}\n{currentDirectory}");
                }
            }

            var configDir = Path.GetDirectoryName(configPath);
            if (string.IsNullOrEmpty(configDir))
                throw new InvalidOperationException("Invalid configuration file path");

            var builder = new ConfigurationBuilder()
                .SetBasePath(configDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        private void ConfigureServices()
        {
            if (_configuration == null)
                throw new InvalidOperationException("Configuration must be loaded before configuring services.");

            var services = new ServiceCollection();

            // Add configuration first
            services.AddSingleton<IConfiguration>(_configuration);

            // Add Core services
            services.AddScoped<IPasswordHasher, Core.Services.PasswordHasher>();
            services.AddScoped<ITokenService>(provider =>
            {
                var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
                var audience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
                var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
                return new Core.Services.TokenService(secretKey, issuer, audience, expirationMinutes);
            });
            services.AddScoped<IUserService, Core.Services.UserService>();

            // Add Database Context and Repositories
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlite(connectionString), 
                ServiceLifetime.Scoped
            );
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            
            // Register all repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBillRepository, BillRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IBackupInfoRepository, BackupInfoRepository>();
            services.AddScoped<IPrintJobRepository, PrintJobRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
            services.AddScoped<IBillingRateRepository, BillingRateRepository>();
            services.AddScoped<IMonthlyBillRepository, MonthlyBillRepository>();
            services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();

            // Add Infrastructure Services
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var dbContext = provider.GetRequiredService<ApplicationDbContext>();
                return new DTCBillingSystem.Infrastructure.Data.UnitOfWork(
                    dbContext,
                    provider.GetRequiredService<IUserRepository>(),
                    provider.GetRequiredService<ICustomerRepository>(),
                    provider.GetRequiredService<IMonthlyBillRepository>(),
                    provider.GetRequiredService<IPaymentRecordRepository>(),
                    provider.GetRequiredService<IBackupInfoRepository>(),
                    provider.GetRequiredService<IPrintJobRepository>(),
                    provider.GetRequiredService<IMeterReadingRepository>(),
                    provider.GetRequiredService<IBackupScheduleRepository>(),
                    provider.GetRequiredService<IBillingRateRepository>(),
                    provider.GetRequiredService<IAuditLogRepository>()
                );
            });
            services.AddScoped<IAuthenticationService, Infrastructure.Services.AuthenticationService>();
            services.AddScoped<ICustomerService, Infrastructure.Services.CustomerService>();
            services.AddScoped<IBillingService, Infrastructure.Services.BillingService>();
            services.AddScoped<IPaymentService, Infrastructure.Services.PaymentService>();
            services.AddScoped<IReportService, Infrastructure.Services.ReportService>();
            services.AddScoped<IAuditService, Infrastructure.Services.AuditService>();
            services.AddScoped<IBackupService, Infrastructure.Services.BackupService>();
            services.AddScoped<IPrintService, Infrastructure.Services.PrintService>();
            services.AddScoped<IMeterReadingService, Infrastructure.Services.MeterReadingService>();
            services.AddScoped<ICurrentUserService, Infrastructure.Services.CurrentUserService>();
            services.AddScoped<DbInitializer>();

            // Add UI Services - Changed from Singleton to Scoped where needed
            services.AddScoped<IDialogService, DialogService>();
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IWindowFactory, WindowFactory>();
            services.AddScoped<ViewLocator>();

            // Register ViewModels - All as Scoped to match their dependencies
            services.AddScoped<LoginViewModel>();
            services.AddScoped<MainViewModel>();
            services.AddScoped<DashboardViewModel>();
            services.AddScoped<CustomerViewModel>();
            services.AddScoped<BillViewModel>();
            services.AddScoped<PaymentViewModel>();
            services.AddScoped<ReportViewModel>();
            services.AddScoped<SettingsViewModel>();

            // Register Views - All as Scoped to match their ViewModels
            services.AddScoped<LoginWindow>();
            services.AddScoped<Views.MainWindow>();
            services.AddScoped<DashboardView>();
            services.AddScoped<CustomerView>();
            services.AddScoped<BillView>();
            services.AddScoped<PaymentView>();
            services.AddScoped<ReportView>();
            services.AddScoped<SettingsView>();
            services.AddScoped<PaymentDialog>();
            services.AddScoped<BillDetailsDialog>();

            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions 
            { 
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        }

        private async void InitializeApplication()
        {
            try
            {
                if (_serviceProvider == null)
                    throw new InvalidOperationException("Service provider not initialized.");

                // Set DataDirectory for SQLite database
                var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                Directory.CreateDirectory(appDataPath);
                AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);

                // Create a scope to resolve scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Initialize database and create admin user if needed
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                    await dbInitializer.InitializeAsync();

                    var windowFactory = scope.ServiceProvider.GetRequiredService<IWindowFactory>();
                    var loginWindow = windowFactory.CreateLoginWindow();
                    loginWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error during application initialization:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                    "Startup Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
