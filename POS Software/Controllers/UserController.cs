using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using POS.DataAccess.Repository.IRepository;
using POS.Models;
using POS.Models.ViewModels;
using POS.Utility;
using System.Text.RegularExpressions;

namespace POS_Software.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Fetch all users
            var userList = _unitOfWork.ApplicationUser.GetAll().ToList();

            // Prepare the ViewModel
            var userRolesVM = userList.Select(user => new UserRolesVM
            {
                ApplicationUser = user,
                Roles = _unitOfWork.ApplicationUser.GetUserRoles(user.Id), // Fetch roles for the user
                //RoleList = _unitOfWork.Role.GetAll() // If needed for dropdowns (optional)
                //     .Select(r => new SelectListItem
                //     {
                //         Text = r.Name,
                //         Value = r.Id.ToString()
                //     })
            }).ToList();

            return View(userRolesVM);
        }

        [HttpGet]
        public async Task<IActionResult> ReverseStatus(Guid id)
        {
            // Retrieve the user by their ID
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound(); // Return 404 if the user does not exist
            }

            // Toggle the IsActive property
            user.Status = !user.Status;

            // Update the user
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["success"] = "The account status is changed.";
                return RedirectToAction("Index");
            }

            // Handle errors and store in TempData
            TempData["error"] = string.Join(" ", result.Errors.Select(e => e.Description));

            return View("Index"); // Optionally return to a view with error messages
        }


        // POST: Upsert action for creation or update
        [HttpPost]
        public IActionResult Delete(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Email not found.";
                return RedirectToAction("Index");
            }

            if (!IsValidEmail(email))
            {
                TempData["error"] = "Invalid email format.";
                return RedirectToAction("Index");
            }

            var applicationUserToBeDeleted = _unitOfWork.ApplicationUser.Get(u => u.Email == email);

            if (applicationUserToBeDeleted == null)
            {
                TempData["error"] = "User not found or already deleted.";
                return RedirectToAction("Index");
            }

            _unitOfWork.ApplicationUser.Remove(applicationUserToBeDeleted);
            _unitOfWork.Save();

            TempData["success"] = "User deleted successfully";
            return RedirectToAction("Index");
        }

        private bool IsValidEmail(string email)
        {
            // Use a simple regex or built-in library for email validation
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }


        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Fetch all users
            List<ApplicationUser> objApplicationUserList = _unitOfWork.ApplicationUser.GetAll().ToList();

            // Collect users with their roles
            var userWithRole = new List<object>();

            foreach (var user in objApplicationUserList)
            {
                var role = _unitOfWork.ApplicationUser.GetUserRoles(user.Id); // Fetch roles asynchronously
                userWithRole.Add(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Division,
                    user.City,
                    user.StreetAddress,
                    user.PostalCode,
                    user.PhoneNumber,
                    user.Status,
                    Role = role.ToList()
                });
            }

            return Json(new { data = userWithRole });
        }

        #endregion

    }
}
