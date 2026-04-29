using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillForge.Areas.User.Controllers;
using SkillForge.Areas.User.Models;
using SkillForge.Services;
using System.Data;
using System.Diagnostics;
namespace SkillForge.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AuthController : UserBaseController
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        private readonly IConfiguration _config;
        public AuthController(AuthService authService, IConfiguration config, ILogger<AuthController> logger)
        {
            _logger = logger;
            _authService = authService;
            _config = config;
        }

        //==================
        //Registration Methods
        //====================
        public IActionResult InstructorRegistration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult InstructorRegistration(string Email, string Password, string ConfirmPassword)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            //pass paramters to Auth Service CLass
            var result = _authService.Register(Email, Password, ConfirmPassword, "Instructor", baseUrl);

            //Check Invalid or Valid
            // empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorRegistration");
            }

            // old user
            if (result.status == AuthMessage.EmailExist)
            {
                TempData["Alert"] = "This Email Already Registered, You can Login";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorRegistration");
            }

            if (result.status == AuthMessage.EmailRegisteredAsStudent)
            {
                TempData["Alert"] = "This email is registered as Student. Please login as Student.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("InstructorRegistration");
            }

            // password mismatch
            if (result.status == AuthMessage.PassNotMatch)
            {
                TempData["Alert"] = "Password and Confirm Password doesn't Match!";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorRegistration");
            }

            // email sent
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Alert"] = "Registered! Please check your email to verify.";
                TempData["AlertType"] = "success";

                TempData["VerifyMessage"] = "We’ve sent a verification OTP on Email .";
                TempData["VerifyEmail"] = result.Email;

                return RedirectToAction("InstructorRegistration");
            }

            // email failed
            if (result.status == AuthMessage.EmailNotSent)
            {
                TempData["Alert"] = "Registered! Email sending failed";
                TempData["AlertType"] = "danger";

                TempData["VerifyMessage"] = "Email not sent. Try again.";
                TempData["VerifyEmail"] = result.Email;

                return RedirectToAction("InstructorRegistration");
            }

            // email verified
            if (result.status == AuthMessage.EmailVerified)
            {
                TempData["Alert"] = "Email verified! You can now login.";
                TempData["AlertType"] = "success";

                return RedirectToAction("InstructorLogin");
            }

            TempData["Alert"] = "Registration successful." +
                " Login and Start Your Journey as Mentor";
            TempData["AlertType"] = "success";
            return RedirectToAction("InstructorLogin");
        }//Registration Method


        //==================
        //Login Methods
        //====================
        public IActionResult InstructorLogin()
        {
            //Google Login
            ViewBag.GoogleClientId = _config["GoogleAuth:ClientId"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InstructorLogin(string Email, string Password)
        {
            var result = _authService.Login(Email, Password, "Instructor");

            // empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin");
            }

            // new user
            if (result.status == AuthMessage.NewUser)
            {
                TempData["Alert"] = "This Email Doesn't Registered, Please Create Account";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin");
            }

            if (result.status == AuthMessage.EmailRegisteredAsStudent)
            {
                TempData["Alert"] = "This email is registered as Student. Please login from Student panel.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("InstructorLogin");
            }

            // email not verified
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Alert"] = "Email not verified";
                TempData["AlertType"] = "danger";

                TempData["VerifyMessage"] = "Please verify your email first.";
                TempData["VerifyEmail"] = Email;

                return RedirectToAction("InstructorLogin");
            }

            // wrong password
            if (result.status == AuthMessage.WrongPassword)
            {
                TempData["Alert"] = "Incorrect Password";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin");
            }

            //successful login
            if (result.status == AuthMessage.LoginSuccess)
            {
                await SigninUser(result.Id.ToString(), result.Email, "Instructor", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
                return RedirectToAction("Dashboard", "Home", new { area = "Instructor" });
            }

            TempData["Alert"] = "Something went wrong"; 
            return RedirectToAction("InstructorLogin");


        }//Login Method

            
         //########################
        [HttpPost]
     public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            
            try
            {
                // validation 
                if (request == null || string.IsNullOrEmpty(request.Token))
                {
                    TempData["Alert"] = "Google login failed: missing token.";
                    TempData["AlertType"] = "danger";
                    return RedirectToAction("InstructorLogin");
                }

                //Verify token with Google
                var verify = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                if (verify == null || string.IsNullOrEmpty(verify.Email))
                {
                    TempData["Alert"] = "Google verification failed or email missing.";
                    TempData["AlertType"] = "danger";
                    return RedirectToAction("InstructorLogin");
                }

                // Call Auth Service
                var result = _authService.GoogleAuth(
                    verify.Email,
                    verify.GivenName,
                    verify.FamilyName,
                    verify.Subject,
                    verify.Picture,
                    "Instructor"
                );

                //  fail
                if (result != null && result.status == AuthMessage.EmailRegisteredAsStudent)
                {
                    TempData["Alert"] = "This email is registered as Student. Please continue as Student.";
                    TempData["AlertType"] = "warning";
                    return RedirectToAction("InstructorLogin");
                }

                if (result == null || !result.Success)
                {
                    TempData["Alert"] = "Google login failed";
                    TempData["AlertType"] = "danger";
                    return RedirectToAction("InstructorLogin");
                }

                // Sign in user
                await SigninUser(
                    result.Id.ToString(),
                    result.Email ?? string.Empty,
                    "Instructor",
                    result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif"
                );

                return RedirectToAction("Dashboard", "Home", new { area = "Instructor" });
            }
            catch (InvalidJwtException )
            {
                // Token invalid
                TempData["Alert"] = "Invalid Google token.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("InstructorLogin");
            }
            catch (Exception )
            {
                TempData["Alert"] = "Something went wrong during Google login.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }
        }


        //########################
        //SEnd Verification Method
        [HttpPost]
        public IActionResult SendEmailOtp(string Email)
        {
            var result = _authService.SendEmailOtp(Email, "Instructor");

            if (result.status == AuthMessage.VerifyEmail)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        //########################
        //Verify EMail  
        [HttpPost]
        public IActionResult VerifyEmailOtp(string Email, string Otp)
        {
            var result = _authService.VerifyEmailOtp(Email, Otp);

            if (result.status == AuthMessage.EmailVerified)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid or expired OTP" });
        }
        //eMail Verification Method


        //########################
        //Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("InstructorLogin");
        }


        
    }
}

