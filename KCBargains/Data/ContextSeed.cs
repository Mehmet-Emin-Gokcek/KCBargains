using KCBargains.Enums;
using KCBargains.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
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

        public static async Task SeedCategory(BargainsDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByEmailAsync("admin@gmail.com");

            //Createa ProductCategory array to hold ProductCategory objects
            ProductCategory[] categories = new ProductCategory[]
            {
                new ProductCategory(){Name = "General Grocery", Admin = user},
                new ProductCategory(){Name = "Baking", Admin = user},
                new ProductCategory(){Name = "Breakfast", Admin = user},
                new ProductCategory(){Name = "Dinner", Admin = user},
                new ProductCategory(){Name = "Cleaning", Admin = user}
            };

            foreach (var category in categories)
            {   //Check to see if the category object exists in the database
                var categoryResult = await dbContext.ProductCategories.Where(c => c.Name.ToLower() == category.Name.ToLower()).FirstOrDefaultAsync();

                //Only add the object if it does not exist in the database
                if (categoryResult == null)
                {
                   await dbContext.ProductCategories.AddAsync(category);
                   await dbContext.SaveChangesAsync();

                }
            }
        }

        public static async Task SeedRetailer(BargainsDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByEmailAsync("admin@gmail.com");

            Retailer[] retailers = new Retailer[]
            {
               new Retailer(){Name = "Walmart", Street = "4434 Gardner Avenue", City = "Kansas City", State = "MO", Zipcode = "64120",
                               Latitude = 39.127231, Longitude = -94.525675, User = user},
               new Retailer(){Name = "Hyve", Street = "3325 Northeast Vivion Road", City = "Kansas City", State = "MO", Zipcode = "64119",
                               Latitude = 39.1870122, Longitude = -94.540646, User = user}
            };

            foreach (var retailer in retailers)
            {   //Check to see if the category object exists in the database
                Retailer retailerResult = await dbContext.Retailers.Where(r => r.Latitude == retailer.Latitude).Where(r => r.Longitude == retailer.Longitude).FirstOrDefaultAsync();

                //Only add the object if it does not exist in the database
                if (retailerResult == null)
                {
                    await dbContext .Retailers.AddAsync(retailer);
                    await dbContext.SaveChangesAsync();
                }
            }
        }



        public static async Task SeedProduct(BargainsDbContext dbContext, UserManager<ApplicationUser> userManager) 
        {
            var user = await userManager.FindByEmailAsync("admin@gmail.com");
            var retailer = await dbContext.Retailers.Where(r => r.Name.ToLower() == "walmart").FirstOrDefaultAsync();
            var category = await dbContext.ProductCategories.Where(c => c.Name.ToLower() == "baking").FirstOrDefaultAsync();

            var retailer2 = await dbContext.Retailers.Where(r => r.Name.ToLower() == "hyve").FirstOrDefaultAsync();
            var category2 = await dbContext.ProductCategories.Where(c => c.Name.ToLower() == "cleaning").FirstOrDefaultAsync();

            Product[] products = new Product[]
            {
                new Product(){
                    Name = "Virgin olive oil",
                    Description = "Spain olive oil, cold press",
                    Quantity = "32oz",
                    Cost = 22,
                    Picture1 = "b78712dc-b2d8-4aa9-97da-a7fd3ad7e4e7_olive_oil.jpg",
                    Picture2 = "df80c76d-d47d-4166-883c-926f27968049_olive_oil2.jpg",
                    Picture3 = "bf58dd2b-1a4b-4010-aafd-20600de0570a_Oliven_V1.jpg",
                    Picture4 = "66499795-ed84-48de-8c24-d9e1ededfda8_pouring-extra-virgin-olive-oil.jpg",
                    Retailer= retailer,
                    Category = category,
                    User = user
                },


            new Product(){
                    Name = "Baby soap",
                    Description = "Organic soap, origin england",
                    Quantity = "4lb",
                    Cost = 32,
                    Picture1 = "d84cbba3-9fe6-49d9-b2da-6013b569dc2e_soap1.jpg",
                    Picture2 = "7f9ab669-569f-4803-b878-d331730ce34c_soap.jpg",
                    Picture3 = "45e84a2a-7cfc-4103-a7ce-ed21de7b0c02_soap2.jpg",
                    Picture4 = "5501b520-b201-4559-80b9-a1d9a3dcc4ff_handcrafted-soaps.jpg",
                    Retailer= retailer2,
                    Category = category2,
                    User = user
                }
            };

            foreach (var product in products)
            {   //Check to see if the category object exists in the database
                var productResult = await dbContext.Products.Where(p => p.Name == product.Name).FirstOrDefaultAsync();

                //Only add the object if it does not exist in the database
                if (productResult == null)
                {
                    await dbContext.Products.AddAsync(product);
                    await dbContext.SaveChangesAsync();
                }
            }

        }
    }
}