using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StricklandPropane.Data;
using StricklandPropane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Components
{
    public class BasketDetails : ViewComponent
    {
        private readonly ProductDbContext _productDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public BasketDetails(ProductDbContext productDbContext,
            UserManager<ApplicationUser> userManager)
        {
            _productDbContext = productDbContext;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            // If the user is not signed in or doesn't have a basket yet, just pass
            // an empty list to the view
            if (user is null || !user.CurrentBasketId.HasValue)
            {
                return View(new List<BasketItem>());
            }

            List<BasketItem> items = await _productDbContext.Baskets.Include(b => b.Items)
                                                                    .SelectMany(b => b.Items)
                                                                    .Include(bi => bi.Product)
                                                                    .ToListAsync();

            return View(items);
        }
    }
}
