using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        [Required]
        public Guid StoreId { get; set; }
        [ForeignKey("StoreId")]
        [ValidateNever]
        public Store Store { get; set; }

        [Required]
        public string CashierId { get; set; }

        [MaxLength(50)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Total amount before discounts

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; } // Discount applied (if any)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PayableAmount { get; set; } // Amount after applying the discount

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } // Amount paid by the customer

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } // Remaining balance or change given to the customer

        [Required]
        [MaxLength(50)]
        public string PaymentMode { get; set; } // Payment method (e.g., Cash, POS, etc.)

        [Required]
        public string InvoiceNumber { get; set; }

        // Navigation Property: A collection of Order Items
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
