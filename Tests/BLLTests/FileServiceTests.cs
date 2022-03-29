using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class FileServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IFileService _service = null!;

        private readonly Mock<IFileManager> _managerMock = new();

        private IFileManager _manager = null!;

        private readonly IEnumerable<FileModel> _invalidFiles = new FileModel[]
        {
            new FileModel()  // 0
            {
                TaskId = 1,
                MessageId = 1,
                Name = "TestFile.cs"
            },
            new FileModel()  // 1
            {
                TaskId = -1,
                Name = "TestFile.cs"
            },
            new FileModel()  // 2
            {
                MessageId = -1,
                Name = "TestFile.cs"
            },
            new FileModel() // 3
            {
                TaskId = 1,
                Name = "Too long name! dolor sit amet, consectetur adipiscing elit. " +
                "Proin purus mauris, suscipit iaculis massa id, tincidunt lacinia augue. " +
                "Etiam condimentum cursus finibus. Fusce vel magna nec magna scelerisque pretium. " +
                "Praesent pellentesque vulputate felis, non vel. "
            },
            new FileModel()     // 4
            {
                Name = "TestFile.cs"
            }
        };

        private readonly IEnumerable<FileModel> _validFiles = new FileModel[]
        {
            new FileModel()     // 0
            {
                TaskId = 1,
                Name = "TestFile.cs"
            },
            new FileModel()     // 1
            {
                MessageId = 1,
                Name = "TestFile.cs"
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _manager = _managerMock.Object;
            _service = new FileService(_context, _mapper, _manager);
        }

        private void SeedRequiredData()
        {
            var task = new Data.Entities.Task() // id 1
            {
                Name = "Task 1"
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var message = new Message()     // id 1
            {
                SenderId = 1,
                Text = "This is message 1"
            };
            _context.Messages.Add(message);
            _context.SaveChanges();
        }

        private void SeedData()
        {
            var tasks = new Data.Entities.Task[]
            {
                new Data.Entities.Task() // id 2
                {
                    Name = "Task 2"
                },
                new Data.Entities.Task() // id 3
                {
                    Name = "Task 3"
                },
                new Data.Entities.Task()    // id 4
                {
                    Name = "Task 4"
                }
            };
            foreach (var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }

            var messages = new Message[]
            {
                new Message()     // id 2
                {
                    SenderId = 2,
                    Text = "This is message 2"
                },
                new Message()     // id 3
                {
                    SenderId = 3,
                    Text = "This is message 3"
                },
                new Message()     // id 4
                {
                    SenderId = 1,
                    Text = "This is message 4"
                },
                new Message()       // id 5
                {
                    Text = "This is message 5"
                }
            };
            foreach (var message in messages)
            {
                _context.Messages.Add(message);
                _context.SaveChanges();
            }

            var files = new File[]
            {
                new File()  // id 1
            {
                TaskId = 1,
                Name = "TestFile1.cs"
            },
            new File()  // id 2
            {
                MessageId = 1,
                Name = "TestFile2.pdf"
            },
            new File()  // id 3
            {
                TaskId = 1,
                Name = "TestFile3.txt"
            },
            new File()  // id 4
            {
                MessageId = 1,
                Name = "TestFile4.docx"
            },
            new File()  // id 5
            {
                TaskId = 1,
                Name = "TestFile5.json"
            },
            new File()  // id 6
            {
                MessageId = 2,
                Name = "TestFile6.rtf"
            },
            new File()  // id 7
            {
                TaskId = 3,
                Name = "TestFile7.html"
            },
            };
            foreach (var file in files)
            {
                _context.Files.Add(file);
                _context.SaveChanges();
            }
            for (int i = 0; i < 10; i++)
            {
                var messageFile = new File()
                {
                    MessageId = 5,
                    Name = $"LimMessageFile{i}.file"
                };
                _context.Files.Add(messageFile);
                _context.SaveChanges();
                var taskFile = new File()
                {
                    TaskId = 4,
                    Name = $"LimTaskFile{i}.file"
                };
                _context.Files.Add(taskFile);
                _context.SaveChanges();
            }
        }

        [Test]
        public async Task AddAsyncTest_ValidFile_AddsToDbAndToFileSystem()
        {
            // Arrange
            SeedData();
            var fileModel = _validFiles.First();
            var file = new Mock<IFormFile>().Object;
            var expectedName = "28.cs";
            var expectedCount = _context.Files.Count() + 1;

            // Act
            await _service.AddAsync(fileModel, file);

            // Assert
            var actualCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a file model to DB");
            _managerMock.Verify(t => t.AddFileAsync(file, expectedName, Business.Enums.EasyWorkFileTypes.File),
                "Method does not add file to file system");
        }

        [Test]
        public void AddAsyncTest_InvalidFile_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var fileModel = _invalidFiles.First();
            var file = new Mock<IFormFile>().Object;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(fileModel, file));
        }

        [Test]
        public void AddAsyncTest_TaskLimitExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            SeedData();
            var taskId = 4;
            var fileModel = _invalidFiles.First();
            fileModel.MessageId = null;
            fileModel.TaskId = taskId;
            var file = new Mock<IFormFile>().Object;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(fileModel, file));
        }

        [Test]
        public void AddAsyncTest_MessageLimitExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            SeedData();
            var messageId = 5;
            var fileModel = _invalidFiles.First();
            fileModel.MessageId = messageId;
            fileModel.TaskId = null;
            var file = new Mock<IFormFile>().Object;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(fileModel, file));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void IsValidTest_ValidFile_ReturnsTrue(int modelNumber)
        {
            // Arrange
            var model = _validFiles.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Method does not write null in error message");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void IsValidTest_InvalidFile_ReturnsFalse(int modelNumber)
        {
            // Arrange
            var model = _invalidFiles.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsFalse(actual, "Method does not return false if model is invalid");
            Assert.IsFalse(string.IsNullOrEmpty(error), "Method does not write proper error message");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(555)]
        public void DeleteByIdAsyncTest_NotExistingId_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(id));
        }

        [Test]
        public async Task DeleteByIdAsyncTest_ValidId_DeletesElementFromDBAndFileSystem()
        {
            // Arrange
            SeedData();
            int id = 2;
            var expectedCount = _context.Files.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not remove the file model from DB");
            _managerMock.Verify(t => t.DeleteFile("2.pdf", Business.Enums.EasyWorkFileTypes.File), 
                "Method does not remove the file from file system");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(777)]
        public async Task GetByIdAsync_InvalidId_ReturnsNull(int id)
        {
            // Arrange
            SeedData();

            // Act
            var returned = await _service.GetByIdAsync(id);

            // Assert
            Assert.IsNull(returned, "Method does not return null if id is invalid");
        }

        [Test]
        public async Task GetByIdAsyncTest_ValidId_ReturnsElement()
        {
            // Arrange
            SeedData();
            var id = 2;

            // Act
            var actual = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, actual!.Id, "Method returns wrong element");
        }

        [Test]
        public void GetMessageFilesTest_ReturnsRightFiles()
        {
            // Arrange
            SeedData();
            var messageId = 2;
            var expectedCount = 1;

            // Act
            var actual = _service.GetMessageFiles(messageId);

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method returns wrong elements");
            Assert.IsTrue(actual.All(f => f.MessageId == messageId), "Method returns wrong elements");
        }

        [Test]
        public void GetTaskFilesTest_ReturnsRightFiles()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var expectedCount = 3;

            // Act
            var actual = _service.GetTaskFiles(taskId);

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method returns wrong elements");
            Assert.IsTrue(actual.All(f => f.TaskId == taskId), "Method returns wrong elements");
        }
    }
}
