using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class AuthenticationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginViewModel : AuthenticationViewModel
    {
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel : AuthenticationViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}

