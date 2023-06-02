using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class Category
    {
        public Category()
        {
            Items = new HashSet<Items>();
        }

        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Items> Items { get; set; }
    }
}
