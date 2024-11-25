using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        // Dropdown for assigning supplier, category & store 
        public Guid SupplierId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StoreId { get; set; }

        // List of assigning suppliers, categories & stores filtered by their id
        [ValidateNever]
        public List<SelectListItem> Suppliers { get; set; }
        [ValidateNever]
        public List<SelectListItem> Categories { get; set; }
        [ValidateNever]
        public List<SelectListItem> Stores { get; set; }
    }
}
