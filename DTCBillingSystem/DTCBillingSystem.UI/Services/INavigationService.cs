using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DTCBillingSystem.UI.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Navigate to a view associated with the specified view model type
        /// </summary>
        void NavigateTo<T>() where T : class;

        /// <summary>
        /// Navigate to a view associated with the specified view model type
        /// </summary>
        void NavigateTo(Type viewModelType);

        /// <summary>
        /// Navigate to a view by its name asynchronously
        /// </summary>
        void NavigateToAsync(string viewName);

        /// <summary>
        /// Navigate back to the previous view
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Navigate to the main window
        /// </summary>
        void NavigateToMain();

        /// <summary>
        /// Navigate to the main window asynchronously
        /// </summary>
        Task NavigateToMainWindow();

        /// <summary>
        /// Whether navigation back is possible
        /// </summary>
        bool CanNavigateBack { get; }

        /// <summary>
        /// Initialize the navigation service with a frame and window
        /// </summary>
        void Initialize(Frame mainFrame, Window mainWindow);

        /// <summary>
        /// Set the navigation frame
        /// </summary>
        void SetFrame(Frame frame);
    }
} 