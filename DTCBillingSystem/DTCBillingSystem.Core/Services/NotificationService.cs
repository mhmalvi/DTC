using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ISMSService smsService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task<NotificationMessage> CreateNotificationAsync(NotificationMessage message, int userId)
        {
            message.Status = NotificationStatus.Pending;
            message.CreatedAt = DateTime.UtcNow;
            message.CreatedBy = userId.ToString();

            await _unitOfWork.NotificationMessages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            // Send notification based on type
            try
            {
                switch (message.Type)
                {
                    case NotificationType.Email:
                        await _emailService.SendEmailAsync(message.Recipient, message.Title, message.Content);
                        message.Status = NotificationStatus.Sent;
                        break;

                    case NotificationType.SMS:
                        await _smsService.SendSMSAsync(message.Recipient, message.Content);
                        message.Status = NotificationStatus.Sent;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported notification type: {message.Type}");
                }

                message.SentAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                message.Status = NotificationStatus.Failed;
                message.ErrorMessage = ex.Message;
                await _unitOfWork.SaveChangesAsync();
                throw;
            }

            return message;
        }
    }
} 