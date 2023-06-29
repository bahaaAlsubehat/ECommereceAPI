using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Ref.Lect4.Models
{
    public partial class VerificationCode
    {
        public int VerificationCodeId { get; set; }
        public string? Code { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int? UserId { get; set; }

        public virtual User? User { get; set; }
    }
}
