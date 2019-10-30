using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Models
{
    public class EditFileInfo
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public string NewHeader { get; set; }
        public string NewDescription { get; set; }
    }
}
