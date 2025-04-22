using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carvisto.Data;
using Carvisto.Models;

namespace Carvisto.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly CarvistoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TripsController(CarvistoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips.Include(t => t.Driver).ToListAsync();
            return View(trips);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.ContactName == "Unknown" || user.ContactPhone == "Unknown")
            {
                TempData["ErrorMessage"] = "Please update your contact information in settings before creating a trip.";
                return RedirectToAction("Index", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trip trip)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.ContactName == "Unknown" || user.ContactPhone == "Unknown")
            {
                TempData["ErrorMessage"] = "Please update your contact information in settings before creating a trip.";
                return RedirectToAction("Index", "Account");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User is not authenticated.");
                return View(trip);
            }

            trip.DriverId = userId;
            ModelState.Remove("DriverId");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(trip);
            }

            try
            {
                _context.Add(trip);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Trip created with DriverId: {trip.DriverId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving trip: {ex.Message}");
                return View(trip);
            }

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isModerator = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Moderator");

            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"Trip Driver ID: {trip.DriverId}");
            Console.WriteLine($"Is Moderator: {isModerator}");

            if (string.IsNullOrEmpty(trip.DriverId))
            {
                TempData["ErrorMessage"] = "This trip has no assigned driver. Please contact support.";
                return RedirectToAction("Index", "Account");
            }

            if (trip.DriverId != userId && !isModerator)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this trip.";
                return RedirectToAction("Index", "Account");
            }

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id) return NotFound();

            var originalTrip = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalTrip == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isModerator = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Moderator");

            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"Trip Driver ID: {originalTrip.DriverId}");
            Console.WriteLine($"Is Moderator: {isModerator}");

            if (string.IsNullOrEmpty(originalTrip.DriverId))
            {
                TempData["ErrorMessage"] = "This trip has no assigned driver. Please contact support.";
                return RedirectToAction("Index", "Account");
            }

            if (originalTrip.DriverId != userId && !isModerator)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this trip.";
                return RedirectToAction("Index", "Account");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                return View(trip);
            }

            try
            {
                originalTrip.StartLocation = trip.StartLocation;
                originalTrip.EndLocation = trip.EndLocation;
                originalTrip.DepartureDateTime = trip.DepartureDateTime;
                originalTrip.Price = trip.Price;
                originalTrip.VehicleBrand = trip.VehicleBrand;
                originalTrip.Comments = trip.Comments;
                originalTrip.DriverId = originalTrip.DriverId;

                _context.Update(originalTrip);
                await _context.SaveChangesAsync();
                Console.WriteLine("Trip updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving trip: {ex.Message}");
                return View(trip);
            }

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isModerator = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Moderator");

            if (trip.DriverId != userId && !isModerator) return Forbid();

            return View(trip);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isModerator = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Moderator");

            if (trip.DriverId != userId && !isModerator) return Forbid();

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Account");
        }
    }
}