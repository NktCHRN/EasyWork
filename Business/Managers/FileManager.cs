﻿using Business.Enums;
using Business.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Business.Managers
{
    public class FileManager : IFileManager
    {
        public static IEnumerable<(string, string)> AllowedImageTypes 
        { get
            {
                return new (string, string)[]
                {
                    ("bmp", "bmp"),
                    ("gif", "gif"),
                    ("ico", "vnd.microsoft.icon"),
                    ("jpeg", "jpeg"),
                    ("jpg", "jpeg"),
                    ("png", "png"),
                    ("svg", "svg+xml"),
                    ("tif", "tiff"),
                    ("tiff", "tiff"),
                    ("webp", "webp")
                };
            } 
        }

        public string? GetSolutionPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
                directory = directory.Parent;
            return directory?.FullName;
        }

        public string GetPathByEWType(string name, EasyWorkFileTypes ewtype)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name), "File name cannot be null");
            string path = GetSolutionPath() + "\\Data\\";
            path += ewtype switch
            {
                EasyWorkFileTypes.UserAvatar => "UserAvatars\\",
                _ => "Files\\",
            };
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path + name;
        }

        public async Task AddFileAsync(IFormFile file, string? name, EasyWorkFileTypes ewtype)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file), "File cannot be null");
            if (name is null)
                name = file.FileName;
            if (ewtype == EasyWorkFileTypes.UserAvatar)
            {
                var type = Path.GetExtension(name);
                var realType = Path.GetExtension(file.FileName);
                if (!IsValidImageType(type) || !IsValidImageType(realType))
                    throw new ArgumentException("This extension is not allowed");
                if (file.Length > 8000000)
                    throw new ArgumentException("The max length of the avatar is 8 MB");
            }
            var path = GetPathByEWType(name, ewtype);
            using var fileStream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fileStream);
        }

        public bool IsValidImageType(string type)
        {
            type = type.ToLower();
            if (type.StartsWith('.'))
                type = type[1..];
            return AllowedImageTypes.Any(t => t.Item1 == type);
        }

        public void DeleteFile(string name, EasyWorkFileTypes ewtype)
        {
            var path = GetPathByEWType(name, ewtype);
            File.Delete(path);
        }

        public string? GetImageMIMEType(string type)
        {
            type = type.ToLower();
            if (type.StartsWith('.'))
                type = type[1..];
            var found = AllowedImageTypes.SingleOrDefault(t => t.Item1 == type);
            if (!(found == default))
                return "image/" + found.Item2;
            return null;
        }

        public string? GetImageType(string MIMEtype)
        {
            var start = "image/";
            if (MIMEtype.StartsWith(start))
                MIMEtype = MIMEtype[start.Length..];
            var found = AllowedImageTypes.FirstOrDefault(t => t.Item2 == MIMEtype);
            if (!(found == default))
                return found.Item1;
            return null;
        }

        public FileStream GetFileStream(string name, EasyWorkFileTypes ewtype) => File.OpenRead(GetPathByEWType(name, ewtype));

        public byte[] GetFileContent(string name, EasyWorkFileTypes ewtype) => File.ReadAllBytes(GetPathByEWType(name, ewtype));

        public async Task AddFileAsync(byte[] file, string name, EasyWorkFileTypes ewtype)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file), "File cannot be null");
            if (name is null)
                throw new ArgumentNullException(nameof(name), "Name cannot be null");
            if (ewtype == EasyWorkFileTypes.UserAvatar)
            {
                var type = Path.GetExtension(name);
                if (!IsValidImageType(type))
                    throw new ArgumentException("This extension is not allowed");
                if (file.Length > 8000000)
                    throw new ArgumentException("The max length of the avatar is 8 MB");
            }
            var path = GetPathByEWType(name, ewtype);
            using var fileStream = new FileStream(path, FileMode.Create);
            await fileStream.WriteAsync(file);
        }
    }
}
