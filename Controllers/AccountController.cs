using Microsoft.AspNetCore.Mvc;
using Carvisto.Models;
using Microsoft.AspNetCore.Authorization;
using Carvisto.Services;

namespace Carvisto.Controllers
{
    // Controller handling user authentication, profile management, and account operations
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        // Initializes a new instance with dependency injection for account services
        public AccountController(
            IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Attempt user creation through service
                var result = await _accountService.RegisterUserAsync(
                    model.Email,
                    model.Password,
                    model.ContactName,
                    model.ContactPhone);
                
                if (result.Succeeded)
                {
                    // Post-registration setup
                    var user = await _accountService.GetUserByEmailAsync(model.Email);
                    await _accountService.AddUserToRoleAsync(user, "User");
                    await _accountService.SignInUserAsync(user, isPersistent: false);
                    return RedirectToAction("Index");
                }

                // Add Identity errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.LoginUserAsync(model.Email, model.Password, model.RememberMe);
                if (result.Succeeded)
                {
                    // Profile completeness check
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

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("Index");
        }

        // GET: /Account/Index
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _accountService.GetCurrentUserAsync();

            var model = await _accountService.GetAccountViewModelAsync(user.Id);
            return View(model);
        }

        // Updates user profile information
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser user)
        {
            
            if (ModelState.IsValid)
            {
                var currentUser = await _accountService.GetCurrentUserAsync();
                {
                    string? originalEmail = currentUser.Email;
                    bool emailChanged = !string.Equals(originalEmail, user.Email, StringComparison.OrdinalIgnoreCase);

                    // Email change validation
                    if (emailChanged)
                    {
                        if (user.Email != null)
                        {
                            var existingUser = await _accountService.GetUserByEmailAsync(user.Email);
                            if (existingUser.Id != currentUser.Id)
                            {
                                TempData["ProfileUpdateError"] = "Email is already in use.";
                                return RedirectToAction("Index", "Account");
                            }
                        }
                        
                        // Apply profile updates
                        currentUser.ContactName = user.ContactName;
                        currentUser.ContactPhone = user.ContactPhone;
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
            }
            else
            {
                // Model validation failed
                var modelErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["ProfileUpdateError"] = string.Join("<br>", modelErrors);
                return RedirectToAction("Index", "Account");
            }
        }
        
        // Changes user password
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            // Basic field validation
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
                var viewModel = await _accountService.GetAccountViewModelAsync(null);
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

        // Handles profile image uploads
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile? profileImage)
        {
            // File validation
            if (profileImage == null || profileImage.Length == 0)
            {
                TempData["ProfileImageError"] = "No image selected.";
                return RedirectToAction("Index", "Account");
            }

            if (profileImage.Length > 1024 * 1024 * 5)
            {
                TempData["ProfileImageError"] = "Image size is too large.";
                return RedirectToAction("Index", "Account");
            }
            
            string[] allowedExtensions = { "image/jpeg", "image/png" };
            if (!allowedExtensions.Contains(profileImage.ContentType))
            {
                TempData["ProfileImageError"] = "Image type is not supported.";
                return RedirectToAction("Index", "Account");
            }

            try
            {
                var user = await _accountService.GetCurrentUserAsync();

                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                // Save image and update profile
                using var memoryStream = new MemoryStream();
                await profileImage.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                string relativePath = await _accountService.SaveProfileImageAsync(
                    user,
                    uploadFolder,
                    profileImage.FileName,
                    fileBytes);

                user.ProfileImagePath = relativePath;
                await _accountService.UpdateUserAsync(user);

                TempData["ProfileImageSuccess"] = "Image successfully uploaded.";
            }
            catch (Exception ex)
            {
                TempData["ProfileImageError"] = $"Error uploading image: {ex.Message}";
            }
            
            return RedirectToAction("Index", "Account");
        }

        // Removes profile image
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProfileImage()
        {
            try
            {
                var user = await _accountService.GetCurrentUserAsync();

                await _accountService.DeleteProfileImageAsync(user);
                
                user.ProfileImagePath = null;
                await _accountService.UpdateUserAsync(user);
                
                TempData["ProfileImageSuccess"] = "Image successfully deleted.";
            }
            catch (Exception ex)
            {
                TempData["ProfileImageError"] = $"Error deleting image: {ex.Message}";
            }
            
            return RedirectToAction("Index", "Account");
        }
    }
}