using KCBargains.Enums;
using KCBargains.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Standard.ToString()));
        }
        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultAvatarPhoto = await InsertAvatarPicture(); //Fetches default avatar picture as profile photo, and returns it as byte[]

            //Seed Default User
            var superAdmin = new ApplicationUser
            {
                UserName = "superadmin",
                Email = "superadmin@gmail.com",
                FirstName = "Super Admin User",
                LastName = "Gokcek",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePicture = defaultAvatarPhoto
            };
            if (userManager.Users.All(u => u.Id != superAdmin.Id))
            {
                var user = await userManager.FindByEmailAsync(superAdmin.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(superAdmin, "123Pa$$word.");
                    await userManager.AddToRoleAsync(superAdmin, Roles.Standard.ToString());
                    await userManager.AddToRoleAsync(superAdmin, Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin.ToString());
                }

            }
        }


        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultAvatarPhoto = await InsertAvatarPicture(); //Fetches default avatar picture as profile photo, and returns it as byte[]

            //Seed Default User
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                FirstName = "Admin User",
                LastName = "Gokcek",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePicture = defaultAvatarPhoto
            };
            if (userManager.Users.All(u => u.Id != adminUser.Id))
            {
                var user = await userManager.FindByEmailAsync(adminUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(adminUser, "123Pa$$word.");
                    await userManager.AddToRoleAsync(adminUser, Roles.Standard.ToString());
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                }
            }
        }


        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultAvatarPhoto = await InsertAvatarPicture(); //Fetches default avatar picture as profile photo, and returns it as byte[]

            //Seed Default User
            var standardUser = new ApplicationUser
            {
                UserName = "standarduser",
                Email = "standarduser@gmail.com",
                FirstName = "Standard User",
                LastName = "Gokcek",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePicture = defaultAvatarPhoto
            };

            if (userManager.Users.All(u => u.Id != standardUser.Id))
            {
                var user = await userManager.FindByEmailAsync(standardUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(standardUser, "123Pa$$word.");
                    await userManager.AddToRoleAsync(standardUser, Roles.Standard.ToString());
                }

            }
        }

        //Fetches default avatar picture as profile photo
        public static async Task<byte[]> InsertAvatarPicture()
        {

            string filePath = @"wwwroot\images\defaultAvatar.png";
            //Instantiate FileStream object and pass the filePath to read the image file
            //The using construct ensures that the file will be closed when you leave the block even if an exception is thrown.
            using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                //Instantiate MemoryStream object to store the data stream coming from FileStream object
                MemoryStream memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                //Convert the data stored in MemoryStream object to byte[] array, and return it. 
                return memoryStream.ToArray();
            }
        }
    }
}