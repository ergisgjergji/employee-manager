﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class CreateRoleVm
    {
        [Required]
        [Display(Name = "Role name")]
        public string RoleName { get; set; }
    }
}
