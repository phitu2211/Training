using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Training.Models;

namespace Training.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IPasswordValidator<IdentityUser> _passwordValidator;
        private readonly IUserValidator<IdentityUser> _userValidator;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<IdentityUser> userManager, IPasswordHasher<IdentityUser> passwordHasher, IPasswordValidator<IdentityUser> passwordValidator, IUserValidator<IdentityUser> userValidator, ILogger<UserController> logger)
        {
            _userValidator = userValidator;
            _logger = logger;
            _passwordValidator = passwordValidator;
            _passwordHasher = passwordHasher;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(_userManager.Users.Select(x => new User { Email = x.Email, Name = x.UserName, Id = x.Id }));
        }

        public ViewResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = user.Name,
                    Email = user.Email
                };
                IdentityResult result = await _userManager.CreateAsync(identityUser, user.Password);
                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(identityUser, "User");
                    if (!result.Succeeded)
                        Errors(result);
                    _logger.LogInformation("Create User Success");
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        public ViewResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = user.Name,
                    Email = user.Email
                };
                IdentityResult result = await _userManager.CreateAsync(identityUser, user.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Create User Success");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }

        public async Task<IActionResult> Update(string id)
        {
            IdentityUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
                return View(new User { Name = user.UserName, Email = user.Email });
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, User user)
        {
            IdentityUser identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser != null)
            {
                var validUser = await _userValidator.ValidateAsync(_userManager, identityUser);
                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(user.Name))
                {
                    if (validUser.Succeeded)
                    {
                        _logger.LogInformation("Edit User Success");
                        identityUser.UserName = user.Name;
                    }
                    else
                        Errors(validUser);
                }
                else
                    ModelState.AddModelError("", "Name cannot be empty");

                if (!string.IsNullOrEmpty(user.Email)) 
                {
                    if (validUser.Succeeded)
                        identityUser.Email = user.Email;
                    else
                        Errors(validUser);
                }
                else
                    ModelState.AddModelError("", "Email cannot be empty");

                if (!string.IsNullOrEmpty(user.Password))
                {
                    validPass = await _passwordValidator.ValidateAsync(_userManager, identityUser, user.Password);
                    if (validPass.Succeeded)
                        identityUser.PasswordHash = _passwordHasher.HashPassword(identityUser, user.Password);
                    else
                        Errors(validPass);
                }
                else
                    ModelState.AddModelError("", "Password cannot be empty");

                if (validUser != null && validPass != null && validUser.Succeeded && validPass.Succeeded)
                {
                    IdentityResult result = await _userManager.UpdateAsync(identityUser);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            IdentityUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Delete User Success");
                    return RedirectToAction("Index");
                }
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("Index", _userManager.Users.Select(x => new User { Email = x.Email, Name = x.UserName, Id = x.Id }));
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                _logger.LogError("Log Error From Role Controller: "+ error.Description);
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}