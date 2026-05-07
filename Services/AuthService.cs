using Microsoft.AspNetCore.Mvc;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Models;
using System.Reflection;
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

                // password match check
                if (Password != ConfirmPassword)
                    return new AuthResult { Success = false, status = AuthMessage.PassNotMatch };

                //Student Registration 
                if (Role == "Student")
                {
                    // don't allow same email in both roles
                    if (EmailExistinStudent)
                        return new AuthResult { Success = false, status = AuthMessage.EmailExist };

                    if (EmailExistinInstrutor)
                        return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsInstructor };

                    // create new student
                    var student = new Student
                    {
                        Email = Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        IsEmailVerified = false,
                        //generate OTP 
                        EmailOtp = new Random().Next(100000, 999999).ToString(),
                        OtpExpiry = DateTime.UtcNow.AddMinutes(5)
                    };
                
                    _context.Students.Add(student);
                    _context.SaveChanges();

                    //Send Verification email
                    return SendEmailOtp(Email, "Student");
                }

                //Instructor Registration Logic
                if (Role == "Instructor")
                {
                    // don't allow same email in both roles
                    if (EmailExistinInstrutor)
                        return new AuthResult { Success = false, status = AuthMessage.EmailExist };

                    if (EmailExistinStudent)
                        return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsStudent };

                    // create new instructor
                    var instructor = new Instructor
                    {
                        Email = Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Password),
                        IsEmailVerified = false,
                       EmailOtp = null
                    };

                    _context.instructors.Add(instructor);
                    _context.SaveChanges();

                    //Send Verification email
                    return SendEmailOtp(Email, "Instructor");
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
                // basic validation
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
                    {
                        bool existsAsInstructor = _context.instructors.Any(i => i.Email == Email);
                        if (existsAsInstructor)
                            return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsInstructor };

                        return new AuthResult { Success = false, status = AuthMessage.NewUser };
                    }

                    // email not verified
                    if (!student.IsEmailVerified)
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
                    {
                        bool existsAsStudent = _context.Students.Any(s => s.Email == Email);
                        if (existsAsStudent)
                            return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsStudent };

                        return new AuthResult { Success = false, status = AuthMessage.NewUser };
                    }

                    // email not verified
                    if (!instructor.IsEmailVerified)
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

                // transaction ensures we don't create partial records
                using var transaction = _context.Database.BeginTransaction();

                if (Role == "Student")
                {
                    // don't allow Google auth with email of another role
                    bool existsAsInstructor = _context.instructors.Any(i => i.Email == Email);
                    if (existsAsInstructor)
                        return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsInstructor };

                    // STUDENT: do all student-only logic here

                    // find user by google id first
                    var student = _context.Students.FirstOrDefault(s => s.GoogleId == GoogleId);

                    // if not found by google id, try matching by email
                    if (student == null)
                    {
                        student = _context.Students.FirstOrDefault(s => s.Email == Email);
                        if (student != null)
                        {
                            // link google id to existing account
                            student.GoogleId = GoogleId;
                            student.IsEmailVerified = true;
                            _context.SaveChanges();
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
                    }

                    // existing user login (found by google id)
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
                        EmailOtp = null,
                        IsEmailVerified = true
                    };

                    _context.Students.Add(student);
                    _context.SaveChanges();

                    // create profile (keep mobile null by default)
                    var studentProfile = new StudentProfile
                    {
                        StudentId = student.Id,
                        FirstName = FirstName ?? "Guest",
                        LastName = LastName ?? "User",
                        Mobile = null,
                        PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                    };

                    _context.StudentProfiles.Add(studentProfile);
                    _context.SaveChanges();

                    transaction.Commit();

                    return new AuthResult
                    {
                        Success = true,
                        status = AuthMessage.LoginSuccess,
                        Email = student.Email,
                        Role = "Student",
                        Id = student.Id,
                        PhotoPath = studentProfile.PhotoPath
                    };
                } // end Student block


                if (Role == "Instructor")
                {
                    // don't allow Google auth with email of another role
                    bool existsAsStudent = _context.Students.Any(s => s.Email == Email);
                    if (existsAsStudent)
                        return new AuthResult { Success = false, status = AuthMessage.EmailRegisteredAsStudent };

                    // INSTRUCTOR: fully separate instructor-only logic

                    // find instructor by GoogleId first
                    var instructor = _context.instructors.FirstOrDefault(i => i.GoogleId == GoogleId);

                    // if not found by GoogleId, try match by email
                    if (instructor == null)
                    {
                        instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

                        if (instructor != null)
                        {
                            // link google id to existing instructor account and mark verified
                            instructor.GoogleId = GoogleId;
                            instructor.IsEmailVerified = true;
                            _context.SaveChanges();
                            transaction.Commit();

                            return new AuthResult
                            {
                                Success = true,
                                status = AuthMessage.LoginSuccess,
                                Email = instructor.Email,
                                Id = instructor.Id,
                                Role = "Instructor",
                                PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                            };
                        }
                    }

                    // existing instructor found by GoogleId
                    if (instructor != null)
                    {
                        transaction.Commit();
                        return new AuthResult
                        {
                            Success = true,
                            status = AuthMessage.LoginSuccess,
                            Email = instructor.Email,
                            Id = instructor.Id,
                            Role = "Instructor",
                            PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                        };
                    }

                    // create new instructor
                    instructor = new Instructor
                    {
                        Email = Email,
                        GoogleId = GoogleId,
                        EmailOtp = null,
                        IsEmailVerified = true
                    };

                    _context.instructors.Add(instructor);
                    _context.SaveChanges();

                    // create instructor profile
                    var instructorProfile = new InstructorProfile
                    {
                        InstructorId = instructor.Id,
                        FirstName = FirstName ?? string.Empty,
                        LastName = LastName ?? string.Empty,
                        PhotoPath = Picture ?? "/images/DefaultProfilePhoto.jfif"
                    };

                    _context.instructorProfiles.Add(instructorProfile);
                    _context.SaveChanges();

                    transaction.Commit();

                    return new AuthResult
                    {
                        Success = true,
                        status = AuthMessage.LoginSuccess,
                        Email = instructor.Email,
                        Id = instructor.Id,
                        Role = "Instructor",
                        PhotoPath = instructorProfile.PhotoPath
                    };
                } // end Instructor block

                // unknown role
                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
            catch (Exception ex)
            {
                // log to console for debugging
                Console.WriteLine($"GoogleAuth exception: {ex.Message}");
                return new AuthResult { Success = false, status = AuthMessage.LoginFailed };
            }
        }



        //#######################
        //#######################
        public AuthResult SendEmailOtp(string Email, string Role)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return new AuthResult { status = AuthMessage.EmptyFields };

            Email = Email.Trim().ToLower();

            var otp = new Random().Next(100000, 999999).ToString();

            // student
            if (Role == "Student")
            {
                var student = _context.Students.FirstOrDefault(s => s.Email == Email);

                if (student == null)
                    return new AuthResult { status = AuthMessage.NewUser };

                student.EmailOtp = otp;
                student.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

                _context.SaveChanges();

                try
                {
                    _emailService.SendOtpEmail(Email, otp);
                    return new AuthResult { status = AuthMessage.VerifyEmail, Email = Email };
                }
                catch
                {
                    return new AuthResult { status = AuthMessage.EmailNotSent };
                }
            }

            // instructor
            if (Role == "Instructor")
            {
                var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

                if (instructor == null)
                    return new AuthResult { status = AuthMessage.NewUser };

                instructor.EmailOtp = otp;
                instructor.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

                _context.SaveChanges();

                try
                {
                    _emailService.SendOtpEmail(Email, otp);
                    return new AuthResult { status = AuthMessage.VerifyEmail, Email = Email };
                }
                catch
                {
                    return new AuthResult { status = AuthMessage.EmailNotSent };
                }
            }

            return new AuthResult { status = AuthMessage.NewUser };
        }



        //#######################
        //#######################
        //Method for VERIFY OTP
        public AuthResult VerifyEmailOtp(string Email, string Otp)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Otp))
                return new AuthResult { status = AuthMessage.EmailNotVerified };

            Email = Email.Trim().ToLower();
            Otp = Otp.Trim();

           
            //student check
            var student = _context.Students.FirstOrDefault(s => s.Email == Email);

            if (student != null)
            {
                if (student.IsEmailVerified)
                    return new AuthResult { status = AuthMessage.EmailVerified };

                if (student.EmailOtp == null || student.EmailOtp.Trim() != Otp)
                    return new AuthResult { status = AuthMessage.EmailNotVerified };

                if (student.OtpExpiry == null || student.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { status = AuthMessage.EmailNotSent };

                student.IsEmailVerified = true;
                student.EmailOtp = null;
                student.OtpExpiry = null;

                _context.SaveChanges();
                return new AuthResult { status = AuthMessage.EmailVerified };
            }

            //instructor check
            var instructor = _context.instructors.FirstOrDefault(i => i.Email == Email);

            if (instructor != null)
            {
                if (instructor.IsEmailVerified)
                    return new AuthResult { status = AuthMessage.EmailVerified };

                if (instructor.EmailOtp == null || instructor.EmailOtp.Trim() != Otp)
                    return new AuthResult { status = AuthMessage.EmailNotVerified };

                if (instructor.OtpExpiry == null || instructor.OtpExpiry < DateTime.UtcNow)
                    return new AuthResult { status = AuthMessage.EmailNotSent };

                instructor.IsEmailVerified = true;
                instructor.EmailOtp = null;
                instructor.OtpExpiry = null;

                _context.SaveChanges();
                return new AuthResult { status = AuthMessage.EmailVerified };
            }

           
            // NOT FOUND    
            return new AuthResult { status = AuthMessage.EmailNotVerified };
        }

    }
}



