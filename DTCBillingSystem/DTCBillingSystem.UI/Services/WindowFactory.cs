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
        UI.MainWindow CreateMainWindow();
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
            var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            var window = new LoginWindow(viewModel);
            return window;
        }

        public UI.MainWindow CreateMainWindow()
        {
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            var window = new UI.MainWindow(navigationService, mainViewModel);
            return window;
        }
    }
} 