using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Threading.Tasks;

namespace KCBargains.Models
{
    public class Retailer
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Store name is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = " must be between 3 and 20  characters long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Street is required")]
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zipcode is required")]
        public string Zipcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string TimeLog { get; set; }
        public List<Product> Products { get; set; }
        public Retailer( ) {
         TimeLog = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
        }

    }
}
