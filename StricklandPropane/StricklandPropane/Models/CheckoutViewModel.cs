using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class CheckoutViewModel
    {
        public decimal ItemsSubTotal { get; set; }
        public decimal Shipping { get; set; }
    }
}
