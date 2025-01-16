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
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        public App()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add configuration
            services.AddSingleton<IConfiguration>(_configuration);

            // Add Core services
            services.AddSingleton<IPasswordHasher, Core.Services.PasswordHasher>();
            services.AddSingleton<ITokenService>(provider =>
            {
                var secretKey = _configuration["JwtSettings:SecretKey"] ?? "your-secret-key-here";
                var issuer = _configuration["JwtSettings:Issuer"] ?? "dtcbilling";
                var audience = _configuration["JwtSettings:Audience"] ?? "dtcbilling-clients";
                var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
                return new Core.Services.TokenService(secretKey, issuer, audience, expirationMinutes);
            });
            services.AddSingleton<IUserService, Core.Services.UserService>();

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                options.UseSqlite(_configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dtcbilling.db");
            });
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<ApplicationDbContext>((provider) =>
            {
                var options = provider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
                var passwordHasher = provider.GetRequiredService<IPasswordHasher>();
                return new ApplicationDbContext(options, passwordHasher);
            });

            // Add Infrastructure services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthenticationService, Infrastructure.Services.AuthenticationService>();
            services.AddScoped<ICustomerService, Infrastructure.Services.CustomerService>();
            services.AddScoped<IBillingService, Infrastructure.Services.BillingService>();
            services.AddScoped<IPaymentService, Infrastructure.Services.PaymentService>();
            services.AddScoped<IBackupService, Infrastructure.Services.BackupService>();
            services.AddScoped<IAuditService, Infrastructure.Services.AuditService>();
            services.AddScoped<IPrintService, Infrastructure.Services.PrintService>();
            services.AddScoped<IMeterReadingService, Infrastructure.Services.MeterReadingService>();

            // Add UI services
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IViewLocator, ViewLocator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDialogService, DialogService>();

            // Register ViewModels
            services.AddScoped<LoginViewModel>();
            services.AddScoped<MainViewModel>();
            services.AddScoped<DashboardViewModel>();
            services.AddScoped<CustomerViewModel>();
            services.AddScoped<BillViewModel>();
            services.AddScoped<PaymentViewModel>();
            services.AddScoped<ReportViewModel>();
            services.AddScoped<SettingsViewModel>();

            // Register Views
            services.AddTransient<DashboardView>();
            services.AddTransient<CustomerView>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.EnsureCreatedAsync();

                var loginWindow = new LoginWindow(
                    _serviceProvider.GetRequiredService<LoginViewModel>());
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}
