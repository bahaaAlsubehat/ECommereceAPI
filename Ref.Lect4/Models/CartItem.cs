using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class CartItem
    {
        public int CartItemId { get; set; }
        public int? CartId { get; set; }
        public int? ItemId { get; set; }
        public int? Qtn { get; set; }
        public string Note { get; set; }
        public double? NetPrice { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual Items Item { get; set; }
    }
}
