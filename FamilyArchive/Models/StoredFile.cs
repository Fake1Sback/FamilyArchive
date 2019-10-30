using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Models
{
    public class StoredFile
    {
        public string Name { get; set; }
        public bool IsVideo { get; set; }
        public bool IsFolder { get; set; }
        public bool FolderEmpty { get; set; }
    }
}
