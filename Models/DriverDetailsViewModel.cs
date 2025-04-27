using System.Collections.Generic;

namespace Carvisto.Models.ViewModels
{
    public class DriverDetailsViewModel
    {
        public ApplicationUser Driver { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
    }
}

