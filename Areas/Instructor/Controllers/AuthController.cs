using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Areas.Instructor.Models;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Services;
using System.Security.Claims;

namespace SkillForge.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AuthController : InstructorBaseController
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;
        public AuthController(AuthService authService, IConfiguration config)
        {
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
            var result = _authService.Register(Email,Password,ConfirmPassword,"Instructor",baseUrl);

            //Check Invalid or Valid
            //empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                return RedirectToAction("InstructorRegistration");
            }

            //Old User
            if (result.status == AuthMessage.EmailExist)
            {
                TempData["Alert"] = "This Email Already Registered, You can Login";
                return RedirectToAction("InstructorRegistration");
            }

            //Password Not Match
            if (result.status == AuthMessage.PassNotMatch)
            {
                TempData["Alert"] = "Password and Confirm Password doesn't Match!";
                return RedirectToAction("InstructorRegistration");
            }

            //email sent
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Email"] = "Registered! Please check your email to verify.";
                return RedirectToAction("InstructorRegistration");
            }
            //verification failded
            if (result.status == AuthMessage.EmailNotSent)
            {
                TempData["Email"] = "Registered! Email sending failed: ";
                return RedirectToAction("InstructorRegistration");

            }
            //Email Verified
            if (result.status == AuthMessage.EmailVerified)
            {
                TempData["Email"] = "Email verified! You can now login.";
                return RedirectToAction("InstructorLogin");
            }


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

            //check Empty Fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                return RedirectToAction("InstructorLogin");
            }

            //Email Not Exist
            if (result.status == AuthMessage.NewUser)
            {
                TempData["Alert"] = "This Email Doesn't Registered, Pleas Create Account";
                return RedirectToAction("InstructorLogin");
            }

            //if Email Not Verified  and store email             
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Verify"] = "Please verify your email first.";
                TempData["UnverifiedEmail"] = Email;
                return RedirectToAction("InstructorLogin");
            }

          //wrong Password
            if (result.status == AuthMessage.WrongPassword)
            {
                TempData["Alert"] = "Incorrect Password";
                return RedirectToAction("InstructorLogin");
            }

            //Login
            await SigninUser(result.Id, result.Email, "Instructor", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
            return RedirectToAction("Dashboard", "Home", new { area = "Instructor" });
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
                    return BadRequest("Invalid Google login request.");
                }

                //Verify token with Google
                var verify = await GoogleJsonWebSignature.ValidateAsync(request.Token);

                if (verify == null)
                {
                    return Unauthorized("Google verification failed.");
                }

                // 3. Call Auth Service
                var result = _authService.GoogleAuth(
                    verify.Email,
                    verify.GivenName,
                    verify.FamilyName,
                    verify.Subject,
                    verify.Picture,
                    "Instructor"
                );

                if (result == null || !result.Success)
                {
                    TempData["Alert"] = "Google login failed";
                    return RedirectToAction("StudentLogin");
                }

                // Sign in user
                await SigninUser(
                    result.Id,
                    result.Email ?? string.Empty,
                    "Instructor",
                    result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif"
                );

                return RedirectToAction("Dashboard", "Home", new { area = "Instructor" });
            }
            catch (InvalidJwtException )
            {
                // Token invalid or tampered
                return Unauthorized("Invalid Google token.");
            }
            catch (Exception )
            {       
               return StatusCode(500, "Something went wrong during login.");
            }
        }


        //########################
        //SEnd Verification Method
        public IActionResult SendVerificationEmail(string Email)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var email = _authService.SendVerificationEmail(Email, baseUrl, "Instructor");
            if (email.status == AuthMessage.VerifyEmail)
            {
                TempData["Email"] = "Verification email sent successfully";
                return RedirectToAction("InstructorLogin");
            }

            if (email.status == AuthMessage.EmailNotSent)
            {
                TempData["Alert"] = "Failed to send email. Try again.";
                return RedirectToAction("InstructorLogin");
            }

            TempData["Alert"] = "Something went wrong";
            return RedirectToAction("InstructorLogin");
        }

        //########################
        //Verify EMail  
        public IActionResult EmailVerifivation(string token)
        {
            //passes to authservice
            var result = _authService.VerifyEmail(token);

            // if auth service returen verified message then email verified
            if (result.status == AuthMessage.EmailVerified)
            {
                return RedirectToAction("InstructorLogin");
            }
            TempData["Alert"] = "Invalid or expired link.";
            return RedirectToAction("InstructorLogin");
        } //EMail Verification Method


        //########################
        //Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("InstructorLogin");
        }


        // Use SigninUser from AuthBaseController (inherited)
    }
}

