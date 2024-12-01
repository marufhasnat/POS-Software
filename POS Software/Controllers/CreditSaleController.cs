using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace POS_Software.Controllers
{
    [Authorize]
    public class CreditSaleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreditSaleController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not logged in or invalid.");
            }

            var userRoles = _unitOfWork.ApplicationUser.GetUserRoles(currentUserId) ?? new List<string>();
            List<Order> creditOrders;

            if (userRoles.Contains("Admin"))
            {
                // Admin: Fetch all credit orders
                creditOrders = _unitOfWork.Order
                    .GetAll(o => o.Balance > 0, includeProperties: "OrderItems,Store")
                    .ToList();
            }
            else
            {
                // Manager or Cashier: Filter credit orders by storeId
                var stores = _unitOfWork.Store
                    .GetAll(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId)
                    .ToList();

                if (!stores.Any())
                {
                    return View(new List<Order>()); // Return empty list if no stores are linked
                }

                var storeIds = stores.Select(s => s.Id).ToList();

                creditOrders = _unitOfWork.Order
                    .GetAll(o => o.Balance > 0 && storeIds.Contains(o.StoreId), includeProperties: "OrderItems,Store")
                    .ToList();
            }

            return View(creditOrders ?? new List<Order>());
        }


        [HttpPost]
        public IActionResult UpdateDue(Guid orderId, decimal paymentAmount)
        {
            // Retrieve the order using the OrderId
            var order = _unitOfWork.Order.Get(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            // Update the balance
            order.PaidAmount += paymentAmount;
            order.Balance -= paymentAmount;

            if (order.Balance < 0)
            {
                TempData["error"] = "Payment amount exceeds the due balance.";
                return RedirectToAction("Index");
            }

            // Save changes
            _unitOfWork.Save();

            TempData["success"] = "Payment updated successfully.";
            return RedirectToAction("Index");
        }

    }
}
