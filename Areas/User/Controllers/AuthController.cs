using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillForge.Areas.User.Models;
using SkillForge.Data;
using SkillForge.Services;
using System;
using System.Security.Claims;

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

            //empty fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                return RedirectToAction("StudentRegistration");
            }

            //Old User
            if (result.status == AuthMessage.EmailExist)
            {
                TempData["Alert"] = "This Email Already Registered, You can Login";
                return RedirectToAction("StudentRegistration");
            }

            //Password Not Match
            if (result.status == AuthMessage.PassNotMatch)
            {
                TempData["Alert"] = "Password and Confirm Password doesn't Match!";
                return RedirectToAction("StudentRegistration");
            }

            //email sent
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Email"] = "Registered! Please check your email to verify.";
                return RedirectToAction("StudentRegistration");
            }
            //verification failded
            if (result.status == AuthMessage.EmailNotSent)
            {
                TempData["Email"] = "Registered! Email sending failed: ";
                return RedirectToAction("StudentRegistration");

            }
            //Email Verified
            if (result.status == AuthMessage.EmailVerified)
            {
                TempData["Email"] = "Email verified! You can now login.";
                return RedirectToAction("StudentLogin");
            }

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

            //check Empty Fields
            if (result.status == AuthMessage.EmptyFields)
            {
                TempData["Alert"] = "Enter All Required Details";
                return RedirectToAction("StudentLogin");
            }

            //Email Not Exist
            if (result.status == AuthMessage.NewUser)
            {
                TempData["Alert"] = "This Email Doesn't Registered, Pleas Create Account";
                return RedirectToAction("StudentLogin");
            }

            //store email if login Failed
            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Verify"] = "Please verify your email first.";
                TempData["UnverifiedEmail"] = Email;
                return RedirectToAction("StudentLogin");
            }
            //wrong Password
            if (result.status == AuthMessage.WrongPassword)
            {
                TempData["Alert"] = "Incorrect Password";
                return RedirectToAction("StudentLogin");
            }

            //Login
            await SigninUser(result.Id, result.Email, "Student", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
            return RedirectToAction("Dashboard", "Home", new { area = "User" });
        }//Login Method


        //########################
        //Google Login    
        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                    if (request == null || string.IsNullOrEmpty(request.Token))
                {
                    TempData["Alert"] = "Google login failed: missing token.";
                    return RedirectToAction("StudentLogin");
                }

                var verify = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                if (verify == null || string.IsNullOrEmpty(verify.Email))
                {
                    TempData["Alert"] = "Google verification failed or email missing.";
                    return RedirectToAction("StudentLogin");
                }

                var result = _authService.GoogleAuth(
                    verify.Email,
                    verify.GivenName,
                    verify.FamilyName,
                    verify.Subject,
                    verify.Picture,
                    "Student"
                );

                // Inspect result and fail gracefully
                if (result == null || !result.Success || string.IsNullOrEmpty(result.Email) || result.Id <= 0)
                {
                    TempData["Alert"] = "Google login failed: account creation or lookup failed.";
                    return RedirectToAction("StudentLogin");
                }

                await SigninUser(result.Id, result.Email ?? string.Empty, "Student", result.PhotoPath ?? "/images/DefaultProfilePhoto.jfif");
                return RedirectToAction("Dashboard", "Home", new { area = "User" });
            }
            catch (InvalidJwtException)
            {
                TempData["Alert"] = "Invalid Google token.";
                return RedirectToAction("StudentLogin");
            }
            catch (Exception)
            {
                TempData["Alert"] = "Something went wrong during Google login.";
                return RedirectToAction("StudentLogin");
            }   
        }

        //Send Verification Email
        [HttpPost]
        public IActionResult SendVerificationEmail(string Email)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = _authService.SendVerificationEmail(Email, baseUrl, "Student");

            if (result.status == AuthMessage.VerifyEmail)
            {
                TempData["Email"] = "Verification email sent successfully";
                return RedirectToAction("StudentLogin");
            }

            if (result.status == AuthMessage.EmailNotSent)
            {
                TempData["Alert"] = "Failed to send email. Try again.";
                return RedirectToAction("StudentLogin");
            }

            TempData["Alert"] = "Something went wrong";
            return RedirectToAction("StudentLogin");
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
                return RedirectToAction("StudentLogin");
            }
            TempData["Alert"] = "Invalid or expired link.";
            return RedirectToAction("StudentLogin");
        } //EMail Verification Method

      
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
