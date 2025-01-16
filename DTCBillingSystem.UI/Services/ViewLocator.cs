using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.UI.ViewModels;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

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
                Debug.WriteLine($"ViewLocator: Attempting to create view '{viewName}'");
                
                // Get the executing assembly
                var assembly = Assembly.GetExecutingAssembly();
                Debug.WriteLine($"ViewLocator: Using assembly {assembly.FullName}");
                
                // Find the view type in the current assembly
                var viewType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == viewName);

                if (viewType == null)
                {
                    var availableTypes = string.Join(", ", assembly.GetTypes()
                        .Where(t => t.Name.EndsWith("View"))
                        .Select(t => t.Name));
                    var error = $"View '{viewName}' not found in assembly {assembly.FullName}. Available views: {availableTypes}";
                    Debug.WriteLine($"ViewLocator ERROR: {error}");
                    throw new ArgumentException(error);
                }
                Debug.WriteLine($"ViewLocator: Found view type {viewType.FullName}");

                // Try to get view model if it exists
                var viewModelType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == $"{viewName}ViewModel");
                
                if (viewModelType != null)
                {
                    Debug.WriteLine($"ViewLocator: Found matching ViewModel type {viewModelType.FullName}");
                    try
                    {
                        Debug.WriteLine("ViewLocator: Creating ViewModel instance");
                        var viewModel = _serviceProvider.GetRequiredService(viewModelType);
                        Debug.WriteLine("ViewLocator: ViewModel created successfully");

                        Debug.WriteLine("ViewLocator: Creating View instance");
                        // Try to create view with ViewModel parameter first
                        var constructor = viewType.GetConstructor(new[] { viewModelType });
                        object view;
                        
                        if (constructor != null)
                        {
                            Debug.WriteLine("ViewLocator: Found constructor with ViewModel parameter");
                            view = constructor.Invoke(new[] { viewModel });
                        }
                        else
                        {
                            Debug.WriteLine("ViewLocator: Using DI to create view");
                            view = _serviceProvider.GetRequiredService(viewType);
                        }
                        
                        Debug.WriteLine("ViewLocator: View created successfully");
                        
                        if (view == null)
                        {
                            Debug.WriteLine($"ViewLocator ERROR: View instance is null for {viewName}");
                            throw new InvalidOperationException($"Could not create instance of view {viewName}");
                        }

                        if (view is FrameworkElement frameworkElement && constructor == null)
                        {
                            Debug.WriteLine("ViewLocator: Setting DataContext");
                            frameworkElement.DataContext = viewModel;
                            Debug.WriteLine("ViewLocator: DataContext set successfully");
                        }

                        Debug.WriteLine($"ViewLocator: Successfully created view {viewName} with view model");
                        return view;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ViewLocator ERROR: Failed to create view {viewName} with view model: {ex.Message}\nStack trace: {ex.StackTrace}");
                        throw;
                    }
                }

                // If no view model exists, just create the view
                Debug.WriteLine($"ViewLocator: No ViewModel found for {viewName}, creating view only");
                var viewOnly = _serviceProvider.GetRequiredService(viewType);
                if (viewOnly == null)
                {
                    Debug.WriteLine($"ViewLocator ERROR: View instance is null for {viewName}");
                    throw new InvalidOperationException($"Could not create instance of view {viewName}");
                }

                Debug.WriteLine($"ViewLocator: Successfully created view {viewName} without view model");
                return viewOnly;
            }
            catch (Exception ex)
            {
                var error = $"Failed to create view {viewName}: {ex.Message}";
                Debug.WriteLine($"ViewLocator CRITICAL ERROR: {error}\nStack trace: {ex.StackTrace}");
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