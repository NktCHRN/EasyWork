using Business.Interfaces;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class UserAvatarServiceTests
    {
        private ApplicationDbContext _context = null!;

        private IUserAvatarService _service = null!;

        private readonly Mock<IFileManager> _fileManagerMock = new();

        private IFileManager _fileManager = null!;

        private UserManager<User> _userManager = null!;

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedData();
            _fileManager = _fileManagerMock.Object;
            var store = new UserStore<User, IdentityRole<int>, IdentityDbContext<User, IdentityRole<int>, int>, int>(_context);
            _userManager = GetTestUserManager(store);
            _service = new UserAvatarService(_userManager, _fileManager);
            foreach (var user in _context.Users)
                _userManager.UpdateSecurityStampAsync(user).Wait();
        }

        private static UserManager<User> GetTestUserManager
            (UserStore<User, IdentityRole<int>, IdentityDbContext<User, IdentityRole<int>, int>, int> store)
        {
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<User>>();
            var validator = new Mock<IUserValidator<User>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<User>>
            {
                new PasswordValidator<User>()
            };
            var userManager = new UserManager<User>(store, options.Object, new PasswordHasher<User>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<User>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<User>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }

        private void SeedData()
        {
            var usersWithFormat = new (User, string)[]
            {
                (_context.Users.Single(u => u.Id == 3), "bmp"),
                (_context.Users.Single(u => u.Id == 4), "png"),
                (_context.Users.Single(u => u.Id == 5), "jpg"),
            };
            foreach (var userTuple in usersWithFormat)
            {
                userTuple.Item1.AvatarFormat = userTuple.Item2;
                _context.Users.Update(userTuple.Item1);
                _context.SaveChanges();
            }
        }

        [Test]
        public async Task UpdateAvatarAsyncTest_ValidUserIdAndFile_AddsNewFile()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "png";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "png" || s == ".png"))).Returns(true);
            var file = fileMock.Object;
            var userId = 2;

            // Act
            await _service.UpdateAvatarAsync(userId, file);

            // Assert
            var actualUser = await _context.Users.SingleAsync(p => p.Id == userId);
            Assert.AreEqual(expectedformat, actualUser.AvatarFormat, "Method does not change the file format");
            _fileManagerMock.Verify(t => t.AddFileAsync(file, "2.png", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not add file to file system");
        }

        [Test]
        public async Task UpdateAvatarAsyncTest_ValidUserIdAndFile_DeletesOldFileAndAddsNew()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var expectedFormat = "jpg";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedFormat}");
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "jpg" || s == ".jpg"))).Returns(true);
            var file = fileMock.Object;
            var userId = 4;

            // Act
            await _service.UpdateAvatarAsync(userId, file);

            // Assert
            var actualUser = await _context.Users.SingleAsync(p => p.Id == userId);
            Assert.AreEqual(expectedFormat, actualUser.AvatarFormat, "Method does not change the file format");
            _fileManagerMock.Verify(t => t.DeleteFile("4.png", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not remove old file from the file system");
            _fileManagerMock.Verify(t => t.AddFileAsync(file, "4.jpg", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not add file to file system");
        }

        [Test]
        public void UpdateAvatarAsyncTest_InvalidFileFormat_ThrowsArgumentException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "ppt";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "ppt" || s == ".ppt"))).Returns(false);
            var file = fileMock.Object;
            var userId = 2;

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAvatarAsync(userId, file),
                "Method does not throw an ArgumentException if the file format is not appropriate");
        }


        [Test]
        [TestCase(".png")]
        [TestCase("png")]
        public async Task UpdateAvatarAsyncByteArrayTest_ValidUserIdAndFile_AddsNewFile(string expectedFormat)
        {
            // Arrange
            var file = Array.Empty<byte>();
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "png" || s == ".png"))).Returns(true);
            var userId = 2;

            // Act
            await _service.UpdateAvatarAsync(userId, file, expectedFormat);

            // Assert
            var actualUser = await _context.Users.SingleAsync(p => p.Id == userId);
            if (expectedFormat.StartsWith("."))
                expectedFormat = expectedFormat[1..];
            Assert.AreEqual(expectedFormat, actualUser.AvatarFormat, "Method does not change the file format");
            _fileManagerMock.Verify(t => t.AddFileAsync(file, "2.png", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not add file to file system");
        }

        [Test]
        public async Task UpdateAvatarAsyncByteArrayTest_ValidUserIdAndFile_DeletesOldFileAndAddsNew()
        {
            // Arrange
            var expectedFormat = "jpg";
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "jpg" || s == ".jpg"))).Returns(true);
            var file = Array.Empty<byte>();
            var userId = 4;

            // Act
            await _service.UpdateAvatarAsync(userId, file, expectedFormat);

            // Assert
            var actualUser = await _context.Users.SingleAsync(p => p.Id == userId);
            Assert.AreEqual(expectedFormat, actualUser.AvatarFormat, "Method does not change the file format");
            _fileManagerMock.Verify(t => t.DeleteFile("4.png", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not remove old file from the file system");
            _fileManagerMock.Verify(t => t.AddFileAsync(file, "4.jpg", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not add file to file system");
        }

        [Test]
        [TestCase("av.ppt")]
        [TestCase("")]
        [TestCase(null)]
        public void UpdateAvatarAsyncByteArrayTest_InvalidFileFormat_ThrowsArgumentException(string name)
        {
            // Arrange
            var userId = 2;
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "ppt" || s == ".ppt"))).Returns(false);
            var file = Array.Empty<byte>();

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAvatarAsync(userId, file, name),
                "Method does not throw an ArgumentException if the file format is not appropriate");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void UpdateAvatarAsyncByteArrayTest_InvalidId_ThrowsInvalidOperationException(int userId)
        {
            // Arrange
            var name = "file.bmp";
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "bmp" || s == ".bmp"))).Returns(true);
            var file = Array.Empty<byte>();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAvatarAsync(userId, file, name),
                "Method does not throw an InvalidOperationException if user id is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void UpdateAvatarAsyncTest_InvalidId_ThrowsInvalidOperationException(int userId)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "bmp";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _fileManagerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "bmp" || s == ".bmp"))).Returns(true);
            var file = fileMock.Object;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAvatarAsync(userId, file),
                "Method does not throw an InvalidOperationException if user id is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void DeleteAvatarByUserIdAsyncTest_InvalidId_ThrowsInvalidOperationException(int userId)
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteAvatarByUserIdAsync(userId),
                "Method does not throw an InvalidOperationException if user id is invalid");
        }

        [Test]
        public void DeleteAvatarByUserIdAsyncTest_NullAvatarFormat_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 2;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteAvatarByUserIdAsync(userId),
                "Method does not throw an InvalidOperationException if user id is invalid");
        }

        [Test]
        public async Task DeleteAvatarByUserIdAsyncTest_ValidId_DeletesPictureFromFileSystemAndChangesModel()
        {
            // Arrange
            var userId = 3;

            // Act
            await _service.DeleteAvatarByUserIdAsync(userId);

            // Assert
            var actualUser = await _context.Users.SingleAsync(p => p.Id == userId);
            Assert.IsNull(actualUser.AvatarFormat, "Method does not change the file format to null");
            _fileManagerMock.Verify(t => t.DeleteFile("3.bmp", Business.Enums.EasyWorkFileTypes.UserAvatar),
                "Method does not remove the file from the file system");
        }
    }
}
