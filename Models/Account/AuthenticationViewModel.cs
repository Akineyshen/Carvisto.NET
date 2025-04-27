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
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage = "You must specify the name")]
        public string ContactName { get; set; }
        
        [Required(ErrorMessage = "You must specify the phone number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactPhone { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}

