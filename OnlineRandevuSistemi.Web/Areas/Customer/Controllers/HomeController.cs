// Areas/Customer/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace OnlineRandevuSistemi.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}