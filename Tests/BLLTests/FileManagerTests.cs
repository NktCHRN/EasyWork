using Business.Enums;
using Business.Interfaces;
using Business.Managers;
using Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Moq;
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
        private IFileManager _manager = null!;

        private readonly Mock<IConfiguration> _configurationMock = new();

        [SetUp]
        public void Setup()
        {
            _configurationMock.Setup(c => c.GetSection("FileSettings:MaxFileSize").Value).Returns("10737418240");
            _configurationMock.Setup(c => c.GetSection("FileSettings:MaxAvatarSize").Value).Returns("8388608");
            _manager = new FileManager(_configurationMock.Object);
        }

        [TearDown]
        public void Dispose()
        {
            var noChunksPath = $"{GetSolutionPath()}\\Data\\TempFiles\\testFolderNoFiles\\";
            if (Directory.Exists(noChunksPath))
                Directory.Delete(noChunksPath);
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
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image1.jpg";
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
        public void AddFileAsyncTest_NotSquareUserAvatar_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image4.jpg";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var formFile = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);

            // Act && Assert
            Assert.ThrowsAsync<ArgumentException>(() => _manager.AddFileAsync(formFile, newFileName, ewtype));
        }

        [Test]
        public async Task AddFileAsyncByteArrayTestB_ValidUserAvatar_AddsFile()
        {
            // Arrange
            var newFileName = "Temp.png";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image3.png";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
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
        public async Task AddFileAsyncByteArrayTestB2_ValidUserAvatar_AddsFile()
        {
            // Arrange
            var newFileName = "Temp.bmp";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image2.bmp";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
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
        public async Task AddFileAsyncByteArrayTestB3_ValidUserAvatar_AddsFile()
        {
            // Arrange
            var newFileName = "Temp.webp";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image5.webp";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
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
        public void AddFileAsyncByteArrayTest_NotSquareUserAvatar_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "image4.jpg";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            var file = File.ReadAllBytes(oldPath);

            // Act && Assert
            Assert.ThrowsAsync<ArgumentException>(() => _manager.AddFileAsync(file, newFileName, ewtype));
        }

        [Test]
        public void AddFileAsyncByteArrayTest_FakeFileType_ThrowsArgumentException()
        {
            // Arrange
            var newFileName = "Temp.jpg";
            var ewtype = EasyWorkFileTypes.UserAvatar;
            var oldFileName = "file1.pdf";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            var file = File.ReadAllBytes(oldPath);

            // Act && Assert
            Assert.ThrowsAsync<ArgumentException>(() => _manager.AddFileAsync(file, newFileName, ewtype));
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

        [Test]
        public void AddFileChunkAsyncTest_NullChunk_ThrowsArgumentNullException()
        {
            // Arrange
            var folderName = "testFolder";
            var chunkIndex = 1;
            IFormFile chunk = null!;
            var model = new FileChunkModel
            {
                Index = chunkIndex,
                ChunkFile = chunk
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _manager.AddFileChunkAsync(folderName, model),
                "Method does not throw ArgumentNullException if the chunk is empty");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFileChunkAsyncTest_NullOrEmptyFolderName_ThrowsArgumentException(string? folderName)
        {
            // Arrange
            var chunkIndex = 1;
            IFormFile chunk = new Mock<IFormFile>().Object;
            var model = new FileChunkModel
            {
                Index = chunkIndex,
                ChunkFile = chunk
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.AddFileChunkAsync(folderName!, model),
                "Method does not throw ArgumentException if the folderName is null or empty");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void AddFileChunkAsyncTest_NegativeIndexOrZero_ThrowsArgumentOutOfRangeException(int chunkIndex)
        {
            // Arrange
            var folderName = "testFolder";
            var chunk = new Mock<IFormFile>().Object;
            var model = new FileChunkModel
            {
                Index = chunkIndex,
                ChunkFile = chunk
            };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _manager.AddFileChunkAsync(folderName, model),
                $"Method does not throw ArgumentOutOfRangeException if the index is {chunkIndex}");
        }

        [Test]
        public void AddFileChunkAsyncTest_TooBigTotalSize_ThrowsInvalidOperationException()
        {
            // Arrange
            var folderName = "testFolder";
            var oldPathStart = GetSolutionPath() + "\\Tests\\TestFiles\\";
            var chunkPathStart = $"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\";
            Directory.CreateDirectory(chunkPathStart);
            var partOneName = "file3-1.txt";
            var partOnePath = oldPathStart + partOneName;
            File.Copy(partOnePath, chunkPathStart + $"1.tmp", true);
            var partOneLength = new FileInfo(partOnePath).Length;
            var partTwoName = "file3-2.txt";
            var partTwoPath = oldPathStart + partTwoName;
            var partTwoLength = new FileInfo(partTwoPath).Length;
            _configurationMock.Setup(c => c.GetSection("FileSettings:MaxFileSize").Value).Returns(partOneLength.ToString());
            _manager = new FileManager(_configurationMock.Object);
            using var stream = new MemoryStream(File.ReadAllBytes(partTwoPath).ToArray());
            var chunk = new FormFile(stream, 0, stream.Length, "streamFile", "anyName");
            var chunkIndex = 2;
            var model = new FileChunkModel
            {
                Index = chunkIndex,
                ChunkFile = chunk
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _manager.AddFileChunkAsync(folderName, model),
                $"Method does not throw InvalidOperationException if the total file size is too big");
        }

        [Test]
        public async Task AddFileChunkAsyncTest_AddsChunk()
        {
            // Arrange
            var oldFileName = "file1.pdf";
            var oldPath = GetSolutionPath() + "\\Tests\\TestFiles\\" + oldFileName;
            long oldLength = new FileInfo(oldPath).Length;
            using var stream = new MemoryStream(File.ReadAllBytes(oldPath).ToArray());
            var chunk = new FormFile(stream, 0, stream.Length, "streamFile", oldFileName);
            var folderName = "testFolder";
            var chunkIndex = 1;
            var model = new FileChunkModel
            {
                Index = chunkIndex,
                ChunkFile = chunk
            };

            // Act
            await _manager.AddFileChunkAsync(folderName, model);

            // Assert
            long newLength = 0;
            string newPath = $"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\{chunkIndex}.tmp";
            var exists = File.Exists(newPath);
            if (exists)
            {
                newLength = new FileInfo(newPath).Length;
                File.Delete(newPath);
            }
            Assert.IsTrue(exists, "Method does not copy a chunk to folder");
            Assert.AreEqual(oldLength, newLength, "Method damages the chunk");
        }

        private static (string OldPathStart, string ChunkPathStart) PrepareChunks(string folderName)
        {
            var oldPathStart = GetSolutionPath() + "\\Tests\\TestFiles\\";
            var chunkPathStart = $"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\";
            Directory.CreateDirectory(chunkPathStart);
            var partFilesNames = new string[] { "file3-1.txt", "file3-2.txt" };
            for (int i = 0; i < partFilesNames.Length; i++)
                File.Copy(oldPathStart + partFilesNames[i], chunkPathStart + $"{i + 1}.tmp", true);
            return (oldPathStart, chunkPathStart);
        }

        [Test]
        public async Task MergeChunksAsyncTest_AddsFileFromChunks()
        {
            // Arrange
            var folderName = "testFolder";
            (string oldPathStart, string chunkPathStart) = PrepareChunks(folderName);
            var fullFileName = "file3.txt";
            var expectedPath = $"{GetSolutionPath()}\\Data\\Files\\{folderName}{Path.GetExtension(fullFileName)}";
            var expectedContent = File.ReadAllBytes($"{oldPathStart}{fullFileName}");

            // Act
            await _manager.MergeChunksAsync(folderName, Path.GetExtension(fullFileName));

            // Assert
            if (Directory.Exists(chunkPathStart))
                Directory.Delete(chunkPathStart, true);
            byte[] actualContent = Array.Empty<byte>();
            if (File.Exists(expectedPath))
            {
                actualContent = File.ReadAllBytes(expectedPath);
                File.Delete(expectedPath);
            }
            Assert.IsTrue(expectedContent.SequenceEqual(actualContent), "Chunks have been merged incorrectly");
        }

        [Test]
        public async Task MergeChunksAsyncTest_DeletesChunks()
        {
            // Arrange
            var folderName = "testFolder";
            (_, string chunkPathStart) = PrepareChunks(folderName);
            var extension = ".txt";

            // Act
            await _manager.MergeChunksAsync(folderName, extension);

            // Assert
            Assert.IsFalse(Directory.Exists(chunkPathStart), "Chunks have not been deleted");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void MergeChunksAsyncTest_NullOrEmptyFolderName_ThrowsArgumentException(string? folderName)
        {
            // Arrange
            var extension = ".txt";

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.MergeChunksAsync(folderName!, extension),
                "Method does not throw ArgumentException if the folderName is null or empty");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void MergeChunksAsyncTest_NullOrEmptyExtension_ThrowsArgumentException(string? extension)
        {
            // Arrange
            var folderName = "testFolder";

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _manager.MergeChunksAsync(folderName, extension!),
                "Method does not throw ArgumentException if the extension is null or empty");
        }

        [Test]
        public void MergeChunksAsyncTest_NotExistingDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var folderName = "THISDIRECTORYDOESNOTEXIST";
            var extension = ".txt";

            // Act & Assert
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _manager.MergeChunksAsync(folderName, extension),
                "Method does not throw DirectoryNotFoundException if the directory was not found");
        }

        [Test]
        public void MergeChunksAsyncTest_NoChunks_ThrowsInvalidOperationException()
        {
            // Arrange
            var folderName = "testFolderNoFiles";
            var chunksPath = $"{GetSolutionPath()}\\Data\\TempFiles\\{folderName}\\";
            Directory.CreateDirectory(chunksPath);
            var extension = ".txt";

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.MergeChunksAsync(folderName, extension),
                "Method does not throw DirectoryNotFoundException if the directory was not found");
        }

        [Test]
        public void DeleteChunksTest_DeletesChunks()
        {
            // Arrange
            var folderName = "testFolder";
            (_, string chunkPathStart) = PrepareChunks(folderName);

            // Act
            _manager.DeleteChunks(folderName);

            // Assert
            Assert.IsFalse(Directory.Exists(chunkPathStart), "Chunks have not been deleted");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void DeleteChunksTest_NullOrEmptyFolderName_ThrowsArgumentException(string? folderName)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _manager.DeleteChunks(folderName!),
                "Method does not throw ArgumentException if the folderName is null or empty");
        }
    }
}
