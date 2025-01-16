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
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<T>();
                return CreateViewForViewModel(viewModel);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create view for {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public object GetView(Type viewModelType)
        {
            try
            {
                var viewModel = _serviceProvider.GetRequiredService(viewModelType);
                return CreateViewForViewModel(viewModel);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create view for {viewModelType.Name}: {ex.Message}", ex);
            }
        }

        public object GetViewByName(string viewName)
        {
            try
            {
                var viewType = Type.GetType($"DTCBillingSystem.UI.Views.{viewName}");
                if (viewType == null)
                    throw new ArgumentException($"View {viewName} not found");

                // Try to get view model if it exists
                var viewModelTypeName = $"DTCBillingSystem.UI.ViewModels.{viewName}ViewModel";
                var viewModelType = Type.GetType(viewModelTypeName);
                
                if (viewModelType != null)
                {
                    try
                    {
                        var viewModel = _serviceProvider.GetService(viewModelType);
                        if (viewModel != null)
                        {
                            var view = Activator.CreateInstance(viewType);
                            if (view == null)
                                throw new InvalidOperationException($"Could not create instance of view {viewName}");

                            if (view is FrameworkElement frameworkElement)
                            {
                                frameworkElement.DataContext = viewModel;
                            }

                            return view;
                        }
                    }
                    catch
                    {
                        // If getting the view model fails, fall back to creating just the view
                    }
                }

                // Create view without view model
                var fallbackView = Activator.CreateInstance(viewType);
                if (fallbackView == null)
                    throw new InvalidOperationException($"Could not create instance of view {viewName}");

                return fallbackView;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create view {viewName}: {ex.Message}", ex);
            }
        }

        public LoginWindow CreateLoginWindow()
        {
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                return new LoginWindow(viewModel);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create login window", ex);
            }
        }

        public MainWindow CreateMainWindow()
        {
            try
            {
                var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
                var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
                var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                return new MainWindow(navigationService, dialogService, viewModel);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create main window", ex);
            }
        }

        private object CreateViewForViewModel(object viewModel)
        {
            var viewModelType = viewModel.GetType();
            var viewTypeName = viewModelType.Name.Replace("ViewModel", "");
            var viewType = Type.GetType($"DTCBillingSystem.UI.Views.{viewTypeName}");

            if (viewType == null)
                throw new InvalidOperationException($"View not found for {viewModelType.Name}");

            var view = Activator.CreateInstance(viewType);
            if (view == null)
                throw new InvalidOperationException($"Could not create instance of view {viewType.Name}");

            if (view is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = viewModel;
            }

            return view;
        }
    }
} 