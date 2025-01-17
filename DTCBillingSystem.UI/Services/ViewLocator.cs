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
                        // Get the view model from the service provider
                        var viewModel = _serviceProvider.GetRequiredService(viewModelType);
                        Debug.WriteLine("ViewLocator: ViewModel instance created");

                        // Try to find a constructor that takes the view model
                        var constructor = viewType.GetConstructor(new[] { viewModelType });
                        
                        object? view;
                        if (constructor != null)
                        {
                            // Create view with view model parameter
                            view = constructor.Invoke(new[] { viewModel });
                            Debug.WriteLine("ViewLocator: View created with ViewModel parameter");
                        }
                        else
                        {
                            // Create view without parameters
                            view = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
                            Debug.WriteLine("ViewLocator: View created without parameters");
                            
                            // Set DataContext manually
                            if (view is FrameworkElement frameworkElement)
                            {
                                frameworkElement.DataContext = viewModel;
                                Debug.WriteLine("ViewLocator: DataContext set manually");
                            }
                        }

                        if (view == null)
                        {
                            Debug.WriteLine($"ViewLocator ERROR: Failed to create view instance for {viewName}");
                            throw new InvalidOperationException($"Failed to create view instance for {viewName}");
                        }

                        Debug.WriteLine($"ViewLocator: Successfully created view {viewName} with view model");
                        return view;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ViewLocator ERROR: Failed to create view with view model: {ex.Message}");
                        throw new InvalidOperationException($"Failed to create view with view model: {ex.Message}", ex);
                    }
                }
                else
                {
                    // Create view without view model
                    Debug.WriteLine("ViewLocator: No matching ViewModel found, creating view without ViewModel");
                    var view = ActivatorUtilities.CreateInstance(_serviceProvider, viewType);
                    
                    if (view == null)
                    {
                        Debug.WriteLine($"ViewLocator ERROR: Failed to create view instance for {viewName}");
                        throw new InvalidOperationException($"Failed to create view instance for {viewName}");
                    }

                    Debug.WriteLine($"ViewLocator: Successfully created view {viewName} without view model");
                    return view;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ViewLocator ERROR: {ex.Message}");
                throw;
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