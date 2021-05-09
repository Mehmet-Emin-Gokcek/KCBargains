using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCBargains.Models
{
    public class Product
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Quantity { get; set; }
        public double Cost { get; set; }
        public string Picture { get; set; }
        public string Picture2 { get; set; }
        public string Picture3 { get; set; }
        public string Picture4 { get; set; }
        public Retailer Retailer { get; set; }
        public ProductCategory Category { get; set; }
        public ApplicationUser User { get; set; }

        public Product(){}

        public Product(string name, string description, string quantity, double cost) 
        {
            Name = name;
            Description = description;
            Quantity = quantity;
            Cost = cost;
        }


    }
}
