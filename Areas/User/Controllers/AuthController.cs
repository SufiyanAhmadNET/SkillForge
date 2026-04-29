using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Areas.User.Models;
using SkillForge.Services;

namespace SkillForge.Areas.User.Controllers
{
    [Area("User")]
    public class AuthController : UserBaseController
    {

        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        //Constructor
        public AuthController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        //get Method to SHow Reg Form
        public IActionResult StudentRegistration()
        {

            return View();
        }

        // Post Method
        [HttpPost]
        public IActionResult StudentRegistration(string Email, string Password, string ConfirmPassword)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            //pass paramters to Auth Service CLass
            var result = _authService.Register(Email, Password, ConfirmPassword, "Student", baseUrl);

            // empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentRegistration");
            }

            // old user
            if (result.status == AuthMessage.EmailExist)
            {
                TempData["Alert"] = "This Email Already Registered, You can Login";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentRegistration");
            }

            if (result.status == AuthMessage.EmailRegisteredAsInstructor)
            {
                TempData["Alert"] = "This email is registered as Instructor. Please login as Instructor.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("StudentRegistration");
            }

            // password mismatch
            if (result.status == AuthMessage.PassNotMatch)
            {
                TempData["Alert"] = "Password and Confirm Password doesn't Match!";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentRegistration");
            }

            // email sent
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Alert"] = "Registered! \"OTP sent to your email.\"";
                TempData["AlertType"] = "success";

                TempData["VerifyMessage"] = "We’ve sent a OTP on  email.";
                TempData["VerifyEmail"] = result.Email;

                return RedirectToAction("StudentRegistration");
            }

            // email failed
            if (result.status == AuthMessage.EmailNotSent)
            {
                TempData["Alert"] = "Registered! Email sending failed";
                TempData["AlertType"] = "danger";

                TempData["VerifyMessage"] = "Email not sent. Try again.";
                TempData["VerifyEmail"] = result.Email;

                return RedirectToAction("StudentRegistration");
            }

            // email verified
            if (result.status == AuthMessage.EmailVerified)
            {
                TempData["Alert"] = "Email verified! You can now login.";
                TempData["AlertType"] = "success";

                return RedirectToAction("StudentLogin");
            }

            TempData["Alert"] = "Registration successful." +
              " Login and Start Shaping Your Career in Riht Path";
            TempData["AlertType"] = "success";
            return RedirectToAction("StudentLogin");
        }


        //########################
        //Login Method
        public IActionResult StudentLogin()
        {
            //Google Login
            ViewBag.GoogleClientId = _config["GoogleAuth:ClientId"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StudentLogin(string Email, string Password)
        {
            var result = _authService.Login(Email, Password, "Student");

            // empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }

            // new user
            if (result.status == AuthMessage.NewUser)
            {
                TempData["Alert"] = "This Email Doesn't Registered, Please Create Account";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }

            if (result.status == AuthMessage.EmailRegisteredAsInstructor)
            {
                TempData["Alert"] = "This email is registered as Instructor. Please login from Instructor panel.";
                TempData["AlertType"] = "warning";
                return RedirectToAction("StudentLogin");
            }

            // email not verified
            if (result.status == AuthMessage.VerifyEmail)
            {
                _authService.SendEmailOtp(Email, "Student");

                TempData["VerifyEmail"] = Email;
                TempData["Alert"] = "OTP sent. Please verify your email.";
                TempData["AlertType"] = "warning";

                return RedirectToAction("StudentLogin");
            }
                // wrong password
                if (result.status == AuthMessage.WrongPassword)
            {
                TempData["Alert"] = "Incorrect Password";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }

          
            //successful login
            if (result.status == AuthMessage.LoginSuccess)
            {
                await SigninUser(result.Id.ToString(), result.Email, "Student", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
                return RedirectToAction("Dashboard", "Home", new { area = "User" });
            }

            TempData["Alert"] = "Something went wrong";
            return RedirectToAction("StudentLogin");
        }//Login Method


        //########################
        //Google Login    
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
                    return RedirectToAction("StudentLogin");
                }

                //Verify token with Google
                var verify = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                if (verify == null || string.IsNullOrEmpty(verify.Email))
                {
                    TempData["Alert"] = "Google verification failed or email missing.";
                    TempData["AlertType"] = "danger";
                    return RedirectToAction("StudentLogin");
                }
                // Call Auth Service
                var result = _authService.GoogleAuth(
                    verify.Email,
                    verify.GivenName,
                    verify.FamilyName,
                    verify.Subject,
                    verify.Picture,
                    "Student"
                );

                //  fail 
                if (result != null && result.status == AuthMessage.EmailRegisteredAsInstructor)
                {
                    TempData["Alert"] = "This email is registered as Instructor. Please continue as Instructor.";
                    TempData["AlertType"] = "warning";
                    return RedirectToAction("StudentLogin");
                }

                if (result == null || !result.Success || string.IsNullOrEmpty(result.Email) || result.Id <= 0)
                {
                    TempData["Alert"] = "Google login failed: account creation or lookup failed.";
                    TempData["AlertType"] = "danger";
                    return RedirectToAction("StudentLogin");
                }

                await SigninUser(result.Id.ToString(), result.Email ?? string.Empty, "Student", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
                return RedirectToAction("Dashboard", "Home", new { area = "User" });
            }
            catch (InvalidJwtException)
            {
                TempData["Alert"] = "Invalid Google token.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }
            catch (Exception)
            {
                TempData["Alert"] = "Something went wrong during Google login.";
                TempData["AlertType"] = "danger";
                return RedirectToAction("StudentLogin");
            }
        }

        //Send Verification Email
        [HttpPost]
        public IActionResult SendEmailOtp(string Email)
        {
            var result = _authService.SendEmailOtp(Email, "Student");

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


        //########################
        //Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("StudentLogin");
        }
    }
}
