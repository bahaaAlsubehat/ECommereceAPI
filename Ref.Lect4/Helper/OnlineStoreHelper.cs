
using Aspose.Email.Clients;
using Microsoft.IdentityModel.Tokens;
using Ref.Lect4.DTO;
using Ref.Lect4.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SmtpClient = Aspose.Email.Clients.Smtp.SmtpClient;

namespace Ref.Lect4.Helper
{
    public class OnlineStoreHelper
    {
        private readonly OnlineStoreContext _storeContext;
        private readonly IConfiguration _configuration;
        private string contraints, outlookemail, outlookpassword;
        public OnlineStoreHelper(OnlineStoreContext storeContext, IConfiguration configuration)
        {
            // Data Source

            this._storeContext = storeContext;
            _configuration = configuration;
            contraints = _configuration.GetValue<string>("SecretToken").ToString();
            outlookemail = _configuration.GetValue<string>("OutlookEmail").ToString();
            outlookpassword = _configuration.GetValue<string>("OutlookPassword").ToString();
        }



        #region GenerateToken
        // We want to create Token for encryption Login
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
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey)  // ready to encoding
                , SecurityAlgorithms.HmacSha512Signature)  // HmacSha256Signature or HmacSha512Signature  are an algorithim method for encryption

            };
            var token = tokenHandler.CreateToken(TokenDescriptor);
            return tokenHandler.WriteToken(token);

        }

        public bool ValidateJWTtoken(string tokenString, out LoginResponseDTO respon)
        {
            String toke = "Bearer " + tokenString;
            var jwtEncodedString = toke.Substring(7);

            var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
            DateTime datetime = DateTime.UtcNow;
            DateTime expired = token.ValidTo;

            if (datetime < expired)
            {
                LoginResponseDTO temprespon = new LoginResponseDTO();
                temprespon.UserId = Int32.Parse(token.Claims.First(c => c.Type == "UserId").Value.ToString());
                temprespon.LoginId = Int32.Parse(token.Claims.First(c => c.Type == "LoginId").Value.ToString());
                temprespon.MyOrder = null;
                temprespon.UserName = token.Claims.First(c => c.Type == "UserName").Value.ToString();
                temprespon.UserType = token.Claims.First(c => c.Type == "UserType").Value.ToString();
                respon = temprespon;
                // int UserId = Int32.Parse((token.Claims.First(c => c.Type == "Userid").Value.ToString()));

                return true;
            }
            respon = null;
            return false;
        }
        // Decoding Code Below
        public bool ValidateJWTtoken(string tokenString)
        {
            String toke = "Bearer " + tokenString;
            var jwtEncodedString = toke.Substring(7);

            var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
            DateTime datetime = DateTime.UtcNow;
            DateTime expired = token.ValidTo;

            if (datetime < expired)
            {
                // int UserId = Int32.Parse((token.Claims.First(c => c.Type == "Userid").Value.ToString()));

                return true;
            }
            return false;

        }
        #endregion



        #region Encryption
        public string EncryptString(string plainText, byte[] key, byte[] iv)
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

        #endregion




        #region Hashing
        public string GenerateSHA384String(string inputString)
        {
            SHA384 sha384 = SHA384Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha384.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        public string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();


        }

        #endregion


        #region Two Factor Authantication
        //Create OTP Authantication (Second Factor To Authantication)
        public void SendOTPCode(string email, int userid)
        {
            Random random = new Random();
            int Vcode = random.Next(111111, 999999);

            VerificationCode verificationCode = new VerificationCode();
            verificationCode.Code = Vcode.ToString();
            verificationCode.UserId = userid;
            verificationCode.ExpiredDate = DateTime.UtcNow.AddSeconds(30);
            _storeContext.Add(verificationCode);
            _storeContext.SaveChanges();

            // create a new instance of MailMessage Class
            MailMessage mailmessage = new MailMessage();
            // set subject to the message, body and sender information
            mailmessage.Subject = "Verification Code Message";
            mailmessage.Body = "Use this following code \n" + Vcode + "\n to Confirm your Login";
            mailmessage.From = new MailAddress(outlookemail, "OnlineStore" );
            // Add to Reciepents and CC Reciepents
            mailmessage.To.Add(new MailAddress(email, "Reciepent 1" ));

            SmtpClient Client = new SmtpClient();
            // specify your mail Host, UserName, Password, port # security option
           
            Client.Host = "smtp.office365.com";
            Client.Username = outlookemail;
            Client.Password = outlookpassword;
            Client.Port = 587;
            Client.SecurityOptions = SecurityOptions.SSLExplicit;
            try
            {
                // Send this message 
                Client.Send((IConnection)mailmessage);
                Client.Dispose();

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString);
            }

        }
        #endregion


    }
}
