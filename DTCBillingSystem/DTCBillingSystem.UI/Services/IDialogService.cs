using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task ShowInformationAsync(string title, string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
        Task ShowInfoAsync(string title, string message);
        Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill);
        Task ShowBillDetailsAsync(MonthlyBill bill);
    }
} 