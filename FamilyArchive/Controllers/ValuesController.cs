using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FamilyArchive.Models;
using FamilyArchive.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using FamilyArchive.Filters;
using System.Net.Http.Headers;
using FamilyArchive.Models.DbModels;
using ImageMagick;
using System.Text;
using System.Drawing;

namespace FamilyArchive.Controllers
{ 
    [AllExceptionsFilter]
    public class ValuesController : Controller
    {
        private PathHelperService _pathHelper;
        private FileSystemService _fsService;
        private DbService _dbService;

        public ValuesController(PathHelperService pathHelper, FileSystemService fsService, DbService dbService)
        {
            _pathHelper = pathHelper;
            _fsService = fsService;
            _dbService = dbService;
        }

        public IActionResult GetMaxLoads(string path)
        {
            string FullPath = _pathHelper.GetFullPathToFile(path);
            return Content(_fsService.GetMaxLoadsAmount(FullPath).ToString());
        }

        public IActionResult GetPhotos(string path,int skip)
        {
            string FullPath = _pathHelper.GetFullPathToFile(path);
            List<EncodedPhoto> encodedPhotos = new List<EncodedPhoto>();
            List<string> Files = _fsService.GetAllFilesInFolder(FullPath);
            List<FileInfo> fileInfos = new List<FileInfo>();

            for (int i = 0; i < Files.Count; i++)
            {
                fileInfos.Add(new FileInfo(Files[i]));
            }
            fileInfos = fileInfos.OrderByDescending(f => f.CreationTime).Skip(skip * 10).Take(10).ToList();

            using (FamilyArchiveContext db = new FamilyArchiveContext())
            {
                for (int i = 0; i < fileInfos.Count; i++)
                {
                    EncodedPhoto encodedPhoto = _dbService.GetPhotoFromDb(path, fileInfos[i].Name, db);
                    if (encodedPhoto == null)
                        encodedPhoto = _fsService.GetEncodedPhotoFromFileSystem(Path.Combine(FullPath,fileInfos[i].Name));

                    encodedPhotos.Add(encodedPhoto);
                }
            }

            return Json(encodedPhotos);
        }

        public IActionResult GetVideo(string path)
        {
            FileStream fs = new FileStream(_pathHelper.GetFullPathToFile(path), FileMode.Open, FileAccess.Read, FileShare.Delete);
            return new FileStreamResult(fs, new MediaTypeHeaderValue("video/mp4").MediaType);
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotos(string path, [FromForm]IList<IFormFile> photos)
        {
            List<UploadedPhoto> uploadedPhotos = new List<UploadedPhoto>();
            for (int i = 0; i < photos.Count; i++)
            {
                string fileName = string.Empty;
                if (string.IsNullOrEmpty(path))
                    fileName = photos[i].FileName;
                else
                    fileName = Path.Combine(path, photos[i].FileName);

                MemoryStream memoryStream = new MemoryStream();
                photos[i].CopyTo(memoryStream);

                bool binaryCheck = false;

                if (photos[i].FileName.EndsWith(".mp4"))
                {
                    binaryCheck = memoryStream.IsMp4();
                }
                else if (photos[i].FileName.EndsWith(".jpg"))
                {
                    binaryCheck = memoryStream.IsJpg();
                }
                else if (photos[i].FileName.EndsWith(".png"))
                {
                    binaryCheck = memoryStream.IsPng();
                }

                if (!binaryCheck)
                    continue;

                UploadedPhoto uploadedPhoto = new UploadedPhoto { filename = fileName, fileContent = memoryStream.ToArray() };
                uploadedPhotos.Add(uploadedPhoto);

                string FullPath = _pathHelper.GetFullPathToFile(fileName);
                if (!System.IO.File.Exists(FullPath))
                    await System.IO.File.WriteAllBytesAsync(FullPath, uploadedPhoto.fileContent);
                else
                    uploadedPhotos.Remove(uploadedPhoto);
            }

            Task task = Task.Factory.StartNew(() =>
            {
                using (FamilyArchiveContext db = new FamilyArchiveContext())
                {
                    for (int i = 0; i < uploadedPhotos.Count; i++)
                    {
                        if (uploadedPhotos[i].filename.EndsWith(".png") || uploadedPhotos[i].filename.EndsWith(".jpg"))
                        {
                            _dbService.AddPhotoToDb(uploadedPhotos[i], db);
                        }
                        else if (uploadedPhotos[i].filename.EndsWith(".mp4"))
                        {
                            string FullPathToVideo = _pathHelper.GetFullPathToFile(uploadedPhotos[i].filename);
                            _dbService.GenerateThumbnailForVideo(uploadedPhotos[i].filename, FullPathToVideo, db);
                        }
                    }

                    db.SaveChanges();
                }
            });

            return Ok();
        }

        public IActionResult GetFolderContent(string path, int skip)
        {
            string FullPath = _pathHelper.GetFullPathToFile(path);
            List<StoredFile> storedFiles = _fsService.GetAllFolderContent(FullPath, skip);
            return Json(storedFiles);
        }

        [HttpPost]
        public IActionResult CreateFolder([FromBody] CreateFolderInfo folderInfo)
        {
            if (string.IsNullOrEmpty(folderInfo.FolderName))
                return BadRequest("Folder name is empty");

            string FullPath = _pathHelper.GetFullPathToFile(folderInfo.CurrentPath);
            List<char> RestrictedSymbols = Path.GetInvalidFileNameChars().ToList();
            RestrictedSymbols.Add('.');

            foreach (char c in folderInfo.FolderName)
            {
                foreach (char c2 in RestrictedSymbols)
                {
                    if (c.Equals(c2))
                        return BadRequest("Invalid folder name");
                }
            }

            string NewFolderFullPath = Path.Combine(FullPath, folderInfo.FolderName);
            if (!Directory.Exists(NewFolderFullPath))
            {
                Directory.CreateDirectory(NewFolderFullPath);
                return Ok();
            }
            else
                return BadRequest("Such directory allready exists");
        }

        [HttpPost]
        public IActionResult DeletePhoto([FromBody] DeleteFileInfo deleteFileInfo)
        {
            string FolderPath = _pathHelper.GetFullPathToFile(deleteFileInfo.Path);
            string FullPath = Path.Combine(FolderPath, deleteFileInfo.PhotoName);

            if (System.IO.File.Exists(FullPath))
            {
                System.IO.File.Delete(FullPath);
                _dbService.DeletePhotoFromDb(deleteFileInfo);

                return Ok();
            }
            else
                return Json("File does not exist");
        }

        [HttpPost]
        public IActionResult EditPhoto([FromBody]EditFileInfo editFileInfo)
        {
            if (_dbService.WriteHeaderDescription(editFileInfo))
                return Ok();
            else
                return BadRequest("No record in database. Wait or try to delete and add photo again");
        }

        public async Task<IActionResult> DownloadPhoto(string path)
        {
            string FullPath = _pathHelper.GetFullPathToFile(path);

            if (System.IO.File.Exists(FullPath))
            {
                byte[] file = await System.IO.File.ReadAllBytesAsync(FullPath);
                HttpContext.Response.Headers.Add("Content-Disposition", new Microsoft.Extensions.Primitives.StringValues("attachment"));
              
                string extension = string.Empty;
                if (path.EndsWith(".png"))
                    extension = "image/png";
                else if (path.EndsWith(".jpg"))
                    extension = "image/jpg";
                else if (path.EndsWith(".mp4"))
                    extension = "video/mp4";

                return File(file, extension,path.Split('\\').Last());
            }
            else
                return BadRequest("File does not exist");
        }
    }
}