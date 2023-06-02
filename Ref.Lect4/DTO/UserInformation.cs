using Ref.Lect4.Models;

namespace Ref.Lect4.DTO
{    

    // We Create this class to join between User Table and UserType Table 
    public class UserInformation
    {
        public User User { get; set; }
        public UserType UserType { get; set; }

    }
}
