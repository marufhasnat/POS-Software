using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;

namespace POS_Software.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }

        #region API CALLS

        // https://localhost:7230/Product/getall => to get all the propoerties of Product in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            // Fetch products and include related with using Include
            var objProductList = _unitOfWork.Product.GetAll(includeProperties: "Supplier,Category,Store").ToList(); // Get data as IQueryable and include relationships

            // Return the data as JSON
            return Json(new { data = objProductList });
        }

        #endregion
    }
}
