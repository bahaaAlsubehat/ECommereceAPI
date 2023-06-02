using System.ComponentModel.DataAnnotations;

namespace Ref.Lect4.DTO
{
    public class ForgetPass
    {
        [Required(ErrorMessage = "The UserName is Required ")]
        [RegularExpression ("[A-Za-z0-9]{3,15}", ErrorMessage = "The UserName is Invalid, User Name must contain from 3 to 15 Letters")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "The New Passwored is Required")]
        [RegularExpression ("^(?=.{8,})", ErrorMessage= "Invalid Password")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage ="The Confirm Password is Required")]
        [RegularExpression("^(?=.{8,})", ErrorMessage ="Invalid Password")]
        public string ConfirmPassword { get; set; }
    }
}
