using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace KCBargains.Models
{
    public class Product
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public double Cost { get; set; }
        public string Picture1 { get; set; }
        public string Picture2 { get; set; }
        public string Picture3 { get; set; }
        public string Picture4 { get; set; }

        //Below Retailer object is declared as a field to Product model class,
        //ASP.NET ORM automatically creates RetailerId column in the product table of the database by relating Retailer and Product tables,
        //in order to read the data on the RetailerId column of the product table,
        //RetailerId field must explicitly be declared as it is seen below. This field is used as foreign key to connect Product table to Retailer table.
        //Same concept and practice applies to CategoryId field and ProductCategory object as well as UserId field and ApplicationUser object.
        public int RetailerId { get; set; }
        public Retailer Retailer { get; set; }
        public int? CategoryId { get; set; }
        public ProductCategory Category { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public Product(){}

        public Product(string name, string description, string quantity, double cost) 
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            Cost = cost;
        }

        public static explicit operator Product(List<Product> v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator List<object>(Product v)
        {
            throw new NotImplementedException();
        }
    }
}
