using System.Net;
using System.Net.Mail;
using FormPlay.Models;
using Microsoft.Extensions.Options;

namespace FormPlay.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendTpsReportNotificationAsync(TpsReport report, User recipient, string action)
        {
            try
            {
                // For development/testing, just log the email
                _logger.LogInformation($"Email would be sent to {recipient.Email} about TPS Report {report.Id} - {action}");
                
                // In a production environment, uncomment this code to actually send emails
                /*
                var emailSettings = _configuration.GetSection("Email");
                
                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["Port"]),
                    Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"]),
                    EnableSsl = bool.Parse(emailSettings["UseSsl"])
                };

                var from = new MailAddress(emailSettings["FromEmail"], emailSettings["FromName"]);
                var to = new MailAddress(recipient.Email, recipient.Name);

                var mailMessage = new MailMessage(from, to)
                {
                    Subject = $"FormPlay - TPS Report #{report.Id} {action}",
                    Body = GetEmailBody(report, recipient, action),
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email notification: {ex.Message}");
                // Don't rethrow - we don't want email failures to break the application flow
            }
        }

        private string GetEmailBody(TpsReport report, User recipient, string action)
        {
            var initiator = report.InitiatedBy;
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:8000";
            
            string body = $@"
            <html>
            <body>
                <h2>FormPlay TPS Report Notification</h2>
                <p>Hello {recipient.Name},</p>
            ";

            switch (action)
            {
                case "Created":
                    body += $@"
                        <p>{initiator.Name} has created a new TPS Report for your review.</p>
                        <p>Please review the details and respond at your earliest convenience.</p>
                    ";
                    break;
                case "Reviewed":
                    body += $@"
                        <p>{recipient.Name} has reviewed your TPS Report and provided feedback.</p>
                        <p>Please check the report to see their response.</p>
                    ";
                    break;
                case "Approved":
                    body += $@"
                        <p>Great news! Your TPS Report has been approved!</p>
                        <p>The event has been added to both of your calendars.</p>
                    ";
                    break;
                case "Denied":
                    body += $@"
                        <p>{recipient.Name} has indicated they're not interested in this TPS Report.</p>
                        <p>Don't worry - there's always next time. Feel free to create a new report when you're ready.</p>
                    ";
                    break;
            }

            body += $@"
                <p><a href='{baseUrl}/TpsReport/Details/{report.Id}'>View TPS Report</a></p>
                <p>Thank you for using FormPlay!</p>
            </body>
            </html>
            ";

            return body;
        }
    }
}
