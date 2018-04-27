using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StricklandPropane.Data;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Administrate()
        {
            return View(_productDbContext.Products);
        }
    }
}