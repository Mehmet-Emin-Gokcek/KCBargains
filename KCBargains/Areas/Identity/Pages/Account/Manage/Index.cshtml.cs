using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KCBargains.Data;
using KCBargains.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KCBargains.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly BargainsDbContext context;


        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            BargainsDbContext dbContext
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            context = dbContext;
        }

        public byte[] DefaultPicture { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public byte[] ProfilePicture { get; set; }

            public bool DeletePicture { get; set; }

        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userInfo = await _userManager.GetUserAsync(User);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var profilePicture = user.ProfilePicture;


            Input = new InputModel
            {
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                PhoneNumber = phoneNumber,
                ProfilePicture = profilePicture,
                DeletePicture = false
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await FetchDefaultAvatar();//Fetch defaultAvatar picture and save it at DefaultPicture field
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            //Check to see if the profile picture is deleted, if it's deleted, replace the profile picture with default avatar picture.
            if (Input.DeletePicture)
            {
                await FetchDefaultAvatar();//Fetch defaultAvatar picture and save it at DefaultPicture field
                user.ProfilePicture = DefaultPicture; //User profile picture is assigned to default avatar picture
                await _userManager.UpdateAsync(user); //Update the User object in database
            }



            //Upload Profile Picture
            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }

            //if first or last name is not changed return status
            if (Input.FirstName == user.FirstName && Input.LastName == user.LastName && Input.ProfilePicture == user.ProfilePicture)
            {
                StatusMessage = "Your profile has not been changed";
                return RedirectToPage();
            }

            //Change User's first name
            user.FirstName = Input.FirstName;

            //Change User's first name
            user.LastName = Input.LastName;

            //Update User Info
            var updateUser = await _userManager.UpdateAsync(user);

            //Show error if update is unsuccessful
            if (!updateUser.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update user info.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }



        //Fetch defaultAvatar picture and save it at DefaultPicture field
        public async Task FetchDefaultAvatar()
        {
            string filePath = @"wwwroot\images\defaultAvatar.png";
            //Instantiate FileStream object and pass the filePath to read the image file
            //The using construct ensures that the file will be closed when you leave the block even if an exception is thrown.
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                //Instantiate MemoryStream object to store the data stream coming from FileStream object
                MemoryStream memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                //Convert the data stored in MemoryStream object to byte[] array, and save it to the ProfilePicture field of the User object. 
                DefaultPicture = memoryStream.ToArray();
            }
        }
    }
}
