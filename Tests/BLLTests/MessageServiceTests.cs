using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data;
using Data.Entities;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class MessageServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IMessageService _service = null!;

        private Mock<IFileManager> _managerMock = new();

        private readonly IEnumerable<MessageModel> _invalidMessages = new MessageModel[]
        {
            new MessageModel()  // 0
            {
                Text = "This is a invalid message 1 text.",
                SenderId = 1,
                IsEdited = false,
                TaskId = -1
            },
            new MessageModel()  // 1
            {
                Text = "This is a invalid message 2 text.",
                SenderId = -1,
                IsEdited = false,
                TaskId = 1
            },
            new MessageModel()  // 2
            {
                Text = "This is a invalid message 3 text. (too long). " +
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin at eros ut nulla blandit gravida. " +
                "Duis elementum mauris et dolor rutrum ultrices. Pellentesque sed luctus tortor, eget facilisis lorem. " +
                "Maecenas eu sem sed erat ullamcorper iaculis ut at est. Nam volutpat condimentum justo vel vulputate. " +
                "Fusce dapibus fringilla diam quis pulvinar. Interdum et malesuada fames ac ante ipsum primis in faucibus. " +
                "Duis gravida, erat id efficitur eleifend, orci augue maximus est, vel malesuada est elit eu ex. " +
                "Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; " +
                "Etiam sed urna blandit, blandit felis vitae, rutrum leo. Quisque ac nulla orci. Fusce quis pretium est. " +
                "Duis maximus mi lorem, in tincidunt arcu volutpat condimentum. " +
                "Pellentesque accumsan, neque et accumsan dignissim, risus quam scelerisque purus, " +
                "ac venenatis lectus neque in mauris. Vivamus tempor cursus sollicitudin. " +
                "Curabitur aliquet quis turpis in pulvinar. Suspendisse quis posuere turpis. " +
                "Nam vulputate vestibulum mauris, nec molestie turpis facilisis ac. " +
                "Proin laoreet nibh augue. Curabitur mollis dui orci, non vestibulum odio ullamcorper at. " +
                "In tellus purus, lacinia ac sollicitudin vitae, cursus et justo. Etiam lacinia justo quis ipsum pretium, " +
                "at lacinia libero vestibulum. Vivamus laoreet cursus neque, vitae placerat nulla ornare quis. " +
                "Sed rhoncus luctus blandit. Ut id dignissim tellus. Vivamus eget placerat turpis. " +
                "In hac habitasse platea dictumst. Pellentesque massa libero, consequat ut velit facilisis, luctus pretium nibh. " +
                "Nullam sed ultrices diam, non eleifend nunc. Cras accumsan viverra diam, ac ultrices eros elementum eleifend. " +
                "Duis felis tortor, condimentum ac tellus vitae, lacinia malesuada ligula. " +
                "Integer ac orci condimentum, consequat orci scelerisque, varius leo. In vitae varius sapien. " +
                "Quisque lacinia massa sed blandit semper. Mauris sit amet libero magna. " +
                "Suspendisse efficitur justo magna, sed iaculis metus bibendum sed. " +
                "Quisque tortor ex, semper sit amet rutrum nulla. ",
                SenderId = 1,
                IsEdited = false,
                TaskId = 1
            }
        };

        private readonly MessageModel[] _validMessages = new MessageModel[]
        {
            new MessageModel()
            {
                Text = "This is a valid message 1 text.",
                SenderId = 1,
                IsEdited = false,
                TaskId = 1
            },
            new MessageModel()
            {
                Text = "This is a valid message 2 (return message) text.",
                SenderId = 1,
                IsEdited = false,
                TaskId = 1
            }
        };

        [SetUp]
        public void Setup()
        {
            _managerMock = new Mock<IFileManager>();
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _service = new MessageService(_context, _mapper, _managerMock.Object);
        }

        private void SeedRequiredData()
        {
            var task = new Data.Entities.Task() // id 1
            {
                Name = "Task 1"
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }

        private void SeedData()
        {
            var tasks = new Data.Entities.Task[]
            {
                new Data.Entities.Task() // id 2
                {
                    Name = "Task 2",
                    ExecutorId = 1
                },
                new Data.Entities.Task() // id 3
                {
                    Name = "Task 3"
                }
            };
            foreach (var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }

            var messages = new Message[]
            {
                new Message()     // id 1
                {
                    SenderId = 2,
                    Text = "This is message 1",
                    TaskId = 1,
                    IsEdited = true,
                    IsRead = true,
                },
                new Message()     // id 2
                {
                    SenderId = 1,
                    Text = "This is message 2",
                    TaskId = 2,
                    IsRead = true,
                },
                new Message()     // id 3
                {
                    SenderId = 1,
                    Text = "This is message 3",
                    TaskId = 1,
                },
                new Message()     // id 4
                {
                    SenderId = 2,
                    Text = "This is message 4",
                    TaskId = 1,
                    IsEdited = true,
                },
                new Message()     // id 5
                {
                    SenderId = 3,
                    Text = "This is message 5",
                    TaskId = 2,
                    IsRead = true
                }
            };
            foreach (var message in messages)
            {
                message.Date = DateTime.Now;
                _context.Messages.Add(message);
                _context.SaveChanges();
            }

            var files = new File[]
            {
                new File()  // id 1
            {
                MessageId = 1,
                Name = "TestFile1.cs"
            },
            new File()  // id 2
            {
                MessageId = 1,
                Name = "TestFile2.pdf"
            },
            new File()
            {
                MessageId = 5,
                Name = "TestFile3.docx"
            }
            };
            foreach (var file in files)
            {
                _context.Files.Add(file);
                _context.SaveChanges();
            }
        }

        private readonly IEnumerable<MessageModel> _invalidForUpdateMessages = new MessageModel[]
        {
            new MessageModel()     // id 1, ind 0
                {
                Id = 1,
                    SenderId = 1,   // changed
                    Text = "This is message 1 edited",
                    TaskId = 1,
                    IsEdited = true
                },
            new MessageModel()     // id 1, ind 1
                {
                Id = 1,
                    SenderId = 2,
                    Text = "This is message 1 edited",
                    TaskId = 2,     // changed
                    IsEdited = true
                },
            new MessageModel()     // id 1, ind 2
                {
                Id = 1,
                    SenderId = 2,
                    Text = "This is message 1 edited",
                    TaskId = 1,
                    IsEdited = true,
                    Date = DateTime.MaxValue    // changed
                },
            new MessageModel()     // id 5, ind 3
                {
                Id = 5,
                                        SenderId = 1,
                    Text = "This is message 5",
                    TaskId = 2,
                    IsRead = false      // changed
                },
        };

        private readonly MessageModel _validForUpdateMessage = new()
        {
            Id = 1,
            SenderId = 2,
            Text = "This is message 1 edited (valid)",
            TaskId = 1,
            IsEdited = true,
            IsRead = true
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidMessages.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsFalse(actual, "Method does not return false if model is invalid");
            Assert.IsFalse(string.IsNullOrEmpty(error), "Method does not write proper error message");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void IsValidTest_ValidModel_ReturnsTrue(int modelNumber)
        {
            // Arrange
            var model = _validMessages.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Method does not write null to the error message field");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void GetByIdAsync_InvalidId_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetByIdAsync(id),
                "Method does not throw an InvalidOperationException if id is invalid");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public async Task GetByIdAsync_ValidId_ReturnesElement(int id)
        {
            // Arrange
            SeedData();

            // Act
            var actual = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, actual.Id, "Method returns wrong element");
            Assert.AreNotEqual(null, actual.FilesIds, "Method does not load message file ids");
        }

        [Test]
        public void GetTaskMessagesTest_ReturnsRightElements()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var expected = new int[] { 1, 3, 4 };

            // Act
            var actual = _service.GetTaskMessages(taskId);

            // Assert
            Assert.AreEqual(expected.Length, actual.Count(), "Method returns wrong elements");
            Assert.IsTrue(actual.All(m => m.TaskId == taskId), "Method returns wrong elements");
            Assert.IsTrue(expected.SequenceEqual(actual.Select(m => m.Id)), "The order of the elements is wrong");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void DeleteByIdAsync_InvalidId_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(id),
                "Method does not throw an InvalidOperationException if id is invalid");
        }

        [Test]
        [TestCase(1)]
        [TestCase(4)]
        [TestCase(5)]
        public async Task DeleteByIdAsync_ValidId_DeletesElementCascade(int id)
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Messages.Count() - 1;
            var deletedFiles = _context.Files.Where(f => f.MessageId == id).Count();
            var expectedFilesCount = _context.Files.Count() - deletedFiles;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Messages.Count();
            var actualFilesCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Messages.Any(m => m.Id == id), "Method deletes wrong element");
            Assert.AreEqual(expectedFilesCount, actualFilesCount, "Method does not delete all files cascadely");
            _managerMock.Verify(t => t.DeleteFile(It.IsAny<string>(), Business.Enums.EasyWorkFileTypes.File), 
                Times.Exactly(deletedFiles), "Method does not remove the file from file system");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validMessages.First();
            var expectedText = model.Text;
            var expectedCount = _context.Messages.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Messages.Count();
            var actual = _context.Messages.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedText, actual.Text, "Method does not add model with needed information");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidMessages.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidMessages.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UpdateAsyncTest_InvalidForUpdateOnlyModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForUpdateMessages.ElementAt(index);
            if (model.Date != DateTime.MaxValue)
                model.Date = (_context.Messages.Find(model.Id))!.Date;       // Fix for unchangeable date

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateMessage;
            model.Date = (await _context.Messages.FindAsync(model.Id))!.Date;       // Fix for unchangeable date
            var expectedText = model.Text;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Messages.FindAsync(model.Id);
            Assert.AreEqual(expectedText, actual!.Text, "Method does not update model");
        }

        [Test]
        public void GetNotReadMessagesForUserTest_ReturnsRightNotifications()
        {
            // Arrange
            SeedData();
            var userId = 1;
            var expectedMessageIds = new[] { 5 };

            // Act
            var actualMessageIds = _service.GetNotReadMessagesForUser(userId).Select(m => m.Id);

            // Assert
            Assert.AreEqual(expectedMessageIds.Length, actualMessageIds.Count(), "Method returned wrong elements");
            Assert.IsTrue(expectedMessageIds.SequenceEqual(actualMessageIds), "Method returned wrong elements");
        }
    }
}
