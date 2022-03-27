using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private IFileManager _manager = null!;

        private readonly IEnumerable<ProjectModel> _invalidProjects = new ProjectModel[]
        {
            new ProjectModel()      // index 0
            {       // no name
                Description = "This is project one description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 1
            {
                Name = "Invalid project 2",
                Description = "This is project two description...",
                MaxInProgress = -3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = false,
            },
            new ProjectModel()      // index 2
            {
                Name = "Invalid project 3",
                Description = "This is project three description...",
                MaxInProgress = 3,
                MaxToDo = -1,
                MaxValidate = 5,
            },
            new ProjectModel()      // index 3
            {
                Name = "Invalid project 4",
                Description = "This is project four description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = -5,
            }
        };

        private readonly IEnumerable<ProjectModel> _validProjects = new ProjectModel[]
        {
            new ProjectModel()      // index 0
            {
                Name = "Valid project 1",
                Description = "This is project one description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 1
            {
                Name = "Valid project 2",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
            },
            new ProjectModel()      // index 2
            {
                Name = "Valid project 3",
                Description = "This is project three description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 0,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = false,
            },
            new ProjectModel()      // index 3
            {
                Name = "Valid project 4",
                Description = "This is project four description...",
                MaxInProgress = 13,
                MaxToDo = 100,
                MaxValidate = 50,
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            _manager = _managerMock.Object;
            _service = new ProjectService(_context, _mapper, _manager);
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
                MainPictureFormat = "jpg"
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
                }
            };
            foreach(var message in messages)
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

            var tags = new Tag[]
            {
                new Tag()
                {
                    Name = "Automatisation",
                },
                new Tag()
                {
                    Name = "Testing",
                }
            };
            foreach(var tag in tags)
            {
                _context.Tags.Add(tag);
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
            new ProjectModel()      // id 1, ind 0
            {
                Id = 1,
                Name = "Project sample 1",
                Description = "This is project one description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
                InviteCode = Guid.NewGuid(),
                IsInviteCodeActive = true,
                MainPictureFormat = "png"   // changed
                // Do not forget to fix start date
            },
            new ProjectModel()      // id 1, ind 2
            {
                Id = 1,
                Name = "Project sample 1",
                Description = "This is project one description...",
                MaxInProgress = 3,
                MaxToDo = 1,
                MaxValidate = 5,
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
                MaxInProgress = 7,
                MaxToDo = 7,
                MaxValidate = 7,
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
        public async Task DeleteByIdAsync_ValidId_DeletesElementCascade()
        {
            // Arrange
            SeedData();
            var id = 1;
            var expectedCount = _context.Projects.Count() - 1;
            var expectedTasksCount = 1;
            var expectedMessagesCount = 0;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Projects.Count();
            var actualTasksCount = _context.Tasks.Count();
            var actualMessagesCount = _context.Messages.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Projects.Any(t => t.Id == id), "Method deletes wrong element");
            Assert.AreEqual(expectedTasksCount, actualTasksCount, "Method does not delete all tasks cascadely");
            Assert.AreEqual(expectedMessagesCount, actualMessagesCount, "Method does not delete all messages cascadely");
        }

        [Test]
        public async Task DeleteByIdAsync_ValidId_DeletesElementAndMainPicture()
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
            _managerMock.Verify(t => t.DeleteFile("2.jpg", Business.Enums.EasyWorkFileTypes.ProjectMainPicture),
                "Method does not remove the file from file system");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validProjects.First();
            var expectedName = model.Name;
            var expectedCount = _context.Projects.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Projects.Count();
            var actual = _context.Projects.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedName, actual.Name, "Method does not add model with needed information");
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

        [Test]
        [TestCase(0)]
        [TestCase(1)]
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
        public void GetByIdAsyncTest_InvalidId_ThrowsInvalidOperationException(int id)
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
        [TestCase(3)]
        public async Task GetByIdAsyncTest_ValidId_ReturnesElement(int id)
        {
            // Arrange
            SeedData();

            // Act
            var actual = await _service.GetByIdAsync(id);

            // Assert
            Assert.AreEqual(id, actual.Id, "Method returns wrong element");
            Assert.AreNotEqual(null, actual.ReleasesIds, "Method does not load releases ids");
            Assert.AreNotEqual(null, actual.TasksIds, "Method does not load tasks ids");
            Assert.AreNotEqual(null, actual.TeamMembersIds, "Method does not load team members ids");
        }

        [Test]
        public void GetAllTest_ReturnsAllElements()
        {
            // Arrange
            SeedData();
            var expectedCount = 3;

            // Act
            var actual = _service.GetAll();

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method does not return all objects");
            Assert.IsTrue(actual.All(p => p is not null), "Method returned null instead of some objects");
        }

        [Test]
        public void GetUserProjectsTest_ReturnsProjectBothAsParticipantAndOwner()
        {
            // Arrange
            SeedData();
            var userId = 1;
            IEnumerable<int> expectedProjectNumbers = new[] {3, 2, 1};

            // Act
            var actual = _service.GetUserProjects(userId);

            // Assert
            var actualProjectNumbers = actual.Select(p => p.Id);
            Assert.AreEqual(expectedProjectNumbers.Count(), actualProjectNumbers.Count(), "The quantities of elements are not equal. " +
                "Wrong elements returned");
            Assert.IsTrue(expectedProjectNumbers.SequenceEqual(actualProjectNumbers), "Elements are wrong or not sorted correctly");
        }

        [Test]
        public async Task UpdateMainPictureAsyncTest_ValidProjectIdAndFile_DeletesOldFileAndAddsNew()
        {
            // Arrange
            SeedData();
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "bmp";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _managerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "bmp" || s == ".bmp"))).Returns(true);
            var file = fileMock.Object;
            var projectId = 2;

            // Act
            await _service.UpdateMainPictureByProjectIdAsync(projectId, file);

            // Assert
            var actualProject = await _context.Projects.SingleAsync(p => p.Id == projectId);
            Assert.AreEqual(expectedformat, actualProject.MainPictureFormat, "Method does not change the file format");
            _managerMock.Verify(t => t.DeleteFile("2.jpg", Business.Enums.EasyWorkFileTypes.ProjectMainPicture),
                "Method does not remove old file from the file system");
            _managerMock.Verify(t => t.AddFileAsync(file, "2.bmp", Business.Enums.EasyWorkFileTypes.ProjectMainPicture),
                "Method does not add file to file system");
        }

        [Test]
        public void UpdateMainPictureAsyncTest_InvalidFileFormat_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "pdf";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _managerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "pdf" || s == ".pdf"))).Returns(false);
            var file = fileMock.Object;
            var projectId = 2;

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateMainPictureByProjectIdAsync(projectId, file),
                "Method does not throw an ArgumentException if the file format is not appropriate");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void UpdateMainPictureAsyncTest_InvalidId_ThrowsInvalidOperationException(int projectId)
        {
            // Arrange
            SeedData();
            var fileMock = new Mock<IFormFile>();
            var expectedformat = "bmp";
            fileMock.Setup(m => m.FileName).Returns($"avtr.{expectedformat}");
            _managerMock.Setup(m => m.IsValidImageType(It.Is<string>(s => s == "bmp" || s == ".bmp"))).Returns(true);
            var file = fileMock.Object;

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateMainPictureByProjectIdAsync(projectId, file),
                "Method does not throw an InvalidOperationException if project id is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void DeleteMainPictureAsyncTest_InvalidId_ThrowsInvalidOperationException(int projectId)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteMainPictureByProjectIdAsync(projectId),
                "Method does not throw an InvalidOperationException if project id is invalid");
        }

        [Test]
        public async Task DeleteMainPictureAsyncTest_ValidId_DeletesPictureFromFileSystemAndChangesModel()
        {
            // Arrange
            SeedData();
            var projectId = 2;

            // Act
            await _service.DeleteMainPictureByProjectIdAsync(projectId);

            // Assert
            var actualProject = await _context.Projects.SingleAsync(p => p.Id == projectId);
            Assert.IsNull(actualProject.MainPictureFormat, "Method does not change the file format to null");
            _managerMock.Verify(t => t.DeleteFile("2.jpg", Business.Enums.EasyWorkFileTypes.ProjectMainPicture),
                "Method does not remove file from the file system");
        }
    }
}
