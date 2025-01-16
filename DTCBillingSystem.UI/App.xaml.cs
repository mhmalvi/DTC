using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
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
using System.Diagnostics;
using MaterialDesignThemes.Wpf;

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
                Debug.WriteLine("Initializing application...");

                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    Debug.WriteLine($"Attempting to resolve assembly: {args.Name}");
                    return null;
                };

                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    Debug.WriteLine($"Unhandled exception: {args.ExceptionObject}");
                };

                _configuration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true)
                    .Build();

                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                Debug.WriteLine("Application initialization completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during application initialization: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show($"Error during startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                Debug.WriteLine("Configuring services...");

                // Add configuration
                services.AddSingleton<IConfiguration>(_configuration!);

                // Add Core services
                services.AddSingleton<IPasswordHasher, Core.Services.PasswordHasher>();
                services.AddSingleton<ITokenService>(provider =>
                {
                    var secretKey = _configuration!["JwtSettings:SecretKey"] ?? "your-secret-key-here";
                    var issuer = _configuration["JwtSettings:Issuer"] ?? "dtcbilling";
                    var audience = _configuration["JwtSettings:Audience"] ?? "dtcbilling-clients";
                    var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
                    return new Core.Services.TokenService(secretKey, issuer, audience, expirationMinutes);
                });
                services.AddSingleton<IUserService, Core.Services.UserService>();

                // Add DbContext
                services.AddDbContext<ApplicationDbContext>((provider, options) =>
                {
                    options.UseSqlite(_configuration!.GetConnectionString("DefaultConnection") ?? "Data Source=dtcbilling.db");
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
                services.AddScoped<IWindowFactory, WindowFactory>();

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
                services.AddTransient<MainWindow>();
                services.AddTransient<LoginWindow>();
                services.AddScoped<DashboardView>();
                services.AddTransient<CustomerView>();
                services.AddTransient<BillView>();
                services.AddTransient<PaymentView>();
                services.AddTransient<ReportView>();
                services.AddTransient<SettingsView>();

                Debug.WriteLine("Services configured successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error configuring services: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw;
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                Debug.WriteLine("Starting application...");

                // Initialize MaterialDesign
                var paletteHelper = new PaletteHelper();
                ITheme theme = paletteHelper.GetTheme();
                theme.SetBaseTheme(Theme.Light);
                theme.SetPrimaryColor(Color.FromRgb(103, 58, 183));  // DeepPurple
                theme.SetSecondaryColor(Color.FromRgb(205, 220, 57));  // Lime
                paletteHelper.SetTheme(theme);

                base.OnStartup(e);

                // Initialize database
                var dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.EnsureCreatedAsync();

                // Show login window
                var loginWindow = new LoginWindow(
                    _serviceProvider.GetRequiredService<LoginViewModel>());

                Debug.WriteLine("Showing login window...");
                loginWindow.Show();
                Debug.WriteLine("Login window shown successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during startup: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show($"Error during startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}
