using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? PhotoPath { get; set; }
    public string? EmailOtp { get; set; }
    public DateTime? OtpExpiry { get; set; }

}