using Microsoft.AspNetCore.Mvc;

namespace POS_Software.Areas.Admin.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
