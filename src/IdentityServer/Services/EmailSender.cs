using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly SendGridSettings _sendgridSettings;

        public EmailSender(ILogger<EmailSender> logger, IOptions<SendGridSettings> sendgridSettings)
        {
            this._logger = logger;
            this._sendgridSettings = sendgridSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //var client = new SendGridClient(_sendgridSettings.ApiKey);
            //string fromEmail = _sendgridSettings.FromEmail;
            //string fromName = _sendgridSettings.FromName;

            //var msg = new SendGridMessage()
            //{
            //    From = new EmailAddress(fromEmail, fromName),
            //    Subject = subject,
            //    PlainTextContent = htmlMessage,
            //    HtmlContent = htmlMessage
            //};
            //msg.AddTo(new EmailAddress(email));

            ////See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            //msg.SetClickTracking(false, false);

            //return client.SendEmailAsync(msg);

            System.IO.File.WriteAllText($"EmailSender-{DateTime.Now}.txt", subject + Environment.NewLine + htmlMessage);

            return Task.Run(() => { });
        }
    }
}
