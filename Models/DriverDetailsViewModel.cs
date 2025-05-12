using System.Collections.Generic;

namespace Carvisto.Models.ViewModels
{
    public class DriverDetailsViewModel
    {
        public ApplicationUser Driver { get; set; }
        public IEnumerable<Trip> Trips { get; set; }
        public IEnumerable<DriverReview> Reviews { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public bool CanLeaveReview { get; set; }
        public DriverReviewViewModel NewReview { get; set; }
    }
}

