using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _from;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["SMS:ApiKey"] ?? throw new ArgumentNullException("SMS:ApiKey is not configured");
            _apiSecret = _configuration["SMS:ApiSecret"] ?? throw new ArgumentNullException("SMS:ApiSecret is not configured");
            _from = _configuration["SMS:FromNumber"] ?? throw new ArgumentNullException("SMS:FromNumber is not configured");
        }

        public async Task SendSMSAsync(NotificationMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
            {
                throw new ArgumentException("Recipient phone number is required", nameof(message));
            }

            try
            {
                // Here you would typically integrate with an SMS provider like Twilio, MessageBird, etc.
                // For now, we'll just log the message
                await LogSMSAsync(message);

                // Example of how you might implement with Twilio:
                /*
                var client = new TwilioRestClient(_apiKey, _apiSecret);
                var result = await client.SendMessageAsync(
                    to: message.To,
                    from: _from,
                    body: message.Body
                );
                
                if (result.Status != MessageStatus.Sent && result.Status != MessageStatus.Queued)
                {
                    throw new Exception($"Failed to send SMS: {result.Status} - {result.ErrorMessage}");
                }
                */
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send SMS to {message.To}: {ex.Message}", ex);
            }
        }

        public async Task SendBulkSMSAsync(IEnumerable<NotificationMessage> messages)
        {
            if (messages == null || !messages.Any())
            {
                throw new ArgumentException("No messages provided for bulk sending", nameof(messages));
            }

            // Process messages in parallel with a reasonable degree of parallelism
            var tasks = messages.Select(message => SendSMSAsync(message));
            await Task.WhenAll(tasks);
        }

        private async Task LogSMSAsync(NotificationMessage message)
        {
            // In a real implementation, you might want to log to a database or monitoring service
            await Task.Run(() =>
            {
                Console.WriteLine($"SMS Log: To: {message.To}, Message: {message.Body}");
            });
        }
    }
} 