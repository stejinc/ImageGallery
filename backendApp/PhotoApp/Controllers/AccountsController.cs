using ImageGalleryDbHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PhotoApp.Models;
using ImageGalleryDbHelper.Models;
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
        public IActionResult Register([FromForm]UserDetailsWithPassword userDetails)
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
            bool loginStatus = dbHelper.LoginUser(loginDetails.userName, loginDetails.password);
            if (loginStatus)
            {
                string tokenString = GenerateJSONWebToken(loginDetails.userName);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        [Authorize]
        [HttpGet("accounts/api/getUserDetails")]
        public IActionResult UserDetails()
        {
            string usernameClaim = null;
            try
            {
                var currentUser = HttpContext.User;
                if (currentUser.HasClaim(x => x.Type == "UserName"))
                {
                    usernameClaim = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                    if (usernameClaim == null)
                        return StatusCode(401);

                    SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                    UserDetailsDb result = dbHelper.getUserDetails(usernameClaim);
                    return Ok(result);
                }
            }catch(Exception ex)
            {
                Console.WriteLine("Exception :" +  ex.Message);
            }

            return BadRequest("Error processing the request");
        }

        [Authorize]
        [HttpPost("accounts/api/UpdateProfilePic")]
        public IActionResult UpdateProfilePic([FromForm] ProfileImage profileImage)
        {
            string usernameClaim = null;
            try
            {
                var currentUser = HttpContext.User;
                if (currentUser.HasClaim(x => x.Type == "UserName"))
                {
                    usernameClaim = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                    if (usernameClaim == null)
                        return StatusCode(401);

                    var file = profileImage.image;
                    byte[] fileBytes = null;

                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                        bool result = dbHelper.UpdateProfilePic(usernameClaim, fileBytes);
                        if(result)
                            return Ok(result);
                        return BadRequest("Error updating profile image");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            return BadRequest("Error updating profile image");
        }

        [Authorize]
        [HttpPost("accounts/api/UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] Passwords password)
        {
            string usernameClaim = null;
            IActionResult response = Unauthorized();
            try
            {
                var currentUser = HttpContext.User;
                if (currentUser.HasClaim(x => x.Type == "UserName"))
                {
                    usernameClaim = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                    if (usernameClaim == null)
                        return StatusCode(401);

                    SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                    bool loginStatus = dbHelper.LoginUser(usernameClaim, password.oldpassword);
                    if (loginStatus)
                    {
                        Console.WriteLine("Old password validated. Updating new password");
                        bool status = dbHelper.UpdatePassword(usernameClaim, password.newpassword);
                        if (status)
                        {
                            string tokenString = GenerateJSONWebToken(usernameClaim);
                            return Ok(new { token = tokenString });
                        }
                    }
                    Console.WriteLine("Unable to login with old password");
                    return BadRequest("Please check the old password. Authentication failed with old password");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            return BadRequest("Error updating profile image");
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
