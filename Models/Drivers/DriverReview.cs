using System;
using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class DriverReview
    {
        public int Id { get; set; }
        
        [Required]
        public string DriverId { get; set; }
        
        [Required]
        public string ReviewerId { get; set; }
        
        [Required]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500, ErrorMessage = "Review cannot exceed 500 characters.")]
        public string Comment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public bool CanDelete { get; set; }
        
        public ApplicationUser Driver { get; set; }
        public ApplicationUser Reviewer { get; set; }
    }
}