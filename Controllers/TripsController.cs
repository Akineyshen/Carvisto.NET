using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carvisto.Models;

namespace Carvisto.Controllers
{
    public class TripsController : Controller
    {
        private readonly CarvistoDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TripsController(CarvistoDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Trips/Create
        [Authorize]
        public IActionResult Create()
        {
            Console.WriteLine("Открыта страница создания путешествия");
            Console.WriteLine($"Пользователь аутентифицирован: {User.Identity.IsAuthenticated}");
            if (User.Identity.IsAuthenticated)
            {
                Console.WriteLine($"Имя пользователя: {User.Identity.Name}");
            }
            return View();
        }

        // POST: /Trips/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Trip trip)
        {
            Console.WriteLine("Получен запрос на создание путешествия");
            Console.WriteLine($"Пользователь аутентифицирован: {User.Identity.IsAuthenticated}");

            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("Ошибка: Пользователь не аутентифицирован");
                ModelState.AddModelError(string.Empty, "Вы должны войти в систему, чтобы создать путешествие.");
                return View(trip);
            }

            Console.WriteLine("Claims пользователя:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            // Удаляем DriverId из валидации
            ModelState.Remove("DriverId");

            // Присваиваем DriverId
            var user = await _userManager.GetUserAsync(User);
            trip.DriverId = user?.Id ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"DriverId назначен: {trip.DriverId}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"Создание путешествия: Откуда={trip.From}, Куда={trip.To}, Дата={trip.DepartureDate}, Цена={trip.Price}");

                if (string.IsNullOrEmpty(trip.DriverId))
                {
                    Console.WriteLine("Ошибка: Не удалось определить ID водителя");
                    ModelState.AddModelError(string.Empty, "Не удалось определить ID водителя. Проверьте аутентификацию.");
                    return View(trip);
                }

                try
                {
                    _context.Add(trip);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Путешествие успешно сохранено в базе данных");
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении в базу данных: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении путешествия: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Модель не прошла валидацию");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка валидации: {error.ErrorMessage}");
                }
            }
            return View(trip);
        }

        // GET: /Trips
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trips.ToListAsync();
            return View(trips);
        }
    }
}