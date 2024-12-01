using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;

namespace POS_Software.Controllers
{
    [Authorize]
    public class ExpireController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpireController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
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
            List<Product> expiredProducts;

            if (userRoles.Contains("Admin"))
            {
                expiredProducts = _unitOfWork.Product
                    .GetAll(p => p.ExpiryDate < DateTime.Now, includeProperties: "Store")
                    .ToList();
            }
            else
            {
                var stores = _unitOfWork.Store
                    .GetAll(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId)
                    .ToList();

                if (!stores.Any())
                {
                    return View(new List<Product>()); // Return empty list if no stores are linked
                }

                var storeIds = stores.Select(s => s.Id).ToList();

                expiredProducts = _unitOfWork.Product
                    .GetAll(p => p.ExpiryDate < DateTime.Now && storeIds.Contains(p.StoreId), includeProperties: "Store")
                    .ToList();
            }

            return View(expiredProducts ?? new List<Product>());
        }


    }
}
