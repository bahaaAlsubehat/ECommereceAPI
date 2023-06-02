namespace Ref.Lect4.DTO
{
    public class UserInformationList
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }   
        
        public UserInformationList ( string name, string email, string phone)
        {
            Name = name;
            Email = email;
            Phone = phone;
        }


    }
}
