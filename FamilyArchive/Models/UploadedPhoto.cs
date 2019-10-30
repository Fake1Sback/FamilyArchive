using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Models
{
    public class UploadedPhoto
    {
        public string filename { get; set; }

        public byte[] fileContent { get; set; }
    }
}
