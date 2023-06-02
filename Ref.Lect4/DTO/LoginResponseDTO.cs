﻿using Ref.Lect4.Models;

namespace Ref.Lect4.DTO
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }

        public int LoginId { get; set; }

        public List<Cart> MyOrder { get; set; }
    }
}
