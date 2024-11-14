using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Supplier
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Name")]
        public string SupplierName { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Company")]
        public string Company { get; set; }

        [Required]
        [DisplayName("Registration Date")]
        public DateTime RegDate { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("E-mail")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [MaxLength(15)]
        [DisplayName("Phone")]
        public string Phone { get; set; }

        [MaxLength(100)]
        [DisplayName("Address")]
        public string Address { get; set; }
    }
}
