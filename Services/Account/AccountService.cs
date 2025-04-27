using Carvisto.Data;
using Carvisto.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carvisto.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CarvistoDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CarvistoDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> RegisterUserAsync(string email, string password, string contactName, string contactPhone)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                ContactName = contactName,
                ContactPhone = contactPhone
            };
            
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<SignInResult> LoginUserAsync(string email, string password, bool rememberMe)
        {
            return await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        }

        public async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }
        
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task AddUserToRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<AccountViewModel> GetAccountViewModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            
            var trips = await _context.Trips
                .Where(t => t.DriverId == user.Id)
                .ToListAsync();

            return new AccountViewModel
            {
                User = user,
                UserTrips = trips,
                ChangePassword = new ChangePasswordViewModel()
            };
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword,
            string newPassword)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password",
                });
            }
            
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
        
        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}