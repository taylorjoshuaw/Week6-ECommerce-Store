using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class Basket
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ICollection<BasketItem> Items { get; set; }

        public DateTime CreationTime { get; set; }

        public bool Closed { get; set; }
    }
}
