using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtentions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>(entity => entity.Property(m => m.UserName).HasMaxLength(255));
            modelBuilder.Entity<ApplicationUser>(entity => entity.Property(m => m.NormalizedUserName).HasMaxLength(255));
            modelBuilder.Entity<ApplicationUser>(entity => entity.Property(m => m.Email).HasMaxLength(255));
            modelBuilder.Entity<ApplicationUser>(entity => entity.Property(m => m.NormalizedEmail).HasMaxLength(255));

            modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.Name).HasMaxLength(255));
            modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.NormalizedName).HasMaxLength(255));

            modelBuilder.Entity<Employee>().HasData(
                    new Employee { Id = 1, Name = "Mark", Email = "mark@gmail.com", Department = Dept.IT, Photo = "" },
                    new Employee { Id = 2, Name = "Jane", Email = "jane@gmail.com", Department = Dept.HR, Photo = "" },
                    new Employee { Id = 3, Name = "Sam", Email = "sam@gmail.com", Department = Dept.IT, Photo = "" },
                    new Employee { Id = 4, Name = "Mary", Email = "mary@gmail.com", Department = Dept.Payroll, Photo = "" }
                );
        }
    }
}
