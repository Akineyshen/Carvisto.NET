using Carvisto.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Carvisto.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUserAsync(string email, string password, string contactName, string contactPhone);
        Task<SignInResult> LoginUserAsync(string email, string password, bool rememberMe);
        Task SignInUserAsync(ApplicationUser user, bool isPersistent);
        Task LogoutAsync();
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task AddUserToRoleAsync(ApplicationUser user, string role);
        Task<AccountViewModel> GetAccountViewModelAsync(string userId);
        Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<string> SaveProfileImageAsync(ApplicationUser user, string uploadFolder, string fileName, byte[] fileBytes);
        Task<bool> DeleteProfileImageAsync(ApplicationUser user);
    }
}