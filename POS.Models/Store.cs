using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Store
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Manager Relationship
        public string? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        [ValidateNever]
        public ApplicationUser Manager { get; set; }

        // Cashier Relationship
        public string? CashierId { get; set; }

        [ForeignKey("CashierId")]
        [ValidateNever]
        public ApplicationUser Cashier { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public bool Status { get; set; }
    }
}
