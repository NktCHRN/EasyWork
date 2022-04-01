using AutoMapper;
using Business.Interfaces;
using Business.Models;
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
    public class ReleaseServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IReleaseService _service = null!;

        private readonly IEnumerable<ReleaseModel> _invalidReleases = new ReleaseModel[]
        {
            new ReleaseModel()      // 0
            {
                Description = "This is the invalid release 1.0.0, the first release of our project.",
                ProjectId = 1
            },
            new ReleaseModel()      // 1
            {
                Name = "v1.0.0",
                Description = "This is the invalid release 1.0.0, the first release of our project.",
                ProjectId = -1
            },
            new ReleaseModel()      // 2
            {
                Name = "v1.0.0TooLong Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Sed nec arcu ac purus bibendum sodales sed. ",
                Description = "This is the invalid release 1.0.0, the first release of our project.",
                ProjectId = 1
            },
        };

        private readonly IEnumerable<ReleaseModel> _validReleases = new ReleaseModel[]
        {
            new ReleaseModel()  // 0
            {
                Name = "v1.0.0",
                Description = "This is the release 1.0.0, the first release of our project.",
                ProjectId = 1
            },
            new ReleaseModel()  // 1
            {
                Name = "v1.0.1",
                ProjectId = 1
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _service = new ReleaseService(_context, _mapper);
        }

        private void SeedRequiredData()
        {
            var project = new Project() // id 1
            {
                Name = "Project 1"
            };
            _context.Projects.Add(project);
            _context.SaveChanges();
        }

        private void SeedData(bool setDateTimeNow = true)
        {
            var projects = new Project[]
            {
                new Project()       // id 2
                {
                    Name = "Project 2"
                },
                new Project()       // id 3
                {
                    Name = "Project 3"
                }
            };
            foreach (var project in projects)
            {
                _context.Projects.Add(project);
                _context.SaveChanges();
            }

            var releases = new Release[]
            {
                new Release()       // id 1
                {
                    Name = "v1.0.0",
                    Description = "The first version of the project 1",
                    ProjectId = 1,
                    Date = new DateTime(2022, 1, 2)
                },
                new Release()       // id 2
                {
                    Name = "v1.1.0",
                    Description = "The 1.1.0 version of the project 1",
                    ProjectId = 1,
                    Date= new DateTime(2022, 3, 1)
                },
                new Release()       // id 3
                {
                    Name = "v1.0.0",
                    Description = "The first version of the project 2",
                    ProjectId = 2,
                    Date= new DateTime(2022, 1, 3)
                },
                new Release()       // id 4
                {
                    Name = "v1.2.0",
                    Description = "The 1.2.0 version of the project 1",
                    ProjectId = 1,
                    Date= new DateTime(2022, 1, 3)
                },
                new Release()       // id 5
                {
                    Name = "v1.0",
                    Description = "The initial version of the project 3",
                    ProjectId = 3
                },
                new Release()       // id 6
                {
                    Name = "v1.1",
                    Description = "The 1.1 version of the project 1",
                    ProjectId = 3
                },
            };
            foreach (var release in releases)
            {
                if (setDateTimeNow)
                    release.Date = DateTime.Now;
                _context.Releases.Add(release);
                _context.SaveChanges();
            }

            SetDateFixes();
        }

        private void SetDateFixes()
        {
            foreach (var release in _invalidForUpdateReleases)
            {
                if (release.Date != DateTime.MaxValue)
                    release.Date = (_context.Releases.Find(release.Id))!.Date;
            }
            _validForUpdateRelease.Date = (_context.Releases.Find(_validForUpdateRelease.Id))!.Date;
        }

        private readonly IEnumerable<ReleaseModel> _invalidForUpdateReleases = new ReleaseModel[]
        {
            new ReleaseModel()       // id 1, ind 0
                {
                Id = 1,
                    Name = "v1.0.0",
                    Description = "The first version of the project 1",
                    ProjectId = 2       // changed
                },
            new ReleaseModel()       // id 1, ind 1
                {
                Id = 1,
                    Name = "v1.0.0",
                    Description = "The first version of the project 1",
                    ProjectId = 1,
                    Date = DateTime.MaxValue    // changed
                },
        };

        private readonly ReleaseModel _validForUpdateRelease = new()        // id 5
        {
            Id = 5,
            Name = "v1.0.0",                                        // changed
            Description = "The first version of the project 3",     // changed
            ProjectId = 3
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidReleases.ElementAt(modelNumber);

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
            var model = _validReleases.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Method does not write null to the error message field");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(7)]
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
        [TestCase(3)]
        [TestCase(6)]
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
        [TestCase(7)]
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
        [TestCase(6)]
        public async Task DeleteByIdAsync_ValidId_DeletesElement(int id)
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Releases.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Releases.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Releases.Any(m => m.Id == id), "Method deletes wrong element");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validReleases.First();
            var expectedName = model.Name;
            var expectedCount = _context.Releases.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Releases.Count();
            var actual = _context.Releases.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedName, actual.Name, "Method does not add model with needed information");
            Assert.AreNotEqual(model.Id, 0, "Method does not set id to the model");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidReleases.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidReleases.First();

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
            var model = _invalidForUpdateReleases.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateRelease;
            var expectedName = model.Name;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Releases.SingleAsync(r => r.Id == model.Id);
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
        }

        [Test]
        public void GetProjectReleasesTest_ReturnsRealProjectReleases()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            IEnumerable<int> expectedReleasesIds = new[] { 4, 2, 1 };

            // Act
            var actualReleases = _service.GetProjectReleases(projectId);

            // Assert
            Assert.AreEqual(expectedReleasesIds.Count(), actualReleases.Count(), "Method returnes wrong elements");
            var actualReleasesIds = actualReleases.Select(r => r.Id);
            Assert.IsTrue(expectedReleasesIds.SequenceEqual(actualReleasesIds), 
                "Method returnes wrong elements or the order is wrong");
        }

        [Test]
        public void GetProjectReleasesByDateTest_ReturnsRealProjectReleases()
        {
            // Arrange
            SeedData(false);
            var projectId = 1;
            var from = new DateTime(2022, 1, 1);
            var to = new DateTime(2022, 1, 20);
            IEnumerable<int> expectedReleasesIds = new[] { 4, 1 };

            // Act
            var actualReleases = _service.GetProjectReleasesByDate(projectId, from, to);

            // Assert
            Assert.AreEqual(expectedReleasesIds.Count(), actualReleases.Count(), "Method returnes wrong elements");
            var actualReleasesIds = actualReleases.Select(r => r.Id);
            Assert.IsTrue(expectedReleasesIds.SequenceEqual(actualReleasesIds),
                "Method returnes wrong elements or the order is wrong");
        }
    }
}
