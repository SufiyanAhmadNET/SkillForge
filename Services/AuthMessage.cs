namespace SkillForge.Services
{
    public enum AuthMessage
    {   
        //status for Register
        None,
        EmptyFields,
        PassNotMatch,
        EmailExist,
        EmailRegisteredAsStudent,
        EmailRegisteredAsInstructor,
        VerifyEmail,
        EmailVerified,
        EmailNotVerified,
        EmailSent,
        EmailNotSent,
        RegisterSuccess,
        RegisterFailed,
        //status for Login
        NewUser,
        WrongPassword,
        LoginFailed,
        LoginSuccess

    }
}
