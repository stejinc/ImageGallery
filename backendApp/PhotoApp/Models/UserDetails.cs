using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoApp.Models
{
    public class UserDetails
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string firstName { get; set; }
        public string? lastName { get; set; }
        public string? dateOfBirth { get; set; }
        public IFormFile? profilePic { get; set; }
        public int gender { get; set; }

    }


    public class TokenUserDetails
    {
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string dateOfBirth { get; set; }

    }
}
