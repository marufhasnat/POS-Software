using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.DataAccess.Repository.IRepository;
using POS.Models;

namespace POS_Software.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Get all the categories
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        // GET: Upsert action to prepare the modal for edit or creation
        [HttpGet]
        public IActionResult Upsert(Guid? id)
        {
            Category category = new Category();
            
            if (id.HasValue)
            {
                category = _unitOfWork.Category.Get(category => category.Id == id.Value);

                if (category == null)
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
        public IActionResult Upsert(Category category)
        {
            if (category.CategoryName == null || category.CategoryName.Length < 3 || category.CategoryName.Length > 20)
            {
                TempData["error"] = "Category name can't be empty and must be between 3 and 20 letters.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid category data. Please check the inputs.";
                return RedirectToAction("Index");
            }

            // Check for existing category with the same name
            var existingCategoryWithSameName = _unitOfWork.Category.Get(c => c.CategoryName == category.CategoryName);

            if (existingCategoryWithSameName != null && existingCategoryWithSameName.Id != category.Id)
            {
                // If category name already exists and it’s not the current category, show error
                TempData["error"] = "A category with the same name already exists.";
                return RedirectToAction("Index");
            }

            if (category.Id == Guid.Empty)
            {
                // New category creation
                category.Id = Guid.NewGuid();
                category.CreatedAt = DateTime.Now;
                _unitOfWork.Category.Add(category);
                TempData["success"] = "Category created successfully.";
            }
            else
            {
                // Updating existing category
                var existingCategory = _unitOfWork.Category.Get(c => c.Id == category.Id);
                if (existingCategory == null)
                {
                    TempData["error"] = "Category not found.";
                    return RedirectToAction("Index");
                }

                existingCategory.CategoryName = category.CategoryName;
                _unitOfWork.Category.Update(existingCategory);
                TempData["success"] = "Category updated successfully.";
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

            var categoryToBeDeleted = _unitOfWork.Category.Get(c => c.Id == id.Value);

            if (categoryToBeDeleted == null)
            {
                TempData["error"] = "Category not found or already deleted.";
                return RedirectToAction("Index");
            }

            _unitOfWork.Category.Remove(categoryToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }


        #region API CALLS

        // https://localhost:7230/Category/getall => to get all the propoerties of Category in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return Json(new { data = objCategoryList });
        }

        #endregion
    }
}
