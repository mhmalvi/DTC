using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.Infrastructure;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // Register core services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            
            services.AddInfrastructure();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDialogService, DialogService>();
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomerBillsViewModel>();
            services.AddTransient<BillGenerationViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<CustomersView>();
            services.AddTransient<CustomerBillsView>();
            services.AddTransient<BillGenerationView>();
            services.AddTransient<SettingsView>();

            // Register MainWindow
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start: {ex.Message}\n\nDetails: {ex}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
