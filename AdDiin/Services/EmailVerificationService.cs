using AdDiin.Data;
using AdDiin.Models.Entities;
using Microsoft.EntityFrameworkCore;
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

        public EmailVerificationService(ApplicationDbContext context, ILogger<EmailVerificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateAndSendCodeAsync(string email, string name)
        {
            // Generate 6-digit random code
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // Invalidate existing codes
            var oldCodes = await _context.VerificationCodes
                .Where(v => v.Email == email && !v.IsUsed)
                .ToListAsync();

            foreach (var old in oldCodes)
            {
                old.IsUsed = true;
            }

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

            _logger.LogInformation("=================================================");
            _logger.LogInformation("EMAIL VERIFICATION CODE for {Email} ({Name}): {Code}", email, name, code);
            _logger.LogInformation("=================================================");

            return code;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var verification = await _context.VerificationCodes
                .Where(v => v.Email == email && v.Code == code && !v.IsUsed && v.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null) return false;

            verification.IsUsed = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
