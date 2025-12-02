using Microsoft.AspNetCore.Mvc;

namespace GrapheneTrace.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
