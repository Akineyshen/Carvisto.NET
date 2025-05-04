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
        private readonly IBookingService _bookingService;

        public AccountController(
            IAccountService accountService,
            IBookingService bookingService)
        {
            _accountService = accountService;
            _bookingService = bookingService;
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
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
                if (user == null)
                {
                    return NotFound();
                }

                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteProfileImage()
        {
            try
            {
                var user = await _accountService.GetCurrentUserAsync();
                if (user == null)
                {
                    return NotFound();
                }

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

        [Authorize]
        public async Task<IActionResult> Bookings()
        {
            var user = await _accountService.GetCurrentUserAsync();
            var bookings = await _bookingService.GetUserBookingASync(user.Id);
            return RedirectToAction("Index", "Account");
        }
    }
}