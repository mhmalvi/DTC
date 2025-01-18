using System.Threading.Tasks;
using System.Windows;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No");
        Task ShowErrorAsync(string title, string message);
        Task ShowInformationAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
        Task<bool?> ShowDialogAsync(Window dialog);
        Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill);
        Task ShowBillDetailsAsync(MonthlyBill bill);
    }
} 