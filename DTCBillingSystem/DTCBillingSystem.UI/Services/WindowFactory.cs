using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;

namespace DTCBillingSystem.UI.Services
{
    public interface IWindowFactory
    {
        Window CreateLoginWindow();
        Window CreateMainWindow();
    }

    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WindowFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Window CreateLoginWindow()
        {
            var scope = _serviceScopeFactory.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<LoginWindow>();
            
            // Dispose the scope when the window is closed
            window.Closed += (s, e) => scope.Dispose();
            
            return window;
        }

        public Window CreateMainWindow()
        {
            var scope = _serviceScopeFactory.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<MainWindow>();
            
            // Dispose the scope when the window is closed
            window.Closed += (s, e) => scope.Dispose();
            
            return window;
        }
    }
} 