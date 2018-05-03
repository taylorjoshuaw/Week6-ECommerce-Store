using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class BasketDetailsViewModel
    {
        public ICollection<BasketItem> Items { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
