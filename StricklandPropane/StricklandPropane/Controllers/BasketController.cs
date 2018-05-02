using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using StricklandPropane.Data;
using StricklandPropane.Models;
using StricklandPropane.Models.Policies;

namespace StricklandPropane.Controllers
{
    [Authorize(Policy = ApplicationPolicies.MemberOnly)]
    public class BasketController : Controller
    {
        private readonly ProductDbContext _productDbContext;
        private readonly BasketDbContext _basketDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public BasketController(ProductDbContext productDbContext,
            BasketDbContext basketDbContext, UserManager<ApplicationUser> userManager)
        {
            _productDbContext = productDbContext;
            _basketDbContext = basketDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(long productId, int quantity, string returnUrl = null)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Basket basket = await _basketDbContext.Baskets.FindAsync(user.CurrentBasketId.Value);

            // If the user doesn't have an open basket yet, then create one
            if (basket is null || basket.Closed)
            {
                basket = (await _basketDbContext.Baskets.AddAsync(new Basket()
                {
                    Closed = false,
                    CreationTime = DateTime.UtcNow,
                    UserId = user.Id
                })).Entity;

                await _basketDbContext.SaveChangesAsync();

                // Set the user's current basket id to the new basket manually
                // since the user exists on a different context than the basket
                user.CurrentBasketId = basket.Id;
                await _userManager.UpdateAsync(user);
            }

            BasketItem item = await _basketDbContext.BasketItems.FirstOrDefaultAsync(bi => bi.BasketId == basket.Id &&
                                                                                           bi.ProductId == productId);
            // If the basket item doesn't exist then create it. Otherwise, just add the specified
            // quantity to the existing item
            if (item is null)
            {
                await _basketDbContext.BasketItems.AddAsync(new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = user.Id
                });
            }
            else
            {
                item.Quantity += quantity;
                _basketDbContext.BasketItems.Update(item);
            }

            // Save changes and return the user to whence they came
            await _basketDbContext.SaveChangesAsync();
            return RedirectToLocal(returnUrl);
        }

        /// <summary>
        /// Attempts to redirect the user to the specifed url if it is
        /// local to the site. Otherwise, the user will be redirected
        /// to the Index action for the Shop controller.
        /// </summary>
        /// <param name="redirectUrl">The url to redirect to</param>
        /// <returns>An action result that redirects the user either
        /// to the specified url (if local) or to the Index action
        /// of the Shop controller.</returns>
        private IActionResult RedirectToLocal(string redirectUrl)
        {
            if (Url.IsLocalUrl(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Shop");
        }
    }
}