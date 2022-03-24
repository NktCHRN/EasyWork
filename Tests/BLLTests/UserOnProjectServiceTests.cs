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
using Tests.Comparers;
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
                IsManager = true
            },
            new UserOnProjectModel()            // 1
            {
                ProjectId = 1,
                UserId = 0,
                IsManager = false
            },
        };

        private readonly IEnumerable<UserOnProjectModel> _validUoP = new UserOnProjectModel[]
        {
            new UserOnProjectModel()            // 0
            {
                ProjectId = 1,
                UserId = 5,
                IsManager = true
            },
            new UserOnProjectModel()            // 1
            {
                ProjectId = 1,
                UserId = 5,
                IsManager = false
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
                OwnerId = 1
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
                    OwnerId = 3
                },
                new Project()       // id 3
                {
                    Name = "Project 3",
                    OwnerId = 5
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
                    IsManager = false
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 3,
                    IsManager = true
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 7,
                    IsManager = false
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 4,
                    IsManager = true
                },
                new UserOnProject()
                {
                    ProjectId = 1,
                    UserId = 6,
                    IsManager = false
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 4,
                    IsManager = false
                },
                new UserOnProject()
                {
                    ProjectId = 3,
                    UserId = 6,
                    IsManager = false
                }
            };
            foreach(var uop in uops)
            {
                _context.UsersOnProjects.Add(uop);
                _context.SaveChanges();
            }
        }

        private readonly IEnumerable<UserOnProjectModel> _invalidForAddUoP = new UserOnProjectModel[]
        {
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    IsManager = true
                },
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    IsManager = false
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 3,
                    IsManager = false
                }
        };

        private readonly IEnumerable<UserOnProjectModel> _validForUpdateUoP = new UserOnProjectModel[]
        {
             new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    IsManager = true
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 3,
                    IsManager = false
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
        public void GetByIdAsync_InvalidId_ThrowsInvalidOperationException(int projectId, int userId)
        {
            // Arrange
            SeedData();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetByIdAsync(projectId, userId),
                "Method does not throw an InvalidOperationException if id is invalid");
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
            Assert.AreEqual(projectId, actual.ProjectId, "Method returns wrong element");
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
            var expectedStatus = model.IsManager;
            var expectedCount = _context.UsersOnProjects.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.UsersOnProjects.Count();
            var actual = _context.UsersOnProjects.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedProjectId, actual.ProjectId, "Method does not add model with needed information");
            Assert.AreEqual(expectedUserId, actual.UserId, "Method does not add model with needed information");
            Assert.AreEqual(expectedStatus, actual.IsManager, "Method does not add model with needed information");
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
            var expectedStatus = model.IsManager;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.UsersOnProjects
                .SingleAsync(uop => uop.ProjectId == model.ProjectId && uop.UserId == model.UserId);
            Assert.AreEqual(expectedStatus, actual.IsManager, "Method does not update model");
        }

        private readonly IEnumerable<IEnumerable<UserOnProjectModel>> _expectedGetProjectUsers = new IEnumerable<UserOnProjectModel>[]
        {
            new UserOnProjectModel[]        // ind 0 , project id 1
            {
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 3,
                    IsManager = true
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 4,
                    IsManager = true
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 2,
                    IsManager = false
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 6,
                    IsManager = false
                },
                new UserOnProjectModel()
                {
                    ProjectId = 1,
                    UserId = 7,
                    IsManager = false
                },
            },
            Array.Empty<UserOnProjectModel>(),          // ind 1, project id 2
            new UserOnProjectModel[]        // ind 2 , project id 3
            {
                new UserOnProjectModel()
                {
                    ProjectId = 3,
                    UserId = 4,
                    IsManager = false
                },
                new UserOnProjectModel()
                {
                    ProjectId = 3,
                    UserId = 6,
                    IsManager = false
                }
            }
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void GetProjectUsersTest_ReturnsRealProjectUsers(int index)
        {
            // Arrange
            SeedData();
            var projectId = index + 1;
            var expected = _expectedGetProjectUsers.ElementAt(index);

            // Act
            var actual = _service.GetProjectUsers(projectId);

            // Assert
            Assert.AreEqual(expected.Count(), actual.Count(), "Method returnes wrong elements");
            Assert.IsTrue(expected.SequenceEqual(actual, new UserOnProjectModelEqualityComparer()), "Method returnes wrong elements");
        }

        private readonly IEnumerable<IEnumerable<(int, UserOnProjectRoles)>> _expectedGetAllProjectUsersAsync = new IEnumerable<(int, UserOnProjectRoles)>[]
        {
            new (int, UserOnProjectRoles)[]        // ind 0 , project id 1
            {
                (1, UserOnProjectRoles.Owner),
                (3, UserOnProjectRoles.Manager),
                (4, UserOnProjectRoles.Manager),
                (2, UserOnProjectRoles.User),
                (6, UserOnProjectRoles.User),
                (7, UserOnProjectRoles.User)
            },
            new (int, UserOnProjectRoles)[]          // ind 1, project id 2
            {
                (3, UserOnProjectRoles.Owner)
            },
            new (int, UserOnProjectRoles)[]        // ind 2 , project id 3
            {
                (5, UserOnProjectRoles.Owner),
                (4, UserOnProjectRoles.User),
                (6, UserOnProjectRoles.User)
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
    }
}
