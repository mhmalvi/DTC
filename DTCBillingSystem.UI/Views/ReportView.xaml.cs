using System;
using System.Windows.Controls;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class ReportView : UserControl
    {
        private readonly ReportViewModel _viewModel;

        public ReportView(ReportViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                InitializeComponent();
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error initializing report view: {ex.Message}",
                    "Initialization Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                throw;
            }
        }
    }
} 