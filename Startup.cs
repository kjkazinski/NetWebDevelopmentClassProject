using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Northwind.Models;

namespace Northwind
{
    public class Startup
    {
        // this class needs the connection info stored in the 
        // appsettings.json config file - that's a dependency
        // with dependency injection we expose the config file to this class
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(Configuration["Data:NWIdentity:ConnectionString"]));
            services.AddIdentity<AppUser, IdentityRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
                opts.Password.RequiredUniqueChars = 1;
            }).AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();
            // this is where we use the config info for our connection string
            services.AddDbContext<NorthwindContext>(options => options.UseSqlServer(Configuration["Data:Northwind:ConnectionString"]));
            // since we created an interface for our repository, we must map the 
            // interface to the concrete class to ensure that when an INorthwindRepository
            // is requested, a new instance of EFNorthwindRepository is returned
            services.AddTransient<INorthwindRepository, EFNorthwindRepository>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Make the authentication service available to the application (Order matters here)
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}