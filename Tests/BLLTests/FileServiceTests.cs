using AutoMapper;
using Business.Enums;
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
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using File = Data.Entities.File;

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
                TaskId = -1,
                Name = "TestFile.cs"
            },
            new FileModel() // 1
            {
                TaskId = 1,
                Name = "Too long name! dolor sit amet, consectetur adipiscing elit. " +
                "Proin purus mauris, suscipit iaculis massa id, tincidunt lacinia augue. " +
                "Etiam condimentum cursus finibus. Fusce vel magna nec magna scelerisque pretium. " +
                "Praesent pellentesque vulputate felis, non vel. "
            },
            new FileModel()     // 2
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
            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();

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
            _context.Messages.AddRange(messages);
            _context.SaveChanges();

            var files = new File[]
            {
                new File()  // id 1
            {
                TaskId = 1,
                Name = "TestFile1.cs",
                IsFull = true,
            },
            new File()  // id 2
            {
                TaskId = 1,
                Name = "TestFile2.pdf",
                IsFull = true,
            },
            new File()  // id 3
            {
                TaskId = 1,
                Name = "TestFile3.txt",
                IsFull = true,
            },
            new File()  // id 4
            {
                TaskId = 1,
                Name = "TestFile4.docx",
                IsFull = true,
            },
            new File()  // id 5
            {
                TaskId = 1,
                Name = "TestFile5.json",
                IsFull = true,
            },
            new File()  // id 6
            {
                TaskId = 2,
                Name = "TestFile6.rtf",
                IsFull = true,
            },
            new File()  // id 7
            {
                TaskId = 3,
                Name = "TestFile7.html",
                IsFull = true,
            },
            new File()  // id 8
            {
                TaskId = 3,
                Name = "TestFile8.xml",
                IsFull = false
            },
            };
            _context.Files.AddRange(files);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddAsyncTest_ValidFile_AddsToDbAndToFileSystem()
        {
            // Arrange
            SeedData();
            var fileModel = _validFiles.First();
            var file = new Mock<IFormFile>().Object;
            var expectedName = "9.cs";
            var expectedCount = _context.Files.Count() + 1;

            // Act
            await _service.AddAsync(fileModel, file);

            // Assert
            var actualCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a file model to DB");
            _managerMock.Verify(t => t.AddFileAsync(file, expectedName, Business.Enums.EasyWorkFileTypes.File),
                "Method does not add file to file system");
            Assert.AreNotEqual(fileModel.Id, 0, "Method does not set id to the model");
            Assert.IsTrue(fileModel.IsFull, $"The {nameof(fileModel.IsFull)} property should be set to true");
        }

        [Test]
        public async Task ChunkAddStartAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var fileModel = _validFiles.First();
            var expectedCount = _context.Files.Count() + 1;

            // Act
            await _service.ChunkAddStartAsync(fileModel);

            // Assert
            var actualCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a file model to DB");
            Assert.AreNotEqual(fileModel.Id, 0, "Method does not set id to the model");
            Assert.IsFalse(fileModel.IsFull, $"The {nameof(fileModel.IsFull)} property should be set to false");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ChunkAddStartAsyncTest_InvalidModel_ThrowsArgumentException(int index)
        {
            // Arrange
            var fileModel = _invalidFiles.ElementAt(index);

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.ChunkAddStartAsync(fileModel));
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
        public void AddChunkAsyncTest_NullChunkModel_ThrowsArgumentNullException()
        {
            // Arrange
            int fileId = 1;
            FileChunkModel chunkModel = null!;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.AddChunkAsync(fileId, chunkModel));
        }

        [Test]
        public void AddChunkAsyncTest_FullFile_ThrowsInvalidOperationException()
        {
            // Arrange
            _context.Files.Add(new File
            {
                TaskId = 1,
                Name = "file1.txt",
                IsFull = true
            });
            _context.SaveChanges();
            int fileId = 1;
            FileChunkModel chunkModel = new();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddChunkAsync(fileId, chunkModel));
        }

        [Test]
        public void AddChunkAsyncTest_InvalidFileId_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = 10;
            FileChunkModel chunkModel = new();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddChunkAsync(fileId, chunkModel));
        }

        [Test]
        public async Task AddChunkAsyncTest_AddsChunk()
        {
            // Arrange
            await _context.Files.AddAsync(new File
            {
                TaskId = 1,
                Name = "file1.txt",
                IsFull = false
            });
            await _context.SaveChangesAsync();
            int fileId = 1;
            FileChunkModel chunkModel = new();

            // Act
            await _service.AddChunkAsync(fileId, chunkModel);

            // Assert
            _managerMock.Verify(t => t.AddFileChunkAsync(fileId.ToString(), chunkModel), 
                "Method does not add the chunk to the file system");
        }

        [Test]
        public void ChunkAddEndAsyncTest_FullFile_ThrowsInvalidOperationException()
        {
            // Arrange
            _context.Files.Add(new File
            {
                TaskId = 1,
                Name = "file1.txt",
                IsFull = true
            });
            _context.SaveChanges();
            int fileId = 1;
            FileChunkModel chunkModel = new();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.ChunkAddEndAsync(fileId));
        }

        [Test]
        public void ChunkAddEndAsyncTest_InvalidFileId_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = 10;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.ChunkAddEndAsync(fileId));
        }

        [Test]
        public async Task ChunkAddEndAsyncTest_MergesChunk()
        {
            // Arrange
            var extension = ".txt";
            _context.Files.Add(new File
            {
                TaskId = 1,
                Name = $"file1{extension}",
                IsFull = false
            });
            _context.SaveChanges();
            int fileId = 1;

            // Act
            await _service.ChunkAddEndAsync(fileId);

            // Assert
            _managerMock.Verify(t => t.MergeChunksAsync(fileId.ToString(), extension),
                "Method does not call the MergeChunksAsync method");
        }

        [Test]
        public async Task ChunkAddEndAsyncTest_UpdatesDB()
        {
            // Arrange
            var taskId = 1;
            _context.Files.Add(new File
            {
                TaskId = taskId,
                Name = $"file1.txt",
                IsFull = false
            });
            _context.SaveChanges();
            int fileId = 1;

            // Act
            await _service.ChunkAddEndAsync(fileId);

            // Assert
            var actualFile = await _context.Files.FindAsync(taskId);
            Assert.NotNull(actualFile);
            Assert.IsTrue(actualFile!.IsFull, $"Method does not set the {nameof(actualFile.IsFull)} property to \"true\"");
        }

        [Test]
        public async Task ChunkAddEndAsyncTest_ReturnedExtendedModel()
        {
            // Arrange
            var file = new File
            {
                TaskId = 1,
                Name = $"file1.txt",
                IsFull = false
            };
            _context.Files.Add(file);
            _context.SaveChanges();
            int fileId = 1;
            var expectedSize = 10000L;
            _managerMock.Setup(m => m.GetFileSizeAsync(It.IsAny<string>(), It.IsAny<EasyWorkFileTypes>()))
                .ReturnsAsync(expectedSize);

            // Act
            var model = await _service.ChunkAddEndAsync(fileId);

            // Assert
            Assert.NotNull(model);
            _managerMock.Verify(m => m.GetFileSizeAsync(file.TaskId + Path.GetExtension(file.Name), EasyWorkFileTypes.File),
                "The service does not manager's method in order to determine the file's size");
            Assert.AreEqual(fileId, model.Id, "Method returned wrong element");
            Assert.AreEqual(file.Name, model.Name, "Method returned wrong element");
            Assert.AreEqual(file.TaskId, model.TaskId, "Method returned wrong element");
            Assert.IsTrue(model.IsFull, $"Method does not set the {nameof(model.IsFull)} property to \"true\"" +
                "or returned the wrong element");
            Assert.AreEqual(expectedSize, model.Size, "Method returned wrong size");
        }

        [Test]
        public void AddAsyncTest_TaskLimitExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            SeedData();
            var taskId = 4;
            for (int i = 0; i < 10; i++)
            {
                _context.Files.Add(new File()
                {
                    TaskId = taskId,
                    Name = $"{i+9}.file"
                });
                _context.SaveChanges();
            }
            var fileModel = _invalidFiles.First();
            fileModel.TaskId = taskId;
            var file = new Mock<IFormFile>().Object;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(fileModel, file));
        }

        [Test]
        [TestCase(0)]
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
        public async Task DeleteByIdAsyncTest_FullFile_DeletesElementFromDBAndFileSystem()
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
                "Method does not remove the file from the file system");
        }

        [Test]
        public async Task DeleteByIdAsyncTest_NotFullFile_DeletesElementFromDBAndFileSystem()
        {
            // Arrange
            SeedData();
            int id = 8;
            var expectedCount = _context.Files.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not remove the file model from DB");
            _managerMock.Verify(t => t.DeleteChunks("8"), "Method does not remove file chunks from the file system");
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
        public async Task GetTaskFilesTest_ReturnsRightFiles()
        {
            // Arrange
            SeedData();
            long size = 111;
            _managerMock.Setup(m => m.GetFileSizeAsync(It.IsAny<string>(), Business.Enums.EasyWorkFileTypes.File)).ReturnsAsync(size);
            var taskId = 1;
            var expectedCount = 5;

            // Act
            var actual = await _service.GetTaskFilesAsync(taskId);

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method returns wrong elements");
            Assert.IsTrue(actual.All(f => f.TaskId == taskId), "Method returns wrong elements");
            Assert.IsTrue(actual.All(f => f.Size == size), "Method returns wrong elements");
        }
    }
}
