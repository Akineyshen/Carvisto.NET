using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Carvisto.Controllers
{
    [Authorize]
    public class MyTripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyTripsController(
            ITripService tripService,
            UserManager<ApplicationUser> userManager)
        {
            _tripService = tripService;
            _userManager = userManager;
        }

        // GET: MyTrips
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            
            // Get the user's trips
            var trips = await _tripService.GetUserTripsAsync(user.Id);
            return View(trips);
        }
    }
}