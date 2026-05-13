namespace SkillForge.Interfaces.Common
{
    public interface IEmailService
    {
        void SendOtpEmail(string email, string otp);
    }
}
