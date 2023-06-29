using System;
using System.Collections.Generic;

namespace Ref.Lect4.Models
{
    public partial class Item
    {
        public Item()
        {
            CartItems = new HashSet<CartItem>();
        }

        public int ItemId { get; set; }
        public int? CategoryId { get; set; }
        public double? Price { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int? Qtn { get; set; }
        public bool? IsAvailable { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
