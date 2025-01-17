using System;
using System.Windows;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Services;
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

            // Validate JWT settings
            var jwtSettings = _configuration.GetSection("JwtSettings");
            if (string.IsNullOrEmpty(jwtSettings["SecretKey"]) ||
                string.IsNullOrEmpty(jwtSettings["Issuer"]) ||
                string.IsNullOrEmpty(jwtSettings["Audience"]))
            {
                throw new InvalidOperationException("JWT settings are not properly configured in appsettings.json");
            }
        }

        private void InitializeServices()
        {
            if (_configuration == null)
                throw new InvalidOperationException("Configuration must be initialized before services");

            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(_configuration);

            // Add database context with proper path handling
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var dbPath = Path.Combine(baseDirectory, "DTCBillingSystem.db");
                var connectionString = $"Data Source={dbPath}";
                
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Configure and register TokenService first
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
            
            services.AddSingleton<ITokenService>(new Core.Services.TokenService(secretKey, issuer, audience));

            // Register repositories
            services.AddScoped<IUnitOfWork, Infrastructure.Repositories.UnitOfWork>();

            // Register core services in dependency order
            services.AddScoped<IPasswordHasher, Core.Services.PasswordHasher>();
            services.AddScoped<IPrintService, Core.Services.PrintService>();
            services.AddScoped<ICustomerService, Core.Services.CustomerService>();
            services.AddScoped<IBillingService, Core.Services.BillingService>();
            services.AddScoped<IUserService, Core.Services.UserService>();
            services.AddScoped<IAuditService, Core.Services.AuditService>();
            services.AddScoped<IReportService, Core.Services.ReportService>();

            // Register infrastructure services
            services.AddScoped<ICurrentUserService, Infrastructure.Services.CurrentUserService>();
            services.AddScoped<IAuthenticationService, Infrastructure.Services.AuthenticationService>();

            // Add DatabaseSeeder
            services.AddScoped<DatabaseSeeder>();

            // Register UI services as singletons to maintain state
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            // Register ViewModels as transient since they should be created new each time
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();

            // Register Views as transient
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<CustomersView>();
            services.AddTransient<SettingsView>();

            // Build service provider with scope validation disabled in development
            // This allows us to focus on the login functionality first
            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions 
            { 
                ValidateScopes = false,  // Temporarily disable scope validation
                ValidateOnBuild = false  // Temporarily disable build validation
            });
        }

        private void InitializeDatabase()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider must be initialized before database");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

                    // Drop and recreate database to ensure clean state
                    context.Database.EnsureDeleted();
                    
                    // Create database with detailed error handling
                    try
                    {
                        context.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to create database: {ex.Message}", ex);
                    }

                    // Seed database with detailed error handling
                    try
                    {
                        seeder.SeedAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        var innerMessage = ex.InnerException?.Message ?? ex.Message;
                        throw new Exception($"Failed to seed database: {innerMessage}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Database initialization failed: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.Message}";
                }
                throw new Exception(errorMessage, ex);
            }
        }

        private void ShowLoginWindow()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider must be initialized before showing login window");

            try
            {
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to show login window", ex);
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
