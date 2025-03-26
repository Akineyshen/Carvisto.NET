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
            var trips = _context.Trips.AsQueryable();

            if (!string.IsNullOrEmpty(model.StartLocation))
            {
                trips = trips.Where(t => t.StartLocation.StartsWith(model.StartLocation));
            }

            if (!string.IsNullOrEmpty(model.EndLocation))
            {
                trips = trips.Where(t => t.EndLocation.EndsWith(model.EndLocation));
            }

            if (model.DepartureDate.HasValue)
            {
                trips = trips.Where(t => t.DepartureDateTime.Date == model.DepartureDate.Value.Date);
            }
            
            var results = await trips.ToListAsync();
            
            return View("SearchResults", results);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

