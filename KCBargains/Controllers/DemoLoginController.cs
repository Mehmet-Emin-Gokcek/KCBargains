using KCBargains.Data;
using KCBargains.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Controllers
{
    [AllowAnonymous]
    public class DemoLoginController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DemoLoginController> _logger;

        public DemoLoginController(
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ILogger<DemoLoginController> logger)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
        }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> LoginUser(string userName) //userRole passes user.Name, which will be used to login the user
        {
            string returnUrl = Url.Content("~/") + "home";

            if (!String.IsNullOrEmpty(userName))
            {
                    // Clear the existing external cookie to ensure a clean login process
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                    string userPassword = "123Pa$$word."; //All demo users are assigned the same password

                    var result = await _signInManager.PasswordSignInAsync(userName, userPassword, false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Success!!! DemoLoginController/LoginUser(): Submitted user role is logged in.");
                    return Redirect("/Home/Index"); //this routing doesn't work that is why I had to add jquery page reload function on Home/Index.cshtml
                }
            }
            
            _logger.LogInformation("Error!!! DemoLoginController/LoginUser(): could not sign in the user. Submitted user role value is either null or empty.");

            return Redirect("/Home/Index"); //this routing doesn't work that is why I had to add jquery page reload function on Home/Index.cshtml
        }
    }
}
