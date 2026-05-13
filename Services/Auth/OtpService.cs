using SkillForge.Data;
using SkillForge.Models;
using SkillForge.Interfaces.Auth;
using SkillForge.Interfaces.Common;
using SkillForge.Services.Auth.Models;

namespace SkillForge.Services.Auth
{
    // OTP generation and verification service
    public class OtpService : IOtpService
    {
        private readonly SkillForgeDbContext _context;
        private readonly IEmailService _emailService;
        public OtpService(SkillForgeDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Generate and send OTP via email
        public AuthResult SendEmailOtp(string Email, string Role)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return new AuthResult { Success = false, status = AuthMessage.EmptyFields };
            Email = Email.Trim().ToLower();
            var otp = new Random().Next(100000, 999999).ToString();
            
            // Handle student OTP
            if (Role == "Student")
            {
                var student = _context.Students.FirstOrDefault(s => s.Email == Email);
                if (student == null)
                    return new AuthResult { Success = false, status = AuthMessage.NewUser };
                student.EmailOtp = otp;
                student.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
                _context.SaveChanges();
                try
                {
                    _emailService.SendOtpEmail(Email, otp);
                    return new AuthResult { Success = true, status = AuthMessage.VerifyEmail, Email = Email };
                }
                catch
                {
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotSent };
                }
            }
            
            // Handle instructor OTP
            if (Role == "Instructor")
            {
                var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);
                if (instructor == null)
                    return new AuthResult { Success = false, status = AuthMessage.NewUser };
                instructor.EmailOtp = otp;
                instructor.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
                _context.SaveChanges();
                try
                {
                    _emailService.SendOtpEmail(Email, otp);
                    return new AuthResult { Success = true, status = AuthMessage.VerifyEmail, Email = Email };
                }
                catch
                {
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotSent };
                }
            }
            return new AuthResult { Success = false, status = AuthMessage.NewUser };
        }

        // Verify OTP for email confirmation
        public AuthResult VerifyEmailOtp(string Email, string Otp)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Otp))
                return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };
            Email = Email.Trim().ToLower();
            Otp = Otp.Trim();
            
            // Verify student OTP
            var student = _context.Students.FirstOrDefault(s => s.Email == Email);
            if (student != null)
            {
                if (student.IsEmailVerified)
                    return new AuthResult { Success = false, status = AuthMessage.EmailVerified };
                if (student.EmailOtp == null || student.EmailOtp.Trim() != Otp)
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };
                if (student.OtpExpiry == null || student.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };

                student.IsEmailVerified = true;
                student.EmailOtp = null;
                student.OtpExpiry = null;
                _context.SaveChanges();
                return new AuthResult { Success = true, status = AuthMessage.EmailVerified };
            }
            
            // Verify instructor OTP
            var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);
            if (instructor != null)
            {
                if (instructor.IsEmailVerified)
                    return new AuthResult { Success = false, status = AuthMessage.EmailVerified };
                if (instructor.EmailOtp == null || instructor.EmailOtp.Trim() != Otp)
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };
                if (instructor.OtpExpiry == null || instructor.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };

                instructor.IsEmailVerified = true;
                instructor.EmailOtp = null;
                instructor.OtpExpiry = null;
                _context.SaveChanges();
                return new AuthResult { Success = true, status = AuthMessage.EmailVerified };
            }
            
            return new AuthResult { Success = false, status = AuthMessage.EmailNotVerified };
        }

        // Verify OTP for security operations (password reset, etc.)
        public AuthResult VerifySecurityOtp(string Email, string Otp, bool shouldClear = true)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Otp))
                return new AuthResult { Success = false, status = AuthMessage.InvalidOtp };
            
            Email = Email.Trim().ToLower();
            Otp = Otp.Trim();
            
            // Student security check
            var student = _context.Students.FirstOrDefault(s => s.Email == Email);
            if (student != null)
            {
                if (student.EmailOtp == null || student.EmailOtp.Trim() != Otp)
                    return new AuthResult { Success = false, status = AuthMessage.InvalidOtp };
                if (student.OtpExpiry == null || student.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { Success = false, status = AuthMessage.OtpExpired };

                if (shouldClear)
                {
                    student.EmailOtp = null;
                    student.OtpExpiry = null;
                    _context.SaveChanges();
                }
                return new AuthResult { Success = true, status = AuthMessage.OtpVerified };
            }
            
            // Instructor security check
            var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);
            if (instructor != null)
            {
                if (instructor.EmailOtp == null || instructor.EmailOtp.Trim() != Otp)
                    return new AuthResult { Success = false, status = AuthMessage.InvalidOtp };
                if (instructor.OtpExpiry == null || instructor.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { Success = false, status = AuthMessage.OtpExpired };

                if (shouldClear)
                {
                    instructor.EmailOtp = null;
                    instructor.OtpExpiry = null;
                    _context.SaveChanges();
                }
                return new AuthResult { Success = true, status = AuthMessage.OtpVerified };
            }
            
            return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
        }
    }
}
