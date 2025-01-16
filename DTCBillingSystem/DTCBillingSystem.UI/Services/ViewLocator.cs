using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Services
{
    public interface IViewLocator
    {
        object GetView<T>() where T : class;
        object GetView(Type viewModelType);
        object GetViewByName(string viewName);
        LoginWindow CreateLoginWindow();
        MainWindow CreateMainWindow();
    }

    public class ViewLocator : IViewLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetView<T>() where T : class
        {
            var viewModel = _serviceProvider.GetRequiredService<T>();
            return CreateViewForViewModel(viewModel);
        }

        public object GetView(Type viewModelType)
        {
            var viewModel = _serviceProvider.GetRequiredService(viewModelType);
            return CreateViewForViewModel(viewModel);
        }

        public object GetViewByName(string viewName)
        {
            var viewType = Type.GetType($"DTCBillingSystem.UI.Views.{viewName}");
            if (viewType == null)
                throw new ArgumentException($"View {viewName} not found");

            var view = Activator.CreateInstance(viewType);
            if (view == null)
                throw new InvalidOperationException($"Could not create instance of view {viewName}");

            return view;
        }

        public LoginWindow CreateLoginWindow()
        {
            var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            return new LoginWindow(viewModel);
        }

        public MainWindow CreateMainWindow()
        {
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            return new MainWindow(navigationService, dialogService, viewModel);
        }

        private object CreateViewForViewModel(object viewModel)
        {
            var viewModelType = viewModel.GetType();
            var viewName = viewModelType.Name.Replace("ViewModel", "View");
            var view = GetViewByName(viewName);
            
            if (view is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = viewModel;
            }

            return view;
        }
    }
} 