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
                var result = await _accountService.RegisterUserAsync(model.Email, model.Password);
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
        public async Task<IActionResult> UpdateProfile(AccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }
            
            var user = await _accountService.GetCurrentUserAsync();
            user.ContactName = model.User.ContactName;
            user.ContactPhone = model.User.ContactPhone;
            
            var result = await _accountService.UpdateUserAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
            
            return RedirectToAction("Index", model);
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
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
                TempData["PasswordChangeSuccess"] = "Пароль успешно изменен";
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
           
                var viewModel = await _accountService.GetAccountViewModelAsync(null);
                viewModel.ChangePassword = model;
                return View("Index", viewModel);
            }
        }
    }
}