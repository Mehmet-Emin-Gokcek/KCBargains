using KCBargains.Data;
using KCBargains.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class RetailersController : Controller
    {

        private readonly BargainsDbContext context;

        private readonly IWebHostEnvironment webHostEnvironment;

        public RetailersController(BargainsDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            context = dbContext;
            webHostEnvironment = hostEnvironment;

        }

        public IActionResult Index()
        {
            List<Retailer> retailers = context.Retailers.Include(r => r.Products).ToList();

            return View(retailers);
        }
    
    }
}
