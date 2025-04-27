using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Carvisto.Models;
using Microsoft.AspNetCore.Authorization;
using Carvisto.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Carvisto.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterUserAsync(
                    model.Email,
                    model.Password,
                    model.ContactName,
                    model.ContactPhone);
                
                if (result.Succeeded)
                {
                    var user = await _accountService.GetUserByEmailAsync(model.Email);
                    
                    await _accountService.AddUserToRoleAsync(user, "User");
                    await _accountService.SignInUserAsync(user, isPersistent: false);
                    
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.LoginUserAsync(model.Email, model.Password, model.RememberMe);
                if (result.Succeeded)
                {
                    var user = await _accountService.GetUserByEmailAsync(model.Email);
                    if (string.IsNullOrEmpty(user.ContactName) || string.IsNullOrEmpty(user.ContactPhone))
                    {
                        return RedirectToAction("Index", "Account");
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _accountService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }
            
            var model = await _accountService.GetAccountViewModelAsync(user.Id);
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser user)
        {
            
            if (ModelState.IsValid)
            {
                var currentUser = await _accountService.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    string originalEmail = currentUser.Email;
                    
                    currentUser.ContactName = user.ContactName;
                    currentUser.ContactPhone = user.ContactPhone;
                    
                    bool emailChanged = !string.Equals(originalEmail, user.Email, StringComparison.OrdinalIgnoreCase);

                    if (emailChanged)
                    {
                        var existingUser = await _accountService.GetUserByEmailAsync(user.Email);
                        if (existingUser != null && existingUser.Id != currentUser.Id)
                        {
                            TempData["ProfileUpdateError"] = "Email is already in use.";
                            return RedirectToAction("Index", "Account");
                        }
                        
                        currentUser.Email = user.Email;
                        currentUser.UserName = user.Email;
                    }
                    
                    
                    var result = await _accountService.UpdateUserAsync(currentUser);
                    
                    if (result.Succeeded)
                    {
                        TempData["ProfileUpdateSuccess"] = "Information successfully updated.";
                        
                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        var errorMessages = string.Join("<br>", result.Errors.Select(e => e.Description));
                        TempData["ProfileUpdateError"] = errorMessages;
                        return RedirectToAction("Index", "Account");
                    }
                }
                else
                {
                    TempData["ProfileUpdateError"] = "User not found.";
                    return RedirectToAction("Index", "Account");
                }
            }
            else
            {
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["ProfileUpdateError"] = string.Join("<br>", modelErrors);
                return RedirectToAction("Index", "Account");
            }
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword) ||
                string.IsNullOrEmpty(model.ConfirmPassword))
            {
                TempData["PasswordError"] = "All fields for changing the password must be filled in.";
                return RedirectToAction("Index", "Account");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["PasswordError"] = "New passwords do not match.";
                return RedirectToAction("Index", "Account");
            }
            
            if (!ModelState.IsValid)
            {
                var viewModel = await _accountService.GetAccountViewModelAsync(null); // здесь нужно получить текущего пользователя
                viewModel.ChangePassword = model;
                return View("Index", viewModel);
            }

            var user = await _accountService.GetCurrentUserAsync();
            var result = await _accountService.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["PasswordSuccess"] = "Password changed successfully";
                return RedirectToAction("Index", "Account");
            }
            else
            {
                var errors = string.Join("<br>", result.Errors.Select(e => e.Description));
                TempData["PasswordError"] = errors;
                return RedirectToAction("Index", "Account");
            }
        }
    }
}