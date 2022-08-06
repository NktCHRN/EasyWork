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
using Tests.Comparers;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class ProjectServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IProjectService _service = null!;

        private readonly Mock<IFileManager> _managerMock = new();

        private readonly IEnumerable<ProjectModel> _invalidProjects = new ProjectModel[]
        {
            new ProjectModel()      // index 0
            {       // no name
                Description = "This is project one description...",
                Limits = new ProjectLimitsModel
                {
                                    MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 1
            {
                Name = "Invalid project 2",
                Description = "This is project two description...",
                Limits = new ProjectLimitsModel{
                MaxInProgress = -3,
                MaxToDo = 1,
                MaxValidate = 5, },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = false,
            },
            new ProjectModel()      // index 2
            {
                Name = "Invalid project 3",
                Description = "This is project three description...",
                Limits = new ProjectLimitsModel{
                MaxInProgress = 3,
                MaxToDo = -1,
                MaxValidate = 5,
                }
            },
            new ProjectModel()      // index 3
            {
                Name = "Invalid project 4",
                Description = "This is project four description...",
                Limits = new ProjectLimitsModel{
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = -5,
                }
            },
            new ProjectModel()      // index 4
            {
                Name = "Invalid project 5",
                Description = "This is project one description...",
                Limits = new ProjectLimitsModel { MaxInProgress = 3, MaxToDo = 1, MaxValidate = 5 },
                IsInviteCodeActive = true,
            }
        };

        private readonly IEnumerable<ProjectModel> _validProjects = new ProjectModel[]
        {
            new ProjectModel()      // index 0
            {
                Name = "Valid project 1",
                Description = "This is project one description...",
                Limits = new ProjectLimitsModel { MaxInProgress = 3, MaxToDo = 1, MaxValidate = 5 },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 1
            {
                Name = "Valid project 2",
                Limits = new ProjectLimitsModel { MaxInProgress = 3, MaxToDo = 1, MaxValidate = 5 },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 2
            {
                Name = "Valid project 3",
                Description = "This is project three description...",
                Limits = new ProjectLimitsModel { MaxInProgress = 3, MaxToDo = 1, MaxValidate = 0 },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = false,
            },
            new ProjectModel()      // index 3
            {
                Name = "Valid project 4",
                Description = "This is project four description...",
                Limits = new ProjectLimitsModel { MaxInProgress = 13, MaxToDo = 100, MaxValidate = 50 },
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            _service = new ProjectService(_context, _mapper, _managerMock.Object);
        }

        private void SeedData()
        {
            var projects = new Project[]
            {
                new Project()
            {
                Name = "Project sample 1",
                Description = "This is project one description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
                new Project()
            {
                Name = "Project sample 2",
                Description = "This is project two description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = false,
            },
                new Project()
            {
                Name = "Project sample 3",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            };
            foreach (var project in projects)
            {
                project.StartDate = DateTime.Now;
                _context.Projects.Add(project);
                _context.SaveChanges();
            }

            var tasks = new Data.Entities.Task[]
            {
                new Data.Entities.Task()
                {
                    Name = "Task 1",
                    ProjectId = 1
                },
                new Data.Entities.Task()
                {
                    Name = "Task 2",
                    ProjectId = 1
                },
                new Data.Entities.Task()
                {
                    Name = "Task 3",
                    ProjectId = 2
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
                    Text = "This is message 1",
                    TaskId = 1
                },
                new Message()
                {
                    Text = "This is message 2",
                    TaskId = 3
                },
            };
            foreach (var message in messages)
            {
                _context.Messages.Add(message);
                _context.SaveChanges();
            }

            var uops = new UserOnProject[]
            {
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 2,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 2,
                    Role = UserOnProjectRoles.Manager
                },
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 3,
                    Role = UserOnProjectRoles.Manager
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 3,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 4,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 1,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 1,
                    Role = UserOnProjectRoles.Owner
                },
                new UserOnProject()
                {
                    ProjectId = 2,
                    UserId = 1,
                    Role = UserOnProjectRoles.Owner
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 5,
                    Role = UserOnProjectRoles.Owner
                }
            };
            foreach (var uop in uops)
            {
                _context.UsersOnProjects.Add(uop);
                _context.SaveChanges();
            }

            var files = new File[]
            {
                new File()
                {
                    Name = "File1.fl",
                    TaskId = 1,
                    IsFull = true
                },
                new File()
                {
                    Name = "File2.fl",
                    TaskId = 1,
                    IsFull = true
                },
                new File()
                {
                    Name = "File3.fl",
                    TaskId = 3
                },
                new File()
                {
                    Name = "File4.fl",
                    TaskId = 1
                },
                new File()
                {
                    Name = "File5.fl",
                    TaskId = 1,
                    IsFull = true
                },
                new File()
                {
                    Name = "File6.fl",
                    TaskId = 1
                },
                new File()
                {
                    Name = "File7.fl",
                    TaskId = 2,
                    IsFull = true
                }
            };
            foreach (var file in files)
            {
                _context.Files.Add(file);
                _context.SaveChanges();
            }

            SetDateFixes();
        }

        private void SetDateFixes()
        {
            foreach (var project in _invalidForUpdateProjects)
            {
                if (project.StartDate != DateTime.MaxValue)
                    project.StartDate = (_context.Projects.Find(project.Id))!.StartDate;
            }
            _validForUpdateProject.StartDate = (_context.Projects.Find(_validForUpdateProject.Id))!.StartDate;
        }

        private readonly IEnumerable<ProjectModel> _invalidForUpdateProjects = new ProjectModel[]
        {
            new ProjectModel()      // id 1, ind 2
            {
                Id = 1,
                Name = "Project sample 1",
                Description = "This is project one description...",
                Limits = new ProjectLimitsModel
                {
                    MaxInProgress = 3,
                    MaxToDo = 1,
                    MaxValidate = 5,
                },
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
                StartDate = DateTime.MaxValue       // changed
            },
        };

        private readonly ProjectModel _validForUpdateProject = new()        // id 1
        {
            Id = 1,
            Name = "Updated project sample 1",
            Description = "This is updated project one description...",
            Limits = new ProjectLimitsModel
            {
                MaxInProgress = 7,
                MaxToDo = 7,
                MaxValidate = 7,
            },
            InviteCode = Guid.NewGuid(),
            IsInviteCodeActive = true,
            // Do not forget to fix start date
        };

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
        public async Task DeleteByIdAsync_ValidId_DeletesElementCascadeWithFiles()
        {
            // Arrange
            SeedData();
            var id = 1;
            var expectedCount = _context.Projects.Count() - 1;
            var expectedTasksCount = 1;
            var expectedMessagesCount = 1;
            var expectedFilesCount = 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Projects.Count();
            var actualTasksCount = _context.Tasks.Count();
            var actualMessagesCount = _context.Messages.Count();
            var actualFilesCount = _context.Files.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Projects.Any(t => t.Id == id), "Method deletes wrong element");
            Assert.AreEqual(expectedTasksCount, actualTasksCount, "Method does not delete all tasks cascadely");
            Assert.AreEqual(expectedMessagesCount, actualMessagesCount, "Method does not delete all messages cascadely");
            Assert.AreEqual(expectedFilesCount, actualFilesCount, "Method does not delete all files cascadely");
            var deletedFilesIds = new int[] { 1, 2, 5 };      // 4, 6 - chunks
            foreach (var fileId in deletedFilesIds)
            {
                _managerMock.Verify(t => t.DeleteFile($"{fileId}.fl", Business.Enums.EasyWorkFileTypes.File),
                "Method does not remove the file from file system");
            }
            var deletedChunksIds = new int[] { 4, 6 };
            foreach (var fileId in deletedChunksIds)
                _managerMock.Verify(t => t.DeleteChunks(fileId.ToString()), "Method does not remove chunks from file system");
        }

        [Test]
        public async Task DeleteByIdAsync_ValidId_DeletesElement()
        {
            // Arrange
            SeedData();
            var id = 2;
            var expectedCount = _context.Projects.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Projects.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Projects.Any(t => t.Id == id), "Method deletes wrong element");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validProjects.First();
            var expectedName = model.Name;
            var expectedCount = _context.Projects.Count() + 1;
            var expectedLimits = model.Limits;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Projects.Count();
            var actual = _context.Projects.Last();
            var actualLimits = new ProjectLimitsModel
            {
                MaxToDo = actual.MaxToDo,
                MaxInProgress = actual.MaxInProgress,
                MaxValidate = actual.MaxValidate
            };
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedName, actual.Name, "Method does not add model with needed information");
            Assert.AreNotEqual(model.Id, 0, "Method does not set id to the model");
            Assert.IsTrue((new ProjectLimitsModelEqualityComparer()).Equals(expectedLimits, actualLimits),
    "Method set wrong limits");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidProjects.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidProjects.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        private readonly IEnumerable<ProjectLimitsModel> _invalidLimits = new ProjectLimitsModel[]
        {
            new ProjectLimitsModel
            {
                                MaxInProgress = -10,
                MaxToDo = 8,
                MaxValidate = 7
            },
            new ProjectLimitsModel
            {
                                MaxInProgress = 10,
                MaxToDo = -8,
                MaxValidate = 7
            },
            new ProjectLimitsModel
            {
                MaxInProgress = 10,
                MaxToDo = 8,
                MaxValidate = -7
            }
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void UpdateLimitsByIdAsyncTest_InvalidModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidLimits.ElementAt(index);
            var projectId = 1;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateLimitsByIdAsync(projectId, model));
        }

        private readonly ProjectLimitsModel _validLimit = new()
        {
            MaxInProgress = 10,
            MaxToDo = 8,
            MaxValidate = 7
        };

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(20)]
        public void UpdateLimitsByIdAsyncTest_InvalidProjectId_ThrowsInvalidOperationException(int projectId)
        {
            // Arrange
            SeedData();
            var model = _validLimit;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateLimitsByIdAsync(projectId, model));
        }

        [Test]
        public async Task UpdateLimitsByIdAsyncTest_ValidModelAndProjectId_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validLimit;
            var expected = model;
            var projectId = 1;
            var project = (await _context.Projects.FindAsync(projectId))!;
            var expectedName = project.Name;

            // Act
            await _service.UpdateLimitsByIdAsync(projectId, model);
            var actualProject = await _context.Projects.FindAsync(projectId);
            Assert.NotNull(actualProject);
            Assert.AreEqual(expectedName, actualProject!.Name, "The name should be left the same");
            var actual = new ProjectLimitsModel
            {
                MaxToDo = actualProject!.MaxToDo,
                MaxInProgress = actualProject!.MaxInProgress,
                MaxValidate = actualProject!.MaxValidate
            };
            Assert.IsTrue(new ProjectLimitsModelEqualityComparer().Equals(expected, actual), 
                "The method has not updated the limits");
        }

        [Test]
        [TestCase(0)]
        public void UpdateAsyncTest_InvalidForUpdateOnlyModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForUpdateProjects.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateProject;
            var expectedName = model.Name;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Projects.SingleAsync(p => p.Id == model.Id);
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidProjects.ElementAt(modelNumber);

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
            var model = _validProjects.ElementAt(modelNumber);

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
        public async Task GetLimitsByIdAsyncTest_InvalidId_ReturnsNull(int id)
        {
            // Arrange
            SeedData();

            // Act
            var returned = await _service.GetLimitsByIdAsync(id);

            // Assert
            Assert.IsNull(returned, "Method does not return null if id is invalid");
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task GetLimitsByIdAsyncTest_ValidId_ReturnesElement(int id)
        {
            // Arrange
            SeedData();
            var project = (await _context.Projects.FindAsync(id))!;
            var expected = new ProjectLimitsModel
            {
                MaxToDo = project.MaxToDo,
                MaxInProgress = project.MaxInProgress,
                MaxValidate = project.MaxValidate
            };

            // Act
            var actual = await _service.GetLimitsByIdAsync(id);

            // Assert
            Assert.IsTrue(new ProjectLimitsModelEqualityComparer().Equals(expected, actual),
                "Method returns wrong element");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
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
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task GetByIdAsyncTest_ValidId_ReturnesElement(int id)
        {
            // Arrange
            SeedData();

            // Act
            var actual = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, actual!.Id, "Method returns wrong element");
            Assert.AreNotEqual(null, actual!.TasksIds, "Method does not load tasks ids");
            Assert.AreNotEqual(null, actual!.TeamMembersIds, "Method does not load team members ids");
        }

        [Test]
        public async Task GetByIdAsyncTest_ValidId_ReturnesElementWithRightLimits()
        {
            // Arrange
            const int id = 1;
            SeedData();
            var expectedLimits = new ProjectLimitsModel
            { MaxToDo = 1, MaxInProgress = 3, MaxValidate = 5 };

            // Act
            var actual = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, actual!.Id, "Method returns wrong element");
            Assert.IsTrue((new ProjectLimitsModelEqualityComparer()).Equals(expectedLimits, actual!.Limits),
                "Method returned wrong limits");

        }

        [Test]
        public void GetAllTest_ReturnsAllElements()
        {
            // Arrange
            SeedData();
            var expected = 3;

            // Act
            var actual = _service.GetCount();

            // Assert
            Assert.AreEqual(expected, actual, "Method returns wrong projects' count");
        }

        [Test]
        public void GetUserProjectsTest_ReturnsProjectBothAsParticipantAndOwner()
        {
            // Arrange
            SeedData();
            var userId = 1;
            IEnumerable<int> expectedProjectNumbers = new[] { 2, 1, 3 };

            // Act
            var actual = _service.GetUserProjects(userId);

            // Assert
            var actualProjectNumbers = actual.Select(p => p.Id);
            Assert.AreEqual(expectedProjectNumbers.Count(), actualProjectNumbers.Count(), "The quantities of elements are not equal. " +
                "Wrong elements returned");
            Assert.IsTrue(!expectedProjectNumbers.Except(actualProjectNumbers).Any() 
                && !actualProjectNumbers.Except(expectedProjectNumbers).Any(), "Elements are wrong");
        }

        [Test]
        public void GetUserProjectsTest_ReturnsSortedProjects()
        {
            // Arrange
            SeedData();
            var userId = 1;
            IEnumerable<int> expectedProjectNumbers = new[] { 2, 1, 3 };
            var lastTime = DateTimeOffset.MaxValue;
            foreach (var projectId in expectedProjectNumbers)
            {
                var uop = _context.UsersOnProjects.Single(u => u.UserId == userId && u.ProjectId == projectId);
                uop.AdditionDate = lastTime;
                _context.SaveChanges();
                lastTime -= new TimeSpan(1, 1, 1);
            }

            // Act
            var actual = _service.GetUserProjects(userId);

            // Assert
            var actualProjectNumbers = actual.Select(p => p.Id);
            Assert.AreEqual(expectedProjectNumbers.Count(), actualProjectNumbers.Count(), "The quantities of elements are not equal. " +
                "Wrong elements returned");
            Assert.IsTrue(expectedProjectNumbers.SequenceEqual(actualProjectNumbers), "Elements are not sorted correctly");
        }

        [Test]
        public async Task GetProjectByActiveInviteCodeAsyncTest_ReturnsProject()
        {
            // Arrange
            SeedData();
            var guid = (await _context.Projects.FirstAsync())!.InviteCode!.Value;
            var expectedId = 1;

            // Act
            var actualId = (await _service.GetProjectByActiveInviteCodeAsync(guid))!.Id;

            // Assert
            Assert.AreEqual(expectedId, actualId);
        }

        IEnumerable<Guid> Guids
            {
            get {
                SeedData();
                return new Guid[]
                {
                    new Guid(),
                    _context.Projects.Find(2)!.InviteCode!.Value
                };
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public async Task GetProjectByActiveInviteCodeAsyncTest_ReturnsNull(int index)
        {
            // Arrange
            var guid = Guids.ElementAt(index);

            // Act
            var result = await _service.GetProjectByActiveInviteCodeAsync(guid);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetProjectByActiveInviteCodeAsyncStringTest_ReturnsProject()
        {
            // Arrange
            SeedData();
            var expectedId = 3;
            var guid = (await _context.Projects.FindAsync(3))!.InviteCode!.Value.ToString();

            // Act
            var actualId = (await _service.GetProjectByActiveInviteCodeAsync(guid))!.Id;

            // Assert
            Assert.AreEqual(expectedId, actualId);
        }

        IEnumerable<string> GuidStrings
        {
            get
            {
                SeedData();
                return new String[]
                {
                    new Guid().ToString(),
                    _context.Projects.Find(2)!.InviteCode!.Value.ToString(),
                    "",
                    "string123"
                };
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task GetProjectByActiveInviteCodeAsyncStringTest_ReturnsNull(int index)
        {
            // Arrange
            var guid = GuidStrings.ElementAt(index);

            // Act
            var result = await _service.GetProjectByActiveInviteCodeAsync(guid);

            // Assert
            Assert.IsNull(result);
        }
    }
}
