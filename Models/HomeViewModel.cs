using System.Collections.Generic;

namespace Carvisto.Models
{
    public class HomeViewModel
    {
        public SearchTripViewModel SearchModel { get; set; }
        public List<Trip> RecentTrips { get; set; }
    }
}