using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoApp.Models
{
    public class ProfileImage
    {
        public IFormFile image { get; set; }
    }
    public class UserImages : ProfileImage
    {
        public string description { get; set; }
        public string shareOption { get; set; }
    }
}
