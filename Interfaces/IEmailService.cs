namespace SkillForge.Interfaces
{
    public interface IEmailService
    {
        void SendOtpEmail(string email, string otp);
    }
}
