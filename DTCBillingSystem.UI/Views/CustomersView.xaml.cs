using System.Windows.Controls;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class CustomersView : UserControl
    {
        public CustomersView()
        {
            InitializeComponent();
        }

        public CustomersView(CustomersViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
} 