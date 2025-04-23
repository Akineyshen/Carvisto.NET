using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carvisto.Data;
using Carvisto.Models;

namespace Carvisto.Controllers
{
    public class HomeController : Controller
    {
        private readonly CarvistoDbContext _context;

        // Embedding the DB context through the constructor
        public HomeController(CarvistoDbContext context)
        {
            _context = context;
        }

        // Home page: 9 recent trips with driver data + search form
        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                SearchModel = new SearchTripViewModel(),
                RecentTrips = await _context.Trips
                    .Include(t => t.Driver)
                    .OrderByDescending(t => t.Id)
                    .Take(9)
                    .ToListAsync()
            };
            return View(model);
        }

        // Redirecting the search query to the Trips controller
        [HttpGet]
        public async Task<IActionResult> Search(SearchTripViewModel model)
        {
            return RedirectToAction("Search", "Trips", model);
        }

        // Static Privacy Policy page
        public IActionResult Privacy()
        {
            return View();
        }
    }
}

