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
            if (!User.Identity.IsAuthenticated)
            {
                return View(new List<BasketItem>());
            }

            string userId = _userManager.GetUserId(HttpContext.User);

            return View(await _basketDbContext.BasketItems.Where(bi => bi.UserId == userId)
                                                          .Include(bi => bi.Basket)
                                                          .Where(bi => !bi.Basket.Closed)
                                                          .ToListAsync());
        }
    }
}
