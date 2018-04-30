using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StricklandPropane.Data;
using StricklandPropane.Models;
using StricklandPropane.Models.Policies;

namespace StricklandPropane.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ProductDbContext _productDbContext;

        public ProductsController(ProductDbContext context)
        {
            _productDbContext = context;
        }

        [Authorize(Policy = ApplicationPolicies.MemberOnly)]
        public IActionResult Index()
        {
            return View(_productDbContext.Products);
        }

        [Authorize(Policy = ApplicationPolicies.AdminOnly)]
        public IActionResult Administrate()
        {
            return View(_productDbContext.Products);
        }

        [Authorize(Policy = ApplicationPolicies.AdminOnly)]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = ApplicationPolicies.AdminOnly)]
        public async Task<IActionResult> Edit(long? id)
        {
            Product product;

            // Make sure that an id was provided and that it corresponds to a Product
            // entity in the database
            if (!id.HasValue || 
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ApplicationPolicies.AdminOnly)]
        public async Task<IActionResult> CommitEdit(long? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            Product existingProduct = await _productDbContext.Products.FindAsync(id.Value);

            if (await TryUpdateModelAsync(existingProduct, "",
                p => p.Name, p => p.Description, p => p.Price, p => p.ImageHref))
            {
                try
                {
                    await _productDbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to commit changes to database. Please try again.");
                }
            }

            return View(existingProduct);
        }

        [Authorize(Policy = ApplicationPolicies.AdminOnly)]
        public async Task<IActionResult> Delete(long? id)
        {
            Product product;

            // Make sure that an id was provided and that it corresponds to a Product
            // entity in the database
            if (!id.HasValue || 
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }
    }
}