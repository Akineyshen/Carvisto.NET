using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Carvisto.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "You must specify the start location")]
        public string StartLocation { get; set; }
        
        [Required(ErrorMessage = "You must specify the end location")]
        public string EndLocation { get; set; }
        
        [Required(ErrorMessage = "You must specify the departure date and time")]
        public DateTime DepartureDateTime { get; set; }
        
        [Required(ErrorMessage = "You must specify the price")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
        
        [Required]
        public string VehicleBrand { get; set; }
        
        public string Comments { get; set; }
        
        [ForeignKey("Driver")]
        public string DriverId { get; set; }
        public ApplicationUser? Driver { get; set; }
        
        [Required(ErrorMessage = "The number of seats is mandatory")]
        public int Seats { get; set; }
        
        public int AvailableSeats { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

