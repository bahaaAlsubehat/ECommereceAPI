using Ref.Lect4.Models;

namespace Ref.Lect4.DTO
{
    public class ResetPassword
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
