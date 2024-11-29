using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;
using System.Diagnostics;

namespace POS_Software.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Check if the current user is an Admin
            var userRoles = _userManager.GetRolesAsync(_userManager.FindByIdAsync(currentUserId).Result).Result;
            var isAdmin = userRoles.Contains("Admin");

            // Fetch the store associated with the current user (if not an Admin)
            var store = isAdmin
                ? null
                : _unitOfWork.Store.Get(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId);

            // Allow access for Admin even if store is null
            if (!isAdmin && store == null)
            {
                return RedirectToAction("AccessDenied", "Account"); // Redirect if not Admin and no store is found
            }

            // Fetch all orders and include related properties
            var orders = _unitOfWork.Order.GetAll(includeProperties: "OrderItems").ToList();
            var today = DateTime.Today;

            // Create the DashboardViewModel
            var dashboardViewModel = new DashboardViewModel
            {
                // Today's metrics
                TodaySale = orders.Where(o => o.Date.Date == today).Sum(o => o.PaidAmount),
                TodayRevenue = orders.Where(o => o.Date.Date == today).Sum(o => o.PayableAmount),

                // Overall metrics
                TotalSale = orders.Sum(o => o.PaidAmount),
                TotalRevenue = orders.Sum(o => o.PayableAmount),

                // Additional details
                TodayOrdersCount = orders.Count(o => o.Date.Date == today),
                TotalOrdersCount = orders.Count(),

                // Use the store name if available; otherwise, set a default for Admin
                StoreName = store?.Name ?? "Admin Dashboard"
            };

            return View(dashboardViewModel);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
