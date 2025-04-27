using Carvisto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Carvisto.Data;
using Carvisto.Models.ViewModels;

namespace Carvisto.Controllers
{
    public class DriversController : Controller
    {
        private readonly CarvistoDbContext _context;

        public DriversController(CarvistoDbContext context)
        {
            _context = context;
        }
        
        //GET: /Drivers
        public async Task<IActionResult> Index(string searchString)
        {
            var driversQuery = _context.Users
                .Where(u => _context.Trips.Any(t => t.DriverId == u.Id));

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                driversQuery = driversQuery.Where(d => 
                    d.ContactName.ToLower().Contains(searchString) || 
                    d.ContactPhone.ToLower().Contains(searchString) || 
                    d.Email.ToLower().Contains(searchString));
            }
            
            var drivers = await driversQuery.ToListAsync();
            
            ViewData["CurrentFilter"] = searchString;
            
            return View(drivers);
        }
        
        // GET: /Drivers/Details
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var hasTrips = await _context.Trips.AnyAsync(t => t.DriverId == id);
            if (!hasTrips)
            {
                return NotFound();
            }
            
            var driver = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            if (driver == null)
            {
                return NotFound();
            }
            
            var driverTrips = await _context.Trips
                .Where(t => t.DriverId == id)
                .OrderByDescending(t => t.DepartureDateTime)
                .ToListAsync();

            var viewModel = new DriverDetailsViewModel
            {
                Driver = driver,
                Trips = driverTrips
            };
            
            return View(viewModel);
        }
    }
}