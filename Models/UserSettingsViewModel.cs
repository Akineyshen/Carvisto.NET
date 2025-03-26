using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class UserSettingsViewModel
    {
        [Required]
        public string ContactName { get; set; }
        
        [Required]
        public string ContactPhone { get; set; }
    }
}