using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Core;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Repositories;
using System.IO;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services);

            serviceProvider = services.BuildServiceProvider();

            // Ensure database creation and apply migrations
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Register Core Services
            services.AddCoreServices();

            // Register Infrastructure Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            serviceProvider?.Dispose();
        }
    }
}
