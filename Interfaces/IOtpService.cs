using SkillForge.Services.Auth.Models;

namespace SkillForge.Interfaces
{
    public interface IOtpService
    {
        AuthResult SendEmailOtp(string email, string role);
        AuthResult VerifyEmailOtp(string email, string otp);
        AuthResult VerifySecurityOtp(string email, string otp, bool shouldClear = true);
    }
}
