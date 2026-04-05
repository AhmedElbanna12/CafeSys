using Foodics.Dtos.Auth;
using Foodics.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.RateLimiting;

namespace Foodics.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;
        private static readonly RateLimiter _rateLimiter = new FixedWindowRateLimiter(
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(_settings.Email))
                throw new InvalidOperationException("Email settings are not configured.");

            // تحقق من الـ Rate Limit قبل الإرسال
            using var lease = await _rateLimiter.AcquireAsync(permitCount: 1);
            if (!lease.IsAcquired)
            {
                _logger.LogWarning("Rate limit exceeded. Email to {To} was rejected.", to);
                throw new InvalidOperationException("Too many email requests. Please try again later.");
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_settings.Email));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.Email, _settings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SMTP Error | Host: {Host} | Port: {Port} | From: {Email} | To: {To} | Message: {Message} | Inner: {Inner}",
                    _settings.Host,
                    _settings.Port,
                    _settings.Email,
                    to,
                    ex.Message,
                    ex.InnerException?.Message ?? "none");

                throw new Exception(
                    $"SMTP Error: {ex.Message} | Inner: {ex.InnerException?.Message ?? "none"}");
            }
        }
    }
}