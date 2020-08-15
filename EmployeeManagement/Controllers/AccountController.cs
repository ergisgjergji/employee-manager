using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);

            // Check if email is confirmed
            //if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
            //{
            //    ModelState.AddModelError("", "Email not confirmed!");
            //    return View(model);
            //}

            // SignIn logic
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
                return View("AccountLocked");
            else
            {
                ModelState.AddModelError(String.Empty, "Invalid login attempt");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser 
            { 
                FullName = model.FullName,
                UserName = model.Email, 
                Email = model.Email 
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if(result.Succeeded)
            {
                //var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                //var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                {
                    //await userManager.ConfirmEmailAsync(user, token);

                    TempData["Success"] = "User was added successfully.";

                    return RedirectToAction("ListUsers", "Administration");
                }

                TempData["Success"] = "Registration was successful.";
                return View("RegisterSuccess");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
                return RedirectToAction("Index", "Home");

            var user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                ViewBag.Message = $"User with id = {userId} not found";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["Info"] = "Email confirmed!";
                return View();
            }
            else
            {
                ViewBag.ErrorTitle = "Confirmation error!";
                ViewBag.ErrorMessage = "Email could not be confirmed.";
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await userManager.GetUserAsync(User);

            EditProfileVm model = new EditProfileVm { FullName = user.FullName, Email = user.Email };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);
            user.FullName = model.FullName;
            user.UserName = model.Email;
            user.Email = model.Email;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Profile was updated successfully.";
                return View("EditProfileSuccess");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login");

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password was changed successfully.";
                return View("ChangePasswordSuccess");
            }
            else
            {
                foreach (var error in result.Errors)

                    ModelState.AddModelError("", error.Description);
                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassword model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);

            //if(user != null && (await userManager.IsEmailConfirmedAsync(user)))
            if(user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme); 

                /* 
                    Insert logic how to handle reset-link. For example: send it as an email
                */
            }

            // Always return this view, even if the email is doesn't exist or not confirmed.
            // We do this for security reasons
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null)
                ModelState.AddModelError("", "Invalid password reset token.");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            /*
                If there is no user (meaning there is no account with this email), we don't want to dispatch this information to the user
                because we want to keep our accounts private. Instead, we show that pasword reset was successful.
            */
            if (user == null)
                return View("ResetPasswordSuccess");

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if(result.Succeeded)
            {
                // If account was locked, unlock it
                if (await userManager.IsLockedOutAsync(user))
                    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

                TempData["Success"] = "Password was reset successfully.";
                return View("ResetPasswordSuccess");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }
        }

        [AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsUniqueEmail(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
                return Json(true);

            return Json($"Email '{email}' is already taken!");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}