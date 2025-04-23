using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carvisto.Data;
using Carvisto.Models;

namespace Carvisto.Controllers
{
    public class HomeController : Controller
    {
        private readonly CarvistoDbContext _context;

        public HomeController(CarvistoDbContext context)
        {
            _context = context;
        }

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

        [HttpGet]
        public async Task<IActionResult> Search(SearchTripViewModel model)
        {
            return RedirectToAction("Search", "Trips", model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

