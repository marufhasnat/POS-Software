using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace POS.Models.ViewModels
{
    public class UserRolesVM
    {
        public ApplicationUser ApplicationUser { get; set; } // Strongly typed to ApplicationUser

        [ValidateNever]
        public IEnumerable<string> Roles { get; set; } // List of roles the user belongs to

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; } // All available roles, for dropdowns if needed
    }
}
