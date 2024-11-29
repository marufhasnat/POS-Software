using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace POS.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; } // Unique ID for the order item

        [Required]
        public Guid OrderId { get; set; } // Foreign key linking to the Order
        [ForeignKey("OrderId")]
        [ValidateNever]
        public Order Order { get; set; } // Navigation property to the Order

        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; } // Quantity of the product

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Total price for this item (Quantity * UnitPrice)
    }
}
