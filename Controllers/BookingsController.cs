using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Carvisto.Models.ViewModels;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Carvisto.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        // This controller handles booking-related actions for users.
        private readonly IBookingService _bookingService;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor to inject dependencies.
        public BookingsController(
            IBookingService bookingService,
            UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        // GET: Bookings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // This action retrieves the list of bookings for the logged-in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Check if the user has any bookings.
            var bookings = await _bookingService.GetUserBookingASync(user.Id);

            // If no bookings are found, return an empty list.
            var viewModel = bookings.Select(b => new UserBookingViewModel
            {
                Id = b.Id,
                TripId = b.TripId,
                BookingDate = b.BookingDate,
                DepartureDate = b.Trip.DepartureDateTime,
                StartLocation = b.Trip.StartLocation,
                EndLocation = b.Trip.EndLocation,
                DriverName = b.Trip.Driver?.ContactName ?? "N/A",
                Price = b.Trip.Price,
                BookingStatus = GetBookingsStatus(b)
            }).ToList();
            
            return View(viewModel);
        }

        // GET: Bookings (Status)
        private string GetBookingsStatus(Booking booking)
        {
            if (booking.IsCancelled)
                return "Cancelled";
            
            if (booking.Trip.DepartureDateTime < DateTime.Now)
                return "Expired";
            
            return "Active";
        }

        // POST: Bookings (Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId)
        {
            // This action creates a new booking for the specified trip.
            var user = await _userManager.GetUserAsync(User);
            var result = user != null && await _bookingService.CreateBookingAsync(tripId, user.Id);

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

        // POST: Bookings (Cancel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var booking = await _bookingService.GetBookingByIdAsync(id);
            int tripId = booking.TripId;
            
            var result = user != null && await _bookingService.CancelBookingAsync(id, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Booking cancelled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Booking cancellation failed!";
            }

            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        // GET: Bookings (Receipt)
        [HttpGet]
        public async Task<IActionResult> Receipt(int id)
        {
            // This action retrieves the booking receipt for the specified booking ID.
            // It generates a PDF receipt using Rotativa and returns it as a file.
            try
            {
                // Ensure the user is authenticated.
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }
                
                var receipt = await _bookingService.GetBookingReceiptAsync(id, user.Id);

                // Generate the PDF receipt using Rotativa
                var pdf = new ViewAsPdf("Receipt", receipt)
                {
                    FileName = $"Booking_{receipt.BookingId}.pdf",
                    PageOrientation = Orientation.Portrait,
                    PageSize = Size.A4,
                    CustomSwitches = "--disable-smart-shrinking --enable-local-file-access",
                    IsJavaScriptDisabled = true
                };

                // Render the PDF and return it as a file
                var binary = await pdf.BuildFile(ControllerContext);
                return File(binary, "application/pdf", $"Booking_{receipt.BookingId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

