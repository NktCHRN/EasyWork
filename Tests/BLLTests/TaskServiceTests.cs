using AutoMapper;
using Business.Exceptions;
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
                ExecutorsIds = new List<int>{2 }
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
                ExecutorsIds = new List<int>{2 }
            },
            new TaskModel()         // 2
            {
                Name = "Task 1",
                StartDate = new DateTime(2022, 1, 1),
                Status = TaskStatuses.ToDo,
                Deadline = new DateTime(2021, 2, 1),
                Description = "This is the description of the task 1",
                Priority = TaskPriorities.Middle,
                ProjectId = 1,
                ExecutorsIds = new List<int>{2 }
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
                ExecutorsIds = new List<int>{2 }
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
                ExecutorsIds = new List<int>{2 }
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
                },
                new Project()       // id 3
                {
                    Name = "Project 3",
                    MaxToDo = 3,
                    MaxInProgress = 2,
                    MaxValidate = 1
                }
            };
            _context.Projects.AddRange(projects);
            _context.SaveChanges();

            var uops = new UserOnProject[]
            {
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 5
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 3
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
                ProjectId = 1
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
                ProjectId = 1
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
                },
                new TaskEntity()            // id 5
                {
                    Name = "Task 3",
                StartDate = new DateTime(2022, 3, 1),
                Deadline = new DateTime(2022, 3, 20),
                EndDate = new DateTime(2022, 3, 19),
                Status = TaskStatuses.Complete,
                Priority = TaskPriorities.Low,
                ProjectId = 1
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
            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();

            var executors = new List<TaskExecutor>
            {
                new TaskExecutor
                {
                    TaskId = 1,
                    UserId = 2
                },
                new TaskExecutor
                {
                    TaskId = 1,
                    UserId = 3
                },
                new TaskExecutor
                {
                    TaskId = 3,
                    UserId = 5
                },
                new TaskExecutor
                {
                    TaskId = 4,
                    UserId = 5
                },
                new TaskExecutor
                {
                    TaskId = 5,
                    UserId = 5
                }
            };
            _context.TaskExecutors.AddRange(executors);
            _context.SaveChanges();

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
                    TaskId = 1,
                    Name = "file2.file"
                },
                new File()
                {
                    TaskId = 3,
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
                ExecutorsIds = new List<int>{2 }
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
                ExecutorsIds = new List<int>{2 }
            }
        };

        private readonly TaskModel _validForUpdateTask = new()        // id 1
        {
            Id = 1,
            Name = "Task 1 edited",
            StartDate = new DateTime(2022, 2, 3),
            Status = TaskStatuses.Complete,
            Deadline = new DateTime(2022, 2, 10),
            EndDate = new DateTime(2022, 3, 5),
            Description = "This is new description of the task 1",
            Priority = TaskPriorities.High,
            ProjectId = 1,
            ExecutorsIds = new List<int> { 5 }
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
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
        public async Task GetByIdAsyncTest_InvalidId_ReturnsNull(int id)
        {
            // Arrange
            SeedData();

            // Act
            var returned = await _service.GetByIdAsync(id);

            // Assert
            Assert.IsNull(returned, "Method does not return null if id is invalid");
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
            Assert.AreEqual(id, actual!.Id, "Method returns wrong element");
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
        [TestCase(1)]
        public void DeleteByIdAsync_NotArchivedTask_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(id),
                "Method does not throw an InvalidOperationException if the task is not archived");
        }

        [Test]
        public async Task DeleteByIdAsync_ValidId_DeletesElementCascadely()
        {
            // Arrange
            SeedData();
            var id = 1;
            var task = await _context.Tasks.SingleAsync(t => t.Id == id);
            task.Status = TaskStatuses.Archived;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            var expectedCount = _context.Tasks.Count() - 1;
            var expectedMessagesCount = 1;
            var expectedFilesCount = 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Tasks.Count();
            var actualFilesCount = _context.Files.Count();
            var actualMessagesCount = _context.Messages.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Tasks.Any(t => t.Id == id), "Method deletes wrong element");
            Assert.AreEqual(expectedFilesCount, actualFilesCount, "Method does not delete all files cascadely");
            Assert.AreEqual(expectedMessagesCount, actualMessagesCount, "Method does not delete all messages cascadely");
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
            Assert.AreNotEqual(model.Id, 0, "Method does not set id to the model");
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
        public void AddAsyncTest_ExceedsToDo_ThrowsLimitsExceededException()
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
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void AddAsyncTest_ExceedsInProgress_ThrowsLimitsExceededException()
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
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void AddAsyncTest_ExceedsValidate_ThrowsLimitsExceededExceptionException()
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
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.AddAsync(model));
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

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Tasks.SingleAsync(r => r.Id == model.Id);
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
            Assert.IsNotNull(actual.EndDate);
        }

        [Test]
        public async Task UpdateAsyncTest_NotChangedStatus_UpdatesEntity()
        {
            // Arrange
            SeedData();
            var project = (await _context.Projects.FindAsync(3))!;
            var task = new TaskEntity
            {
                Name = "TaskToUpdate",
                Status = TaskStatuses.Validate,
                ProjectId = project.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            var model = new TaskModel
            {
                Id = task.Id,
                Status = task.Status,
                Name = "New name",
                ProjectId = task.ProjectId
            };
            var expectedName = model.Name;
            for (int i = 0; i < project.MaxValidate - 1; i++)
            {
                await _context.Tasks.AddAsync(new TaskEntity() { Status = model.Status, ProjectId = project.Id }); ;
                await _context.SaveChangesAsync();
            }

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Tasks.SingleAsync(r => r.Id == task.Id);
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
        }

        [Test]
        public async Task UpdateAsyncTest_ExceedsToDo_ThrowsLimitsExceededException()
        {
            // Arrange
            SeedData();
            var testedStatus = TaskStatuses.ToDo;
            var project = (await _context.Projects.FindAsync(3))!;
            var task = new TaskEntity
            {
                Name = "TaskToUpdate",
                Status = TaskStatuses.Complete,
                ProjectId = project.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            var model = new TaskModel
            {
                Id = task.Id,
                Status = testedStatus,
                Name = "New name",
                ProjectId = task.ProjectId
            };
            var expectedName = model.Name;
            for (int i = 0; i < project.MaxToDo; i++)
            {
                await _context.Tasks.AddAsync(new TaskEntity() { Status = testedStatus, ProjectId = project.Id });
                await _context.SaveChangesAsync();
            }

            // Act & Assert
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ExceedsInProgress_ThrowsLimitsExceededException()
        {
            // Arrange
            SeedData();
            var testedStatus = TaskStatuses.InProgress;
            var project = (await _context.Projects.FindAsync(3))!;
            var task = new TaskEntity
            {
                Name = "TaskToUpdate",
                Status = TaskStatuses.Complete,
                ProjectId = project.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            var model = new TaskModel
            {
                Id = task.Id,
                Status = testedStatus,
                Name = "New name",
                ProjectId = task.ProjectId
            };
            var expectedName = model.Name;
            for (int i = 0; i < project.MaxInProgress; i++)
            {
                await _context.Tasks.AddAsync(new TaskEntity() { Status = testedStatus, ProjectId = project.Id });
                await _context.SaveChangesAsync();
            }

            // Act & Assert
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ExceedsValidate_ThrowsLimitsExceededException()
        {
            // Arrange
            SeedData();
            var testedStatus = TaskStatuses.Validate;
            var project = (await _context.Projects.FindAsync(3))!;
            var task = new TaskEntity
            {
                Name = "TaskToUpdate",
                Status = TaskStatuses.Complete,
                ProjectId = project.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            var model = new TaskModel
            {
                Id = task.Id,
                Status = testedStatus,
                Name = "New name",
                ProjectId = task.ProjectId
            };
            var expectedName = model.Name;
            for (int i = 0; i < project.MaxValidate; i++)
            {
                await _context.Tasks.AddAsync(new TaskEntity() { Status = testedStatus, ProjectId = project.Id });
                await _context.SaveChangesAsync();
            }

            // Act & Assert
            Assert.ThrowsAsync<LimitsExceededException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public void GetProjectTasksByStatusTest_Archived_ReturnsRightTasks()
        {
            // Arrange
            SeedData();
            var status = TaskStatuses.Archived;
            var projectId = 1;
            IEnumerable<int> expectedTasksIds = new[] { 6, 7 };

            // Act
            var actualTasks = _service.GetProjectTasksByStatus(projectId, status);

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
        public void GetUserTasksTest_ReturnsOnlyTasksWhereUserIsOnProject()
        {
            // Arrange
            SeedData();
            var task = new TaskEntity
            {
                ProjectId = 3
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();
            _context.TaskExecutors.Add(new TaskExecutor
            {
                UserId = 1,
                TaskId = task.Id
            });
            _context.SaveChanges();
            var userId = 3;
            IEnumerable<int> expectedTasksIds = new[] { 1 };

            // Act
            var actualTasks = _service.GetUserTasks(userId);

            // Assert
            Assert.AreEqual(expectedTasksIds.Count(), actualTasks.Count(), "Method returnes wrong elements");
            var actualTasksIds = actualTasks.Select(r => r.Id);
            Assert.IsTrue(expectedTasksIds.SequenceEqual(actualTasksIds),
                "Method returnes wrong elements or the order is wrong");
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

        [Test]
        public void GetProjectTasksByStatusTest_ReturnsRightTasks()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            var status = TaskStatuses.ToDo;
            var expectedIds = new int[] { 1, 3};

            // Act
            var actualIds = _service.GetProjectTasksByStatus(projectId, status).Select(t => t.Id);

            // Assert
            Assert.AreEqual(expectedIds.Length, actualIds.Count(), "Method returnes wrong elements");
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds), "Method returnes wrong elements");
        }

        [Test]
        public void GetProjectTasksByStatusTest_ReturnsRightTasksWithMessagesAndFiles()
        {
            // Arrange
            SeedData();        
            var projectId = 1;
            var status = TaskStatuses.ToDo;
            var tempTask = new TaskEntity()     // id 8
            {
                Name = "Temp task",
                ProjectId = projectId,
                Status = status
            };
            _context.Tasks.Add(tempTask);
            _context.SaveChanges();
            var expectedMsgCount = 2;
            var expectedFilesCount = 1;
            for (int i = 0; i < expectedMsgCount; i++)
            {
                var msg = new Message()
                {
                    Text = $"Message {i + 1}",
                    TaskId = 8
                };
                _context.Messages.Add(msg);
                _context.SaveChanges();
            }
            var file = new File()
            {
                Name = "File 1",
                TaskId = 8
            };
            _context.Files.Add(file);
            _context.SaveChanges();
            var expectedIds = new int[] { 1, 3, 8 };

            // Act
            var actual = _service.GetProjectTasksByStatus(projectId, status);
            var actualIds = actual.Select(t => t.Id);

            // Assert
            Assert.AreEqual(expectedIds.Length, actualIds.Count(), "Method returnes wrong elements");
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds), "Method returnes wrong elements");
            Assert.AreEqual(expectedMsgCount, actual.Last().MessagesIds.Count);
            Assert.AreEqual(expectedFilesCount, actual.Last().FilesIds.Count);
        }

        [Test]
        public void GetTaskExecutorsAsyncTest_ReturnsRightUsers()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var expected = new int[] { 2, 3 };

            // Act
            var actual = _service.GetTaskExecutors(taskId).Select(e => e.Id);

            // Assert
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void DeleteExecutorFromTaskAsyncTest_InvalidTaskId_ThrowsInvalidOperationException(int taskId)
        {
            // Arrange
            SeedData();
            var userId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteExecutorFromTaskAsync(taskId, userId),
                "Method does not throw InvalidOperationException if the task id is invalid");
        }

        [Test]
        [TestCase(-1)]          // invalid id
        [TestCase(0)]           // invalid id
        [TestCase(1)]           // valid, but does not belong to the task with id "1"
        [TestCase(4)]           // valid, but does not belong to the task with id "1"
        [TestCase(7)]           // invalid id
        public void DeleteExecutorFromTaskAsyncTest_UserDoesNotBelongToTask_ThrowsInvalidOperationException(int userId)
        {
            // Arrange
            SeedData();
            var taskId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteExecutorFromTaskAsync(taskId, userId),
                "Method does not throw InvalidOperationException if the tag id is invalid");
        }

        [Test]
        public async Task DeleteExecutorFromTaskAsyncTest_ValidData_DeletesUserFromTask()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            var userId = 2;
            var expectedTaskUsersCount = 1;
            var expectedUserTasksCount = 0;

            // Act
            await _service.DeleteExecutorFromTaskAsync(taskId, userId);

            // Assert
            var task = await _context.Tasks.Include(t => t.Executors).SingleAsync(t => t.Id == taskId);
            var user = await _context.Users.Include(t => t.Tasks).SingleAsync(t => t.Id == userId);
            Assert.AreEqual(expectedTaskUsersCount, task.Executors.Count, "Method does not delete a user from a task");
            Assert.AreEqual(expectedUserTasksCount, user.Tasks.Count, "Method does not delete a task from a user");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void AddExecutorToTaskAsyncTest_InvalidTaskId_ThrowsInvalidOperationException(int taskId)
        {
            // Arrange
            SeedData();
            var userId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddExecutorToTaskAsync(taskId, userId),
                "Method does not throw InvalidOperationException if the task id is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(8)]
        public void AddExecutorToTaskAsyncTest_InvalidUserId_ThrowsInvalidOperationException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddExecutorToTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the user id is invalid");
        }

        [Test]
        [TestCase(1)]
        [TestCase(4)]
        public void AddExecutorToTaskAsyncTest_UserIsNotOnProject_ThrowsArgumentException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddExecutorToTaskAsync(taskId, tagId),
                "Method does not throw ArgumentException if the user is not on the task's project");
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void AddExecutorToTaskAsyncTest_AlreadyTaskUserId_ThrowsInvalidOperationException(int tagId)
        {
            // Arrange
            SeedData();
            var taskId = 1;         // projectId = 1

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddExecutorToTaskAsync(taskId, tagId),
                "Method does not throw InvalidOperationException if the task already has such an executor");
        }

        [Test]
        public void AddExecutorToTaskAsyncTest_TooManyExecutors_ThrowsInvalidOperationException()
        {
            // Arrange
            SeedData();
            var task = new TaskEntity
            {
                Name = "Add Executor Test Task",
                ProjectId = 1
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();
            for (int i = 0; i < 5; i++)
            {
                var user = new User() { FirstName = $"User{i + 1}" };
                _context.Users.Add(user);
                _context.SaveChanges();
                _context.TaskExecutors.Add(new TaskExecutor
                {
                    UserId = user.Id,
                    TaskId = task.Id
                });
                _context.SaveChanges();
            }
            
            var newUserId = 2;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AddExecutorToTaskAsync(task.Id, newUserId),
                "Method does not throw InvalidOperationException if the task already has too many executors");
        }

        [Test]
        public async Task AddExecutorToTaskAsyncTest_ValidData_AddsUserToTask()
        {
            // Arrange
            SeedData();
            var taskId = 3;
            var userId = 3;
            var expectedExecutorsCount = 2;
            var oldUser = await _context.Users.Include(t => t.Tasks).SingleAsync(t => t.Id == userId);

            // Act
            await _service.AddExecutorToTaskAsync(taskId, userId);

            // Assert
            var task = await _context.Tasks.Include(t => t.Executors).SingleAsync(t => t.Id == taskId);
            var user = await _context.Users.Include(t => t.Tasks).SingleAsync(t => t.Id == userId);
            Assert.AreEqual(expectedExecutorsCount, task.Executors.Count, "Method does not add the user to the task");
            Assert.AreEqual(userId, task.Executors.Last().UserId, "Method added wrong user to the task");
            Assert.AreEqual(taskId, user.Tasks.Last().TaskId, "Method added wrong task to the user");
        }
    }
}
