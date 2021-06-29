using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryDbHelper.Models
{
    public class ImageContent
    {
        public long imageId { get; set; }
        public string image { get; set; }
        public string description { get; set; }
    }
}
