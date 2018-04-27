using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StricklandPropane.Data;
using StricklandPropane.Models;

namespace StricklandPropane
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                UserManager<ApplicationUser> userManager = 
                    services.GetRequiredService<UserManager<ApplicationUser>>();

                try
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task task = SeedRoles.Initialize(services, userManager);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    while (!task.IsCompleted) { }
                }
                catch
                {
                    Console.Error.WriteLine(
                        "Could not seed database with admin user and roles.");
                    throw;
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
