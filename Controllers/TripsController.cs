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
        private readonly IGoogleMapsService _mapsService; // Google Maps service
        private readonly IBookingService _bookingService; // Service for working with bookings

        // DI constructor
        public TripsController(
            ITripService tripService,
            UserManager<ApplicationUser> userManager,
            IGoogleMapsService mapsService,
            IBookingService bookingService)
        {
            _tripService = tripService;
            _userManager = userManager;
            _mapsService = mapsService;
            _bookingService = bookingService;
        }

        // GET: Trips/
        public async Task<IActionResult> Index()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return View(trips);
        }

        // GET: Trips/Create
        [Authorize]
        public IActionResult Create()
        {
            // Setting the current user as the driver
            ViewBag.DriverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View();
        }

        // POST: Trips/Create
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
                ModelState.AddModelError("", $"Error when creating a trip: {ex.Message}");
                return View(trip);
            }
        }
        
        // GET: Trips/Edit/{id}
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

        // POST: Trips/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Trip trip)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            // Checking access rights (owner or moderator)
            var existingTrip = await _tripService.GetTripByIdAsync(id);

            // If the trip is not found
            if (existingTrip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            // Checking the model state
            if (ModelState.IsValid)
            {
                try
                {
                    // Updating the trip
                    trip.DriverId = existingTrip.DriverId;
                    await _tripService.UpdateTripAsync(trip);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Checking if the trip exists
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

        // GET: Trips/Delete/{id}
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // Checking access rights (owner or moderator)
            var trip = await _tripService.GetTripByIdAsync(id);

            // If the trip is not found
            if (trip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            return View(trip);
        }

        // POST: Trips/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Checking access rights (owner or moderator)
            var trip = await _tripService.GetTripByIdAsync(id);
            
            // If the trip is not found
            if (trip.DriverId != User.FindFirstValue(ClaimTypes.NameIdentifier) && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }
            
            // Deleting the trip
            await _tripService.DeleteTripAsync(id);
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Trips/Search
        [HttpGet]
        public async Task<IActionResult> Search(string StartLocation, string EndLocation, DateTime? DepartureDate, SearchTripViewModel model)
        {
            // Checking if the model is valid
            var trips = _tripService.GetTripsQuery();

            // Filtering trips based on the search criteria
            if (!string.IsNullOrEmpty(model.StartLocation))
            {
                trips = trips
                    .Where(t => t.StartLocation.StartsWith(model.StartLocation));
            }

            // Filtering trips based on the search criteria
            if (!string.IsNullOrEmpty(model.EndLocation))
            {
                trips = trips
                    .Where(t => t.EndLocation.EndsWith(model.EndLocation));
            }

            // Filtering trips based on the search criteria
            if (model.DepartureDate.HasValue)
            {
                trips = trips
                    .Where(t => t.DepartureDateTime.Date == model.DepartureDate.Value.Date);
            }

            // Sorting trips by departure date
            var results = await trips.ToListAsync();

            // Setting the search criteria in the ViewData
            ViewData["StartLocation"] = StartLocation;
            ViewData["EndLocation"] = EndLocation;
            ViewData["DepartureDate"] = DepartureDate?.ToString("yyyy-MM-dd");
            
            return View("Index", results);
        }
        
        // GET: Trips/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Getting the trip by ID
            var trip = await _tripService.GetTripByIdAsync(id);
            var passengers = await _tripService.GetPassengersUsersAsync(id);

            if (trip == null)
            {
                return NotFound();
            }

            // Checking if the trip is active
            var viewModel = new TripDetailsViewModel 
            {
                Trip = trip,
                IsAuthenticated = User.Identity.IsAuthenticated,
                IsModerator = User.IsInRole("Moderator"),
                IsDriver = false,
                HasActivateBooking = false,
                Passengers = passengers,
            };

            // Checking if the user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                // Getting the current user
                var user = await _userManager.GetUserAsync(User);
                viewModel.IsDriver = trip.DriverId == user.Id;
                viewModel.IsModerator = User.IsInRole("Moderator");
                    
                // Checking if the user is a passenger
                var activeBooking = await _bookingService
                    .GetUserActiveBookingAsync(id, user.Id);
                viewModel.HasActivateBooking = activeBooking != null;
                if (activeBooking != null)
                {
                    viewModel.ActiveBookingId = activeBooking.Id;
                }
            }
                
            // Getting the route information
            var routeInfo = await _mapsService.GetRouteInfoAsync(
                    trip.StartLocation, 
                trip.EndLocation);
                
            // Setting the route information in the view model
            viewModel.RouteDistance = routeInfo.Distance;
            viewModel.RouteDuration = routeInfo.Duration;

            return View(viewModel);
        }
    }
}