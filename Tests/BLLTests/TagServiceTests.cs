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
    public class TagServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private ITagService _service = null!;

        private readonly IEnumerable<TagModel> _invalidTags = new TagModel[]
        {
            new TagModel()      // 0
            {
                ProjectId = 1
            },
            new TagModel()      // 1
            {
                Name = "TooLongNameMoreThan20characters",
                ProjectId = 1
            },
            new TagModel()      // 2
            {
                Name = "Development",
                ProjectId = -1
            }
        };

        private readonly IEnumerable<TagModel> _validTags = new TagModel[]
        {
            new TagModel()      // 0
            {
                Name = "Development",
                ProjectId = 1
            }
        };

        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedRequiredData();
            _service = new TagService(_context, _mapper);
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
                    ProjectId = 1
                },
                new Tag()       // id 2
                {
                    Name = "Testing",
                    ProjectId = 2
                },
                new Tag()       // id 3
                {
                    Name = "Programming",
                    ProjectId = 1
                },
                new Tag()       // id 4
                {
                    Name = "Hotfix",
                    ProjectId = 1
                }
            };
            foreach(var tag in tags)
            {
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }

            var tasks = new Data.Entities.Task[]
            {
                new Data.Entities.Task()
                {
                    Name = "Task 1",
                    ProjectId = 1,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 1),
                        _context.Tags.Single(t => t.Id == 3)
                    }
                },
                new Data.Entities.Task()
                {
                    Name = "Task 1",
                    ProjectId = 2,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 2)
                    }
                },
                new Data.Entities.Task()
                {
                    Name = "Task 2",
                    ProjectId = 1,
                    Tags = new List<Tag>()
                    {
                        _context.Tags.Single(t => t.Id == 1)
                    }
                }
            };
            foreach(var task in tasks)
            {
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }
        }

        private readonly IEnumerable<TagModel> _invalidForUpdateTags = new TagModel[]
        {
            new TagModel()       // id 1, ind 0
                {
                Id = 1,
                    Name = "Automatisation",
                    ProjectId = 2       // changed
                }
        };

        private readonly TagModel _validForUpdateTag = new()        // id 1
        {
            Id = 1,
            Name = "Engineering",       // changed
            ProjectId = 1
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidTags.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsFalse(actual, "Method does not return false if model is invalid");
            Assert.IsFalse(string.IsNullOrEmpty(error), "Method does not write proper error message");
        }

        [Test]
        [TestCase(0)]
        public void IsValidTest_ValidModel_ReturnsTrue(int modelNumber)
        {
            // Arrange
            var model = _validTags.ElementAt(modelNumber);

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
        [TestCase(3)]
        [TestCase(4)]
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
        [TestCase(2)]
        [TestCase(4)]
        public async Task DeleteByIdAsync_ValidId_DeletesElement(int id)
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Tags.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.Tags.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Tags.Any(m => m.Id == id), "Method deletes wrong element");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsToDb()
        {
            // Arrange
            SeedData();
            var model = _validTags.First();
            var expectedName = model.Name;
            var expectedCount = _context.Tags.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.Tags.Count();
            var actual = _context.Tags.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedName, actual.Name, "Method does not add model with needed information");
        }

        [Test]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidTags.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidTags.First();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        [TestCase(0)]
        public void UpdateAsyncTest_InvalidForUpdateOnlyModel_ThrowsArgumentException(int index)
        {
            // Arrange
            SeedData();
            var model = _invalidForUpdateTags.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateTag;
            var expectedName = model.Name;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.Tags.SingleAsync(r => r.Id == model.Id);
            Assert.AreEqual(expectedName, actual.Name, "Method does not update model");
        }

        [Test]
        public void GetProjectTagsTest_ReturnsRealProjectTags()
        {
            // Arrange
            SeedData();
            var projectId = 1;
            IEnumerable<int> expectedTagsIds = new[] { 1, 3, 4 };

            // Act
            var actualTags = _service.GetProjectTags(projectId);

            // Assert
            Assert.AreEqual(expectedTagsIds.Count(), actualTags.Count(), "Method returnes wrong elements");
            var actualTagsIds = actualTags.Select(r => r.Id);
            Assert.IsTrue(expectedTagsIds.SequenceEqual(actualTagsIds),
                "Method returnes wrong elements or the order is wrong");
        }

        [Test]
        public async Task GetTaskTagsAsyncTest_ReturnsRealTaskTags()
        {
            // Arrange
            SeedData();
            var taskId = 1;
            IEnumerable<int> expectedTagsIds = new[] { 1, 3 };

            // Act
            var actualTags = await _service.GetTaskTagsAsync(taskId);

            // Assert
            Assert.AreEqual(expectedTagsIds.Count(), actualTags.Count(), "Method returnes wrong elements");
            var actualTagsIds = actualTags.Select(r => r.Id);
            Assert.IsTrue(expectedTagsIds.SequenceEqual(actualTagsIds),
                "Method returnes wrong elements or the order is wrong");
        }
    }
}
