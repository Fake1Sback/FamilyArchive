using FamilyArchive.Models;
using FamilyArchive.Models.DbModels;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyArchive.Services
{
    public class DbService
    {
        public bool WriteHeaderDescription(EditFileInfo editFileInfo)
        {
            string searchString = string.Empty;
            if (string.IsNullOrEmpty(editFileInfo.Path))
                searchString = editFileInfo.FileName;
            else
                searchString = Path.Combine(editFileInfo.Path, editFileInfo.FileName);

            using (FamilyArchiveContext db = new FamilyArchiveContext())
            {
                Photos photo = db.Photos.Where(p => p.Name == searchString).FirstOrDefault();
                if (photo != null)
                {
                    photo.Header = editFileInfo.NewHeader;
                    photo.Description = editFileInfo.NewDescription;
                    db.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
        }

        public void DeletePhotoFromDb(DeleteFileInfo deleteFileInfo)
        {
            using (FamilyArchiveContext db = new FamilyArchiveContext())
            {
                string searchString = string.Empty;
                if (string.IsNullOrEmpty(deleteFileInfo.Path))
                    searchString = deleteFileInfo.PhotoName;
                else
                    searchString = Path.Combine(deleteFileInfo.Path, deleteFileInfo.PhotoName);

                List<Photos> photos = db.Photos.Where(p => p.Name == searchString).ToList();
                db.Photos.RemoveRange(photos);
                db.SaveChanges();
            }
        }

        public void AddPhotoToDb(UploadedPhoto uploadedPhoto, FamilyArchiveContext context)
        {
            if (context.Photos.Where(p => p.Name == uploadedPhoto.filename && !p.IsThumbnail).FirstOrDefault() == null)
            {
                byte[] Base64Photo;
                if (uploadedPhoto.fileContent.Length > 1000000)
                {
                    MemoryStream memoryStream = new MemoryStream(uploadedPhoto.fileContent);

                    ImageOptimizer imageOptimizer = new ImageOptimizer();
                    imageOptimizer.LosslessCompress(memoryStream);

                    using (MagickImage sprite = new MagickImage(memoryStream))
                    {
                        sprite.Quality = 100;

                        sprite.AutoOrient();

                        if (sprite.Width > sprite.Height)
                            sprite.Resize(1280, 960);
                        else
                            sprite.Resize(960, 1280);

                        Base64Photo = Encoding.ASCII.GetBytes(sprite.ToBase64());
                    }
                }
                else
                    Base64Photo = Encoding.ASCII.GetBytes(Convert.ToBase64String(uploadedPhoto.fileContent));

                Photos DbPhotoToAdd = new Photos();
                DbPhotoToAdd.Name = uploadedPhoto.filename;
                DbPhotoToAdd.IsThumbnail = false;
                DbPhotoToAdd.PhotoBase64 = Base64Photo;
                context.Photos.Add(DbPhotoToAdd);
            }
        }

        public EncodedPhoto GetPhotoFromDb(string partPath,string filename, FamilyArchiveContext context)
        {
            string searchString = string.Empty;
            if (string.IsNullOrEmpty(partPath))
                searchString = filename;
            else
                searchString = Path.Combine(partPath, filename);

            Photos photo = context.Photos.Where(p => p.Name == searchString).FirstOrDefault();
            EncodedPhoto encodedPhoto;
            if (photo != null)
            {
                encodedPhoto = new EncodedPhoto();
                string Base64PhotoString = string.Empty;

                if (photo.Name.EndsWith(".png") && !photo.IsThumbnail)
                {
                    Base64PhotoString = "data:image/png;base64," + Encoding.ASCII.GetString(photo.PhotoBase64);
                    encodedPhoto.IsVideo = false;
                }
                else if ((photo.Name.EndsWith(".jpg") || photo.Name.EndsWith(".jpeg")) && !photo.IsThumbnail)
                {
                    Base64PhotoString = "data:image/jpg;base64," + Encoding.ASCII.GetString(photo.PhotoBase64);
                    encodedPhoto.IsVideo = false;
                }
                else if (photo.Name.EndsWith(".mp4") && photo.IsThumbnail)
                {
                    Base64PhotoString = "data:image/png;base64," + Encoding.ASCII.GetString(photo.PhotoBase64);
                    encodedPhoto.IsVideo = true;
                }

                encodedPhoto.PhotoName = photo.Name.Split('\\').Last();
                encodedPhoto.PhotoHeader = photo.Header;
                encodedPhoto.PhotoDescription = photo.Description;
                encodedPhoto.PhotoBase64 = Base64PhotoString;
                return encodedPhoto;
            }
            else
                return null;
        }

        public void GenerateThumbnailForVideo(string filename, string fullPathToVideo, FamilyArchiveContext context)
        {
            string ReplacingPart = fullPathToVideo.Split('\\').Last();
            string fullPathToThumbnail = fullPathToVideo.Replace(ReplacingPart, "temp_thumbnail.png");

            if (File.Exists(fullPathToThumbnail))
                File.Delete(fullPathToThumbnail);

            string ThumbnailGenerationString = string.Format(@"-ss 3 -i ""{0}"" -vframes 1 -s 1280x960 ""{1}""", fullPathToVideo, fullPathToThumbnail);
   
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg\\bin\\ffmpeg.exe"),
                Arguments = ThumbnailGenerationString
            };

            Process process = new Process()
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit(5000);

            byte[] ThumbnailImage = System.IO.File.ReadAllBytes(fullPathToThumbnail);
            System.IO.File.Delete(fullPathToThumbnail);

            Photos photo = new Photos();
            photo.Name = filename;
            photo.IsThumbnail = true;
            photo.PhotoBase64 = Encoding.ASCII.GetBytes(Convert.ToBase64String(ThumbnailImage));
            context.Photos.Add(photo);
        }
    }
}
