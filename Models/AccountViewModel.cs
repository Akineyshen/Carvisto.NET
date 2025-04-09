using System.Collections.Generic;

namespace Carvisto.Models
{
    public class AccountViewModel
    {
        public UserSettingsViewModel UserSettings { get; set; } = new UserSettingsViewModel();
        public IEnumerable<Trip> UserTrips { get; set; } = new List<Trip>();
    }
}