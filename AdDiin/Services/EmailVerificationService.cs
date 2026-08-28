using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace AdDiin.Services
{
    public interface IEmailVerificationService
    {
        Task<string> GenerateAndSendCodeAsync(string email, string name);
        Task<bool> VerifyCodeAsync(string email, string code);
    }

    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailVerificationService> _logger;
        private readonly IConfiguration _configuration;

        public EmailVerificationService(
            ApplicationDbContext context,
            ILogger<EmailVerificationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GenerateAndSendCodeAsync(string email, string name)
        {
            // Generate 6-digit OTP
            var code = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            // Invalidate previous unused codes
            var oldCodes = await _context.VerificationCodes
                .Where(v => v.Email == email && !v.IsUsed)
                .ToListAsync();

            foreach (var old in oldCodes)
            {
                old.IsUsed = true;
            }

            // Save new verification code
            var verification = new VerificationCode
            {
                Email = email,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.VerificationCodes.Add(verification);
            await _context.SaveChangesAsync();

            // Send email
            await SendEmailAsync(email, name, code);

            // Also log for development/debugging
        

            return code;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var verification = await _context.VerificationCodes
                .Where(v =>
                    v.Email == email &&
                    v.Code == code &&
                    !v.IsUsed &&
                    v.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null)
            {
                return false;
            }

            verification.IsUsed = true;

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task SendEmailAsync(
            string email,
            string name,
            string code)
        {
            var smtpHost = _configuration["EmailSettings:Host"];
            var smtpPortString = _configuration["EmailSettings:Port"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(
                    "Email settings are missing. OTP email was not sent."
                );

                throw new InvalidOperationException(
                    "Email SMTP settings are not configured."
                );
            }

            int smtpPort = 587;

            if (!string.IsNullOrWhiteSpace(smtpPortString) &&
                int.TryParse(smtpPortString, out var parsedPort))
            {
                smtpPort = parsedPort;
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail ?? username,
                fromName ?? "Ad-Diin"
            );

            message.To.Add(new MailAddress(email, name));

            message.Subject = "Ad-Diin Email Verification Code";

            message.IsBodyHtml = true;

            message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>

<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>

    <div style='max-width:600px; margin:auto; background:white;
                padding:30px; border-radius:10px;'>

        <h2 style='text-align:center;'>Ad-Diin</h2>

        <p>Assalamu Alaikum {WebUtility.HtmlEncode(name)},</p>

        <p>
            Thank you for registering with Ad-Diin.
        </p>

        <p>
            Your email verification code is:
        </p>

        <div style='text-align:center; margin:30px 0;'>
            <span style='font-size:32px; font-weight:bold;
                         letter-spacing:8px;'>
                {code}
            </span>
        </div>

        <p>
            This code will expire in <strong>15 minutes</strong>.
        </p>

        <p>
            If you did not request this code, you can safely ignore
            this email.
        </p>

        <hr>

        <p style='font-size:12px; color:#777; text-align:center;'>
            Ad-Diin Islamic Portal
        </p>

    </div>

</body>
</html>";

            using var smtp = new SmtpClient(smtpHost, smtpPort);

            smtp.EnableSsl = true;

            smtp.UseDefaultCredentials = false;

            smtp.Credentials = new NetworkCredential(
                username,
                password
            );

            await smtp.SendMailAsync(message);

            _logger.LogInformation(
                "Verification email successfully sent to {Email}",
                email
            );
        }
    }
}