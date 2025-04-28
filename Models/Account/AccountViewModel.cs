using System.Collections.Generic;

namespace Carvisto.Models
{
    public class AccountViewModel
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public IEnumerable<Trip> UserTrips { get; set; } = new List<Trip>();
        public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();
        public IFormFile ProfileImage { get; set; }
    }
}