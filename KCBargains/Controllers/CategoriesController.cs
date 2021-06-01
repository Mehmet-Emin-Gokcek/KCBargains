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

    [Authorize(Roles = "SuperAdmin, Admin")]
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
        public IActionResult Index()
        {

            return View();
        }


        public ContentResult UpdateCategory(string id, string name)
        {
            int ID = 0;
            string response = $"UpdateCategory() Error!!!  Product Id: {id}, Product Name: {name} input is either null or empty!";

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
                        response = ID.ToString();//Successful response
                    }

                    else
                    {
                        response = ($"UpdateCategory() Error!!! Product Id: '{ID}' could not be found in the database!");
                    }
                }
                catch (FormatException)
                {
                    response = ($"UpdateCategory() Error!!! Product Id: '{id}' could not be parsed!");
                }
            }
            return Content(response);
        }


        [Authorize(Roles = "SuperAdmin")]
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
                        category.Admin = null;
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
        public ContentResult CreateCategory(string name, string userId)
        {
            string response = $"AddCategory() Error!!! Name: {name} Input is either null or empty!";

            if (!String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(userId))
            {
                if (!context.ProductCategories.Any(c => c.Name == name))
                {
                    ProductCategory category = new ProductCategory(name, userId);
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

            List<ProductCategory> categories = context.ProductCategories.Include(c => c.products).Include(c => c.Admin).ToList(); //get all ProductCategories

            //categories array include too many fields that we are not interested in.
            //Viewmodel helps avoid sending too much redundant data to the view by only including fields that we need in the view.
            List<CategoryViewModel> categoriesView = new List<CategoryViewModel>(); 

            if (categories.Count != 0)
            {
                foreach (var category in categories)//iterate through categories
                { 
                    //Only assign fields that are needed in the view 
                    CategoryViewModel newCategory = new CategoryViewModel(category.Id, category.Name, category.Admin.Email, category.TimeLog);

                    //Iterate through multiple products that relate to one ProductCategory
                    foreach (var product in category.products)
                    { 
                        //Only product names are needed for the view
                        newCategory.ProductNames.Add(product.Name);
                    }

                    categoriesView.Add(newCategory);
                }

                json = JsonConvert.SerializeObject(categoriesView);

                Console.WriteLine("json: " + json);
            }
            return Content(json);
        }
    }
}
