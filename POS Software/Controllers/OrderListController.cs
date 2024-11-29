using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using static POS_Software.DTOs.OrderListDTO;

namespace POS_Software.Controllers
{
    public class OrderListController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderListController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            List<Order> objOrderList = _unitOfWork.Order.GetAll(includeProperties: "OrderItems").ToList();
            return View(objOrderList);
        }

        #region API CALLS

        // https://localhost:7230/orderList/getall => to get all the propoerties of orderList in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _unitOfWork.Order.GetAll(includeProperties: "OrderItems");
            var orderDtos = orders.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                Email = o.Email,
                Phone = o.Phone,
                TotalAmount = o.TotalAmount,
                Discount = o.Discount,
                PayableAmount = o.PayableAmount,
                PaidAmount = o.PaidAmount,
                PaymentMode = o.PaymentMode,
                Balance = o.Balance,
                InvoiceNumber = o.InvoiceNumber,
                Cashier = _userManager.FindByIdAsync(o.CashierId).Result?.Name,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId.ToString(),
                    Quantity = oi.Quantity,
                    Price = oi.Amount
                }).ToList()
            }).ToList();

            return Json(new { data = orderDtos });
        }


        #endregion
    }
}
