using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Carvisto.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ITripService _tripService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(
            IBookingService bookingService,
            ITripService tripService,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _tripService = tripService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var result = await _bookingService.CreateBookingAsync(tripId, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Booking created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Booking creation failed!";
            }
            
            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var result = await _bookingService.CancelBookingAsync(id, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Booking cancelled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Booking cancellation failed!";
            }

            return RedirectToAction("Details", "Trips", new { id });
        }
    }
}

