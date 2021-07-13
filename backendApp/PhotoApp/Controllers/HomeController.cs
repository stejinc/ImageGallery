using ImageGalleryDbHelper;
using ImageGalleryDbHelper.Models;
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
            return Ok("Server is reachable");
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
                        ErrorObjects uploadStatus = dbHelper.StoreUserImage(usernameClaim, fileBytes, null, imageDetails.shareOption, imageDetails.description);
                        return Ok(new { status = uploadStatus.status, message = uploadStatus.message });
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
                    return BadRequest(new { status = false, message = "Invalid username" });

                var image = dbHelper.RetrieveImage(userName, imageid);
                if (image.Result == null)
                    return BadRequest(new { status = false, message = "Failed to retrieve image" });

                return Ok(new { status = true, data = image.Result , message = "Success"});
            }

            return StatusCode(401);

        }

        //[HttpGet("home/api/imagecount")]
        //public IActionResult imageCount()
        //{
        //    SqlImageDbHelper dbHelper = new SqlImageDbHelper();

        //    string userName = null;
        //    var currentUser = HttpContext.User;
        //    if (currentUser.HasClaim(x => x.Type == "UserName"))
        //    {
        //        userName = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
        //        if (userName == null)
        //            return BadRequest(new { status = false, message = "Invalid username" });
        //        int? Count = dbHelper.GetTotalImageCountPerUser(userName);

        //        if (Count == null)
        //            return BadRequest(new { status = false, message = "Failed to retrieve image count" });

        //        return Ok(new { status = true, count = Count, message = "Success" });
        //    }

        //    return StatusCode(401);
        //}

        [HttpGet("home/api/image/{pageSize}/{pageOffset}")]
        public IActionResult getUserImages(int pageSize, int pageOffset)
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();

            string userName = null;
            var currentUser = HttpContext.User;
            if (currentUser.HasClaim(x => x.Type == "UserName"))
            {
                userName = currentUser.Claims.FirstOrDefault(x => x.Type == "UserName").Value;
                if (userName == null)
                    return BadRequest(new { status = false, message = "Invalid username" });
                var image = dbHelper.RetrievePaginatedImages(userName, pageSize, pageOffset);

                if (image == null)
                    return BadRequest(new { status = false, message = "Failed to retrieve images" });

                return Ok(new { status = true, isNext = image.next, data = image.imageContents, message = "Success" });
            }

            return StatusCode(401);

        }

        [AllowAnonymous]
        [HttpGet("home/api/sharedimages/{pageSize}/{pageOffset}/{isLoggedIn}")]
        public IActionResult getSharedImages(int pageSize, int pageOffset, bool isLoggedIn)
        {
            SqlImageDbHelper dbHelper = new SqlImageDbHelper();
            string sharedOption = "public";
            if (isLoggedIn)
                sharedOption = "internal";

            var image = dbHelper.RetrieveSharedPaginatedImages(pageSize, pageOffset, sharedOption);

            if (image == null)
                return BadRequest(new { status = false, message = "Failed to retrieve images" });

            return Ok(new { status = true, isNext = image.next, data = image.imageContents, message = "Success" });

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
                    return BadRequest(new { status = false, message = "Invalid username" });
                ErrorObjects deletionStatus = dbHelper.DeleteImage(imageid);

                if (deletionStatus.status)
                    return Ok(new { status = true, message = deletionStatus.message });
                
                return BadRequest(new { status = true, message = deletionStatus.message });
            }

            return BadRequest(new { status = false, message = "Operation failed" });

        }
    }
}
