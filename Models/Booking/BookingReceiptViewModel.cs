using System;

namespace Carvisto.Models.ViewModels
{
    public class BookingReceiptViewModel
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public string PassengerName { get; set; }
        public string DriverName { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public decimal  Price { get; set; }
        public string BookingStatus { get; set; }
        public string BookingReference { get; set; }
    }
}