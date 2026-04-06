using Microsoft.AspNetCore.Mvc;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Models;
using System.Runtime.Intrinsics.X86;
namespace SkillForge.Services
{
    public class AuthService
    {
        private SkillForgeDbContext _context;
        private EmailService _emailService;

        //Constructor
        //inject db context and emailservice
        public AuthService(SkillForgeDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        //#######################
        //#######################
        //Registration Method
        public AuthResult Register(string Email, string Password, string ConfirmPassword, string Role, string baseUrl)
        {
            try
            {
                // basic check, avoid empty input
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                    return new AuthResult { Success = false, status = AuthMessage.EmptyFields };      

                // normalize email to avoid duplicates
                Email = Email.Trim().ToLower();

                // check if email already exists in both tables
                bool EmailExistinStudent = _context.Students.Any(s => s.Email == Email);
                bool EmailExistinInstrutor = _context.instructors.Any(i => i.Email == Email);

                // if email exist user should login
                if (EmailExistinStudent || EmailExistinInstrutor)
                    return new AuthResult { Success = false, status = AuthMessage.EmailExist };

                // password match check
                if (Password != ConfirmPassword)
                    return new AuthResult { Success = false, status = AuthMessage.PassNotMatch };

                //Student Registration 
                if (Role == "Student")
                {
                    // create new student
                    var student = new Student
                    {
                        Email = Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        IsVerified = false,
                        //generate Verification Token
                        VerificationToken = Guid.NewGuid().ToString()
                    };

                    _context.Students.Add(student);
                    _context.SaveChanges();

                    //Send Verification email
                    return SendVerificationEmail(Email, baseUrl, "Student");
                }

                //Instructor Registration Logic
                if (Role == "Instructor")
                {
                    // create new instructor
                    var instructor = new Instructor
                    {
                        Email = Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        IsVerified = false,
                        //generate Verification Token
                        VerificationToken = Guid.NewGuid().ToString()
                    };

                    _context.instructors.Add(instructor);
                    _context.SaveChanges();

                    //Send Verification email
                    return SendVerificationEmail(Email, baseUrl, "Instructor");
                }

                return new AuthResult { Success = false, status = AuthMessage.RegisterFailed };
            }
            catch (Exception)
            {
                // any failure handled here
                return new AuthResult { Success = false, status = AuthMessage.RegisterFailed };
            }
        }//Register Method



        //#######################
        //#######################
        //Login Methods
        public AuthResult Login(string Email, string Password, string Role)
        {
            try
            {
                //check empty input
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                    return new AuthResult { Success = false, status = AuthMessage.EmptyFields };

                // normalize email 
                Email = Email.Trim().ToLower();

                //Student Login    
                if (Role == "Student")
                {
                    // find student by email
                    var student = _context.Students.FirstOrDefault(s => s.Email == Email);

                    // user not found
                    if (student == null)
                        return new AuthResult { Success = false, status = AuthMessage.NewUser };

                    // email not verified
                    if (!student.IsVerified)
                        return new AuthResult { Success = false, status = AuthMessage.VerifyEmail };

                    // avoid null password - for google user
                    if (string.IsNullOrEmpty(student.Password))
                        return new AuthResult { Success = false, status = AuthMessage.LoginFailed };

                    // wrong password
                    if (!BCrypt.Net.BCrypt.Verify(Password, student.Password))
                        return new AuthResult { Success = false, status = AuthMessage.WrongPassword };

                    // success
                    return new AuthResult
                    {
                        Success = true,
                        status = AuthMessage.LoginSuccess,
                        Role = "Student",
                        Id = student.Id,
                        Email = student.Email
                    };
                }

                //instructor login
                if (Role == "Instructor")
                {
                    // find instructor by email
                    var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

                    // user not found
                    if (instructor == null)
                        return new AuthResult { Success = false, status = AuthMessage.NewUser };

                    // email not verified
                    if (!instructor.IsVerified)
                        return new AuthResult { Success = false, status = AuthMessage.VerifyEmail };

                    // avoid null password crash (google users)
                    if (string.IsNullOrEmpty(instructor.Password))
                        return new AuthResult { Success = false, status = AuthMessage.LoginFailed };

                    // wrong password
                    if (!BCrypt.Net.BCrypt.Verify(Password, instructor.Password))
                        return new AuthResult { Success = false, status = AuthMessage.WrongPassword };

                    // success
                    return new AuthResult
                    {
                        Success = true,
                        status = AuthMessage.LoginSuccess,
                        Role = "Instructor",
                        Id = instructor.Id,
                        Email = instructor.Email
                    };
                }

                //Admin Login
                if (Role == "Admin")
                {
                    // simple static admin check (not safe for production)
                    if (Email == "sufiyan@admin.com" && Password == "abcd")
                    {
                        return new AuthResult { Success = true, status = AuthMessage.LoginSuccess, Role = "Admin" };
                    }

                    // wrong admin credentials
                    return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
                }

                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
            catch (Exception)
            {
                // unexpected failure 
                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
        } //Login Method



        //#######################
        //#######################
        //Google Login
        public AuthResult GoogleAuth(string Email, string FirstName, string LastName, string GoogleId, string Picture, string Role)
        {
            try
            {
                // basic safety check, don't allow empty critical values
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(GoogleId))
                    return new AuthResult { Success = false, status = AuthMessage.EmptyFields };

                // normalize email to avoid duplicates like A@gmail vs a@gmail
                Email = Email.Trim().ToLower();
                // transaction
                using var transaction = _context.Database.BeginTransaction();
                if (Role == "Student")
                {
                    // find user by google id
                    var student = _context.Students.FirstOrDefault(s => s.GoogleId == GoogleId);
                    // if not found, the check email
                    if (student == null)
                    {
                        student = _context.Students.FirstOrDefault(s => s.Email == Email);
                        // link google id to existing account instead of creating duplicate
                        if (student != null)
                        {
                            student.GoogleId = GoogleId;
                            _context.SaveChanges();
                            transaction.Commit();
                            return new AuthResult
                            {
                                Success = true,
                                status = AuthMessage.LoginSuccess,
                                Email = student.Email,
                                Role = "Student",
                                Id = student.Id,
                                PhotoPath = Picture
                            };
                        }
                    }

                    // existing user login
                    if (student != null)
                    {
                        transaction.Commit();
                        return new AuthResult
                        {
                            Success = true,
                            status = AuthMessage.LoginSuccess,
                            Email = student.Email,
                            Role = "Student",
                            Id = student.Id,
                            PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                        };
                    }
                    // create new student 
                    student = new Student
                    {
                        Email = Email,
                        GoogleId = GoogleId,
                        Password = null,
                        VerificationToken = null,
                        IsVerified = true,
                        //PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                    };

                    _context.Students.Add(student);
                    _context.SaveChanges();

                    // create profile 
                    var profile = new StudentProfile
                    {
                        StudentId = student.Id,
                        FirstName = FirstName ?? "Guest",
                        LastName = LastName ?? "User",
                        PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                    };

                    _context.StudentProfiles.Add(profile);
                    _context.SaveChanges();

                    transaction.Commit();

                    return new AuthResult { Success = true, status = AuthMessage.LoginSuccess, Email = student.Email,
                                            Role = "Student",Id = student.Id};                                                                               
                }//Student Role


                if (Role == "Instructor")
                {
                    // find instructor by google id
                    var instructor = _context.instructors.FirstOrDefault(i => i.GoogleId == GoogleId);

                    // email check
                    if (instructor == null)
                    {
                        instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

                        // link existing account with google id
                        if (instructor != null)
                        {
                            instructor.GoogleId = GoogleId;
                            _context.SaveChanges();

                            transaction.Commit();

                            return new AuthResult
                            {
                                Success = true,
                                Email = instructor.Email,
                                Id = instructor.Id,
                                Role = "Instructor"
                            };
                        }
                    }

                    // existing instructor login
                    if (instructor != null)
                    {
                        transaction.Commit();

                        return new AuthResult
                        {
                            Success = true,
                            Email = instructor.Email,
                            Id = instructor.Id,
                            Role = "Instructor"
                        };
                    }

                    // create new instructor
                    instructor = new Instructor
                    {
                        Email = Email,
                        GoogleId = GoogleId,
                        VerificationToken = null,
                        IsVerified = true
                    };

                    _context.instructors.Add(instructor);
                    _context.SaveChanges();

                    // create instructor profile
                    var profile = new InstructorProfile
                    {
                        InstructorId = instructor.Id,
                        FirstName = FirstName ?? "",
                        LastName = LastName ?? "",
                        PhotoPath = Picture
                    };

                    _context.instructorProfiles.Add(profile);
                    _context.SaveChanges();

                    transaction.Commit();

                    return new AuthResult
                    {
                        Success = true,
                        Email = instructor.Email,
                        Id = instructor.Id,
                        Role = "Instructor",
                         PhotoPath = Picture
                    };
                }

                // if failded
                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
            catch (Exception)
            {
                // any failure 
                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
        }
        



        //#######################
        //#######################
     //send Email Verification Link 
public AuthResult SendVerificationEmail(string Email, string baseUrl, string Role)
{
    // basic check, avoid empty input
    if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(baseUrl))
        return new AuthResult { status = AuthMessage.EmailNotVerified };

    // normalize email to match db
    Email = Email.Trim().ToLower();

    if (Role == "Student")
    {
        // find student by email
        var student = _context.Students.FirstOrDefault(s => s.Email == Email);

        // check  null 
        if (student == null)
        {
            return new AuthResult { status = AuthMessage.EmailNotVerified };
        }

        // generate new token
        student.VerificationToken = Guid.NewGuid().ToString();
        _context.SaveChanges();

        //send Email
        try
        {
            _emailService.SendVerificationEmail(Email, student.VerificationToken, baseUrl);
            return new AuthResult { status = AuthMessage.VerifyEmail };
        }
        catch (Exception)
        {
            return new AuthResult { status = AuthMessage.EmailNotSent };
        }
    }

    if (Role == "Instructor")
    {
        // find instructor by email
        var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

        // check null 
        if (instructor == null)
        {
            return new AuthResult { status = AuthMessage.EmailNotVerified };
        }

        // generate new token
        instructor.VerificationToken = Guid.NewGuid().ToString();
        _context.SaveChanges();

        //send Verification EMail
        try
        {
            _emailService.SendVerificationEmail(Email, instructor.VerificationToken, baseUrl);
            return new AuthResult { status = AuthMessage.VerifyEmail };
        }
        catch (Exception)
        {
            return new AuthResult { status = AuthMessage.EmailNotSent };
        }
    }
    return new AuthResult { status = AuthMessage.EmailNotVerified };
} //Send Email


        //#######################
        //#######################
        //Method for verify token from email link
        public AuthResult VerifyEmail(string token)
        {
            try
            {
                // check empty token
                if (string.IsNullOrWhiteSpace(token))
                    return new AuthResult { status = AuthMessage.EmailNotSent};

                // find student by token
                var student = _context.Students.FirstOrDefault(s => s.VerificationToken == token);

                // find instructor by token
                var instructor = _context.instructors.FirstOrDefault(i => i.VerificationToken == token);

                // if token exists in both tables
                if (student != null && instructor != null)
                    return new AuthResult { status = AuthMessage.EmailNotVerified };

                if (student != null)
                {
                    // already verified check
                    if (student.IsVerified)
                        return new AuthResult { status = AuthMessage.EmailVerified };

                    // mark as verified
                    student.IsVerified = true;
                    student.VerificationToken = null; // token used
                    _context.SaveChanges();

                    return new AuthResult { status = AuthMessage.EmailVerified };
                }

                if (instructor != null)
                {
                    // already verified check
                    if (instructor.IsVerified)
                        return new AuthResult { status = AuthMessage.EmailVerified };

                    // mark as verified
                    instructor.IsVerified = true;
                    instructor.VerificationToken = null; // token used
                    _context.SaveChanges();

                    return new AuthResult { status = AuthMessage.EmailVerified };
                }

                // token not found in db
                return new AuthResult { status = AuthMessage.EmailNotVerified };
            }
            catch (Exception)
            {
                //failure
                return new AuthResult { status = AuthMessage.EmailNotSent };
            }
        }
    }
}



