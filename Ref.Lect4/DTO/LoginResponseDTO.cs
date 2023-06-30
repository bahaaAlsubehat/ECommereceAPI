using Ref.Lect4.Models;
using System.Security.Claims;
using Ref.Lect4.Helper;



namespace Ref.Lect4.DTO
{
    public class LoginResponseDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }

        public int LoginId { get; set; }

        public List<Cart> MyOrder { get; set; }
        public ClaimsIdentity? Email { get; internal set; }
    }
}
