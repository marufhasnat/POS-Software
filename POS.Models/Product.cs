using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public string Batch { get; set; }
        
        [Required]
        public double CostPrice { get; set; }

        [Required]
        public double SellPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public Guid SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        [ValidateNever]
        public Supplier Supplier { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        public Guid StoreId { get; set; }
        [ForeignKey("StoreId")]
        [ValidateNever]
        public Store Store { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ManufactureDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
    }
}
