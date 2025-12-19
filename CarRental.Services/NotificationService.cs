using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace CarRental.Services
{
    public interface INotificationService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly bool _isConfigured;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Check if SMTP is configured
            _isConfigured = !string.IsNullOrEmpty(_configuration["Smtp:Host"]);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@carrental.com";
            var fromName = _configuration["Smtp:FromName"] ?? "Car Rental System";
            
            emailMessage.From.Add(new MailboxAddress(fromName, fromEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };

            if (_isConfigured)
            {
                // Real SMTP sending
                try
                {
                    using var client = new SmtpClient();
                    
                    var host = _configuration["Smtp:Host"]!;
                    var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                    var username = _configuration["Smtp:Username"];
                    var password = _configuration["Smtp:Password"];
                    var useSsl = bool.Parse(_configuration["Smtp:UseSsl"] ?? "false");

                    await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                    
                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(username, password);
                    }
                    
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    
                    Console.WriteLine($"[Email Sent] To: {toEmail}, Subject: {subject}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email send failed: {ex.Message}");
                    // Log but don't throw - email failure shouldn't break the rental process
                }
            }
            else
            {
                // Mock mode - just log
                await Task.Delay(50); // Simulate network delay
                Console.WriteLine($"[Mock Email] To: {toEmail}");
                Console.WriteLine($"  Subject: {subject}");
                Console.WriteLine($"  Body Preview: {body.Substring(0, Math.Min(100, body.Length))}...");
                System.Diagnostics.Debug.WriteLine($"[Mock Email Sent] To: {toEmail}, Subject: {subject}");
            }
        }
    }
}

