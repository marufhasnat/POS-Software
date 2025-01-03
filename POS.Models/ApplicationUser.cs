﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string StreetAddress { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Division { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public bool Status { get; set; } = false;
    }
}
