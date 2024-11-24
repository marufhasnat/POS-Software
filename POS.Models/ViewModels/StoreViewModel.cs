using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models.ViewModels
{
    public class StoreViewModel
    {
        public Store Store { get; set; }

        // Dropdown for assigning users
        public string? ManagerId { get; set; }
        public string? CashierId { get; set; }

        // List of users filtered by Manager and Cashier roles
        [ValidateNever]
        public List<SelectListItem> Managers { get; set; }
        [ValidateNever]
        public List<SelectListItem> Cashiers { get; set; }
    }
}
