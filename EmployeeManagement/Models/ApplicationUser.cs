using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Maximum length is 100 characters")]
        public string FullName { get; set; }
    }
}
