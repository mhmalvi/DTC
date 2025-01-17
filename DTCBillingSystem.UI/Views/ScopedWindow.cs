using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.UI.Views
{
    public class ScopedWindow : Window, IDisposable
    {
        protected readonly IServiceScope ServiceScope;

        public ScopedWindow(IServiceProvider serviceProvider)
        {
            ServiceScope = serviceProvider.CreateScope();
        }

        public void Dispose()
        {
            ServiceScope?.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }
    }
} 