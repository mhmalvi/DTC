using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Linq;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(NotificationMessage message)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress ?? "noreply@dtcbilling.com"),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };
            
            mailMessage.To.Add(message.To);

            // Add CC recipients
            if (message.CC?.Any() == true)
            {
                foreach (var cc in message.CC)
                {
                    mailMessage.CC.Add(cc);
                }
            }

            // Add BCC recipients
            if (message.BCC?.Any() == true)
            {
                foreach (var bcc in message.BCC)
                {
                    mailMessage.Bcc.Add(bcc);
                }
            }

            // Add attachments
            if (message.Attachments?.Any() == true)
            {
                foreach (var attachmentPath in message.Attachments)
                {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }
            }

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendBulkEmailAsync(IEnumerable<NotificationMessage> messages)
        {
            // Process messages in parallel for better performance
            var tasks = messages.Select(message => SendEmailAsync(message));
            await Task.WhenAll(tasks);
        }
    }
} 