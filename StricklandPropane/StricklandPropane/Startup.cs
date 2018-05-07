using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Html;
using StricklandPropane.Data;
using StricklandPropane.Models;
using StricklandPropane.Models.Policies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace StricklandPropane
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /*
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<ProductDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            */

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration["DbPass"]));

            services.AddDbContext<ProductDbContext>(options =>
                options.UseSqlServer(Configuration["DbPass"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login";
            });

            services.AddAuthentication()
                .AddTwitter(twitterOptions =>
                {
                    twitterOptions.ConsumerKey = Configuration["TwitterConsumerKey"];
                    twitterOptions.ConsumerSecret = Configuration["TwitterConsumerSecret"];
                    twitterOptions.RetrieveUserDetails = true;
                })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["GoogleClientId"];
                    googleOptions.ClientSecret = Configuration["GoogleClientSecret"];
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(ApplicationPolicies.AdminOnly, p => p.RequireRole(ApplicationRoles.Admin));
                options.AddPolicy(ApplicationPolicies.MemberOnly, p => p.RequireRole(ApplicationRoles.Member, ApplicationRoles.Admin));
                options.AddPolicy(ApplicationPolicies.TexansOnly, p => p.RequireClaim(ClaimTypes.StateOrProvince, ((int)State.Texas).ToString()));
                options.AddPolicy(ApplicationPolicies.PropaneAdvocatesOnly, p => p.RequireClaim("GrillingPreference", ((int)GrillingPreference.Propane).ToString()));
            });

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes => routes.MapRoute(
                name: "Default",
                template: "{controller=Home}/{action=Index}/{id?}"));
        }
    }
}
