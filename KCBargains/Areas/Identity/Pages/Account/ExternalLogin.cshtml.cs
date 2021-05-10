using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using KCBargains.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace KCBargains.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {


            [Display(Name = "UserName")]
            public string UserName { get; set; }

            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }


        //OnGetCallbackAsync: identity will check the external login user information and confirm a local user is associated with external user , 
        //if yes , sign in user ; If no, it will creat a new account for the user with the information from external account provider
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            // If the user doesn't have an account create a new account and sign in the user using external login information.
            else
            {
                //Get user Name from external provider
                var NewUserName = info.Principal.FindFirstValue(ClaimTypes.Name);

                //Get Email address from external provider
                var NewEmail = info.Principal.FindFirstValue(ClaimTypes.Email);

                //Split External user name to into first and last names
                string[] names = NewUserName.Split(' ');

                //Create user object using the info from external provider
                var user = new ApplicationUser { UserName = NewEmail, Email = NewEmail, FirstName = names[0], LastName = names[1] };

                //create database user without password 
                var UserCreateResult = await _userManager.CreateAsync(user);

                if (UserCreateResult.Succeeded)
                {

                    //Confirm Email to avoid sending email confirmation link
                    user.EmailConfirmed = true;


                    //Convert the data stored in MemoryStream object to byte[] array, and save it to the ProfilePicture field of the User object. 
                    user.ProfilePicture = await GetProfilePictureAsync(info);

                    //Update user info to confirm email confirmation column and ProfilePicture Column in Database
                    await _userManager.UpdateAsync(user);

                    //Add login info for the user
                    var UserLoginResult = await _userManager.AddLoginAsync(user, info);

                    if (UserLoginResult.Succeeded)
                    {
                        //Sign in the new user
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }

                }
                
                return Page();
            }
        }

        public async Task<byte[]> GetProfilePictureAsync(ExternalLoginInfo info)
        {
            string picture = null;

            if (info.LoginProvider == "Google")
            {
                //Get web path for External User Profile Picture
                picture = info.Principal.FindFirstValue("urn:google:picture");
            }

            if (info.LoginProvider == "Facebook")
            {
                string identifier = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                //Get web path for External User Profile Picture
                picture = $"https://graph.facebook.com/v10.0/{identifier}/picture?type=large";
            }

            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(picture);

            // Get the response.  
            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();

            //Instantiate MemoryStream object to store the data stream coming from dataStream object
            MemoryStream memoryStream = new MemoryStream();

            //Copy the data coming from the datastream to MemoryStream object 
            await dataStream.CopyToAsync(memoryStream);

            byte[] array = memoryStream.ToArray();
            
            if (array.Length == 0) //if no picture data is retrieved through external login assign default avatar picture
            {
                    string filePath = @"wwwroot\images\defaultAvatar.png";
                    //Instantiate FileStream object and pass the filePath to read the image file
                    FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);
                    //Instantiate MemoryStream object to store the data stream coming from FileStream object
                    MemoryStream fileMemoryStream = new MemoryStream();
                    await stream.CopyToAsync(fileMemoryStream);
                    //Convert the data stored in MemoryStream object to byte[] array, and save it to the ProfilePicture field of the User object. 
                    array = fileMemoryStream.ToArray();
            }
           
            return array;
        }


        //OnPostConfirmationAsync: This method is required if we want to ask the user to confirm his/her email address
        //Social media accounts already contain confirmed email information so there is no need to confirm user's email again.
        /* public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {

                var NewUserName = info.Principal.FindFirstValue(ClaimTypes.Name);
                var NewEmail = info.Principal.FindFirstValue(ClaimTypes.Email);

                var user = new ApplicationUser { UserName = NewUserName, Email = NewEmail };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);


                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }

                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
             }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();

        }*/


    }
}
