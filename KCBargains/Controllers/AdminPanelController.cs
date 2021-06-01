using KCBargains.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly BargainsDbContext context;

        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminPanelController(BargainsDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            context = dbContext;
            webHostEnvironment = hostEnvironment;

        }

        [AllowAnonymous]
        public IActionResult Index()
        {

            return View();
        }

    }
}
