using Microsoft.AspNetCore.Mvc;

namespace OnlineRandevuSistemi.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}