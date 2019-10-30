using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Services
{
    public class PathHelperService
    {
        private IConfiguration _config;
        public PathHelperService(IConfiguration config)
        {
            _config = config;
        }

        public string GetFullPathToFile(string partPath)
        {
            string FullPath = string.Empty;
            if (!string.IsNullOrEmpty(partPath))
            {
                if (partPath.StartsWith("\\"))
                {
                    partPath = partPath.Substring(1, partPath.Length - 1);
                }
                FullPath = Path.Combine(_config["PhotosFolder"], partPath);
            }
            else
                FullPath = _config["PhotosFolder"];
            return FullPath;
        }

    }
}
