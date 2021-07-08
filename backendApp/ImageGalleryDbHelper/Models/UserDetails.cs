using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGalleryDbHelper.Models
{
    public class UserDetailsDb
    {
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string dateOfBirth { get; set; }
        public string profilePic { get; set; }
        public int gender { get; set; }

    }
}
