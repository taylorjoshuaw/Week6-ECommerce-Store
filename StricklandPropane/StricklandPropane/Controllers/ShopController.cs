using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StricklandPropane.Data;
using StricklandPropane.Models;

namespace StricklandPropane.Controllers
{
    public class ShopController : Controller
    {
        private readonly ProductDbContext _productDbContext;

        public ShopController(ProductDbContext productDbContext)
        {
            _productDbContext = productDbContext;
        }

        public IActionResult Index()
        {
            return View(_productDbContext.Products);
        }

        public async Task<IActionResult> Details(long? id)
        {
            Product product;

            if (!id.HasValue ||
                (product = await _productDbContext.Products.FindAsync(id.Value)) is null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}