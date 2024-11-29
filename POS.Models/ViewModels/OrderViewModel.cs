using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public OrderItem OrderItem { get; set; }

        // List of Order Items
        [ValidateNever]
        public List<OrderItem> OrderItems { get; set; }

        // Dropdown for assigning product
        public Guid ProductId {  get; set; }

        // List of assigning products
        [ValidateNever]
        public List<SelectListItem> Products { get; set; }

        // Serialized Cart Data
        public string CartData { get; set; }
    }
}
