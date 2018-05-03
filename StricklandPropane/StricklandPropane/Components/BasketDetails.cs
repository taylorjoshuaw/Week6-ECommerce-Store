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

            BasketDetailsViewModel bvm = new BasketDetailsViewModel()
            {
                Items = await _productDbContext.Baskets.Include(b => b.Items)
                                                       .SelectMany(b => b.Items)
                                                       .Include(bi => bi.Product)
                                                       .ToListAsync()
            };

            // Find the total quantity in the basket by summing each line item's quantity
            bvm.TotalQuantity = bvm.Items.Select(bi => bi.Quantity)
                                         .Sum();

            // Find the grand total price in the basket by summing the products of each
            // line item's quantity by its product's price. The Product navigational
            // property was already included in the Items LINQ expression seen above.
            bvm.TotalPrice = bvm.Items.Select(bi => bi.Quantity * bi.Product.Price)
                                      .Sum();

            return View(bvm);
        }
    }
}
