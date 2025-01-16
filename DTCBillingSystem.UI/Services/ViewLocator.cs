using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using System.Reflection;
using System.Linq;

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
                // Get the executing assembly
                var assembly = Assembly.GetExecutingAssembly();
                
                // Find the view type in the current assembly
                var viewType = assembly.GetTypes()
                    .FirstOrDefault(t => t.FullName == $"DTCBillingSystem.UI.Views.{viewName}");

                if (viewType == null)
                {
                    var error = $"View {viewName} not found in assembly {assembly.FullName}";
                    System.Diagnostics.Debug.WriteLine(error);
                    throw new ArgumentException(error);
                }

                // Try to get view model if it exists
                var viewModelType = assembly.GetTypes()
                    .FirstOrDefault(t => t.FullName == $"DTCBillingSystem.UI.ViewModels.{viewName}ViewModel");
                
                if (viewModelType != null)
                {
                    try
                    {
                        var viewModel = _serviceProvider.GetRequiredService(viewModelType);
                        var view = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
                        
                        if (view == null)
                            throw new InvalidOperationException($"Could not create instance of view {viewName}");

                        if (view is FrameworkElement frameworkElement)
                        {
                            frameworkElement.DataContext = viewModel;
                        }

                        System.Diagnostics.Debug.WriteLine($"Successfully created view {viewName} with view model");
                        return view;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating view {viewName} with view model: {ex.Message}");
                        // Continue to fallback
                    }
                }

                // Fallback to creating view without view model
                System.Diagnostics.Debug.WriteLine($"Creating view {viewName} without view model");
                var fallbackView = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
                if (fallbackView == null)
                    throw new InvalidOperationException($"Could not create instance of view {viewName}");

                return fallbackView;
            }
            catch (Exception ex)
            {
                var error = $"Failed to create view {viewName}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(error);
                throw new InvalidOperationException(error, ex);
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