using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Check if the current user is authenticated
            if (string.IsNullOrEmpty(currentUserId))
            {
                // Render the _LoginPartial.cshtml partial view
                return PartialView("_LoginPartial");
            }

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = userRoles.Contains("Admin");

            // Fetch the store associated with the current user (if not an Admin)
            var store = isAdmin
                ? null
                : _unitOfWork.Store.Get(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId);

            // Allow access for Admin even if store is null
            if (!isAdmin && store == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Fetch all orders and include related properties
            var orders = _unitOfWork.Order.GetAll(includeProperties: "OrderItems").ToList();
            var today = DateTime.Today;

            // If the user is not Admin, filter orders based on the store
            if (!isAdmin && store != null)
            {
                orders = orders.Where(o => o.StoreId == store.Id).ToList();
            }

            // Calculate TodaySale, TodayRevenue, TotalSale, TotalRevenue, TodayOrdersCount, and TotalOrdersCount
            var dashboardViewModel = new DashboardViewModel
            {
                // Today's metrics (for the store if not Admin)
                TodaySale = orders.Where(o => o.Date.Date == today).Sum(o => o.PaidAmount),
                TodayRevenue = orders.Where(o => o.Date.Date == today).Sum(o => o.PayableAmount),

                // Total metrics (for the store if not Admin)
                TotalSale = orders.Sum(o => o.PaidAmount),
                TotalRevenue = orders.Sum(o => o.PayableAmount),

                // Today's order count (for the store if not Admin)
                TodayOrdersCount = orders.Count(o => o.Date.Date == today),

                // Total order count (for the store if not Admin)
                TotalOrdersCount = orders.Count(),

                // Store Name (if Admin, show "Admin Dashboard")
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
