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
        private readonly UserManager<ApplicationUser> _userManager;

        public BasketController(ProductDbContext productDbContext,
            UserManager<ApplicationUser> userManager)
        {
            _productDbContext = productDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(
            [Bind("ProductId", "Quantity", "ReturnUrl")] BasketAdderViewModel vm)
        {
            // Server-side validation for view components might be tricky. For now,
            // redirect the user to the Shop Index. jQuery client-side validation will
            // improve this experience once implemented
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Shop");
            }

            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            Basket basket = user.CurrentBasketId.HasValue ?
                await _productDbContext.Baskets.FindAsync(user.CurrentBasketId) : null;

            // If the user doesn't have an open basket yet, then create one
            if (basket is null || basket.Closed)
            {
                basket = (await _productDbContext.Baskets.AddAsync(new Basket()
                {
                    Closed = false,
                    CreationTime = DateTime.UtcNow,
                    UserId = user.Id
                })).Entity;

                await _productDbContext.SaveChangesAsync();

                // Set the user's current basket id to the new basket manually
                // since the user exists on a different context than the basket
                user.CurrentBasketId = basket.Id;
                await _userManager.UpdateAsync(user);
            }

            BasketItem item = await _productDbContext.BasketItems.FirstOrDefaultAsync(bi => bi.BasketId == basket.Id &&
                                                                                           bi.ProductId == vm.ProductId);
            // If the basket item doesn't exist then create it. Otherwise, just add the specified
            // quantity to the existing item
            if (item is null)
            {
                await _productDbContext.BasketItems.AddAsync(new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = vm.ProductId,
                    Quantity = vm.Quantity,
                    UserId = user.Id
                });
            }
            else
            {
                item.Quantity += vm.Quantity;
                _productDbContext.BasketItems.Update(item);
            }

            // Save changes and return the user to whence they came
            await _productDbContext.SaveChangesAsync();
            return RedirectToLocal(vm.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(BasketDetailsViewModel bvm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Shop");
            }

            foreach ((long itemId, int quantity) in bvm.Quantities)
            {
                BasketItem item = await _productDbContext.BasketItems.FindAsync(itemId);

                if (item is null)
                {
                    continue;
                }

                // If the new quantity is greater than 0, then update; otherwise, delete
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                    _productDbContext.BasketItems.Update(item);
                }
                else
                {
                    _productDbContext.Remove(item);
                }
            }

            await _productDbContext.SaveChangesAsync();
            return RedirectToLocal(bvm.ReturnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View();
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