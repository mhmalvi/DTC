using System;
using System.ComponentModel;
using System.Windows;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.UI.Views
{
    public partial class MainWindow : ScopedWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public MainWindow(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            try
            {
                InitializeComponent();

                // Get services from the scope
                _viewModel = ServiceScope.ServiceProvider.GetRequiredService<MainViewModel>();
                _navigationService = ServiceScope.ServiceProvider.GetRequiredService<INavigationService>();
                _dialogService = ServiceScope.ServiceProvider.GetRequiredService<IDialogService>();

                if (MainFrame == null)
                {
                    throw new InvalidOperationException("MainFrame not found in XAML");
                }

                // Initialize navigation service with the main frame
                _navigationService.Initialize(MainFrame, this);

                // Set DataContext after initialization
                DataContext = _viewModel;

                // Subscribe to window events
                ContentRendered += MainWindow_ContentRendered;
                Closing += MainWindow_Closing;

                // Navigate to dashboard after initialization
                _navigationService.NavigateToDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing MainWindow: {ex.Message}\n\nDetails: {ex}",
                              "Initialization Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            try
            {
                _viewModel.OnContentRendered();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during content rendering: {ex.Message}",
                              "Rendering Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                _viewModel.OnClosing();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during window closing: {ex.Message}",
                              "Closing Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
} 