using System;

namespace Carvisto.Models.ViewModels
{
    public class UserBookingViewModel
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public string DriverName { get; set; }
        public decimal Price { get; set; }
        public string BookingStatus { get; set; }
    }

    public enum BookingStatus
    {
        Active,
        Cancelled,
        Completed
    }
}