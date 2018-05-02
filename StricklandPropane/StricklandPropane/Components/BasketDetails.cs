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
        private readonly BasketDbContext _basketDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public BasketDetails(BasketDbContext basketDbContext,
            UserManager<ApplicationUser> userManager)
        {
            _basketDbContext = basketDbContext;
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

            Basket basket = await _basketDbContext.Baskets.Include(b => b.Items)
                                                          .FirstOrDefaultAsync(b => b.Id == user.CurrentBasketId.Value);

            // If we couldn't find the basket, items weren't found, or the basket has
            // already been closed, then just pass an empty list to the view
            if (basket is null || basket.Items is null || basket.Closed)
            {
                return View(new List<BasketItem>());
            }

            return View(basket.Items.ToList());
        }
    }
}
