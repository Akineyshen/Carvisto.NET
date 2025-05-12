using Carvisto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Carvisto.Services;
using Carvisto.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using Carvisto.Data;
using Carvisto.Models.ViewModels;
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
            var driversQuery = _context.Users
                .Where(u => _context.Trips.Any(t => t.DriverId == u.Id));

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                driversQuery = driversQuery.Where(d => 
                    d.ContactName.ToLower().Contains(searchString) || 
                    d.ContactPhone.ToLower().Contains(searchString) || 
                    d.Email.ToLower().Contains(searchString));
            }
            
            var drivers = await driversQuery.ToListAsync();

            Dictionary<string, double> driverRatings = new Dictionary<string, double>();

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
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isModerator = User.IsInRole("Moderator");
            
            var trips = await _tripService.GetUserTripsAsync(id);
            var reviews = await _reviewService.GetDriverReviewsAsync(id);
            foreach (var review in reviews)
            {
                review.CanDelete = isModerator || review.ReviewerId == userId;
            }
            
            var (averageRating, reviewsCount) = await _reviewService.GetDriverRatingInfoAsync(id);

            var canLeaveReview = User.Identity.IsAuthenticated &&
                                 userId != id &&
                                 await _reviewService.CanUserReviewDriverAsync(id, userId);
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
        
        // POST: /Drivers/Details (Review)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(string driverId, DriverReviewViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(driverId) || 
                model.Rating < 1 || model.Rating > 5 || 
                string.IsNullOrEmpty(model.Comment))
            {
                TempData["ErrorMessage"] = "Пожалуйста, заполните все поля отзыва";
                return RedirectToAction("Details", new { id = driverId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
            try 
            {
                await _reviewService.AddReviewAsync(driverId, userId, model.Rating, model.Comment);
                TempData["SuccessMessage"] = "Отзыв успешно добавлен!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при добавлении отзыва";
            }

            return RedirectToAction("Details", new { id = driverId });
        }
        
        // POST: /Drivers/Details/DeleteReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isModerator = User.IsInRole("Moderator");
            
            var review = await _context.DriverReviews.FindAsync(reviewId);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Отзыв не найден";
                return RedirectToAction("Details", new { id = review.DriverId });
            }
            
            var result = await _reviewService.DeleteReviewAsync(reviewId, userId, isModerator);

            if (result)
            {
                TempData["SuccessMessage"] = "Отзыв успешно удален!";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении отзыва";
            }
            
            return RedirectToAction("Details", new { id = review.DriverId });
        }
    }
}