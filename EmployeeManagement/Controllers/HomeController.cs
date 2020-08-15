using System;
using System.IO;
using System.Linq;
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private IHostingEnvironment env;
        private readonly IDataProtector protector;

        public HomeController(
            IEmployeeRepository employeeRepository, 
            IHostingEnvironment env, 
            IDataProtectionProvider dataProtectionProvider,
            DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            this.env = env;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAll()
                .OrderBy(e => e.Name)
                .Select(e => {
                    e.EncryptedId = protector.Protect(e.Id.ToString());
                    return e;
                });

            return View(model);
        }

        [AllowAnonymous]
        public ViewResult Details(string id)
        {
            int decryptedId = Convert.ToInt32(protector.Unprotect(id));

            Employee model = _employeeRepository.GetEmployee(decryptedId);

            if(model == null)
            {
                Response.StatusCode = 404;
                ViewBag.Message = $"Employee with id '{id}' not found.";
                return View("NotFound");
            }

            model.EncryptedId = id;

            return View(model);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateEmployeeVm model)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            string photosFolder = Path.Combine(env.WebRootPath, "images", "employees");

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;

            string filePath = Path.Combine(photosFolder, uniqueFileName);

            model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));

            Employee newEmployee = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                Department = model.Department,
                Photo = uniqueFileName
            };

            _employeeRepository.Add(newEmployee);

            newEmployee.EncryptedId = protector.Protect(newEmployee.Id.ToString());

            TempData["Success"] = "Employee was created successfully.";
            return RedirectToAction("Details", new { id = newEmployee.EncryptedId });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            int decryptedId = Convert.ToInt32(protector.Unprotect(id));

            Employee employee = _employeeRepository.GetEmployee(decryptedId);

            if(employee ==  null)
            {
                ViewBag.Message = $"Employee with id = {id} not found.";
                return View("NotFound");
            }

            EditEmployeeVm model = new EditEmployeeVm
            {
                EncryptedId = id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                PhotoPath = employee.Photo
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditEmployeeVm model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            int decryptedId = Convert.ToInt32(protector.Unprotect(model.EncryptedId));

            Employee employee = _employeeRepository.GetEmployee(decryptedId);

            if (employee == null)
            {
                ViewBag.Message = $"Employee with id = {model.EncryptedId} not found.";
                return View("NotFound");
            }

            employee.Name = model.Name;
            employee.Email = model.Email;
            employee.Department = model.Department;

            if(model.Photo != null)
            {
                // If user had a photo, remove it
                if (!String.IsNullOrEmpty(model.PhotoPath))
                    RemoveOldPhoto(model.PhotoPath);

                employee.Photo = ProcessUploadedFile(model.Photo);
            }

            _employeeRepository.Update(employee);

            TempData["Success"] = $"Employee '{employee.Name}' was updated successfully.";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(string id)
        {
            int decryptedId = Convert.ToInt32(protector.Unprotect(id));

            var employee = _employeeRepository.GetEmployee(decryptedId);

            if(employee == null)
            {
                ViewBag.Message = $"Employee with id = {id} not found";
                return View("NotFound");
            }

            _employeeRepository.Delete(decryptedId);

            TempData["Info"] = $"Employee '{employee.Name}' was deleted successfully.";
            return RedirectToAction("Index");
        }

        // Returns the unique file name or null (if there is no file)
        private string ProcessUploadedFile(IFormFile file)
        {
            if (file == null)
                return null;

            string photosFolder = Path.Combine(env.WebRootPath, "images", "employees");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(photosFolder, uniqueFileName);
            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return uniqueFileName;
        }

        private void RemoveOldPhoto(string fileName)
        {
            string photosFolder = Path.Combine(env.WebRootPath, "images", "employees");
            string filePath = Path.Combine(photosFolder, fileName);
            System.IO.File.Delete(filePath);
        }
    }
}