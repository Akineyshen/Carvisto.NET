using System.ComponentModel.DataAnnotations;

namespace Carvisto.Models
{
    public class TripDetailsViewModel
    {
        public Trip Trip { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsModerator { get; set; }
        public bool IsDriver { get; set; }
        public bool HasActivateBooking { get; set; }
        public int? ActiveBookingId { get; set; }
        public string RouteDistance { get; set; }
        public string RouteDuration { get; set; }
    }
}