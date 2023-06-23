using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class Items
    {
        public Items()
        {
            CartItem = new HashSet<CartItem>();
        }

        public int ItemId { get; set; }
        public int? CategoryId { get; set; }
        public double? Price { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int? Qtn { get; set; }
        public bool? IsAvailable { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<CartItem> CartItem { get; set; }
    }
}
