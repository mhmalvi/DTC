using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.Infrastructure;
using DTCBillingSystem.Core;
using System.Threading.Tasks;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        private IConfiguration Configuration { get; set; }

        public App()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
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
                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        MessageBox.Show("Ensuring database is created...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        await dbContext.Database.EnsureCreatedAsync();
                        MessageBox.Show("Database created successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Create default admin user if none exists
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        await CreateDefaultAdminUserAsync(userService);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database initialization error: {ex.Message}\n\nDetails: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        throw;
                    }
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
            // Add Infrastructure services
            services.AddInfrastructure(Configuration);

            // Add Core services
            services.AddCoreServices(Configuration);

            // Register UI services
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
        }

        private async Task CreateDefaultAdminUserAsync(IUserService userService)
        {
            try
            {
                var adminExists = await userService.GetUserByUsernameAsync("admin");
                if (adminExists == null)
                {
                    await userService.RegisterUserAsync("admin", "admin@dtc.com", "Admin@123", Core.Models.Enums.UserRole.Administrator);
                    MessageBox.Show("Default admin user created successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create default admin user: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
