using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles.OrderBy(r => r.Name);
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleVm model)
        {
            if(!ModelState.IsValid)
                return View(model);

            var role = new IdentityRole { Name = model.RoleName };

            var result = await roleManager.CreateAsync(role);

            if(result.Succeeded)
            {
                TempData["Success"] = "Role was added successfully.";
                return RedirectToAction("ListRoles");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(String.Empty, error.Description);

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.Message = $"Role with id = {id} not found.";
                return View("NotFound");
            }

            var model = new EditRoleVm { Id = role.Id, RoleName = role.Name };

            foreach(var user in userManager.Users)
            {
                if(await userManager.IsInRoleAsync(user, role.Name))
                    model.Users.Add(user.UserName);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleVm model)
        {
            if(!ModelState.IsValid)
                return View(model);

            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.Message = $"Role with id = {model.Id} not found.";
                return View("NotFound");
            }

            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{role.Name}' was updated successfully.";
                return RedirectToAction("ListRoles");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(String.Empty, error.Description);

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.Message = $"Role with id = {id} not found.";
                return View("NotFound");
            }

            var result = await roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                TempData["Info"] = $"Role '{role.Name}' was deleted successfully.";
                return RedirectToAction("ListRoles");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("ListRoles");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRoleUsers(string roleId)
        {
            ViewBag.RoleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.Message = $"Role with id = {roleId} not found.";
                return View("NotFound");
            }

            var model = new List<UserRoleVm>();

            foreach(var user in userManager.Users)
            {
                var userRoleVm = new UserRoleVm { UserId = user.Id, UserName = user.UserName };

                if (await userManager.IsInRoleAsync(user, role.Name))
                    userRoleVm.IsSelected = true;
                else
                    userRoleVm.IsSelected = false;

                model.Add(userRoleVm);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoleUsers(List<UserRoleVm> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.Message = $"Role with id = {roleId} not found.";
                return View("NotFound");
            }

            foreach(var userModel in model)
            {
                var user = await userManager.FindByIdAsync(userModel.UserId);

                IdentityResult result = null;

                if (userModel.IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                    result = await userManager.AddToRoleAsync(user, role.Name);
                else if (!userModel.IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                else
                    continue;

                if (result.Succeeded)
                    continue;
            }

            TempData["Success"] = $"List of '{role.Name}' users was updated successfully.";
            return RedirectToAction("EditRole", "Administration", new { id = roleId });
        }

        public IActionResult ListUsers()
        {
            var users = userManager.Users.OrderBy(r => r.FullName);
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.Message = $"User with id = {id} not found.";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);

            var model = new EditUserVm 
            { 
                Id = user.Id,
                UserName = user.UserName, 
                FullName = user.FullName,
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.Message = $"User with id = {model.Id} not found.";
                return View("NotFound");
            }

            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = model.UserName;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = $"User '{user.FullName}' was updated successfully.";
                return RedirectToAction("ListUsers");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.Message = $"User with id = {id} not found.";
                return View("NotFound");
            }

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Info"] = $"User '{user.FullName}' was deleted successfully.";
                return RedirectToAction("ListUsers");
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("ListUsers");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserRoles(string userId)
        {
            ViewBag.UserId = userId;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.Message = $"User with id = {userId} not found.";
                return View("NotFound");
            }

            var model = new List<RoleUserVm>();

            foreach (var role in roleManager.Roles)
            {
                var roleUserVm = new RoleUserVm { RoleId = role.Id, RoleName = role.Name };

                if (await userManager.IsInRoleAsync(user, role.Name))
                    roleUserVm.IsSelected = true;
                else
                    roleUserVm.IsSelected = false;

                model.Add(roleUserVm);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRoles(List<RoleUserVm> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.Message = $"User with id = {userId} not found.";
                return View("NotFound");
            }

            foreach (var roleModel in model)
            {
                var role = await roleManager.FindByIdAsync(roleModel.RoleId);

                IdentityResult result = null;

                if (roleModel.IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                    result = await userManager.AddToRoleAsync(user, role.Name);
                else if (!roleModel.IsSelected && (await userManager.IsInRoleAsync(user, role.Name)))
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                else
                    continue;

                if (result.Succeeded)
                    continue;
            }

            TempData["Success"] = $"'{user.FullName}' roles were updated successfully.";
            return RedirectToAction("EditUser", "Administration", new { id = userId });
        }
    }
}