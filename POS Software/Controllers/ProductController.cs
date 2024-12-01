using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;
using POS.Utility;

namespace POS_Software.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Get all suppliers, categories, stores
            var suppliers = _unitOfWork.Supplier.GetAll().ToList();
            var categories = _unitOfWork.Category.GetAll().ToList();
            var stores = _unitOfWork.Store.GetAll().ToList();

            ProductViewModel productViewModel = new ProductViewModel
            {
                Suppliers = suppliers.Select(supplier => new SelectListItem
                {
                    Value = supplier.Id.ToString(),
                    Text = supplier.SupplierName
                }).ToList(),

                Categories = categories.Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                }).ToList(),

                Stores = stores.Select(store => new SelectListItem
                {
                    Value = store.Id.ToString(),
                    Text = store.Name
                }).ToList(),
            };

            return View(productViewModel);
        }

        [HttpGet]
        public IActionResult Upsert(Guid? id)
        {
            // Get all suppliers, categories, stores
            var suppliers = _unitOfWork.Supplier.GetAll().ToList();
            var categories = _unitOfWork.Category.GetAll().ToList();
            var stores = _unitOfWork.Store.GetAll().ToList();

            // Initialize the ViewModel
            ProductViewModel productViewModel = new ProductViewModel
            {
                Suppliers = suppliers.Select(supplier => new SelectListItem
                {
                    Value = supplier.Id.ToString(),
                    Text = supplier.SupplierName
                }).ToList(),

                Categories = categories.Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                }).ToList(),

                Stores = stores.Select(store => new SelectListItem
                {
                    Value = store.Id.ToString(),
                    Text = store.Name
                }).ToList(),
            };

            // Handle product retrieval for update
            if (id.HasValue)
            {
                var product = _unitOfWork.Product.Get(p => p.Id == id.Value);
                if (product == null)
                {
                    TempData["error"] = "Store doesn't exist.";
                    return RedirectToAction("Index");
                }

                // Populate the ViewModel with the existing store data
                productViewModel.Product = product;
                productViewModel.SupplierId = product.SupplierId;
                productViewModel.CategoryId = product.CategoryId;
                productViewModel.StoreId = product.StoreId;
            }
            else
            {
                // For creation, initialize a new Product instance
                productViewModel.Product = new Product();
            }

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productViewModel)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Log errors to debug
                }
                TempData["error"] = "Invalid product data. Please check the inputs.";
                return RedirectToAction("Index");
            }

            // If the Product has an Id, it's an update; otherwise, it's a new creation
            if (productViewModel.Product.Id == Guid.Empty)
            {
                // Creating a new product
                productViewModel.Product.Id = Guid.NewGuid(); // Generate a new Guid for the product

                // Assigning the Supplier, Category and Store based on the selected ids
                productViewModel.Product.SupplierId = productViewModel.SupplierId;
                productViewModel.Product.CategoryId = productViewModel.CategoryId;
                productViewModel.Product.StoreId = productViewModel.StoreId;

                _unitOfWork.Product.Add(productViewModel.Product); // Add the new store to the repository
                TempData["success"] = "Product created successfully.";
            }
            else
            {
                // Updating an existing store
                var existingProduct = _unitOfWork.Product.Get(p => p.Id == productViewModel.Product.Id);
                if (existingProduct == null)
                {
                    TempData["error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                // Update the store properties
                existingProduct.Name = productViewModel.Product.Name;
                existingProduct.Description = productViewModel.Product.Description;
                existingProduct.Batch = productViewModel.Product.Batch;
                existingProduct.CostPrice = productViewModel.Product.CostPrice;
                existingProduct.SellPrice = productViewModel.Product.SellPrice;
                existingProduct.Quantity = productViewModel.Product.Quantity;
                existingProduct.ManufactureDate = productViewModel.Product.ManufactureDate;
                existingProduct.ExpiryDate = productViewModel.Product.ExpiryDate;

                // Update Manager and Cashier ids
                existingProduct.SupplierId = productViewModel.SupplierId;
                existingProduct.CategoryId = productViewModel.CategoryId;
                existingProduct.StoreId = productViewModel.StoreId;

                // Perform the update in the repository
                _unitOfWork.Product.Update(existingProduct);
                TempData["success"] = "Product updated successfully.";
            }

            // Save the changes to the database
            _unitOfWork.Save();

            // After creating/updating the store, redirect to the Index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                TempData["error"] = "Invalid ID";
                return RedirectToAction("Index");
            }

            var productToBeDeleted = _unitOfWork.Product.Get(p => p.Id == id.Value);

            if (productToBeDeleted == null)
            {
                TempData["error"] = "Product not found or already deleted.";
                return RedirectToAction("Index");
            }

            // Check for references in the OrderItems table
            var isProductReferenced = _unitOfWork.OrderItem.Get(oi => oi.ProductId == id.Value);
            if (isProductReferenced != null)
            {
                TempData["error"] = "Cannot delete this product as it is referenced by existing order items.";
                return RedirectToAction("Index");
            }

            // Proceed to delete the product
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }


        #region API CALLS

        // https://localhost:7230/product/getall => to get all the propoerties of Product in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return BadRequest("User is not logged in or invalid.");
            }

            var userRoles = _unitOfWork.ApplicationUser.GetUserRoles(currentUserId) ?? new List<string>();
            List<Product> products;

            if (userRoles.Contains("Admin"))
            {
                // Admin: Fetch all products
                products = _unitOfWork.Product
                    .GetAll(includeProperties: "Supplier,Category,Store")
                    .ToList();
            }
            else
            {
                // Manager or Cashier: Filter products by StoreId
                var stores = _unitOfWork.Store
                    .GetAll(s => s.CashierId.ToString() == currentUserId || s.ManagerId.ToString() == currentUserId)
                    .ToList();

                if (!stores.Any())
                {
                    return Json(new { data = new List<Product>() }); // Return empty list if no stores are linked
                }

                var storeIds = stores.Select(s => s.Id).ToList();

                products = _unitOfWork.Product
                    .GetAll(p => storeIds.Contains(p.StoreId), includeProperties: "Supplier,Category,Store")
                    .ToList();
            }

            // Return the filtered product list as JSON
            return Json(new { data = products });
        }


        #endregion
    }
}
