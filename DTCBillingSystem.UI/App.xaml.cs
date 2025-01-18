using System;
using System.Windows;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.Infrastructure;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        private IConfiguration? _configuration;

        public static IServiceProvider? ServiceProvider => (Current as App)?._serviceProvider;

        public App()
        {
            // Initialize in OnStartup instead of constructor
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                InitializeConfiguration();
                InitializeServices();
                InitializeDatabase();
                ShowLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void InitializeConfiguration()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(baseDirectory, "appsettings.json");
            var dbPath = Path.Combine(baseDirectory, "DTCBillingSystem.db");

            // Copy appsettings.json to output if it doesn't exist
            if (!File.Exists(configPath))
            {
                var sourceConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json");
                if (File.Exists(sourceConfigPath))
                {
                    File.Copy(sourceConfigPath, configPath, true);
                }
                else
                {
                    throw new FileNotFoundException($"Configuration file not found at: {configPath}");
                }
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        private void InitializeServices()
        {
            if (_configuration == null)
                throw new InvalidOperationException("Configuration must be initialized before services");

            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(_configuration);

            // Add database context FIRST
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(baseDirectory, "DTCBillingSystem.db");
            var connectionString = $"Data Source={dbPath}";
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Register core services
            services.AddTransient<IPasswordHasher, Core.Services.PasswordHasher>();

            // Configure and register TokenService
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
            
            services.AddSingleton<ITokenService>(new Core.Services.TokenService(secretKey, issuer, audience));

            // Register repositories
            services.AddScoped<ICustomerRepository>(sp => new Infrastructure.Repositories.CustomerRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IMonthlyBillRepository>(sp => new Infrastructure.Repositories.MonthlyBillRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IPaymentRecordRepository>(sp => new Infrastructure.Repositories.PaymentRecordRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IUserRepository>(sp => new Infrastructure.Repositories.UserRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IMeterReadingRepository>(sp => new Infrastructure.Repositories.MeterReadingRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IPrintJobRepository>(sp => new Infrastructure.Repositories.PrintJobRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IAuditLogRepository>(sp => new Infrastructure.Repositories.AuditLogRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IBackupInfoRepository>(sp => new Infrastructure.Repositories.BackupInfoRepository(sp.GetRequiredService<ApplicationDbContext>()));
            services.AddScoped<IBackupScheduleRepository>(sp => new Infrastructure.Repositories.BackupScheduleRepository(sp.GetRequiredService<ApplicationDbContext>()));

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork>(sp => new Infrastructure.Data.UnitOfWork(sp.GetRequiredService<ApplicationDbContext>()));

            // Register infrastructure services
            services.AddScoped<ICustomerService, Infrastructure.Services.CustomerService>();
            services.AddScoped<IBillingService, Infrastructure.Services.BillingService>();
            services.AddScoped<IPaymentService, Infrastructure.Services.PaymentService>();
            services.AddScoped<IUserService, Core.Services.UserService>();
            services.AddScoped<IAuditService, Core.Services.AuditService>();
            services.AddScoped<IBackupService, Core.Services.BackupService>();
            services.AddScoped<IMeterReadingService, Core.Services.MeterReadingService>();
            services.AddScoped<IPrintService, Core.Services.PrintService>();
            services.AddScoped<IReportService, Core.Services.ReportService>();
            services.AddScoped<ICurrentUserService, Infrastructure.Services.CurrentUserService>();
            services.AddScoped<IAuthenticationService, Infrastructure.Services.AuthenticationService>();
            services.AddScoped<IDashboardService, Infrastructure.Services.DashboardService>();

            // Register UI services as singletons
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IViewLocator, ViewLocator>();
            services.AddSingleton<IWindowFactory, WindowFactory>();

            // Register ViewModels as transient
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomerDialogViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();

            // Register Views as transient
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<CustomersView>();
            services.AddTransient<CustomerDialog>();
            services.AddTransient<SettingsView>();

            // Add DatabaseSeeder
            services.AddScoped<DatabaseSeeder>();

            // Build service provider
            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions 
            { 
                ValidateScopes = true,
                ValidateOnBuild = true
            });
        }

        private void InitializeDatabase()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider must be initialized before database");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Ensure database exists and is up to date
                context.Database.EnsureCreated();
                
                // Only try to seed if database was just created
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                seeder.SeedAsync().Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void ShowLoginWindow()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider must be initialized before showing login window");

            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Show();
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
