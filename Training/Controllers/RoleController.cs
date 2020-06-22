using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Training.Models;

namespace Training.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ILogger<RoleController> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await ListRole());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(role.Name));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Create Role Success");
                    return RedirectToAction("Index");
                }
                else
                    Errors(result);
            }
            return View(role.Name);
        }

        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Delete Role Success");
                    return RedirectToAction("Index");
                }
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");
            return View("Index",await ListRole());
        }

        public async Task<IActionResult> SetUser(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<IdentityUser> members = new List<IdentityUser>();
            List<IdentityUser> nonMembers = new List<IdentityUser>();
            foreach (IdentityUser user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleUpdate
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Members = members,
                NonMembers = nonMembers
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetUser(RoleUpdate model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    IdentityUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await _userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    IdentityUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction("Index");
            else
                return await SetUser(model.RoleId);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
                return View(new Role { Name = role.Name });
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role role)
        {
            try
            {
                IdentityRole thisRole = await _roleManager.FindByIdAsync(role.Id);
                thisRole.Name = role.Name;
                var result = _roleManager.UpdateAsync(thisRole);
                if (result.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("Edit Role Success");
                    return RedirectToAction("Index");
                }

                return View(role);
            }
            catch (Exception ex)
            {
                return View(role);
            }
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                _logger.LogError("Log Error From Role Controller: " + error.Description);
                ModelState.AddModelError("", error.Description);
            }
        }

        private async Task<IEnumerable<Role>> ListRole()
        {
            var listRole = new List<Role>();
            var roles = _roleManager.Roles;
            List<string> names = new List<string>();
            foreach (var item in roles)
            {
                names.Clear();
                foreach (IdentityUser user in _userManager.Users)
                {
                    if (await _userManager.IsInRoleAsync(user, item.Name))
                        names.Add(user.UserName);
                }
                listRole.Add(new Role { Id = item.Id, Name = item.Name, User = string.Join(", ", names) });
            }
            return listRole;
        }
    }
}