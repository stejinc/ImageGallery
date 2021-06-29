using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoApp.Models
{
    public class UserImages
    {
        public IFormFile image { get; set; }
        public string description { get; set; }
    }
}
