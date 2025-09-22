using Microsoft.AspNetCore.Mvc;

namespace EmployeeMaster.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
