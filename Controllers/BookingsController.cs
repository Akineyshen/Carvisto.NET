using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Carvisto.Models.ViewModels;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var bookings = await _bookingService.GetUserBookingASync(user.Id);

            var viewModel = bookings.Select(b => new UserBookingViewModel
            {
                Id = b.Id,
                TripId = b.TripId,
                BookingDate = b.BookingDate,
                DepartureDate = b.Trip.DepartureDateTime,
                StartLocation = b.Trip.StartLocation,
                EndLocation = b.Trip.EndLocation,
                DriverName = b.Trip.Driver.ContactName,
                Price = b.Trip.Price,
                BookingStatus = GetBookingsStatus(b)
            }).ToList();
            
            return View(viewModel);
        }

        private string GetBookingsStatus(Booking booking)
        {
            if (booking.IsCancelled)
                return "Cancelled";
            
            if (booking.Trip.DepartureDateTime < DateTime.Now)
                return "Expired";
            
            return "Active";
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
            var booking = await _bookingService.GetBookingByIdAsync(id);
            int tripId = booking.TripId;
            
            var result = await _bookingService.CancelBookingAsync(id, user.Id);

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

        [HttpGet]
        public async Task<IActionResult> Receipt(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var receipt = await _bookingService.GetBookingReceiptAsync(id, user.Id);

                if (receipt == null)
                {
                    return NotFound();
                }

                var pdf = new ViewAsPdf("Receipt", receipt)
                {
                    FileName = $"Booking_{receipt.BookingId}.pdf",
                    PageOrientation = Orientation.Portrait,
                    PageSize = Size.A4,
                    CustomSwitches = "--disable-smart-shrinking --enable-local-file-access",
                    IsJavaScriptDisabled = true
                };

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

