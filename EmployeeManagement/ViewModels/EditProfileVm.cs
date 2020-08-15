using EmployeeManagement.CustomValidations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class EditProfileVm
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Maximum length is 100 characters")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [EmailDomain(domain: "test.com")]
        public string Email { get; set; }
    }
}
