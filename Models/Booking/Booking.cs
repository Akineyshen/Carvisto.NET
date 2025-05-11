using System;
using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        [Required]
        public int TripId { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public bool IsCancelled { get; set; } = false;
        
        public virtual Trip Trip { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}

