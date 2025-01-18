using System.Windows.Controls;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public DashboardView(DashboardViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
} 