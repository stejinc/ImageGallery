using ImageGalleryDbHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PhotoApp.Controllers
{
    [ApiController]
    [Authorize]
    public class HomeController : ControllerBase
    {
        //For testing purpose
        [AllowAnonymous]
        [HttpGet("home/api/isLoggedIn")]
        public IActionResult isLoggedIn()
        {
            return Ok();
        }

        [HttpPost("home/api/upload"), DisableRequestSizeLimit]
        public IActionResult Upload([FromForm] UserImages imageDetails)
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

                    var file = imageDetails.image;
                    byte[] fileBytes, thumbBytes = null;

                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }

                        // using (var ms = new MemoryStream())
                        // using (Image thumbnail = Image.FromStream(new MemoryStream(fileBytes)).GetThumbnailImage(200, 300, null, new IntPtr()))
                        // {
                        //     thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        //     thumbBytes = ms.ToArray();
                        // }

                        SqlImageDbHelper dbHelper = new SqlImageDbHelper();
                        

                        bool status = dbHelper.StoreUserImage(usernameClaim, fileBytes, null, imageDetails.description);
                        Console.WriteLine("Status after calling store image :" + status);
                        if (status)
                            return Ok();
                        return new StatusCodeResult(500);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return StatusCode(401);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured at store image:"+ ex.Message);
                Console.WriteLine("Exception occured at store image:"+ ex);
                return new StatusCodeResult(500);
            }
        }

        [HttpGet("home/api/image/{imageid}")]
        public IActionResult getImage(long imageid)
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();

            string userName = null;
            var currentUser = HttpContext.User;
            if (currentUser.HasClaim(x => x.Type == "UserName"))
            {
                userName = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                if (userName == null)
                    return StatusCode(401);

                if (userName == null)
                    return BadRequest("username null");

                var image = dbHelper.RetrieveImage(userName, imageid);
                if (image.Result == null)
                    return BadRequest();

                return Ok(image.Result);
            }

            return StatusCode(401);

        }

        [HttpGet("home/api/image")]
        public IActionResult getAllImage()
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();

            string userName = null;
            var currentUser = HttpContext.User;
            if (currentUser.HasClaim(x => x.Type == "UserName"))
            {
                userName = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                if (userName == null)
                    return StatusCode(401);

                if (userName == null)
                    return BadRequest("username null");
                var image = dbHelper.RetrieveAllImages(userName);

                if (image.Result == null)
                    return BadRequest();

                return Ok(image.Result);
            }

            return StatusCode(401);

        }

        [HttpDelete("home/api/image/{imageid}")]
        public IActionResult deleteImage(long imageid)
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();

            string userName = null;
            var currentUser = HttpContext.User;
            if (currentUser.HasClaim(x => x.Type == "UserName"))
            {
                userName = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                if (userName == null)
                    return StatusCode(401);

                if (userName == null)
                    return BadRequest("username null");
                bool deletionStatus = dbHelper.DeleteImage(imageid);

                if (!deletionStatus)
                    return BadRequest();

                return Ok();
            }

            return StatusCode(401);

        }
    }
}
