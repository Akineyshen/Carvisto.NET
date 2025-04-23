using System.Collections.Generic;

namespace Carvisto.Models
{
    public class HomeViewModel
    {
        public required SearchTripViewModel SearchModel { get; set; }
        public List<Trip> RecentTrips { get; set; }
    }
}