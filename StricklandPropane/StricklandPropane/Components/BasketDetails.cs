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
            // Always create a view model to avoid the view component view trying
            // to take the current action's model (possible MVC bug?)
            BasketDetailsViewModel bvm = new BasketDetailsViewModel();

            long? currentBasketId =
                (await _userManager.GetUserAsync(HttpContext.User))?.CurrentBasketId;

            // If the user is logged in and has a basket, fill in the view model;
            // otherwise, the default values will suffice
            if (currentBasketId.HasValue)
            {
                // Retrieve all of the items in the current basket, making sure
                // to include the Product navigational property
                bvm.Items = await _productDbContext.Baskets.Where(b => b.Id == currentBasketId)
                                                           .Include(b => b.Items)
                                                           .SelectMany(b => b.Items)
                                                           .Include(bi => bi.Product)
                                                           .ToListAsync();

                // Find the total quantity in the basket by summing each line item's quantity
                bvm.TotalQuantity = bvm.Items.Select(bi => bi.Quantity)
                                             .Sum();

                // Find the grand total price in the basket by summing the products of each
                // line item's quantity by its product's price. The Product navigational
                // property was already included in the Items LINQ expression seen above.
                bvm.TotalPrice = bvm.Items.Select(bi => bi.Quantity * bi.Product.Price)
                                          .Sum();
            }

            return View(bvm);
        }
    }
}
