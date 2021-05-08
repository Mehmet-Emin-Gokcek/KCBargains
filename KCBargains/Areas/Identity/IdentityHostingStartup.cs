using KCBargains.Data;
using KCBargains.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(KCBargains.Areas.Identity.IdentityHostingStartup))]
namespace KCBargains.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<BargainsDbContext>(options =>
                    options.UseMySql(
                        context.Configuration.GetConnectionString("DefaultConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole>(options =>
               {
                   options.SignIn.RequireConfirmedAccount = false;
                   options.SignIn.RequireConfirmedEmail = true;
               })
                  .AddEntityFrameworkStores<BargainsDbContext>()
                  .AddDefaultUI()
                  .AddDefaultTokenProviders();
              });


        }
    }
}