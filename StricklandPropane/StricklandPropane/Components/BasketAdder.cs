using Microsoft.AspNetCore.Mvc;
using StricklandPropane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Components
{
    public class BasketAdder : ViewComponent
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IViewComponentResult> InvokeAsync(long productId)
        {
            return View(new BasketAdderViewModel()
            {
                ProductId = productId,
                Quantity = 1
            });
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
