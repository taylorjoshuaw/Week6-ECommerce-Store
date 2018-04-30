using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StricklandPropane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Data
{
    public static class SeedProducts
    {
        private static List<Product> Products { get; } = new List<Product>
        {
            new Product()
            {
                Name = "20lbs Propane Tank",
                Description = "A perfect choice for the griller who prefers to taste the meat, not the heat!",
                ImageHref = "http://clipart-library.com/image_gallery/81212.png",
                Price = 47.64M
            },

            new Product()
            {
                Name = "20lbs Propane Tank Exchange",
                Description = "Already have a tank but cooked for all of your family and in-laws? Exchange your tank for a new, filled tank so you can keep tasting the meat and not the heat!",
                ImageHref = "http://clipart-library.com/image_gallery/81212.png",
                Price = 14.95M
            }
        };

        public static async Task Initialize(IServiceProvider services)
        {
            using (ProductDbContext context = new ProductDbContext(
                services.GetRequiredService<DbContextOptions<ProductDbContext>>()))
            {
                if (await context.Products.AnyAsync())
                {
                    return;
                }

                await context.Database.EnsureCreatedAsync();

                await context.AddRangeAsync(Products);
                await context.SaveChangesAsync();
            }
        }
    }
}
