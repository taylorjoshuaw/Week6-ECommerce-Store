using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class BasketItem
    {
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public long BasketId { get; set; }
        public Basket Basket { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
