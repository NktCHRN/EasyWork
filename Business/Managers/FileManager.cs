using Business.Enums;
using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;

namespace Business.Managers
{
    public class FileManager : IFileManager
    {
        public IEnumerable<(string, string)> AllowedImageTypes => _allowedImageTypes;

        private readonly IEnumerable<(string, string)> _allowedImageTypes = new (string, string)[]
        {
            ("bmp", "bmp"),
            ("gif", "gif"),
            ("ico", "vnd.microsoft.icon"),
            ("jpeg", "jpeg"),
            ("jpg", "jpeg"),
            ("png", "png"),
            ("tif", "tiff"),
            ("tiff", "tiff"),
            ("webp", "webp")
        };

        private readonly long _maxAvatarSize;

        private readonly long _maxFileSize;

        public FileManager(IConfiguration configuration)
        {
            _maxFileSize = long.Parse(configuration.GetSection("FileSettings:MaxFileSize").Value);
            _maxAvatarSize = long.Parse(configuration.GetSection("FileSettings:MaxAvatarSize").Value);
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < suffixes.Length && bytes >= 1024; i++, bytes /= 1024)
                dblSByte = bytes / 1024.0;
            return $"{dblSByte:0.##} {suffixes[i]}";
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
            string path = GetSolutionPath()!;
            if (ewtype == EasyWorkFileTypes.EasyWorkProjectImage)
                path += "\\WebAPI\\Images\\";
            else
            {
                path += "\\Data\\";
                path += ewtype switch
                {
                    EasyWorkFileTypes.UserAvatar => "UserAvatars\\",
                    _ => "Files\\",
                };
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path + name;
        }

        private const string _chunkExtension = ".tmp";

        public async Task AddFileChunkAsync(string folderName, FileChunkModel model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model), "Chunk cannot be null");
            if (model.ChunkFile is null)
                throw new ArgumentNullException(nameof(model.ChunkFile), "Chunk cannot be null");
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException("The name of the folder cannot be null or empty", nameof(folderName));
            if (model.Index <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Index), "Only positive indices are allowed");
            var path = $"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var totalSize = Directory.GetFiles(path)
            .Where(f =>
            {
                var parsed = int.TryParse(Path.GetFileNameWithoutExtension(f), out int number);
                return parsed && number > 0;
            })
            .Sum(f => new FileInfo(f).Length);
            if (totalSize + model.ChunkFile.Length > _maxFileSize)
                throw new InvalidOperationException($"The max total file length is {FormatBytes(_maxFileSize)}.");
            path += $"{model.Index}{_chunkExtension}";
            await DownloadFileToPath(model.ChunkFile, path);
        }

        public async Task MergeChunksAsync(string folderName, string? extension)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException("The name of the folder cannot be null or empty", nameof(folderName));
            var path = $"{GetSolutionPath()!}\\Data\\TempFiles\\{folderName}\\";
            var fileNames = Directory.EnumerateFiles(path, $"*{_chunkExtension}")
                    .Where(f =>
                    {
                        var parsed = int.TryParse(Path.GetFileNameWithoutExtension(f), out int number);
                        return parsed && number > 0;
                    });
            var bytes = new List<byte>();
            using (PhysicalFileProvider provider = new(path))
            {
                var contents = provider.GetDirectoryContents(string.Empty)
                    .Where(f =>
                    {
                        if (f.IsDirectory)
                            return false;
                        var parsed = int.TryParse(Path.GetFileNameWithoutExtension(f.Name), out int number);
                        return parsed && number > 0;
                    })
                    .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f.Name)));
                if (!contents.Any())
                    throw new InvalidOperationException("No chunks were found");
                foreach (IFileInfo fileInfo in contents)
                {
                    await using Stream reader = fileInfo.CreateReadStream();
                    int fileLength = (int)fileInfo.Length;
                    byte[] currentBytes = new byte[fileLength];
                    reader.Read(currentBytes, 0, fileLength);
                    bytes.AddRange(currentBytes);
                }
            }
            if (extension != null && !extension.StartsWith('.'))
                extension = '.' + extension;
            await AddFileAsync(bytes.ToArray(), folderName + extension, EasyWorkFileTypes.File);
            DeleteChunks(folderName);
        }

        public void DeleteChunks(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException("The name of the folder cannot be null or empty", nameof(folderName));
            Directory.Delete($"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\", true);
        }

        private static async Task DownloadFileToPath(IFormFile file, string path)
        {
            using var fileStream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fileStream);
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
                if (file.Length > _maxAvatarSize)
                    throw new ArgumentException($"The max length of the avatar is {FormatBytes(_maxAvatarSize)}");
                var data = await Image.IdentifyAsync(file.OpenReadStream());
                CheckAvatar(data);
            }
            else if (ewtype == EasyWorkFileTypes.File && file.Length > _maxFileSize)
                throw new ArgumentException($"The max length of the file is {FormatBytes(_maxFileSize)}");
            var path = GetPathByEWType(name, ewtype);
            await DownloadFileToPath(file, path);
        }

        private static void CheckAvatar(IImageInfo? avatar)
        {
            if (avatar is null)
                throw new ArgumentException("Cannot read image data");
            if (avatar.Width != avatar.Height)
                throw new ArgumentException("Image should be square");
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

        public async Task<byte[]> GetFileContentAsync(string name, EasyWorkFileTypes ewtype) => await File.ReadAllBytesAsync(GetPathByEWType(name, ewtype));

        public async Task<long> GetFileSizeAsync(string name, EasyWorkFileTypes ewtype) => (await File.ReadAllBytesAsync(GetPathByEWType(name, ewtype))).LongLength;

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
                if (file.LongLength > _maxAvatarSize)
                    throw new ArgumentException($"The max length of the avatar is {FormatBytes(_maxAvatarSize)}");
                var data = Image.Identify(file);
                CheckAvatar(data);
            }
            else if (ewtype == EasyWorkFileTypes.File && file.LongLength > _maxFileSize)
                throw new ArgumentException($"The max length of the file is {FormatBytes(_maxFileSize)}");
            var path = GetPathByEWType(name, ewtype);
            using var fileStream = new FileStream(path, FileMode.Create);
            await fileStream.WriteAsync(file);
        }
    }
}
