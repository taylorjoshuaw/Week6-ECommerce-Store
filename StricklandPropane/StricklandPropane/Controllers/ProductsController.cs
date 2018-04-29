using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StricklandPropane.Data;
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
    }
}