using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StricklandPropane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Data
{
    public class SeedRoles
    {
        private static string AdminUserName => "Administrator";
        private static string AdminPassword => "$SuPeRsEcR37!";

        private static List<IdentityRole> Roles { get; } = new List<IdentityRole>
        {
            new IdentityRole(ApplicationRoles.Admin)
            {
                NormalizedName = ApplicationRoles.AdminNormalized,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },

            new IdentityRole(ApplicationRoles.Member)
            {
                NormalizedName = ApplicationRoles.MemberNormalized,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        };

        public static async Task Initialize(IServiceProvider services,
            UserManager<ApplicationUser> userManager)
        {
            using (ApplicationDbContext context = new ApplicationDbContext(
                services.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                await context.Database.EnsureCreatedAsync();

                if (await context.UserRoles.AnyAsync())
                {
                    return;
                }


            }
        }

        public static async Task AddUserRoles(ApplicationDbContext context)
        {
            if (await context.UserRoles.AnyAsync())
            {
                return;
            }

            
        }

        public static async Task AddAdminUser(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }

            ApplicationUser user = new ApplicationUser()
            {
                UserName = AdminUserName,
                NormalizedUserName = AdminUserName.ToUpper(),
                Email = AdminUserName,
                NormalizedEmail = AdminUserName.ToUpper(),
                EmailConfirmed = true,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            await userManager.CreateAsync(user);
        }
    }
}
