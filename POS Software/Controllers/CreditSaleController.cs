using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;

namespace POS_Software.Controllers
{
    public class CreditSaleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreditSaleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Fetch orders with non-zero balance
            var creditOrders = _unitOfWork.Order
                .GetAll(includeProperties: "OrderItems")
                .Where(o => o.Balance > 0)
                .ToList();

            return View(creditOrders);
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
