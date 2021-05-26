using KCBargains.Data;
using KCBargains.Models;
using KCBargains.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KCBargains.Controllers
{
    public class CategoriesController: Controller
    {
        private readonly BargainsDbContext context;

        private readonly IWebHostEnvironment webHostEnvironment;

        public CategoriesController(BargainsDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            context = dbContext;
            webHostEnvironment = hostEnvironment;

        }

        //Return empty view, get and post requests will be handled by AJAX on the 'Category.cshtml' view file 
        [AllowAnonymous]
        public IActionResult Index()
        {

            return View();
        }

       
        public ContentResult UpdateCategory(string id, string name)
        {
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

        public ContentResult DeleteCategory(string id)
        {
            int ID = 0;
            string response = $"DeleteCategory() Error!!!  Product Id: {id} input is either null or empty!";

            if (!String.IsNullOrEmpty(id))
            {
                try
                {
                    ID = Int32.Parse(id);
                    //check to see if a ProductCategory table contains a 'ProductCategory' object with the parsed ID
                    if (context.ProductCategories.Any(c => c.Id == ID))
                    {
                        //Nullify the relationship between the ProductCategory object and the Product object 
                        //CategoryId foreign fey field on the Products table will become null.
                        //This ensures that Products related to ProductCategory are not deleted.
                        //Default behavior is to delete all Products that are in relationship with a ProductCategory
                        //Learn more here: https://docs.microsoft.com/en-us/ef/core/saving/cascade-delete
                        ProductCategory category = context.ProductCategories.Include(c => c.products).Single(c => c.Id == ID);
                        foreach (var product in category.products)
                        {
                            product.Category = null;
                        }
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
                    response = $"AddCategory() Error!!!  Product Name: {name} already exists in the database. Please create a unique category!";
                }
            }
            return Content(response);
        }

        public ContentResult GetCategory()
        {

            string json = "";

            List<ProductCategory> categories = context.ProductCategories.ToList(); //get all ProductCategories
            List<Product> productList = context.Products.Include(e => e.Category).ToList(); //get all Products and its related categories
            List<CategoryViewModel> updatedCategories = new List<CategoryViewModel>();

            if (categories.Count != 0)
            {
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
