﻿using System.Linq;
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
        public IActionResult Detail(int Id)
        {
            Product product = context.Products.Include(e => e.Category).Include(e => e.Retailer).Single(e => e.Id == Id);
            return View(product);
        }

        [Authorize(Roles = "SuperAdmin, Admin, Standard")]
        public IActionResult Add()
        {
            List<ProductCategory> categories = context.ProductCategories.ToList();

            Retailer retailer = new Retailer();

            ProductViewModel productViewModel = new ProductViewModel(categories, retailer);

            return View(productViewModel);
        }


        [Authorize(Roles = "SuperAdmin, Admin, Standard")]
        [HttpPost]
        public IActionResult Add(ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                //Find the product category from database
                ProductCategory category = context.ProductCategories.Find(productViewModel.CategoryId);
                
                //Find the user who is creating the Product object 
                ApplicationUser user = context.Users.Find(productViewModel.UserId);

                //Check to see if newly entered Retailer already exists in the database
                //If it does, assign it to the newly entered Product
                //This helps avoid duplicate entries for Retail locations and decreasing data redundancy in the database
                Retailer retailer = null;
                retailer = context.Retailers
                    .Where(r => r.Latitude == productViewModel.Retailer.Latitude)
                    .Where(r => r.Longitude== productViewModel.Retailer.Longitude).FirstOrDefault();

                
                if (retailer == null)
                {
                    retailer = new Retailer()
                    {
                        Name = CapitalizeFirstLetter(productViewModel.Retailer.Name),
                        Street = productViewModel.Retailer.Street,
                        City = productViewModel.Retailer.City,
                        State = productViewModel.Retailer.State,
                        Zipcode = productViewModel.Retailer.Zipcode,
                        Latitude = productViewModel.Retailer.Latitude,
                        Longitude = productViewModel.Retailer.Longitude,
                        User = user,
                    };
                    context.Retailers.Add(retailer);
                }

                string[] pictureList = UploadPicture(productViewModel);//returns list of uploaded pictures

              
                Product product = new Product()
                {
                    Name = CapitalizeFirstLetter(productViewModel.Name),
                    Description = CapitalizeFirstLetter(productViewModel.Description),
                    Quantity = productViewModel.Quantity,
                    Cost = productViewModel.Cost,
                    Category = category,
                    Retailer = retailer,
                    Picture1 = pictureList[0],
                    Picture2 = pictureList[1],
                    Picture3 = pictureList[2],
                    Picture4 = pictureList[3],
                    User = user,
                };


                context.Products.Add(product);
                context.SaveChanges();

                return Redirect("/Products");
            }
            //reload category list options to make sure they will appear after the data validation errors
            List<ProductCategory> categories = context.ProductCategories.ToList();

            //passing new
            //Model Object with categories list options
            return View(new ProductViewModel(categories, new Retailer())); 
        }


        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Edit(int Id)
        {

            //Check to see if there is any Product Id data being passed from Edit Post Action
            if (TempData.Peek("ProductID") != null)
            {
                Id = (int)TempData["ProductID"];
            }


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
                ProductId = Id,
                UserId = theProduct.UserId,
                TimeLog = theProduct.TimeLog
            };
            return View(productViewModel);
        }


        [Authorize(Roles = "SuperAdmin, Admin, Standard")]
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
                retailer.Name = CapitalizeFirstLetter(productViewModel.Retailer.Name);
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
                product.Name = CapitalizeFirstLetter(productViewModel.Name);
                product.Description = CapitalizeFirstLetter(productViewModel.Description);
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
                return RedirectToAction("Edit");
            }

            //reload category list options to make sure they will appear after the data validation errors
            List<ProductCategory> categories = context.ProductCategories.ToList(); 
            productViewModel.Categories = productViewModel.CategoryUpdate(categories);

            //passing new Model Object with categories list options
            return View(productViewModel); 
        }


        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult List()
        {
            List<Product> products = context.Products.Include(e => e.Category).Include(e => e.Retailer).ToList();
            return View(products);
        }


        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Delete(int Id)
        {
            // Pull the Product object that will be deleted from the database
            Product product = context.Products.Include(p => p.Category).Single(p => p.Id == Id);

            // Nullify relationship with Category table to avoid cascading delete behavior
            //otherwise deleting product might delete related object information at other tables
            product.Category = null;
            product.Retailer = null;
            product.User = null;
            
            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("List");
        }



        //Uploading, Resizing and Saving Product Pictures
        private string[] UploadPicture(ProductViewModel model)
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

        private string CapitalizeFirstLetter(string str) { 
        return (char.ToUpper(str[0]) + str.Substring(1).ToLower());
        }

    }
}
