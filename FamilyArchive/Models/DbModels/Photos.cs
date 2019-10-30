using System;
using System.Collections.Generic;

namespace FamilyArchive.Models.DbModels
{
    public partial class Photos
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public byte[] PhotoBase64 { get; set; }
        public bool IsThumbnail { get; set; }
    }
}
