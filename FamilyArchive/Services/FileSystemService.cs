using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FamilyArchive.Models;
using System.Diagnostics;
using FamilyArchive.Models.DbModels;
using System.Text;

namespace FamilyArchive.Services
{
    public class FileSystemService
    {
        public int GetMaxLoadsAmount(string FullPath)
        {
            List<string> AllFilesInFolder = GetAllFilesInFolder(FullPath);
            int LoadsCount = AllFilesInFolder.Count() / 10;
            int rest = AllFilesInFolder.Count() % 10;

            if (rest != 0)
                LoadsCount += 1;

            return LoadsCount;
        }

        public List<string> GetAllFilesInFolder(string FullPath)
        {
            List<string> Files = new List<string>();
            Files.AddRange(Directory.GetFiles(FullPath).Where(s => !s.EndsWith("temp_thumbnail.png") && (s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".mp4"))));         
            return Files;
        }

        public List<string> GetAllFoldersInFolder(string FullPath)
        {
            List<string> Folders = new List<string>();
            Folders.AddRange(Directory.GetDirectories(FullPath));
                
            for(int i = 0;i < Folders.Count;i++)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Folders[i]);
                if (directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    Folders.RemoveAt(i);
            }

            return Folders;
        }

        public List<StoredFile> GetAllFolderContent(string FullPath, int skip)
        {
            List<StoredFile> storedFiles = new List<StoredFile>();
            List<string> files = GetAllFilesInFolder(FullPath);
            
            if (skip == 0)
            {
                List<string> folders = GetAllFoldersInFolder(FullPath);

                for (int i = 0; i < folders.Count; i++)
                {
                    int FolderSubDirs = Directory.GetDirectories(folders[i]).Length;
                    int FilesInFolder = Directory.GetFiles(folders[i]).Length;
                    bool Empty;
                    if (FolderSubDirs == 0 && FilesInFolder == 0)
                        Empty = true;
                    else
                        Empty = false;
                    storedFiles.Add(new StoredFile { IsFolder = true, FolderEmpty = Empty, Name = folders[i].Split('\\').Last() });
                }
            }

            List<FileInfo> fileInfos = new List<FileInfo>();
            for(int i = 0;i < files.Count; i++)
            {
                fileInfos.Add(new FileInfo(files[i]));
            }
            fileInfos = fileInfos.OrderByDescending(f => f.CreationTime).Skip(skip * 10).Take(10).ToList();

            for (int i = 0; i < fileInfos.Count; i++)
            {
                if(fileInfos[i].FullName.EndsWith(".png") || fileInfos[i].FullName.EndsWith(".jpg"))
                    storedFiles.Add(new StoredFile { IsFolder = false, IsVideo = false, FolderEmpty = true, Name = fileInfos[i].FullName.Split('\\').Last() });
                else if(fileInfos[i].FullName.EndsWith(".mp4"))
                    storedFiles.Add(new StoredFile { IsFolder = false, IsVideo = true, FolderEmpty = true, Name = fileInfos[i].FullName.Split('\\').Last() });
            }

            return storedFiles;
        }

        public EncodedPhoto GetEncodedPhotoFromFileSystem(string Fullpath)
        {
            EncodedPhoto encodedPhoto = new EncodedPhoto();

            byte[] photoByteArray = System.IO.File.ReadAllBytes(Fullpath);
            string imgBase64 = Convert.ToBase64String(photoByteArray);
            string imgSrcString = string.Empty;

            if (Fullpath.EndsWith(".png"))
            {
                imgSrcString = string.Format("data:image/png;base64,{0}", imgBase64);
                encodedPhoto.IsVideo = false;
            }
            else if (Fullpath.EndsWith(".jpg"))
            {
                imgSrcString = string.Format("data:image/jpg;base64,{0}", imgBase64);
                encodedPhoto.IsVideo = false;
            }
            else if (Fullpath.EndsWith(".mp4"))
                encodedPhoto.IsVideo = true;

            encodedPhoto.PhotoBase64 = imgSrcString;
            encodedPhoto.PhotoName = Fullpath.Split('\\').Last();

            return encodedPhoto;
        }
    }
}
