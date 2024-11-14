using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        [DisplayName("Category Name")]
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
