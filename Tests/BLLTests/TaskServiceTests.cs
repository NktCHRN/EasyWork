using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using TaskEntity = Data.Entities.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class TaskServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private ITaskService _service = null!;

        private readonly Mock<IFileManager> _managerMock = new();

        private readonly IEnumerable<TaskModel> _invalidTasks = new TaskModel[]
        {
            new TaskModel()         // 0
            {
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = 2
            },
            new TaskModel()         // 1
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = -1,
                ExecutorId = 2
            },
            new TaskModel()         // 2
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = -2
            },
            new TaskModel()         // 3
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2021, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = 2
            }
        };

        private readonly IEnumerable<TaskModel> _validTasks = new TaskModel[]
        {
            new TaskModel()             // 0
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = 2
            },
            new TaskModel()             // 1
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1
            },
            new TaskModel()             // 2
            {
                Name = "Task 1",
                Status = TaskStatuses.ToDo,
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Lowest,
                ProjectId = 1,
                ExecutorId = 2
            },
            new TaskModel()             // 3
            {
                Name = "Task 1",
                Status = TaskStatuses.InProgress,
                Priority = TaskPriorities.High,
                ProjectId = 1,
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _service = new TaskService(_context, _mapper, _managerMock.Object);
        }

        private void SeedRequiredData()
        {
            var project = new Project() // id 1
            {
                Name = "Project 1"
            };
            _context.Projects.Add(project);
            _context.SaveChanges();

            var uop = new UserOnProject()
            {
                UserId = 2,
                ProjectId = 1
            };
            _context.UsersOnProjects.Add(uop);
            _context.SaveChanges();
        }

        private void SeedData()
        {
            var projects = new Project[]
            {
                new Project()       // id 2
                {
                    Name = "Project 2"
                }
            };
            foreach (var project in projects)
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
            }

            var tags = new Tag[]
            {
                new Tag()       // id 1
                {
                    Name = "Automatisation",
                },
                new Tag()       // id 2
                {
                    Name = "Testing",
                },
                new Tag()       // id 3
                {
                    Name = "Programming",
                },
                new Tag()       // id 4
                {
                    Name = "Hotfix",
                }
            };
            foreach (var tag in tags)
            {
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }

            var uops = new UserOnProject[]
            {
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 5
                },
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 5
                },
            };
            foreach(var uop in uops)
            {
                _context.UsersOnProjects.Add(uop);
                _context.SaveChanges();
            }

            var tasks = new TaskEntity[]
            {
                new TaskEntity()            // id 1
                {
                    Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = 2,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 1),
                        _context.Tags.Single(t => t.Id == 3)
                    }
                },
                new TaskEntity()            // id 2
                {
                    Name = "Task 1.5",
                Status = TaskStatuses.ToDo,
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Highest,
                ProjectId = 2,
                EndDate = DateTime.MinValue.AddMonths(1)
                },
                new TaskEntity()            // id 3
                {
                    Name = "Task 2",
                StartDate = new DateTime(2022, 1, 3),
                EndDate = new DateTime(2022, 2, 3),
                Status = TaskStatuses.ToDo,
                Priority = TaskPriorities.Low,
                ProjectId = 1,
                ExecutorId = 5,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 4)
                    }
                },
                new TaskEntity()            // id 4
                {
                    Name = "Task 2",
                StartDate = new DateTime(2022, 2, 5),
                                Deadline = new DateTime(2022, 2, 20),
                EndDate = new DateTime(2022, 2, 26),
                Status = TaskStatuses.Validate,
                Priority = TaskPriorities.Low,
                ProjectId = 2,
                ExecutorId = 5,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 4)
                    }
                },
                new TaskEntity()            // id 5
                {
                    Name = "Task 3",
                StartDate = new DateTime(2022, 3, 1),
                Deadline = new DateTime(2022, 3, 20),
                EndDate = new DateTime(2022, 3, 19),
                Status = TaskStatuses.Complete,
                Priority = TaskPriorities.Low,
                ProjectId = 1,
                ExecutorId = 5
                },
                new TaskEntity()                // id 6
                {
                    Name = "Task 4",
                    ProjectId = 1,
                    Status = TaskStatuses.Archived,
                    StartDate = new DateTime(2022, 2, 5),
                                Deadline = new DateTime(2022, 2, 20),
                EndDate = new DateTime(2022, 2, 26),
                },
                new TaskEntity()                // id 7
                {
                    Name = "Task 5",
                    ProjectId = 1,
                    Status = TaskStatuses.Archived,
                    EndDate = DateTime.MinValue.AddMonths(1)
                }
            };
            foreach (var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }

            var messages = new Message[]
            {
                new Message()
                {
                    TaskId = 1,
                    Text = "Message 1"
                },
                new Message()
                {
                    TaskId = 1,
                    Text = "Message 2"
                },
                new Message()
                {
                    TaskId = 3,
                    Text = "Message 3"
                },
            };
            foreach (var message in messages)
            {
                _context.Messages.Add(message);
                _context.SaveChanges();
            }

            var files = new File[]
            {
                new File()
                {
                    TaskId = 1,
                    Name = "file1.file"
                },
                new File()
                {
                    MessageId = 1,
                    Name = "file2.file"
                },
                new File()
                {
                    MessageId = 3,
                    Name = "file1.file"
                },
            };
            foreach(var file in files)
            {
                _context.Files.Add(file);
                _context.SaveChanges();
            }
        }

        private readonly IEnumerable<TaskModel> _invalidForAddTasks = new TaskModel[]
        {
            new TaskModel()
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                EndDate = new DateTime(2022, 3, 1),         // !!!
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorId = 2
            }
        };

        private readonly IEnumerable<TaskModel> _invalidForUpdateTasks = new TaskModel[]
        {
            new TaskModel()                         // id 1, ind 0
            {
                Id = 1,
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2022, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 2,              // changed
                ExecutorId = 2
            }
        };

        private readonly TaskModel _validForUpdateTask = new()        // id 1
        {
            Id = 1,
            Name = "Task 1 edited",
            StartDate = new DateTime(2022, 2, 3),
            Status = TaskStatuses.ToDo,
            Deadline = new DateTime(2022, 2, 10),
            EndDate = new DateTime(2022, 3, 5),
            Description = "This is new description of the task 1",
            Priority = TaskPriorities.High,
            ProjectId = 1,              // changed
            ExecutorId = 5
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidTasks.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsFalse(actual, "Method does not return false if model is invalid");
            Assert.IsFalse(string.IsNullOrEmpty(error), "Method does not write proper error message");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IsValidTest_ValidModel_ReturnsTrue(int modelNumber)
        {
            // Arrange
            var model = _validTasks.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Method does not write null to the error message field");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
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
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void DeleteByIdAsync_InvalidId_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(id),
                "Method does not throw an InvalidOperationException if id is invalid");
        }

        [Test]
        public async Task DeleteByIdAsync_ValidId_DeletesElementCascadely()
        {
            // Arrange
            SeedData();
            var id = 1;
            var expectedCount = _context.Tasks.Count() - 1;
            var expectedTagsCount = 4;          // no changes
            var expectedMessagesCount = 1;
            var expectedFilesCount = 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Tasks.Count();
            var actualFilesCount = _context.Files.Count();
            var actualMessagesCount = _context.Messages.Count();
            var actualTagsCount = _context.Tags.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Tasks.Any(t => t.Id == id), "Method deletes wrong element");
            Assert.AreEqual(expectedFilesCount, actualFilesCount, "Method does not delete all files cascadely");
            Assert.AreEqual(expectedMessagesCount, actualMessagesCount, "Method does not delete all messages cascadely");
            Assert.AreEqual(expectedTagsCount, actualTagsCount, "Method should not delete any tags");
            _managerMock.Verify(t => t.DeleteFile("1.file", Business.Enums.EasyWorkFileTypes.File),
                "Method does not remove the file from file system");
            _managerMock.Verify(t => t.DeleteFile("2.file", Business.Enums.EasyWorkFileTypes.File),
                "Method does not remove the file from file system");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validTasks.First();
            var expectedName = model.Name;
            var expectedCount = _context.Tasks.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Tasks.Count();
            var actual = _context.Tasks.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedName, actual.Name, "Method does not add model with needed information");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidTasks.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void AddAsyncTest_ExceedsToDo_ThrowsInvalidOperationException()
        {
            // Arrange
            var tempProject = new Project()             // id 2
            {
                Name = "Temp project",
                MaxToDo = 1
            };
            _context.Projects.Add(tempProject);
            _context.SaveChanges();
            var tempTask = new TaskEntity()
            {
                ProjectId = 2,
                Name = "Temp task",
                Status = TaskStatuses.ToDo
            };
            _context.Tasks.Add(tempTask);
            _context.SaveChanges();
            var model = new TaskModel()
            {
                ProjectId = 2,
                Name = "New task",
                Status = TaskStatuses.ToDo
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void AddAsyncTest_ExceedsInProgress_ThrowsInvalidOperationException()
        {
            // Arrange
            var tempProject = new Project()             // id 2
            {
                Name = "Temp project",
                MaxInProgress = 1
            };
            _context.Projects.Add(tempProject);
            _context.SaveChanges();
            var tempTask = new TaskEntity()
            {
                ProjectId = 2,
                Name = "Temp task",
                Status = TaskStatuses.InProgress
            };
            _context.Tasks.Add(tempTask);
            _context.SaveChanges();
            var model = new TaskModel()
            {
                ProjectId = 2,
                Name = "New task",
                Status = TaskStatuses.InProgress
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void AddAsyncTest_ExceedsValidate_ThrowsInvalidOperationException()
        {
            // Arrange
            var tempProject = new Project()             // id 2
            {
                Name = "Temp project",
                MaxValidate = 1
            };
            _context.Projects.Add(tempProject);
            _context.SaveChanges();
            var tempTask = new TaskEntity()
            {
                ProjectId = 2,
                Name = "Temp task",
                Status = TaskStatuses.Validate
            };
            _context.Tasks.Add(tempTask);
            _context.SaveChanges();
            var model = new TaskModel()
            {
                ProjectId = 2,
                Name = "New task",
                Status = TaskStatuses.Validate
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddAsync(model));
        }

        [Test]
        [TestCase(0)]
        public void AddAsyncTest_InvalidForAddModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForAddTasks.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidTasks.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        [TestCase(0)]
        public void UpdateAsyncTest_InvalidForUpdateOnlyModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForUpdateTasks.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateTask;
            var expectedName = model.Name;
            var expectedTagsCount = 2;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Tasks.Include(t => t.Tags).SingleAsync(r => r.Id == model.Id);
            var actualTagsCount = actual.Tags.Count;
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
            Assert.AreEqual(expectedTagsCount, actualTagsCount, "Method changed tags");
        }

        [Test]
        public void GetProjectNotArchivedTasksTest_ReturnsRealProjectTasks()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            IEnumerable<int> expectedTasksIds = new[] { 5, 3, 1 };

            // Act
            var actualTasks = _service.GetProjectNotArchivedTasks(projectId);

            // Assert
            Assert.AreEqual(expectedTasksIds.Count(), actualTasks.Count(), "Method returnes wrong elements");
            var actualTasksIds = actualTasks.Select(r => r.Id);
            Assert.IsTrue(expectedTasksIds.SequenceEqual(actualTasksIds),
                "Method returnes wrong elements or the order is wrong");
        }

        [Test]
        public void GetProjectArchivedTasksTest_ReturnsRealProjectTasks()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            IEnumerable<int> expectedTasksIds = new[] { 7, 6 };

            // Act
            var actualTasks = _service.GetProjectArchivedTasks(projectId);

            // Assert
            Assert.AreEqual(expectedTasksIds.Count(), actualTasks.Count(), "Method returnes wrong elements");
            var actualTasksIds = actualTasks.Select(r => r.Id);
            Assert.IsTrue(expectedTasksIds.SequenceEqual(actualTasksIds),
                "Method returnes wrong elements or the order is wrong");
        }

        [Test]
        public void GetUserTasksTest_ReturnsRealUserTasks()
        {
            // Arrange
            SeedData();
            var userId = 5;
            IEnumerable<int> expectedTasksIds = new[] { 3, 5, 4 };

            // Act
            var actualTasks = _service.GetUserTasks(userId);

            // Assert
            Assert.AreEqual(expectedTasksIds.Count(), actualTasks.Count(), "Method returnes wrong elements");
            var actualTasksIds = actualTasks.Select(r => r.Id);
            Assert.IsTrue(expectedTasksIds.SequenceEqual(actualTasksIds),
                "Method returnes wrong elements or the order is wrong");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void AddTagToTaskAsyncTest_InvalidTaskId_ThrowsInvalidOperationException(int taskId)
        {
            // Arrange
            SeedData();
            var tagId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddTagToTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the task id is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(5)]
        public void AddTagToTaskAsyncTest_InvalidTagId_ThrowsInvalidOperationException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddTagToTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the tag id is invalid");
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        public void AddTagToTaskAsyncTest_AlreadyTaskTagId_ThrowsInvalidOperationException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;         // projectId = 1

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddTagToTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the task already has such a tag");
        }

        [Test]
        public async Task AddTagToTaskAsyncTest_ValidData_AddsTagToTask()
        {
            // Arrange
            SeedData();
            var taskId = 2;
            var tagId = 2;
            var expectedTagsCount = 1;
            var oldTag = await _context.Tags.Include(t => t.Tasks).SingleAsync(t => t.Id == tagId);
            var expectedTagName = oldTag.Name;
            var expectedTagTasksCount = oldTag.Tasks.Count + 1;

            // Act
            await _service.AddTagToTaskAsync(taskId, tagId);

            // Assert
            var task = await _context.Tasks.Include(t => t.Tags).SingleAsync(t => t.Id == taskId);
            var tag = await _context.Tags.Include(t => t.Tasks).SingleAsync(t => t.Id == tagId);
            Assert.AreEqual(expectedTagsCount, task.Tags.Count, "Method does not add the tag to the task");
            Assert.AreEqual(expectedTagTasksCount, tag.Tasks.Count, "Method does not add the tag to the task");
            Assert.AreEqual(expectedTagName, task.Tags.Last().Name, "Method added wrong tag to the task");
            Assert.AreEqual(taskId, tag.Tasks.Last().Id, "Method added wrong task to the tag");
        }

        [Test]
        public async Task AddTagToTaskAsyncTest2_ValidData_AddsTagToTask()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var tagId = 4;
            var expectedTagsCount = 3;
            var oldTag = await _context.Tags.Include(t => t.Tasks).SingleAsync(t => t.Id == tagId);
            var expectedTagName = oldTag.Name;
            var expectedTagTasksCount = oldTag.Tasks.Count + 1;

            // Act
            await _service.AddTagToTaskAsync(taskId, tagId);

            // Assert
            var task = await _context.Tasks.Include(t => t.Tags).SingleAsync(t => t.Id == taskId);
            var tag = await _context.Tags.Include(t => t.Tasks).SingleAsync(t => t.Id == tagId);
            Assert.AreEqual(expectedTagsCount, task.Tags.Count, "Method does not add the tag to the task");
            Assert.AreEqual(expectedTagTasksCount, tag.Tasks.Count, "Method does not add the tag to the task");
            Assert.AreEqual(expectedTagName, task.Tags.Last().Name, "Method added wrong tag to the task");
            Assert.AreEqual(taskId, tag.Tasks.Last().Id, "Method added wrong task to the tag");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void DeleteTagFromTaskAsyncTest_InvalidTaskId_ThrowsInvalidOperationException(int taskId)
        {
            // Arrange
            SeedData();
            var tagId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteTagFromTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the task id is invalid");
        }

        [Test]
        [TestCase(-1)]          // invalid id
        [TestCase(0)]           // invalid id
        [TestCase(2)]           // valid, but does not belong to the task with id "1"
        [TestCase(4)]           // valid, but does not belong to the task with id "1"
        [TestCase(5)]           // invalid id
        public void DeleteTagFromTaskAsyncTest_TagDoesNotBelongToTask_ThrowsInvalidOperationException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteTagFromTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the tag id is invalid");
        }

        [Test]
        public async Task DeleteTagFromTaskAsyncTest_ValidData_DeletesTagFromTask()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var tagId = 3;
            var expectedTaskTagsCount = 1;
            var expectedTagTasksCount = 0;

            // Act
            await _service.DeleteTagFromTaskAsync(taskId, tagId);

            // Assert
            var task = await _context.Tasks.Include(t => t.Tags).SingleAsync(t => t.Id == taskId);
            var tag = await _context.Tags.Include(t => t.Tasks).SingleAsync(t => t.Id == tagId);
            Assert.AreEqual(expectedTaskTagsCount, task.Tags.Count, "Method does not delete a tag from a task");
            Assert.AreEqual(expectedTagTasksCount, tag.Tasks.Count, "Method does not delete a task from a tag");
        }

        [Test]
        public void GetProjectTasksByDate_ReturnsRightTasks()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            var from = new DateTime(2022, 1, 2);
            var to = new DateTime(2022, 2, 6);
            var expectedIds = new int[] { 1, 3, 6 };

            // Act
            var actualIds = _service.GetProjectTasksByDate(projectId, from, to).Select(t => t.Id);

            // Assert
            Assert.AreEqual(expectedIds.Length, actualIds.Count(), "Method returnes wrong elements");
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds), "Method returnes wrong elements");
        }
    }
}
