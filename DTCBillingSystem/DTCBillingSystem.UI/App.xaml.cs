using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=dtcbilling.db"));

            // Register services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<INavigationService, NavigationService>();
            services.AddScoped<IViewLocator, ViewLocator>();

            // Register ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            // Initialize database
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            // Show login window
            var viewLocator = _serviceProvider.GetRequiredService<IViewLocator>();
            var loginWindow = viewLocator.CreateLoginWindow();
            loginWindow.Show();
        }
    }
}
