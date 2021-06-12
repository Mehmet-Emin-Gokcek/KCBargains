using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KCBargains.Data;
using KCBargains.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KCBargains
{
    public class Program
    {
        public async static Task Main(string[] args)
        {

            //Create roles data and seed user information into the database
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var context = services.GetRequiredService<BargainsDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await ContextSeed.SeedRolesAsync(roleManager);
                    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager); //Create Super Admin account using the information at Data\ContextSeed.cs
                    await ContextSeed.SeedAdminAsync(userManager, roleManager);//Create Admin account using the information at Data\ContextSeed.cs
                    await ContextSeed.SeedUserAsync(userManager, roleManager);//Create User account using the information at Data\ContextSeed.cs
                    await ContextSeed.SeedCategory(context, userManager);
                    await ContextSeed.SeedRetailer(context, userManager);
                    await ContextSeed.SeedProduct(context, userManager);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
