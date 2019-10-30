using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Models
{
    public class EncodedPhoto
    {
        public string PhotoName { get; set; }
        public string PhotoBase64 { get; set; }
        public bool IsVideo { get; set; }
        public string PhotoHeader { get; set; }
        public string PhotoDescription { get; set; }   
    }
}
