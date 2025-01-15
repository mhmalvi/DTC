using System.Windows.Controls;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class CustomersView : UserControl
    {
        public CustomersView(CustomersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 