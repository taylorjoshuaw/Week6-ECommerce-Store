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
    public static class SeedRoles
    {
        private static string AdminEmail => "admin@admin.com";
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

                await AddRoles(context);
                await AddAdminUser(context, userManager);
                await AddUserRoles(context);
            }
        }

        public static async Task AddRoles(ApplicationDbContext context)
        {
            if (await context.Roles.AnyAsync())
            {
                return;
            }

            await context.Roles.AddRangeAsync(Roles);
            await context.SaveChangesAsync();
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
                UserName = AdminEmail,
                NormalizedUserName = AdminEmail.ToUpper(),
                Email = AdminEmail,
                NormalizedEmail = AdminEmail.ToUpper(),
                EmailConfirmed = true,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            await userManager.CreateAsync(user, AdminPassword);
        }

        public static async Task AddUserRoles(ApplicationDbContext context)
        {
            if (await context.UserRoles.AnyAsync())
            {
                return;
            }

            IdentityUserRole<string> userRole = new IdentityUserRole<string>()
            {
                UserId = (await context.Users.SingleAsync(u => u.UserName == AdminEmail)).Id,
                RoleId = (await context.Roles.SingleAsync(r => r.Name == ApplicationRoles.Admin)).Id
            };

            await context.AddAsync(userRole);
            await context.SaveChangesAsync();
        }
    }
}
