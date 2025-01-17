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
            try
            {
                // Get the application's base directory
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var dbPath = Path.Combine(baseDirectory, "DTCBillingSystem.db");
                var configPath = Path.Combine(baseDirectory, "appsettings.json");

                // Ensure appsettings.json exists
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Configuration file not found at: {configPath}");
                }

                // Create configuration
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"ConnectionStrings:DefaultConnection", $"Data Source={dbPath}"}
                    })
                    .Build();

                var services = new ServiceCollection();
                ConfigureServices(services);
                serviceProvider = services.BuildServiceProvider();

                // Initialize database
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application initialization error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging(); // For development debugging
                options.EnableDetailedErrors(); // For development debugging
            });

            // Add Core and Infrastructure services
            services.AddCoreServices(Configuration);
            services.AddInfrastructure(Configuration);

            // Add DatabaseSeeder
            services.AddScoped<DatabaseSeeder>();

            // Register UI services
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IDialogService, DialogService>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();

            // Register Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<CustomersView>();
            services.AddTransient<SettingsView>();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

                    // Ensure database is created and migrated
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    // Seed the database
                    seeder.SeedAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var loginWindow = serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing login window: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}
