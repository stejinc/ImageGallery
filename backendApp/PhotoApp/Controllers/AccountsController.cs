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
                ErrorObjects response = dbHelper.CreateUser(userDetails.userName, userDetails.password, userDetails.firstName,
                    userDetails.lastName, userDetails.gender, userDetails.dateOfBirth, fileBytes);

                if(response == null)
                    return new StatusCodeResult(500);

                if (response.status)
                {
                    string tokenString = GenerateJSONWebToken(userDetails.userName);
                    return Ok(new { status = response.status, message = response.message,token = tokenString });
                }
                else
                    return BadRequest(new { status = response.status, message = response.message});
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while registering user");
                Console.WriteLine("Exception message: " + ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpPost("accounts/api/login")]
        public IActionResult Login([FromBody]LoginDetails loginDetails)
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();
            ErrorObjects loginStatus = dbHelper.LoginUser(loginDetails.userName, loginDetails.password);
            if (loginStatus.status)
            {
                string tokenString = GenerateJSONWebToken(loginDetails.userName);
                return Ok(new { token = tokenString, status = loginStatus.status, message = loginStatus.message });
            }

            return BadRequest(new { status = loginStatus.status, message = loginStatus.message});
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
                    {
                        return BadRequest(new { status = false, message = "Invalid user details" });
                    }

                    SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                    UserDetailsDb result = dbHelper.getUserDetails(usernameClaim);
                    if(result != null)
                        return Ok(new {status = true, message = "Success", data = result });
                }
            }catch(Exception ex)
            {
                Console.WriteLine("Exception :" +  ex.Message);
            }

            return BadRequest(new { status = false, message = "Failed to retireve user details" });
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
                        ErrorObjects updateResponse = dbHelper.UpdateProfilePic(usernameClaim, fileBytes);
                        if (updateResponse.status)
                            return Ok(new { status = true , message = updateResponse.message});
                        return BadRequest(new { status = false, message = "Error updating profile image" }) ;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            return BadRequest(new { status = false, message = "Error updating profile image" });
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
                    ErrorObjects loginStatus = dbHelper.LoginUser(usernameClaim, password.oldpassword);
                    if (loginStatus.status)
                    {
                        Console.WriteLine("Old password validated. Updating new password");
                        ErrorObjects updateResponse = dbHelper.UpdatePassword(usernameClaim, password.newpassword);
                        if (updateResponse.status)
                        {
                            string tokenString = GenerateJSONWebToken(usernameClaim);
                            return Ok(new { status = true, message = "Password updated successfully", token = tokenString });
                        }
                        return BadRequest(new { status = false, message = "Operation failed" });
                    }
                    Console.WriteLine("Unable to login with old password");
                    return BadRequest(new { status = false, message = "Incorrect old password"});
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            return BadRequest(new { status = false, message = "Operation failed" });
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
