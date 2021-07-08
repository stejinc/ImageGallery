using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGalleryDbHelper.Models
{
    public class ErrorObjects
    {
        public bool status { get; set; }
        public string message { get; set; }

        public ErrorObjects()
        {
            this.status = false;
            this.message = "Internal server error";
        }
    }
}
