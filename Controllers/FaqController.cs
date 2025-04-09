using Microsoft.AspNetCore.Mvc;

namespace Carvisto.Controllers
{
    //GET: Info page
    public class FaqController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}