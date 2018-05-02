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

            Basket basket = await _basketDbContext.Baskets.FindAsync(user.CurrentBasketId.Value);

            // If the user's current basket is found, pass it to the view. Otherwise
            // pass in an empty list
            return View(basket?.Items ?? new List<BasketItem>());
        }
    }
}
