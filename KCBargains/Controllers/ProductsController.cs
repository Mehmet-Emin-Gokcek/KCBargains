using System.Linq;
using System;
using System.IO;
using KCBargains.Models;
using KCBargains.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;
using KCBargains.Data;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KCBargains.Controllers
{
    public class ProductsController : Controller
    {

        private readonly BargainsDbContext context;

        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductsController(BargainsDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            context = dbContext;
            webHostEnvironment = hostEnvironment;

        }

        [AllowAnonymous]
        public IActionResult Index()
        {

            List<Product> products = context.Products.Include(e => e.Category).Include(e => e.Retailer).ToList();
            return View(products);
        }

        [AllowAnonymous]
        public IActionResult Detail(int id)
        {
            //Check to see if there is any Product Id data being passed from Edit Post Action
            if (TempData.Peek("ProductID") != null)
            {
                id = (int)TempData["ProductID"];
            }

            Product product = context.Products.Include(e => e.Category).Include(e => e.Retailer).Single(e => e.Id == id);
            return View(product);
        }

        public IActionResult Add()
        {
            List<ProductCategory> categories = context.ProductCategories.ToList();

            Retailer retailer = new Retailer();

            ProductViewModel productViewModel = new ProductViewModel(categories, retailer);

            return View(productViewModel);
        }

        [HttpPost]
        public IActionResult Add(ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                //Find the product category from database
                ProductCategory category = context.ProductCategories.Find(productViewModel.CategoryId);

                Retailer retailer = new Retailer
                {
                    Name = productViewModel.Retailer.Name,
                    Street = productViewModel.Retailer.Street,
                    City = productViewModel.Retailer.City,
                    State = productViewModel.Retailer.State,
                    Zipcode = productViewModel.Retailer.Zipcode,
                    Latitude = productViewModel.Retailer.Latitude,
                    Longitude = productViewModel.Retailer.Longitude
                };

                Console.WriteLine("retailer.Latitude: " + retailer.Latitude);
                Console.WriteLine("retailer.Longitude: " + retailer.Longitude);


                string[] pictureList = UploadPicture(productViewModel);//returns list of uploaded pictures


                //Find signed in user from database
                ApplicationUser SignedUser = context.Users.Find(productViewModel.UserId);

                Product product = new Product
                {
                    Name = productViewModel.Name,
                    Description = productViewModel.Description,
                    Quantity = productViewModel.Quantity,
                    Cost = productViewModel.Cost,
                    User = SignedUser,
                    Category = category,
                    Retailer = retailer,
                    Picture1 = pictureList[0],
                    Picture2 = pictureList[1],
                    Picture3 = pictureList[2],
                    Picture4 = pictureList[3],
                };

                context.Retailers.Add(retailer);
                context.Products.Add(product);
                context.SaveChanges();

                return Redirect("/Products");
            }
            //reload category list options to make sure they will appear after the data validation errors
            List<ProductCategory> categories = context.ProductCategories.ToList();

            //passing new empty Model Object with categories list options
            return View(new ProductViewModel(categories, new Retailer())); 
        }

        public IActionResult Edit(int Id)
        {
            // Pull the Product object that will be edited from the database
            Product theProduct = context.Products.Find(Id);
            Retailer theRetailer = context.Retailers.Find(theProduct.RetailerId);
            ProductCategory theCategory = context.ProductCategories.Find(theProduct.CategoryId);

            //Make sure category list will show up 
            List<ProductCategory> categories = context.ProductCategories.ToList();

            //Update productViewModel fields before returning it to the Edit View
            ProductViewModel productViewModel = new ProductViewModel(categories, theRetailer)
            {
                Name = theProduct.Name,
                Description = theProduct.Description,
                Quantity = theProduct.Quantity,
                Cost = theProduct.Cost,
                Picture1 = theProduct.Picture1,
                Picture2 = theProduct.Picture2,
                Picture3 = theProduct.Picture3,
                Picture4 = theProduct.Picture4,
                Category = theCategory,
                CategoryId = theCategory.Id,
                RetailerId = theRetailer.Id,
                ProductId = Id
            };
            return View(productViewModel);
        }


        [HttpPost]
        public IActionResult Edit(ProductViewModel productViewModel)
        {
            //Find the category from database
            ProductCategory category = context.ProductCategories.Find(productViewModel.CategoryId);

            if (ModelState.IsValid)
            {
                //Find the retailer from database
                Retailer retailer = context.Retailers.Find(productViewModel.RetailerId);

                //Find the product from database
                Product product = context.Products.Find(productViewModel.ProductId);

                //Update the Retailer Object
                retailer.Name = productViewModel.Retailer.Name;
                retailer.Street = productViewModel.Retailer.Street;
                retailer.City = productViewModel.Retailer.City;
                retailer.State = productViewModel.Retailer.State;
                retailer.Zipcode = productViewModel.Retailer.Zipcode;
                retailer.Latitude = productViewModel.Retailer.Latitude;
                retailer.Longitude = productViewModel.Retailer.Longitude;

                string[] pictureList = UploadPicture(productViewModel);//returns list of uploaded pictures

                if (pictureList[0] != null) //means a new picture is being uploaded
                {
                    productViewModel.Picture1 = pictureList[0];
                }

                if (pictureList[1] != null) //means a new picture is being uploaded
                {
                    productViewModel.Picture2 = pictureList[1];
                }

                if (pictureList[2] != null) //means a new picture is being uploaded
                {
                    productViewModel.Picture3 = pictureList[2];
                }

                if (pictureList[3] != null) //means a new picture is being uploaded
                {
                    productViewModel.Picture4 = pictureList[3];
                }

                //Update the Product Object
                product.Name = productViewModel.Name;
                product.Description = productViewModel.Description;
                product.Quantity = productViewModel.Quantity;
                product.Cost = productViewModel.Cost;
                product.Picture1 = productViewModel.Picture1;
                product.Picture2 = productViewModel.Picture2;
                product.Picture3 = productViewModel.Picture3;
                product.Picture4 = productViewModel.Picture4;
                product.CategoryId = category.Id;
                product.RetailerId = retailer.Id;
                product.Category = category;
                product.Retailer = retailer;

                context.Retailers.Update(retailer);
                context.Products.Update(product);
                context.SaveChanges();

                TempData.Add("ProductID", productViewModel.ProductId); //Pass Product ID data to Detail View
                return RedirectToAction("Detail");
            }

            //reload category list options to make sure they will appear after the data validation errors
            List<ProductCategory> categories = context.ProductCategories.ToList(); 
            productViewModel.Categories = productViewModel.CategoryUpdate(categories);

            //passing new Model Object with categories list options
            return View(productViewModel); 
        }

        //Uploading, Resizing and Saving Product Pictures
        public string[] UploadPicture(ProductViewModel model)
        {
            IFormFile[] fileArr = new IFormFile[4];//store all files in an array
            fileArr[0] = model.ProductPicture1;
            fileArr[1] = model.ProductPicture2;
            fileArr[2] = model.ProductPicture3;
            fileArr[3] = model.ProductPicture4;

            string[] imageArr = new string[4]; //store all unique photo names in an array to be returned

            for (int i = 0; i < fileArr.Length; i++)
            {
                if (fileArr[i] != null)
                {
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images"); //create uploads folder path
                    imageArr[i] = Guid.NewGuid().ToString() + "_" + fileArr[i].FileName; //create a unique file name
                    string filePath = Path.Combine(uploadsFolder, imageArr[i]); //combine uploads folder path and unique file name

                    using var image = Image.Load(fileArr[i].OpenReadStream()); //load the IFormFile product picture to Image object included by SixLabors.ImageSharp package
                    var size = image.Size();

                    var finalWidth = size.Width - (size.Width % 4); //gets a width value divisible by 4
                    var finalHeight = size.Height - (size.Height % 3); //gets a height value divisible by 3

                    if (finalHeight / 3 < finalWidth / 4) //limitin factor is height
                    {
                        finalWidth = finalHeight / 3 * 4; //calculate new width to reach 4:3 aspect ratio
                        var x = (size.Width / 2) - finalWidth / 2; //calculate x coordinate of top left corner of the crop box
                        var y = (size.Height / 2) - finalHeight / 2; //calculate y coordinate of top left corner of the crop box

                        image.Mutate(img => img.Crop(new Rectangle(x, y, finalWidth, finalHeight))); //crop out the width to create 4:3 image
                    }

                    //when limiting factor is the width(like a vertical image)
                    //or when dealing width square shaped image (width = height),
                    //then add padding to increase the width to create 4:3 image
                    else
                    {
                        finalWidth = finalHeight / 3 * 4; //calculate new width to reach 4:3 aspect ratio
                        image.Mutate(img => img.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.BoxPad, //initiate padding mode
                            Position = AnchorPositionMode.Center, //padding should start from the center
                            Size = new Size(finalWidth, finalHeight) //final size will use finalWidth and finalHeight
                        }).BackgroundColor(new Rgba32(255, 255, 255)));//add white background padding, default background padding is black
                    }
                    image.Save(filePath); //save the image to the path created above
                }
            }
            return imageArr;
        }


        //Return empty view, get and post requests will be handled by AJAX on the 'Category.cshtml' view file 
        public IActionResult Category()
        {
            return View();
        }

        public ContentResult UpdateCategory(string id, string name) {
            int ID = 0;
            string response = $"DeleteCategory() Error!!!  Product Id: {id}, Product Name: {name} input is either null or empty!";

            if (!String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(name))
            {
                try
                {
                    ID = Int32.Parse(id);

                    Console.WriteLine($" Product Id: '{ID}' has been parsed successfully");

                    //check to see if a ProductCategory table contains a 'ProductCategory' object with the parsed ID
                    if (context.ProductCategories.Any(c => c.Id == ID))
                    {
                        ProductCategory category = context.ProductCategories.Find(ID);
                        category.Name = name;
                        context.ProductCategories.Update(category);
                        context.SaveChanges();
                        response = ID.ToString();
                    }

                    else
                    {
                        response = ($"DeleteCategory() Error!!! Product Id: '{ID}' could not be found in the database!");
                    }
                }
                catch (FormatException)
                {
                    response = ($"DeleteCategory() Error!!! Product Id: '{id}' could not be parsed!");
                }
            }
            return Content(response);
        }



        public ContentResult DeleteCategory(string id) {
            int ID = 0;
            string response = $"DeleteCategory() Error!!!  Product Id: {id} input is either null or empty!";

            if (!String.IsNullOrEmpty(id))
            {
                try
                {
                    ID = Int32.Parse(id);

                    Console.WriteLine($" Product Id: '{ID}' has been parsed successfully");

                    //check to see if a ProductCategory table contains a 'ProductCategory' object with the parsed ID
                    if (context.ProductCategories.Any(c => c.Id == ID)) 
                    {
                        ProductCategory category = context.ProductCategories.Find(ID);
                        context.ProductCategories.Remove(category);
                        context.SaveChanges();
                        response = ID.ToString();
                    }

                    else
                    {
                        response = ($"DeleteCategory() Error!!! Product Id: '{ID}' could not be found in the database!");
                    }
                }
                catch (FormatException)
                {
                    response = ($"DeleteCategory() Error!!! Product Id: '{id}' could not be parsed!");
                }
            }
            return Content(response);
        }


        [HttpPost]
        public ContentResult AddCategory(string name)
        {
            string response = $"AddCategory() Error!!! Name: {name} Input is either null or empty!";

            if (!String.IsNullOrEmpty(name))
            {
                if (!context.ProductCategories.Any(c => c.Name == name))
                {
                    ProductCategory category = new ProductCategory(name);
                    context.ProductCategories.Add(category);
                    context.SaveChanges();
                    response = name;
                }
                else
                {
                    response= $"AddCategory() Error!!!  Product Name: {name} already exists in the database. Please create a unique category!";
                }
            }
            return Content(response);
        }

        public ContentResult GetCategory() {
            
            string json = "";
           
            List<ProductCategory> categories = context.ProductCategories.ToList(); //get all ProductCategories
            List<Product> productList = context.Products.Include(e => e.Category).ToList(); //get all Products and its related categories
            List<CategoryViewModel> updatedCategories = new List<CategoryViewModel>();
            
            if (categories.Count != 0) {
                foreach (var category in categories)
                { //iterate through categories
                    CategoryViewModel newCategory = new CategoryViewModel(category.Id, category.Name);

                    foreach (var product in productList)
                    { //We are trying to find the multiple products that relate to one ProductCategory

                        if (category.Id == product.CategoryId) //If found a matching category, add it to the tempArray
                        {
                            newCategory.ProductNames.Add(product.Name);
                        }
                    }
                    updatedCategories.Add(newCategory);
                }
                   json = JsonConvert.SerializeObject(updatedCategories);
            }
            return Content(json);
        }
    }
}
