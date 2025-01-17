using System;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.UI.Services
{
    public interface IWindowFactory
    {
        LoginWindow CreateLoginWindow();
        MainWindow CreateMainWindow();
    }

    public class WindowFactory : IWindowFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public LoginWindow CreateLoginWindow()
        {
            return new LoginWindow(_serviceProvider);
        }

        public MainWindow CreateMainWindow()
        {
            return new MainWindow(_serviceProvider);
        }
    }
} 