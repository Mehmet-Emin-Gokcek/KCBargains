using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace KCBargains.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Product> products { get; set; }
        public string AdminId { get; set; }
        public ApplicationUser Admin { get; set; }
        public string TimeLog { get; set; }

        public ProductCategory(string name, string adminId) 
        {
            Name = name;
            AdminId = adminId;
            TimeLog = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
        }


        public ProductCategory()
        {
            TimeLog = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
        }


    }
}
