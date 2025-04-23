using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Carvisto.Controllers
{
    public class TripsController : Controller
    {
        private readonly ITripService _tripService; // Service for working with trips
        private readonly UserManager<ApplicationUser> _userManager; // User Manager

        // DI constructor
        public TripsController(ITripService tripService, UserManager<ApplicationUser> userManager)
        {
            _tripService = tripService;
            _userManager = userManager;
        }

        // GET: List of all trips
        public async Task<IActionResult> Index()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return View(trips);
        }

        // GET: Form for creating a trip
        [Authorize]
        public IActionResult Create()
        {
            // Setting the current user as the driver
            ViewBag.DriverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        // POST: Processing the creation of a trip
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Trip trip)
        {
            if (!ModelState.IsValid)
            {
                // Logging validation errors
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Error: {state.Key}: {state.Value.Errors[0].ErrorMessage}");

                    }
                }
                return View(trip);
            }

            try
            {
                // Linking the current user as a driver
                trip.DriverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                trip.Driver = await _userManager.FindByIdAsync(trip.DriverId);

                await _tripService.CreateTripAsync(trip);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании поездки: {ex.Message}");
                return View(trip);
            }
        }

        // GET: Trip editing form
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);

            // Checking access rights (owner or moderator)
            if (trip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            return View(trip);
        }

        // POST: Editing Processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            var existingTrip = await _tripService.GetTripByIdAsync(id);

            if (existingTrip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    trip.DriverId = existingTrip.DriverId; // Сохраняем оригинального водителя
                    await _tripService.UpdateTripAsync(trip);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_tripService.TripExists(trip.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trip);
        }

        // GET: Confirmation of deletion
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);

            if (trip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            return View(trip);
        }

        // POST: Deletion Processing
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            
            if (trip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            await _tripService.DeleteTripAsync(id);
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Search trips
        [HttpGet]
        public async Task<IActionResult> Search(SearchTripViewModel model)
        {
            var trips = _tripService.GetTripsQuery();

            if (!string.IsNullOrEmpty(model.StartLocation))
            {
                trips = trips
                    .Where(t => t.StartLocation.StartsWith(model.StartLocation));
            }

            if (!string.IsNullOrEmpty(model.EndLocation))
            {
                trips = trips
                    .Where(t => t.EndLocation.EndsWith(model.EndLocation));
            }

            if (model.DepartureDate.HasValue)
            {
                trips = trips
                    .Where(t => t.DepartureDateTime.Date == model.DepartureDate.Value.Date);
            }

            var results = await trips.ToListAsync();
            
            return View("Index", results);
        }
    }
}