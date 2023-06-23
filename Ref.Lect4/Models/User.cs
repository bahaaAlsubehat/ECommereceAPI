using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class User
    {
        public User()
        {
            Cart = new HashSet<Cart>();
            Login = new HashSet<Login>();
            Order = new HashSet<Order>();
        }

        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? UserTypeId { get; set; }
        public string Key { get; set; }
        public string Iv { get; set; }

        public virtual UserType UserType { get; set; }
        public virtual ICollection<Cart> Cart { get; set; }
        public virtual ICollection<Login> Login { get; set; }
        public virtual ICollection<Order> Order { get; set; }
    }
}
