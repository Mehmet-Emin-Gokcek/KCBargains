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
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class UsersController : Controller
    {
        private readonly BargainsDbContext context;

        private readonly IWebHostEnvironment webHostEnvironment;

        public UsersController(BargainsDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            context = dbContext;
            webHostEnvironment = hostEnvironment;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }
    }
}
