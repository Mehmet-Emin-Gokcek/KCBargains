using KCBargains.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.ViewModels
{
    public class AddProductViewModel
    {

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = " must be between 3 and 20 characters long")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Desciption is required")]
        [StringLength(70, MinimumLength = 10, ErrorMessage = "Sorry, but the description is too short. Description must be at least 6 characters long.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity/Weight is required")]
        public string Quantity { get; set; }

        [Required(ErrorMessage = "Dollar Price/Cost is required")]
        public double Cost { get; set; }
        public IFormFile ProductPicture { get; set; }
        public IFormFile ProductPicture2 { get; set; }
        public IFormFile ProductPicture3 { get; set; }
        public IFormFile ProductPicture4 { get; set; }


        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }
        public int RetailerId { get; set; }

        //This property will be used for Product editting
        public int ProductId { get; set; }
        public string Picture { get; set; }
        public string Picture2 { get; set; }
        public string Picture3 { get; set; }
        public string Picture4 { get; set; }

        public Retailer Retailer { get; set; }
        public ProductCategory Category { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public AddProductViewModel(List<ProductCategory> categories, Retailer retailer)
        {
            Categories = new List<SelectListItem>();

            foreach (var category in categories)
            {
                Categories.Add(
                  new SelectListItem
                  {
                      Value = category.Id.ToString(),
                      Text = category.Name
                  }
                  ); ;
            }

            Retailer = retailer;
        }

        public AddProductViewModel() { }

        public List<SelectListItem> CategoryUpdate(List<ProductCategory> categories)

        {
            List<SelectListItem> Categories = new List<SelectListItem>();

            foreach (var category in categories)
            {
                Categories.Add(
                  new SelectListItem
                  {
                      Value = category.Id.ToString(),
                      Text = category.Name.ToString()
                  }
                  ); ;
            }

            return Categories;
        }
    }
}
