using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Carvisto.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string StartLocation { get; set; }
        
        [Required]
        public string EndLocation { get; set; }
        
        [Required]
        public DateTime DepartureDateTime { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        public string VehicleBrand { get; set; }
        
        public string Comments { get; set; }
        
        
        [ForeignKey("Driver")]
        public string DriverId { get; set; }
        public ApplicationUser? Driver { get; set; }
    }
}

