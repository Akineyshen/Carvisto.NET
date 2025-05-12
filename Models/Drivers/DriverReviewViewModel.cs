using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class DriverReviewViewModel
    {
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }
    }
}

