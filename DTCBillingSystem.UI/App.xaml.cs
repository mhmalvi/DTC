using System;
using System.Windows;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using DTCBillingSystem.Core;
using DTCBillingSystem.Infrastructure;
using DTCBillingSystem.UI.Services;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        private IConfiguration Configuration;

        public App()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add Core and Infrastructure services
            services.AddCoreServices(Configuration);
            services.AddInfrastructure(Configuration);

            // Add DatabaseSeeder
            services.AddTransient<DatabaseSeeder>();

            // Add UI Services
            services.AddSingleton<IViewLocator, ViewLocator>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowFactory, WindowFactory>();

            // Add ViewModels
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();

            // Add Views
            services.AddTransient<DashboardView>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Get the DbContext and ensure database is created
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await dbContext.Database.EnsureCreatedAsync();

                    // Seed the database
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                    await seeder.SeedAsync();
                }

                // Show login window
                var loginWindow = serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}
