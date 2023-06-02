using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class Cart
    {
        public Cart()
        {
            CartItem = new HashSet<CartItem>();
        }

        public int CartId { get; set; }
        public int? UserId { get; set; }
        public bool? IsActive { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItem { get; set; }
    }
}
