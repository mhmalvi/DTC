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
            var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            var window = new LoginWindow(viewModel);
            return window;
        }

        public MainWindow CreateMainWindow()
        {
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            var window = new MainWindow(navigationService, dialogService, mainViewModel);
            return window;
        }
    }
} 