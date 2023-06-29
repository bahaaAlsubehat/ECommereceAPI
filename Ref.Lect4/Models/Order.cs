using System;
using System.Collections.Generic;

namespace Ref.Lect4.Models
{
    public partial class Order
    {
        public int OrderId { get; set; }
        public int? CartId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public double? TotalPrice { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApprovedBy { get; set; }
        public string? Note { get; set; }
        public int? StatusId { get; set; }

        public virtual User? ApprovedByNavigation { get; set; }
        public virtual OrderStatus? Status { get; set; }
    }
}
