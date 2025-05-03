using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Carvisto.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string ContactName { get; set; }
        
        [Required]
        public string ContactPhone { get; set; }
        
        [AllowNull]
        public string? ProfileImagePath { get; set; }
    }
}