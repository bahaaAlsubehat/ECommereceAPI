using Aspose.Email;
using Aspose.Email.Clients;
using Aspose.Email.Clients.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ref.Lect4.DTO;
using Ref.Lect4.Helper;
using Ref.Lect4.Models;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Ref.Lect4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly OnlineStoreContext _storeContext;
        private readonly IConfiguration _configuration;
        private string contraints, outlookemail, outlookpassword;
        private readonly OnlineStoreHelper _helper;
        public IndexController(OnlineStoreContext storeContext, IConfiguration configuration)
        {
            // Data Source

            this._storeContext = storeContext;
            _configuration = configuration;
            contraints = _configuration.GetValue<string>("SecretToken").ToString();
            outlookemail = _configuration.GetValue<string>("OutlookEmail").ToString();
            outlookpassword = _configuration.GetValue<string>("OutlookPassword").ToString();
            _helper = new OnlineStoreHelper(_storeContext, _configuration);
        }


        // First IAction Function To create Account and we use Httppostverb, we use To object User and Login cause if we create Account we must save info in Login(UserName and Password) so we will Use DTO to collect Two object in one Object
        [HttpPost]
        [Route("Register")]
        public IActionResult CreateAccount([FromBody] NewUserAccount newUser)
        {
            // We unhancing the data what I need 

            User user = new User();
            user.UserTypeId = 1;
            using (Aes aes = Aes.Create())
            {
                user.Name = _helper.EncryptString(newUser.Name, aes.Key, aes.IV);  // Initialization
                user.Email = _helper.EncryptString(newUser.Email, aes.Key, aes.IV);
                user.Phone = _helper.EncryptString(newUser.Phone, aes.Key, aes.IV);
                user.Key = Convert.ToBase64String(aes.Key);
                user.Iv = Convert.ToBase64String(aes.IV);
                _storeContext.Add(user);
                _storeContext.SaveChanges();

            }

            Login login = new Login();
            login.Username = _helper.GenerateSHA384String(newUser.Email); // hashing
            login.Password = _helper.GenerateSHA384String(newUser.Password);
            login.UserId = _storeContext.Users.Where(x => x.Email == user.Email).OrderByDescending(x => x.UserId).First().UserId;  // This is called Linq we Filter, Condition and Sorting
            // Select Top(1) UserId from [User] Where Email = 'Bahaa@gmail.com' order by [UserId] desc.
            _storeContext.Add(login);
            _storeContext.SaveChanges();
            _helper.SendOTPCode(newUser.Email, (int)login.UserId);

            return Ok("");
        }

       

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginAccount login)
        {
            login.Email = _helper.GenerateSHA384String(login.Email);
            login.Password = _helper.GenerateSHA384String(login.Password);
            // Linq Object
            var UserLoginInf = _storeContext.Logins.Where(x => x.Username == login.Email && x.Password == login.Password).SingleOrDefault();
            if (UserLoginInf == null)
            {
                // Unautharized
                return Unauthorized("Either UserName or Password is not Correct");
            }
            else
            {
                // Update Login Table
                UserLoginInf.IsActive = true;
                _storeContext.Update(UserLoginInf);
                _storeContext.SaveChanges();

                // Return User Data
                var response = _storeContext.Users.Where(x => x.UserId == UserLoginInf.UserId).FirstOrDefault();
                //return Ok(response);
                LoginResponseDTO respon = new LoginResponseDTO();
                respon.UserId = response.UserId;
                respon.UserName = response.Email;
                // Join - First way (I need to inner join between user table and usertypetable)
                var users = _storeContext.Users.Where(x => x.UserId == UserLoginInf.UserId).ToList();
                var userTypes = _storeContext.UserTypes.ToList();
                var innerjoin = from u in users
                                join t in userTypes
                                on u.UserTypeId equals t.UserTypeId
                                select new UserInformation
                                {
                                    User = u,
                                    UserType = t
                                };
                string id = innerjoin.ElementAt(0).User.UserId.ToString();
                //return Ok(innerjoin);

                // Join Second way 
                var output = users.Join(userTypes, x => x.UserId, y => y.UserTypeId, (user1, user2) => new
                {
                    userId = user1.UserId,
                    UserName = user1.Email,
                    UserType = user2.Name



                }).ToList();
                //return Ok(output);


                LoginResponseDTO respond = new LoginResponseDTO();
                respond.UserId = response.UserId;
                respond.UserName = response.Email;

                // Third Way Join(Single Connection) في حال ما بدنا نعمل كلاس جديد في ال دي تي اوخ في اليوزر تايب تيبل
                var user = _storeContext.Users.Where(x => x.UserId == UserLoginInf.UserId).FirstOrDefault();
                LoginResponseDTO RESPONS = new LoginResponseDTO();
                RESPONS.UserId = user.UserId;
                RESPONS.UserName = user.Email;
                RESPONS.UserType = _storeContext.UserTypes.Where(x => x.UserTypeId == user.UserTypeId).First().Name;
                RESPONS.LoginId = _storeContext.Logins.Where(x => x.UserId == user.UserId).First().LoginId;
                RESPONS.MyOrder = _storeContext.Carts.Where(x => x.UserId == user.UserId).ToList();
                return Ok(_helper.GenerateJwtToken(RESPONS));






            }


        }

        [HttpPut]
        public IActionResult Logout([FromHeader] string token)
        {
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO();

            if (loginResponseDTO.LoginId != 0)
            {
                var login = _storeContext.Logins.Where(x => x.LoginId == loginResponseDTO.LoginId).SingleOrDefault();

                if (login != null)
                {
                    if (login.IsActive == true)
                    {
                        login.IsActive = false;
                        _storeContext.Update(login);
                        _storeContext.SaveChanges();
                        return Ok("Logout");
                    }
                }

            }
            return Unauthorized("Please Login First");

        }

        

        [HttpPut]
        [Route("Resetpassword")]
        public IActionResult UpdatePassword([FromBody] ResetPassword reset)
        {

            reset.UserName = _helper.GenerateSHA384String(reset.UserName);

            var Check = _storeContext.Logins.Where(x => x.Username == reset.UserName && x.Password == reset.OldPassword).FirstOrDefault();
            if (Check != null)
            {
                if (reset.NewPassword == reset.ConfirmPassword)
                {
                    Check.Password = _helper.GenerateSHA384String(reset.ConfirmPassword); // or Check.Password = reset.NewPassword; ...... The Same 
                    _storeContext.Update(Check);
                    _storeContext.SaveChanges();
                    return Ok(Check);
                }
            }
            return Ok("Invalid UserName or PassWord");

        }


        [HttpPut]
        [Route("ForgetPassword")]
        public IActionResult ForgetPassword([FromBody] ForgetPass FORGET)
        {
            FORGET.UserName = _helper.GenerateSHA384String(FORGET.UserName); // validation username 

            var Check1 = _storeContext.Logins.Where(x => x.Username == FORGET.UserName).FirstOrDefault();

            if (Check1 != null)
            {
                if (FORGET.NewPassword == FORGET.ConfirmPassword)
                {
                    Check1.Password =   _helper.GenerateSHA384String(FORGET.ConfirmPassword);
                    _storeContext.Update(Check1);
                    _storeContext.SaveChanges();
                    return Ok(Check1);

                }
            }

            return Ok("Invalid UserName or PassWord");
        }

        // We want to delete account and this method is rarely used cause the companies saved all account and they fill activation is acrive or not active 
        [HttpDelete]
        [Route("RemoveAccount/{Id}")]
        public IActionResult DeleteAccount(int Id)
        {
            var account = _storeContext.Logins.Where(x => x.UserId == Id).SingleOrDefault();
            if (account != null)
            {
                _storeContext.Remove(account);
                _storeContext.SaveChanges();
                // bellow to make sure that the account is removed 
                var user = _storeContext.Users.Where(x => x.UserId == Id).SingleOrDefault();
                if (user != null)
                {
                    _storeContext.Remove(user);
                    _storeContext.SaveChanges();
                }
                return Ok("Account Removed Successfully ");
            }
            else
            {
                return Ok("Account Not Found");
            }


        }
        
       

    }
}
