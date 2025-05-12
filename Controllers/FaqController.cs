using Microsoft.AspNetCore.Mvc;

namespace Carvisto.Controllers
{
    //GET: FAQ/
    public class FaqController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}