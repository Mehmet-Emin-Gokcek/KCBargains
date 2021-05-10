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

namespace KCBargains.Controllers
{
    public class ProductsController : Controller{

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

            Product product = context.Products
                .Include(e => e.Category)
                .Include(e => e.Retailer)
                .Single(e => e.Id == id);
            return View(product);
        }


        public IActionResult Add()
        {
            List<ProductCategory> categories = context.ProductCategories.ToList();


            Retailer retailer = new Retailer();

            AddProductViewModel addProductViewModel = new AddProductViewModel(categories, retailer);

            return View(addProductViewModel);
        }


        [HttpPost]
        public IActionResult Add(AddProductViewModel addProductViewModel)
        {

            if (ModelState.IsValid)
            {
                //Find the product category from database
                ProductCategory category = context.ProductCategories.Find(addProductViewModel.CategoryId);

                Retailer retailer = new Retailer
                {
                    Name = addProductViewModel.Retailer.Name,
                    Street = addProductViewModel.Retailer.Street,
                    City = addProductViewModel.Retailer.City,
                    State = addProductViewModel.Retailer.State,
                    Zipcode = addProductViewModel.Retailer.Zipcode,
                    Latitude = addProductViewModel.Retailer.Latitude,
                    Longitude = addProductViewModel.Retailer.Longitude
                };

                string[] uniqueFileName = UploadPhoto(addProductViewModel);//returns unique names of uploaded images

                //Find signed in user from database
                ApplicationUser SignedUser = context.Users.Find(addProductViewModel.UserId);

                Product product = new Product
                {
                    Name = addProductViewModel.Name,
                    Description = addProductViewModel.Description,
                    Quantity = addProductViewModel.Quantity,
                    Cost = addProductViewModel.Cost,
                    Picture = uniqueFileName[0],
                    Picture2 = uniqueFileName[1],
                    Picture3 = uniqueFileName[2],
                    Picture4 = uniqueFileName[3],
                    User = SignedUser,
                    Category = category,
                    Retailer = retailer
                };

                context.Retailers.Add(retailer);
                context.Products.Add(product);
                context.SaveChanges();

                return Redirect("/Products");
            }


            List<ProductCategory> categories = context.ProductCategories.ToList(); //reload category list options to make sure they will appear after the data validation errors

            return View(new AddProductViewModel(categories, new Retailer())); //passing new Model Object with categories list options
        }


        //Uploading, Resizing and Saving Product Pictures
        public string[] UploadPhoto(AddProductViewModel model)
        {
            IFormFile[] fileArr = new IFormFile[4];//store all files in an array
            fileArr[0] = model.ProductPicture;
            fileArr[1] = model.ProductPicture2;
            fileArr[2] = model.ProductPicture3;
            fileArr[3] = model.ProductPicture4;

            string[] imageArr = new string[4]; //store all unique photo names in an array to be returned

            for( int i = 0; i < fileArr.Length; i++)
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

                    if (finalHeight/3 < finalWidth/4) //limitin factor is height
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

        public IActionResult Edit(int Id)
        {
            // Pull the Product object that will be edited from the database
            Product theProduct = context.Products.Find(Id);
            Retailer theRetailer = context.Retailers.Find(theProduct.RetailerId);
            ProductCategory theCategory = context.ProductCategories.Find(theProduct.CategoryId);

            //Make sure category list will show up 
            List<ProductCategory> categories = context.ProductCategories.ToList();

            //Update addProductViewModel fields before returning it to the Edit View
            AddProductViewModel addProductViewModel = new AddProductViewModel(categories, theRetailer)
            {
                Name = theProduct.Name,
                Description = theProduct.Description,
                Quantity = theProduct.Quantity,
                Cost = theProduct.Cost,
                Picture = theProduct.Picture,
                Picture2 = theProduct.Picture2,
                Picture3 = theProduct.Picture3,
                Picture4 = theProduct.Picture4,
                Category = theCategory,
                CategoryId = theProduct.Category.Id,
                Retailer = theProduct.Retailer,
                RetailerId = theRetailer.Id,
                ProductId = Id
            };

            return View(addProductViewModel);
        }


        [HttpPost]
        public IActionResult Edit(AddProductViewModel addProductViewModel)
        {

            if (ModelState.IsValid)
            {

                //Find the retailer from database
                Retailer retailer = context.Retailers.Find(addProductViewModel.RetailerId);

                //Find the category from database
                ProductCategory category = context.ProductCategories.Find(addProductViewModel.CategoryId);

                //Find the product from database
                Product product = context.Products.Find(addProductViewModel.ProductId);
                
                //Update the Retailer Object
                retailer.Name = addProductViewModel.Retailer.Name;
                retailer.Street = addProductViewModel.Retailer.Street;
                retailer.City = addProductViewModel.Retailer.City;
                retailer.State = addProductViewModel.Retailer.State;
                retailer.Zipcode = addProductViewModel.Retailer.Zipcode;
                retailer.Latitude = addProductViewModel.Retailer.Latitude;
                retailer.Longitude = addProductViewModel.Retailer.Longitude;



                string[] uniqueFileName = UploadPhoto(addProductViewModel);//returns unique names of uploaded images

                if (uniqueFileName[0] != null) //means a new picture is being uploaded
                {
                    addProductViewModel.Picture = uniqueFileName[0];
                }

                if (uniqueFileName[1] != null) //means a new picture is being uploaded
                {
                    addProductViewModel.Picture2 = uniqueFileName[1];
                }

                if (uniqueFileName[2] != null) //means a new picture is being uploaded
                {
                    addProductViewModel.Picture3 = uniqueFileName[2];
                }

                if (uniqueFileName[3] != null) //means a new picture is being uploaded
                {
                    addProductViewModel.Picture4 = uniqueFileName[3];
                }

                //Update the Product Object
                product.Name = addProductViewModel.Name;
                product.Description = addProductViewModel.Description;
                product.Quantity = addProductViewModel.Quantity;
                product.Cost = addProductViewModel.Cost;
                product.Picture = addProductViewModel.Picture;
                product.Picture2 = addProductViewModel.Picture2;
                product.Picture3 = addProductViewModel.Picture3;
                product.Picture4 = addProductViewModel.Picture4;
                product.Category.Id = category.Id;
                product.Retailer.Id = retailer.Id;



                context.Retailers.Update(retailer);
                context.Products.Update(product);
                context.SaveChanges();

                TempData.Add("ProductID", addProductViewModel.ProductId); //Pass Product ID data to Detail View

                return RedirectToAction("Detail");
            }

                List<ProductCategory> categories = context.ProductCategories.ToList(); //reload category list options to make sure they will appear after the data validation errors
                addProductViewModel.Categories = addProductViewModel.CategoryUpdate(categories);
                //Find the category from database
/*                ProductCategory category = context.ProductCategories.Find(addProductViewModel.CategoryId);
                //Make sure to update Product Category  
                addProductViewModel.Category = category;*/

            return View(addProductViewModel); //passing new Model Object with categories list options

            }
        }


   /* //Handles Photo Upload
    private string UploadedFile(AddProductViewModel model)
    {
        string uniqueFileName = null;
        if (model.ProductPicture != null)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images"); //create uploads folder path
            uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductPicture.FileName; //create a unique file name
            string filePath = Path.Combine(uploadsFolder, uniqueFileName); //combine uploads folder path and unique file name
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                model.ProductPicture.CopyTo(fileStream);
            }
        }
        else if (model.ProductPicture == null)
        {
            Console.WriteLine("model.ProductImage is Null");
        }
        return uniqueFileName;
    }*/


}

