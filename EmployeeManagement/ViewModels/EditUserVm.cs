using EmployeeManagement.CustomValidations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class EditUserVm
    {
        public EditUserVm()
        {
            Roles = new List<string>();
        }

        [Required]
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email/Username")]
        [EmailDomain(domain: "test.com")]
        public string UserName { get; set; }

        public IList<string> Roles { get; set; }
    }
}
