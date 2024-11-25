using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;
using POS.Utility;

namespace POS_Software.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class StoreController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public StoreController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            // Get all users
            var users = _unitOfWork.ApplicationUser.GetAll();
            var managers = users.Where(user => _unitOfWork.ApplicationUser.GetUserRoles(user.Id).Contains("Manager")).ToList();
            var cashiers = users.Where(user => _unitOfWork.ApplicationUser.GetUserRoles(user.Id).Contains("Cashier")).ToList();

            StoreViewModel storeViewModel = new StoreViewModel
            {
                Managers = managers.Select(user => new SelectListItem
                {
                    Value = user.Id,
                    Text = user.Name
                }).ToList(),
                Cashiers = cashiers.Select(user => new SelectListItem
                {
                    Value = user.Id,
                    Text = user.Name
                }).ToList()
            };

            return View(storeViewModel); // Pass the populated ViewModel to the view
        }


        [HttpGet]
        public IActionResult Upsert(Guid? id)
        {
            // Fetch all users and their roles efficiently
            var users = _unitOfWork.ApplicationUser.GetAll();
            var userRoles = users.ToDictionary(user => user.Id, user => _unitOfWork.ApplicationUser.GetUserRoles(user.Id));

            // Filter managers and cashiers based on roles
            var managers = users.Where(user => userRoles[user.Id].Contains("Manager")).ToList();
            var cashiers = users.Where(user => userRoles[user.Id].Contains("Cashier")).ToList();

            // Initialize the ViewModel
            var storeViewModel = new StoreViewModel
            {
                Managers = managers.Select(user => new SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.Name} ({user.Email})"
                }).ToList(),
                Cashiers = cashiers.Select(user => new SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.Name} ({user.Email})"
                }).ToList()
            };

            // Handle store retrieval for update
            if (id.HasValue)
            {
                var store = _unitOfWork.Store.Get(s => s.Id == id.Value);
                if (store == null)
                {
                    TempData["error"] = "Store doesn't exist.";
                    return RedirectToAction("Index");
                }

                // Populate the ViewModel with the existing store data
                storeViewModel.Store = store;
                storeViewModel.ManagerId = store.ManagerId;
                storeViewModel.CashierId = store.CashierId;
            }
            else
            {
                // For creation, initialize a new Store instance
                storeViewModel.Store = new Store();
            }

            return View(storeViewModel); // Return the view with the ViewModel
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(StoreViewModel storeViewModel)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                //foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                //{
                //    Console.WriteLine(error.ErrorMessage); // Log errors to debug
                //}
                TempData["error"] = "Invalid store data. Please check the inputs.";
                return RedirectToAction("Index");
            }

            // Check for existing store with the same name
            var existingStoreWithSameName = _unitOfWork.Store.Get(s => s.Name == storeViewModel.Store.Name);

            if (existingStoreWithSameName != null && existingStoreWithSameName.Id != storeViewModel.Store.Id)
            {
                // If category name already exists and it’s not the current category, show error
                TempData["error"] = "A store with the same name already exists.";
                return RedirectToAction("Index");
            }

            // Check if the Manager is already assigned to another store (other than the current store if updating)
            var existingManagerAssignment = _unitOfWork.Store.Get(s => s.ManagerId == storeViewModel.ManagerId && s.Id != storeViewModel.Store.Id);
            if (existingManagerAssignment != null)
            {
                TempData["error"] = "The selected manager is already assigned to another store.";
                return RedirectToAction("Index");
            }

            // Check if the Cashier is already assigned to another store (other than the current store if updating)
            var existingCashierAssignment = _unitOfWork.Store.Get(s => s.CashierId == storeViewModel.CashierId && s.Id != storeViewModel.Store.Id);
            if (existingCashierAssignment != null)
            {
                TempData["error"] = "The selected cashier is already assigned to another store.";
                return RedirectToAction("Index");
            }

            // If the Store has an Id, it's an update; otherwise, it's a new creation
            if (storeViewModel.Store.Id == Guid.Empty)
            {
                // Creating a new store
                storeViewModel.Store.Id = Guid.NewGuid(); // Generate a new Guid for the store

                // Assigning the Manager and Cashier based on the selected ids
                storeViewModel.Store.ManagerId = storeViewModel.ManagerId;
                storeViewModel.Store.CashierId = storeViewModel.CashierId;

                _unitOfWork.Store.Add(storeViewModel.Store); // Add the new store to the repository
                TempData["success"] = "Store created successfully.";
            }
            else
            {
                // Updating an existing store
                var existingStore = _unitOfWork.Store.Get(s => s.Id == storeViewModel.Store.Id);
                if (existingStore == null)
                {
                    TempData["error"] = "Store not found.";
                    return RedirectToAction("Index");
                }

                // Update the store properties
                existingStore.Name = storeViewModel.Store.Name;
                existingStore.Phone = storeViewModel.Store.Phone;
                existingStore.Status = storeViewModel.Store.Status;
                existingStore.Location = storeViewModel.Store.Location;

                // Update Manager and Cashier ids
                existingStore.ManagerId = storeViewModel.ManagerId;
                existingStore.CashierId = storeViewModel.CashierId;

                // Perform the update in the repository
                _unitOfWork.Store.Update(existingStore);
                TempData["success"] = "Store updated successfully.";
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

            var storeToBeDeleted = _unitOfWork.Store.Get(s => s.Id == id.Value);

            if (storeToBeDeleted == null)
            {
                TempData["error"] = "Store not found or already deleted.";
                return RedirectToAction("Index");
            }

            _unitOfWork.Store.Remove(storeToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "Store deleted successfully.";
            return RedirectToAction("Index");
        }

        #region API CALLS

        // https://localhost:7230/Store/getall => to get all the propoerties of Store in json for using in datatable
        [HttpGet]
        public IActionResult GetAll()
        {
            // Fetch stores and include related 'Manager' and 'Cashier' using Include
            var objStoreList = _unitOfWork.Store.GetAll("Manager,Cashier").ToList(); // Get data as IQueryable and include relationships

            // Return the data as JSON
            return Json(new { data = objStoreList });
        }

        #endregion
    }

}
