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
    [Authorize(Policy = ApplicationPolicies.AdminOnly)]
    public class ProductsController : Controller
    {
        private readonly ProductDbContext _productDbContext;

        public ProductsController(ProductDbContext context)
        {
            _productDbContext = context;
        }

        public IActionResult Index()
        {
            return View(_productDbContext.Products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CommitCreate(
            [Bind("Name", "Description", "Price", "ImageHref")] Product newProduct)
        {
            // Only create an item if the model state is valid
            if (ModelState.IsValid)
            {
                await _productDbContext.Products.AddAsync(newProduct);

                try
                {
                    await _productDbContext.SaveChangesAsync();

                    // TODO(taylorjoshuaw): Redirect to details view once it's implemented
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    // Something went wrong, add the error to the model state and give the
                    // user another chance to commit their new product
                    ModelState.AddModelError("", "Unable to commit changes to database. Please try again.");

                    // TODO(taylorjoshuaw): Add logging here
                }
            }

            // Something went wrong. Give the user another chance to create their product
            return View(newProduct);
        }


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

        public async Task<IActionResult> Delete(long? id)
        {
            Product product;

            // Make sure that an id was provided and that it corresponds to a Product
            // entity in the database
            if (!id.HasValue ||
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> CommitDelete(long? id)
        {
            Product product;

            // Make sure that an id was provided and that it corresponds to a Product
            // entity in the database
            if (!id.HasValue ||
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return NotFound();
            }

            _productDbContext.Products.Remove(product);

            try
            {
                await _productDbContext.SaveChangesAsync();
            }
            catch
            {
                // TODO(taylorjoshuaw): Add logging here
                return View(product);
            }

            // TODO(taylorjoshuaw): Change to redirect to Details once implemented
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long? id)
        {
            Product product;

            if (!id.HasValue ||
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }
    }
}