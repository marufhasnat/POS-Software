using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Utility;

namespace POS_Software.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public SupplierController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // get all the suppliers
        public IActionResult Index()
        {
            List<Supplier> objSupplierList = _unitOfWork.Supplier.GetAll().ToList();
            return View(objSupplierList);
        }

        // GET: Upsert action to prepare the modal for edit or creation
        [HttpGet]
        public IActionResult Upsert(Guid? id)
        {
            Supplier supplier = new Supplier();
            
            if(id.HasValue)
            {
                supplier = _unitOfWork.Supplier.Get(s => s.Id == id.Value);

                if (supplier == null)
                {
                    return NotFound();
                }
            }

            // Redirect to Index to open the modal with the data
            return RedirectToAction("Index");
        }

        // POST: Upsert action for creation or update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid supplier data. Please check the inputs.";
                return RedirectToAction("Index");
            }

            // Check for existing supplier with the same name and different ID to avoid duplicates
            var existingSupplierWithSameCompany = _unitOfWork.Supplier.Get(s => s.Company == supplier.Company && s.Id != supplier.Id);
            if (existingSupplierWithSameCompany != null)
            {
                TempData["error"] = "A supplier with the same company already exists.";
                return RedirectToAction("Index");
            }

            if (supplier.Id == Guid.Empty)
            {
                // New supplier creation
                supplier.Id = Guid.NewGuid();
                supplier.RegDate = DateTime.Now; // Assuming registration date is set on creation
                _unitOfWork.Supplier.Add(supplier);
                TempData["success"] = "Supplier created successfully.";
            }
            else
            {
                // Updating existing supplier
                var existingSupplier = _unitOfWork.Supplier.Get(s => s.Id == supplier.Id);
                if (existingSupplier == null)
                {
                    TempData["error"] = "Supplier not found.";
                    return RedirectToAction("Index");
                }

                // Update existing supplier details
                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.Company = supplier.Company;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Address = supplier.Address;

                _unitOfWork.Supplier.Update(existingSupplier);
                TempData["success"] = "Supplier updated successfully.";
            }

            _unitOfWork.Save();
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

            var supplierToBeDeleted = _unitOfWork.Supplier.Get(s => s.Id == id.Value);

            if (supplierToBeDeleted == null)
            {
                TempData["error"] = "Supplier not found or already deleted.";
                return RedirectToAction("Index");
            }

            // Check for references in the OrderItems table
            var isSupplierReferenced = _unitOfWork.OrderItem.Get(oi => oi.Product.SupplierId == id.Value);
            if (isSupplierReferenced != null)
            {
                TempData["error"] = "Cannot delete this supplier as it is referenced by existing order items.";
                return RedirectToAction("Index");
            }

            // Proceed to delete the supplier
            _unitOfWork.Supplier.Remove(supplierToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Supplier deleted successfully.";
            return RedirectToAction("Index");
        }



        #region API CALLS

        // https://localhost:7230/Supplier/getall => to get all the propoerties of Supplier in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Supplier> objSupplierList = _unitOfWork.Supplier.GetAll().ToList();
            return Json(new { data = objSupplierList });
        }

        #endregion
    }
}
