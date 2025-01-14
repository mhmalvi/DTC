using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Core Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // UI Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService>(sp => 
                new NavigationService(((MainWindow)Current.MainWindow).MainFrame));

            // ViewModels
            services.AddTransient<CustomersViewModel>();
            services.AddTransient<CustomerDialogViewModel>();
            services.AddTransient<CustomerBillsViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<CustomersView>();
            services.AddTransient<CustomerDialog>();
            services.AddTransient<CustomerBillsView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _serviceProvider?.Dispose();
        }
    }
}
