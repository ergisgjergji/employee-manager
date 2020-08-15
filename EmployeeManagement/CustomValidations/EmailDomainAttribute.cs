using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.CustomValidations
{
    public class EmailDomainAttribute : ValidationAttribute
    {
        private readonly string domain;
        public EmailDomainAttribute(string domain)
        {
            this.domain = domain;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string[] strings = value.ToString().Split('@');

            if (strings[1].ToLower() != domain.ToLower())
                return new ValidationResult($"Email domain must be '@{domain}'");

            return ValidationResult.Success;
        }
    }
}
