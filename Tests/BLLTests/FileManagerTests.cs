using Business.Enums;
using Business.Interfaces;
using Business.Managers;
using Microsoft.AspNetCore.Http.Internal;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.BLLTests
{
    [TestFixture]
    public class FileManagerTests
    {
        private readonly IFileManager _manager = new FileManager();

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [TestCase("jpg")]
        [TestCase("JPEG")]
        [TestCase(".png")]
        [TestCase("tif")]
        [TestCase(".tiff")]
        public void IsValidImageTypeTest_ValidType_ReturnsTrue(string type)
        {
            // Act
            var result = _manager.IsValidImageType(type);

            // Assert
            Assert.IsTrue(result, "Method does not return true if image type is valid");
        }

        [Test]
        [TestCase(".jp")]
        [TestCase(".docx")]
        [TestCase("photo.png")]
        [TestCase("pdf")]
        [TestCase(".cs")]
        public void IsValidImageTypeTest_InvalidType_ReturnsFalse(string type)
        {
            // Act
            var result = _manager.IsValidImageType(type);

            // Assert
            Assert.IsFalse(result, "Method does not return false if image type is invalid");
        }

        [Test]
        [TestCase("jpg", "image/jpeg")]
        [TestCase("JPEG", "image/jpeg")]
        [TestCase(".png", "image/png")]
        [TestCase("tif", "image/tiff")]
        [TestCase(".tiff", "image/tiff")]
        [TestCase("svg", "image/svg+xml")]
        [TestCase("bmp", "image/bmp")]
        [TestCase("ico", "image/vnd.microsoft.icon")]
        [TestCase("gif", "image/gif")]
        [TestCase(".webp", "image/webp")]
        public void GetMIMETypeTest_ValidType_ReturnsTrueMIMEType(string type, string expected)
        {
            // Act
            var actual = _manager.GetImageMIMEType(type);

            // Assert
            Assert.AreEqual(expected, actual, "Method does not return correct MIME type");
        }

        [Test]
        [TestCase("jpeg", "image/jpeg")]
        [TestCase("png", "image/png")]
        [TestCase("tif", "image/tiff")]
        [TestCase("svg", "svg+xml")]
        [TestCase("bmp", "image/bmp")]
        [TestCase("ico", "image/vnd.microsoft.icon")]
        [TestCase("gif", "gif")]
        [TestCase("webp", "image/webp")]
        public void GetImageTypeTest_ValidType_ReturnsTrueMIMEType(string expected, string MIMEType)
        {
            // Act
            var actual = _manager.GetImageType(MIMEType);

            // Assert
            Assert.AreEqual(expected, actual, "Method does not return correct image type");
        }

        [Test]
        [TestCase(".jp")]
        [TestCase(".docx")]
        [TestCase("photo.png")]
        [TestCase("pdf")]
        [TestCase(".cs")]
        public void GetMIMETypeTest_InvalidType_ReturnsNull(string type)
        {
            // Arrange
            string? expected = null;

            // Act
            var actual = _manager.GetImageMIMEType(type);

            // Assert
            Assert.AreEqual(expected, actual, "Method does not return null if type is invalid");
        }

        private static string? GetSolutionPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
                directory = directory.Parent;
            return directory?.FullName;
        }

        [Test]
        public async Task AddFileAsyncTest_ValidFile_AddsFile()
        {
            // Arrange
            var newFileName = "TempFile.docx";
            var ewtype = EasyWorkFileTypes.File;
            var oldFileName = "file2.docx";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);

            // Act
            await _manager.AddFileAsync(formFile, newFileName, ewtype);

            // Assert
            long newLength = 0;
            string newPath = GetSolutionPath() + "\\Data\\Files\\" + newFileName;
            var exists = File.Exists(newPath);
            if (exists)
            {
                newLength = new FileInfo(newPath).Length;
                File.Delete(newPath);
            }
            Assert.IsTrue(exists, "Method does not copy a file to folder");
            Assert.AreEqual(oldLength, newLength, "Method damages the file");
        }

        [Test]
        public async Task AddFileAsyncTest_ValidUserAvatar_AddsFile()
        {
            // Arrange
            var newFileName = "Temp.svg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image4.svg";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);

            // Act
            await _manager.AddFileAsync(formFile, newFileName, ewtype);

            // Assert
            long newLength = 0;
            string newPath = GetSolutionPath() + "\\Data\\UserAvatars\\" + newFileName;
            var exists = File.Exists(newPath);
            if (exists)
            {
                newLength = new FileInfo(newPath).Length;
                File.Delete(newPath);
            }
            Assert.IsTrue(exists, "Method does not copy a file to folder");
            Assert.AreEqual(oldLength, newLength, "Method damages the file");
        }

        [Test]
        public async Task AddFileAsyncByteArrayTestB_ValidUserAvatar_AddsFile()
        {
            // Arrange
            var newFileName = "Temp.svg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image4.svg";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var file = await File.ReadAllBytesAsync(oldPath);

            // Act
            await _manager.AddFileAsync(file, newFileName, ewtype);

            // Assert
            long newLength = 0;
            string newPath = GetSolutionPath() + "\\Data\\UserAvatars\\" + newFileName;
            var exists = File.Exists(newPath);
            if (exists)
            {
                newLength = new FileInfo(newPath).Length;
                File.Delete(newPath);
            }
            Assert.IsTrue(exists, "Method does not copy a file to folder");
            Assert.AreEqual(oldLength, newLength, "Method damages the file");
        }

        [Test]
        public void AddFileAsyncTest_TooBigSize_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image6.jpg";
            var path = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            using var stream = new MemoryStream(File.ReadAllBytes(path).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.AddFileAsync(formFile, newFileName, ewtype),
                "Method does not throw ArgumentException if image size is too big");
        }

        [Test]
        public void AddFileAsyncByteArrayTest_TooBigSize_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image6.jpg";
            var path = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            using var stream = new MemoryStream(File.ReadAllBytes(path).ToArray());
            var file = File.ReadAllBytes(path);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.AddFileAsync(file, newFileName, ewtype),
                "Method does not throw ArgumentException if image size is too big");
        }

        [Test]
        public void AddFileAsyncTest_InvalidUserAvatarType_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.png";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "file2.docx";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.AddFileAsync(formFile, newFileName, ewtype),
                "Method does not throw ArgumentException if image type is invalid");
        }

        [Test]
        public void DeleteFileTest_ValidPath_DeletesFile()
        {
            // Arrange
            var fileName = "file2.docx";
            var ewtype = EasyWorkFileTypes.File;
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + fileName;
            var path = GetSolutionPath() + "\\Data\\Files\\" + fileName;
            File.Copy(oldPath, path);

            // Act
            _manager.DeleteFile(fileName, ewtype);

            // Assert
            var exists = File.Exists(path);
            Assert.IsFalse(exists, "Method does not deletes the file");
            if (exists)
                File.Delete(path);
        }
    }
}
