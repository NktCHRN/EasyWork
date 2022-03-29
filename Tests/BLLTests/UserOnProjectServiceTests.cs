using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Other;
using Business.Services;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Tests.BLLTests
{
    [TestFixture]
    public class UserOnProjectServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IUserOnProjectService _service = null!;

        private readonly IEnumerable<UserOnProjectModel> _invalidUoP = new UserOnProjectModel[]
        {
            new UserOnProjectModel()            // 0
            {
                ProjectId = -1,
                UserId = 5,
                Role = UserOnProjectRoles.Manager
            },
            new UserOnProjectModel()            // 1
            {
                ProjectId = 1,
                UserId = 0,
                Role = UserOnProjectRoles.User
            },
        };

        private readonly IEnumerable<UserOnProjectModel> _validUoP = new UserOnProjectModel[]
        {
            new UserOnProjectModel()            // 0
            {
                ProjectId = 1,
                UserId = 5,
                Role = UserOnProjectRoles.Manager
            },
            new UserOnProjectModel()            // 1
            {
                ProjectId = 1,
                UserId = 5,
                Role = UserOnProjectRoles.User
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _service = new UserOnProjectService(_context, _mapper);
        }

        private void SeedRequiredData()
        {
            var project = new Project() // id 1
            {
                Name = "Project 1",
            };
            _context.Projects.Add(project);
            _context.SaveChanges();
        }

        private void SeedData()
        {
            var additionalUsers = new User[]
            {
                new User()              // id 6
                {
                    FirstName = "Nicolas",
                    LastName = "Flamel",
                    Email = "nf30@fakemail.com"
                },
                new User()              // id 7
                {
                    FirstName = "Roger",
                    LastName = "Phillips",
                    Email = "rogph@fakemail.com"
                }
            };
            foreach (var user in additionalUsers)
            {
                user.NormalizedEmail = user.Email.ToUpperInvariant();
                user.UserName = user.Email;
                user.NormalizedUserName = user.NormalizedEmail;
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var projects = new Project[]
            {
                new Project()       // id 2
                {
                    Name = "Project 2",
                },
                new Project()       // id 3
                {
                    Name = "Project 3",
                },
            };
            foreach (var project in projects)
            {
                _context.Projects.Add(project);
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
                    ProjectId = 1,
                    UserId = 3,
                    Role = UserOnProjectRoles.Manager
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 7,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 4,
                    Role = UserOnProjectRoles.Manager
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 6,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 4,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 6,
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
                    UserId = 3,
                    Role = UserOnProjectRoles.Owner
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 5,
                    Role = UserOnProjectRoles.Owner
                },
            };
            foreach(var uop in uops)
            {
                _context.UsersOnProjects.Add(uop);
                _context.SaveChanges();
            }

            var tasks = new Data.Entities.Task[]
            {
                new Data.Entities.Task()
                {
                    Name = "Task 1",
                    ProjectId = 1,
                    ExecutorId = 1,
                    Status = TaskStatuses.InProgress
                },
                new Data.Entities.Task()
                {
                    Name = "Task 2",
                    ProjectId = 1,
                    ExecutorId = 1,
                    Status = TaskStatuses.Validate
                },
                new Data.Entities.Task()
                {
                    Name = "Task 3",
                    ProjectId = 1,
                    ExecutorId = 7,
                    Status = TaskStatuses.ToDo
                },
                new Data.Entities.Task()
                {
                    Name = "Task 4",
                    ProjectId = 1,
                    ExecutorId = 7,
                    Status = TaskStatuses.Complete
                },
                new Data.Entities.Task()
                {
                    Name = "Task 5",
                    ProjectId = 1,
                    ExecutorId = 7,
                    Status = TaskStatuses.Complete
                },
                new Data.Entities.Task()
                {
                    Name = "Task 6",
                    ProjectId = 3,
                    ExecutorId = 5,
                    Status = TaskStatuses.Archived
                }
            };
            foreach (var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }
        }

        private readonly IEnumerable<UserOnProjectModel> _invalidForAddUoP = new UserOnProjectModel[]
        {
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    Role = UserOnProjectRoles.Manager
                },
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    Role = UserOnProjectRoles.User
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 3,
                    Role = UserOnProjectRoles.User
                }
        };

        private readonly IEnumerable<UserOnProjectModel> _validForUpdateUoP = new UserOnProjectModel[]
        {
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    Role = UserOnProjectRoles.Manager
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 3,
                    Role = UserOnProjectRoles.User
                }
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidUoP.ElementAt(modelNumber);

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
            var model = _validUoP.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Method does not write null to the error message field");
        }

        [Test]
        [TestCase(-1, 1)]
        [TestCase(0, 1)]
        [TestCase(7, 3)]
        [TestCase(2, 5)]
        [TestCase(3, 1)]
        public async Task GetByIdAsyncTest_InvalidData_ReturnsNull(int projectId, int userId)
        {
            // Arrange
            SeedData();

            // Act
            var returned = await _service.GetByIdAsync(projectId, userId);

            // Assert
            Assert.IsNull(returned, "Method does not return null if id is invalid");
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(3, 6)]
        public async Task GetByIdAsync_ValidId_ReturnesElement(int projectId, int userId)
        {
            // Arrange
            SeedData();

            // Act
            var actual = await _service.GetByIdAsync(projectId, userId);

            // Assert
            Assert.AreEqual(projectId, actual!.ProjectId, "Method returns wrong element");
            Assert.AreEqual(userId, actual.UserId, "Method returns wrong element");
        }

        [Test]
        [TestCase(-1, 1)]
        [TestCase(0, 1)]
        [TestCase(7, 3)]
        [TestCase(2, 5)]
        [TestCase(3, 1)]
        public void DeleteByIdAsync_InvalidId_ThrowsInvalidOperationException(int projectId, int userId)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(projectId, userId),
                "Method does not throw an InvalidOperationException if id is invalid");
        }

        [Test]
        public void DeleteByIdAsync_OnlyOwnerId_ThrowsInvalidOperationException()
        {
            // Arrange
            var projectId = 1;
            var userId = 1;
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DeleteByIdAsync(projectId, userId),
                "Method does not throw an InvalidOperationException if id is invalid");
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(3, 6)]
        public async Task DeleteByIdAsync_ValidId_DeletesElement(int projectId, int userId)
        {
            // Arrange
            SeedData();
            var expectedCount = _context.UsersOnProjects.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(projectId, userId);

            // Assert
            var actualCount = _context.UsersOnProjects.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.UsersOnProjects.Any(m => m.ProjectId == projectId && m.UserId == userId), 
                "Method deletes wrong element");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validUoP.First();
            var expectedProjectId = model.ProjectId;
            var expectedUserId = model.UserId;
            var expectedRole = model.Role;
            var expectedCount = _context.UsersOnProjects.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.UsersOnProjects.Count();
            var actual = _context.UsersOnProjects.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedProjectId, actual.ProjectId, "Method does not add model with needed information");
            Assert.AreEqual(expectedUserId, actual.UserId, "Method does not add model with needed information");
            Assert.AreEqual(expectedRole, actual.Role, "Method does not add model with needed information");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidUoP.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void AddAsyncTest_AlreadyAddedModel_ThrowsException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForAddUoP.ElementAt(index);

            // Act & Assert
            Assert.That(async () => await _service.AddAsync(model), Throws.Exception, "Method does not throw any type of exception");
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidUoP.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel(int index)
        {
            // Arrange
            SeedData();
            var model = _validForUpdateUoP.ElementAt(index);
            var expectedRole = model.Role;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.UsersOnProjects
                .SingleAsync(uop => uop.ProjectId == model.ProjectId && uop.UserId == model.UserId);
            Assert.AreEqual(expectedRole, actual.Role, "Method does not update model");
        }

        private readonly IEnumerable<IEnumerable<UserOnProjectModelExtended>> _expectedGetAllProjectUsersAsync = new IEnumerable<UserOnProjectModelExtended>[]
        {
            new UserOnProjectModelExtended[]        // ind 0 , project id 1
            {
                new UserOnProjectModelExtended()
                {
                    UserId = 1,
                    Role = UserOnProjectRoles.Owner,
                    TasksDone = 1,
                    TasksNotDone = 1
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 3,
                    Role = UserOnProjectRoles.Manager,
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 4,
                    Role = UserOnProjectRoles.Manager,
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 7,
                    Role = UserOnProjectRoles.User,
                    TasksDone = 2,
                    TasksNotDone = 1
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 2,
                    Role = UserOnProjectRoles.User,
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 6,
                    Role = UserOnProjectRoles.User,
                },
            },
            new UserOnProjectModelExtended[]        // ind 1 , project id 2
            {
                new UserOnProjectModelExtended()
                {
                    UserId = 3,
                    Role = UserOnProjectRoles.Owner,
                }
            },
            new UserOnProjectModelExtended[]        // ind 2 , project id 3
            {
                new UserOnProjectModelExtended()
                {
                    UserId = 5,
                    Role = UserOnProjectRoles.Owner,
                    TasksDone = 1,
                },
new UserOnProjectModelExtended()
                {
                    UserId = 4,
                    Role = UserOnProjectRoles.User,
                },
                new UserOnProjectModelExtended()
                {
                    UserId = 6,
                    Role = UserOnProjectRoles.User,
                },
            }
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetTaskTagsAsyncTest_ReturnsRealTaskTags(int index)
        {
            // Arrange
            SeedData();
            var projectId = index + 1;
            var expected = _expectedGetAllProjectUsersAsync.ElementAt(index);

            // Act
            var actual = await _service.GetAllProjectUsersAsync(projectId);

            // Assert
            Assert.AreEqual(expected.Count(), actual.Count(), "Method returnes wrong elements");
            Assert.IsTrue(expected.SequenceEqual(actual), "Method returnes wrong elements");
        }

        [Test]
        [TestCase(-1, 1)]
        [TestCase(0, 1)]
        [TestCase(7, 3)]
        [TestCase(2, 5)]
        [TestCase(3, 1)]
        public async Task GetRoleOnProjectAsyncTest_InvalidId_ReturnsNull(int projectId, int userId)
        {
            // Arrange
            SeedData();

            // Act
            var returned = await _service.GetRoleOnProjectAsync(projectId, userId);

            Assert.IsNull(returned);
        }

        [Test]
        [TestCase(1,2, UserOnProjectRoles.User)]
        [TestCase(1, 4, UserOnProjectRoles.Manager)]
        [TestCase(3, 5, UserOnProjectRoles.Owner)]
        public async Task GetRoleOnProjectAsyncTest_ValidId_ReturnsRightRole(int projectId, int userId, 
            UserOnProjectRoles expected)
        {
            // Arrange
            SeedData();

            // Act
            var actual = await _service.GetRoleOnProjectAsync(projectId, userId);

            // Assert
            Assert.AreEqual(expected, actual, "Method returnes wrong role");
        }
    }
}
