using SkillForge.Services.Auth.Models;

namespace SkillForge.Interfaces
{
    // Authentication service interface
    public interface IAuthService
    {
        // Register new user
        AuthResult Register(string email, string password, string confirmPassword, string role, string baseUrl);
        
        // Handle user login
        AuthResult Login(string email, string password, string role);
        
        // Google authentication
        AuthResult GoogleAuth(string email, string firstName, string lastName, string googleId, string picture, string role);
        
        // Change existing password (authenticated flow)
        AuthResult ChangePassword(string email, string currentPassword, string newPassword, string role);
        
        // Reset forgotten password
        AuthResult ResetPassword(string email, string newPassword, string otp, string role);
    }
}
