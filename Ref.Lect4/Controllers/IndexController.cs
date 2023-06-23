using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ref.Lect4.DTO;
using Ref.Lect4.Models;
using System.Data;
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
        private string contraints;
        public IndexController(OnlineStoreContext storeContext, IConfiguration configuration)
        {
            // Data Source

            this._storeContext = storeContext;
            _configuration = configuration;
            contraints = _configuration.GetValue<string>("SecretToken").ToString();
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
                user.Name = EncryptString(newUser.Name, aes.Key,aes.IV);  // Initialization
                user.Email = EncryptString(newUser.Email , aes.Key, aes.IV);
                user.Phone = EncryptString(newUser.Phone, aes.Key,aes.IV);
                user.Key = Convert.ToBase64String(aes.Key);
                user.Iv = Convert.ToBase64String(aes.IV);
                _storeContext.Add(user);
                _storeContext.SaveChanges();

            }
           
            Login login = new Login();
            login.Username = GenerateSHA384String(newUser.Email); // hashing
            login.Password = GenerateSHA384String(newUser.Password);
            login.UserId = _storeContext.User.Where(x => x.Email == user.Email).OrderByDescending(x => x.UserId).First().UserId;  // This is called Linq we Filter, Condition and Sorting
            // Select Top(1) UserId from [User] Where Email = 'Bahaa@gmail.com' order by [UserId] desc.
            _storeContext.Add(login);
            _storeContext.SaveChanges();

            return Ok("");
        }

        // We want to create Token for encryption Login

        [NonAction]
        public string GenerateJwtToken(LoginResponseDTO logincredintial)  // 1- Create Method
        {
            var tokenHandler = new JwtSecurityTokenHandler();  // 2- create object from class JwtSecurityTokenHandler in Token Package I have istalled it 
            var jwttoken = contraints; // declare type to get SecretToken from appsetting.json
            var tokenkey = Encoding.UTF8.GetBytes(jwttoken.ToCharArray()); // Array of char يوصلني مقطع على شكل  
            var TokenDescriptor = new SecurityTokenDescriptor  // هون بحدد الاشياء اللي بدي اعمللها تشفير
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, logincredintial.UserName),
                    new Claim("Userid", logincredintial.UserId.ToString()),
                    new Claim("Loginid", logincredintial.LoginId.ToString()),
                    new Claim("UserType", logincredintial.UserType)
                }),
                Expires = DateTime.Now.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey)  // ready to encoding
                , SecurityAlgorithms.HmacSha512Signature)  // HmacSha256Signature or HmacSha512Signature  are an algorithim method for encryption

            };
            var token = tokenHandler.CreateToken(TokenDescriptor);
            return tokenHandler.WriteToken(token);

        }



        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromBody] LoginAccount login)
        {
            login.Email = GenerateSHA384String(login.Email);
            login.Password = GenerateSHA384String(login.Password);
            // Linq Object
            var UserLoginInf = _storeContext.Login.Where(x => x.Username == login.Email && x.Password == login.Password).SingleOrDefault();
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
                var response = _storeContext.User.Where(x => x.UserId == UserLoginInf.UserId).FirstOrDefault();
                //return Ok(response);
                LoginResponseDTO respon = new LoginResponseDTO();
                respon.UserId = response.UserId;
                respon.UserName = response.Email;
                // Join - First way (I need to inner join between user table and usertypetable)
                var users = _storeContext.User.Where(x => x.UserId == UserLoginInf.UserId).ToList();
                var userTypes = _storeContext.UserType.ToList();
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
                var user = _storeContext.User.Where(x => x.UserId == UserLoginInf.UserId).FirstOrDefault();
                LoginResponseDTO RESPONS = new LoginResponseDTO();
                RESPONS.UserId = user.UserId;
                RESPONS.UserName = user.Email;
                RESPONS.UserType = _storeContext.UserType.Where(x => x.UserTypeId == user.UserTypeId).First().Name;
                RESPONS.LoginId = _storeContext.Login.Where(x => x.UserId == user.UserId).First().LoginId;
                RESPONS.MyOrder = _storeContext.Cart.Where(x => x.UserId == user.UserId).ToList();
                return Ok(GenerateJwtToken(RESPONS));






            }


        }


        [HttpPut]
        [Route("Resetpassword")]
        public IActionResult UpdatePassword([FromBody] ResetPassword reset)
        {

            reset.UserName = GenerateSHA384String(reset.UserName);

            var Check = _storeContext.Login.Where(x => x.Username == reset.UserName && x.Password == reset.OldPassword).FirstOrDefault();
            if (Check != null)
            {
                if (reset.NewPassword == reset.ConfirmPassword)
                {
                    Check.Password = GenerateSHA384String(reset.ConfirmPassword); // or Check.Password = reset.NewPassword; ...... The Same 
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
            FORGET.UserName = GenerateSHA384String(FORGET.UserName); // validation username 

            var Check1 = _storeContext.Login.Where(x => x.Username == FORGET.UserName).FirstOrDefault();
            
            if (Check1 != null)
            {
                if (FORGET.NewPassword == FORGET.ConfirmPassword)
                {
                    Check1.Password = GenerateSHA384String(FORGET.ConfirmPassword);
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
            var account = _storeContext.Login.Where(x => x.UserId == Id).SingleOrDefault();
            if (account != null)
            {
                _storeContext.Remove(account);
                _storeContext.SaveChanges();
                // bellow to make sure that the account is removed 
                var user = _storeContext.User.Where(x => x.UserId == Id).SingleOrDefault();
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
        [NonAction]
        static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                // Create a new MemoryStream object to contain the encrypted bytes.
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Create a CryptoStream object to perform the encryption.
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Encrypt the plaintext.
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        encrypted = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        [NonAction]
        static string GenerateSHA384String(string inputString)
        {
            SHA384 sha384 = SHA384Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha384.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        [NonAction]
        static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();


        }
}   }
