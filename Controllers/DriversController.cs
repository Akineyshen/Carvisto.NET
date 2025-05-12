using Carvisto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carvisto.Services;
using Carvisto.Models.ViewModels;
using System.Security.Claims;
using Carvisto.Data;
using Microsoft.AspNetCore.Authorization;

namespace Carvisto.Controllers
{
    public class DriversController : Controller
    {
        private readonly CarvistoDbContext _context;
        private readonly IReviewService _reviewService;
        private readonly ITripService _tripService;

        public DriversController(
            CarvistoDbContext context,
            IReviewService reviewService,
            ITripService tripService)
        {
            _context = context;
            _reviewService = reviewService;
            _tripService = tripService;
        }
        
        //GET: /Drivers
        public async Task<IActionResult> Index(string searchString)
        {
            // This action retrieves a list of drivers who have trips.
            var driversQuery = _context.Users
                .Where(u => _context.Trips.Any(t => t.DriverId == u.Id));

            // Filter drivers based on the search string.
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                driversQuery = driversQuery.Where(d => 
                    d.Email != null && 
                    (d.ContactName.ToLower().Contains(searchString) || 
                     d.ContactPhone.ToLower().Contains(searchString) || 
                     d.Email.ToLower().Contains(searchString)));
            }
            
            // Include the trips for each driver.
            var drivers = await driversQuery.ToListAsync();

            // Get the average rating for each driver.
            Dictionary<string, double> driverRatings = new Dictionary<string, double>();

            // Fetch the rating for each driver.
            foreach (var driver in drivers)
            {
                var (rating, _) = await _reviewService.GetDriverRatingInfoAsync(driver.Id);
                driverRatings[driver.Id] = rating;
            }
            
            ViewBag.DriverRatings = driverRatings;
            ViewData["CurrentFilter"] = searchString;
            
            return View(drivers);
        }
        
        // GET: /Drivers/Details
        public async Task<IActionResult> Details(string? id)
        {
            if (id != null)
            {
                return NotFound();
            }
            
            // This action retrieves the details of a specific driver.
            var driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }
            // Check if the driver has any trips.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isModerator = User.IsInRole("Moderator");
            
            // Fetch the trips for the driver.
            var trips = await _tripService.GetUserTripsAsync(id);
            var reviews = await _reviewService.GetDriverReviewsAsync(id);
            
            // Set the canDelete property for each review based on the user's role.
            foreach (var review in reviews)
            {
                review.CanDelete = isModerator || review.ReviewerId == userId;
            }
            
            // Get the average rating and review count for the driver.
            var (averageRating, reviewsCount) = await _reviewService.GetDriverRatingInfoAsync(id);

            // Check if the user can leave a review for the driver.
            var canLeaveReview = User.Identity.IsAuthenticated &&
                                 userId != id &&
                                 await _reviewService.CanUserReviewDriverAsync(id, userId);
            
            // Create the view model for the driver details.
            var viewModel = new DriverDetailsViewModel
            {
                Driver = driver,
                Trips = trips,
                Reviews = reviews,
                AverageRating = averageRating,
                ReviewsCount = reviewsCount,
                CanLeaveReview = canLeaveReview,
                NewReview = new DriverReviewViewModel()
            };
            
            return View(viewModel);
        }
        
        // POST: /Drivers/Details (Add Review)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(string driverId, DriverReviewViewModel model)
        {
            // This action adds a review for a specific driver.
            if (!ModelState.IsValid || string.IsNullOrEmpty(driverId) || 
                model.Rating < 1 || model.Rating > 5 || 
                string.IsNullOrEmpty(model.Comment))
            {
                TempData["ErrorMessage"] = "Please fill in all fields of the review";
                return RedirectToAction("Details", new { id = driverId });
            }

            // Check if the user is authenticated and can leave a review.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
            // Check if the user is allowed to leave a review.
            try 
            {
                await _reviewService.AddReviewAsync(driverId, userId, model.Rating, model.Comment);
                TempData["SuccessMessage"] = "Review has been successfully added!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "There was an error when adding a review";
            }

            return RedirectToAction("Details", new { id = driverId });
        }
        
        // POST: /Drivers/Details (Delete Review)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            // This action deletes a review for a specific driver.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isModerator = User.IsInRole("Moderator");
            
            // Check if the review exists.
            var review = await _context.DriverReviews.FindAsync(reviewId);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Review not found";
                return RedirectToAction("Details", new { id = review.DriverId });
            }
            
            // Check if the user is allowed to delete the review.
            var result = await _reviewService.DeleteReviewAsync(reviewId, userId, isModerator);

            if (result)
            {
                TempData["SuccessMessage"] = "The review has been successfully deleted!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error when deleting a review";
            }
            
            return RedirectToAction("Details", new { id = review.DriverId });
        }
    }
}