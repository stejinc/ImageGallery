using ImageGalleryDbHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PhotoApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoApp.Controllers
{
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IConfiguration config;

        public AccountsController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpPost("accounts/api/register"), DisableRequestSizeLimit]
        public IActionResult Register([FromForm]UserDetails userDetails)
        {
            try
            {
                var file = userDetails.profilePic;
                byte[] fileBytes = default(byte[]);
                if (file!=null && file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }
                }
                SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                bool status = dbHelper.CreateUser(userDetails.userName, userDetails.password, userDetails.firstName,
                    userDetails.lastName, userDetails.gender, userDetails.dateOfBirth, fileBytes);
                if (status)
                {
                    string tokenString = GenerateJSONWebToken(userDetails.userName);
                    return Ok(new { token = tokenString });
                }
                return new StatusCodeResult(500);
                
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost("accounts/api/login")]
        public IActionResult Login([FromBody]LoginDetails loginDetails)
        {
            IActionResult response = Unauthorized();
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();
            Task<bool> loginStatus = dbHelper.LoginUser(loginDetails.userName, loginDetails.password);
            if (loginStatus.Result)
            {
                string tokenString = GenerateJSONWebToken(loginDetails.userName);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string GenerateJSONWebToken(string userName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserName", userName)
            };

            var token = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
