using KCBargains.Enums;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using KCBargains.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;

namespace KCBargains.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;



        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
        
            [Required]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "LastName")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // validate input
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();


            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, FirstName = Input.FirstName, LastName = Input.LastName};



                //Fetch and save default Avatar picture to the database if there is not profile picture uploaded
                if (Request.Form.Files.Count == 0)
                {
                    string filePath = @"wwwroot\images\defaultAvatar.png";
                    //Instantiate FileStream object and pass the filePath to read the image file
                    //The using construct ensures that the file will be closed when you leave the block even if an exception is thrown.
                    using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate)) {
                        //Instantiate MemoryStream object to store the data stream coming from FileStream object
                        MemoryStream memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        //Convert the data stored in MemoryStream object to byte[] array, and save it to the ProfilePicture field of the User object. 
                        user.ProfilePicture = memoryStream.ToArray();
                        //Update the User object in database
                        await _userManager.UpdateAsync(user);
                    }
                    

                }

                //Upload Profile Picture
                if (Request.Form.Files.Count > 0)
                {
                    IFormFile file = Request.Form.Files.FirstOrDefault();
                    MemoryStream memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    user.ProfilePicture = memoryStream.ToArray();
                    await _userManager.UpdateAsync(user);
                }


             var result = await _userManager.CreateAsync(user, Input.Password);
                

                if (result.Succeeded)
                {
                     await _userManager.AddToRoleAsync(user, Roles.Standard.ToString()); //Assign basic role to the user
                    _logger.LogInformation("{Name} User created a new account with password.", user.FirstName);
                    

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string FirstName = user.FirstName;
                    //Lower case all letters of the first except for the first letters
                    FirstName = FirstName.Substring(0, 1).ToUpper() + FirstName.Substring(1).ToLower();
                    
                    string LastName = user.LastName;
                    //Lower case all letters of the last name except for the first letters
                    LastName = LastName.Substring(0, 1).ToUpper() + LastName.Substring(1).ToLower();
                    
                    //Append first and last name
                    string UserName = FirstName + " " + LastName;

                        await _emailSender.SendEmailAsync(Input.Email, "KCBargains - Confirm your email",
                        $"Hey {UserName},<br><br>" +
                        $"Thank you for signing up!<br><br> " +
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a> <br><br>" +
                        $"Best Regards,<br><br>" +
                        $"KCBargains Team");

                    if (_userManager.Options.SignIn.RequireConfirmedEmail)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
