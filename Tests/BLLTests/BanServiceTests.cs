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
    public class BanServiceTests
    {
        private IMapper _mapper = null!;

        private ApplicationDbContext _context = null!;

        private IBanService _service = null!;

        private readonly IEnumerable<BanModel> _invalidBans = new BanModel[]
        {
            new BanModel()  // 0
            {
                AdminId = 1,
                UserId = 2,
                Hammer = "Lorem ipsum",
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now.AddMonths(-1)
            },
            new BanModel()  // 1
                {
                    AdminId = 1,
                    UserId = 4,
                    Hammer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                    "Suspendisse porttitor fermentum porta. Proin sagittis, lacus id sollicitudin vehicula, " +
                    "velit augue sagittis lacus, eget lobortis ipsum ipsum in tortor. Quisque nunc nulla, " +
                    "pulvinar ut aliquam sed, vestibulum nec nisl. Nunc nec lacus id sapien posuere malesuada. " +
                    "Donec a lorem non tortor sollicitudin lobortis. Morbi et consectetur lacus augue. " +
                "More than 400 bytes (characters)!",
                    DateFrom = DateTime.Now,
                    DateTo =  DateTime.Now.AddMonths(1)
                },
            new BanModel()  // 2
            {
                AdminId = 1,
                Hammer = "Lorem ipsum",
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now.AddMonths(1)
            },
            new BanModel()  // 3
            {
                AdminId = -1,
                UserId = 2,
                Hammer = "Lorem ipsum",
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now.AddMonths(1)
            }
        };

        private readonly BanModel _validBan = new()
        {
            AdminId = 1,
            UserId = 2,
            Hammer = "Lorem ipsum",
            DateFrom = DateTime.Now,
            DateTo = DateTime.Now.AddMonths(1)
        };

        [SetUp]
        public void Setup()
        {
            _mapper = UnitTestHelper.CreateMapperProfile();
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            _service = new BanService(_context, _mapper);
        }

        private void SeedData()
        {
            var bans = new Ban[]
            {
                new Ban()   // id 1
                {
                    AdminId = 1,
                    UserId = 2,
                    Hammer = "Lorem ipsum",
                    DateFrom = DateTime.Now.AddMonths(-1),
                    DateTo = DateTime.Now.AddMonths(2)
                },
                new Ban()   // id 2
                {
                    AdminId = 1,
                    UserId = 3,
                    DateFrom = new DateTime(2022, 1, 10),
                    DateTo = new DateTime(2022, 2, 25)
                },
                new Ban()   // id 3
                {
                    AdminId = 1,
                    UserId = 4,
                    Hammer = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                    "Suspendisse porttitor fermentum porta. Proin sagittis, lacus id sollicitudin vehicula, " +
                    "velit augue sagittis lacus, eget lobortis ipsum ipsum in tortor. Quisque nunc nulla, " +
                    "pulvinar ut aliquam sed, vestibulum nec nisl. Nunc nec lacus id sapien posuere malesuada. " +
                    "Donec a lorem non tortor sollicitudin lobortis. Morbi et consectetur lacus augue.",
                    DateFrom = new DateTime(2022, 3, 8, 1, 0, 0),
                    DateTo = new DateTime(2022, 3, 8, 3, 0, 0)
                },
                new Ban()   // id 4
                {
                    AdminId = 5,
                    UserId = 2,
                    Hammer = "Lorem ipsum 2",
                    DateFrom = DateTime.Now.AddMonths(-1),
                    DateTo = DateTime.Now.AddMonths(3)
                },
                new Ban()   // id 4
                {
                    AdminId = 5,
                    UserId = 2,
                    Hammer = "Lorem ipsum 3",
                    DateFrom = DateTime.Now.AddMonths(-3),
                    DateTo = DateTime.Now.AddMonths(-2)
                }
            };
            foreach (var ban in bans)
            {
                _context.Bans.Add(ban);
                _context.SaveChanges();
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException(int modelNumber)
        {
            // Arrange
            var model = _invalidBans.ElementAt(modelNumber);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model),
                "Method does not throw an ArgumentException if model is invalid");
        }

        [Test]
        public async Task AddAsyncTest_ValidModel_AddsModel()
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Bans.Count() + 1;

            // Act
            await _service.AddAsync(_validBan);
            var actualCount = _context.Bans.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount, "Method does not adds an element");
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
        [TestCase(2)]
        [TestCase(3)]
        public async Task DeleteByIdAsync_ValidId_DeletesElement(int id)
        {
            // Arrange
            SeedData();
            var expectedCount = _context.Bans.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);
            var actualCount = _context.Bans.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.Bans.Any(b => b.Id == id), "Method deletes wrong element");
        }

        [Test]
        public async Task DeleteActiveUserBansAsync_DeletesAllBans()
        {
            // Arrange
            SeedData();
            var userId = 2;
            var expectedCount = _context.Bans.Count() - 2;

            // Act
            await _service.DeleteActiveUserBansAsync(userId);
            var actualCount = _context.Bans.Count();

            // Assert
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete all user bans");
            Assert.IsFalse(_context.Bans.Any(b => b.UserId == userId && b.DateTo >= DateTime.Now && b.DateFrom <= DateTime.Now), "Method deletes wrong bans");
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
        [TestCase(3)]
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
        public void GetUserActiveBans_ReturnsAllBans()
        {
            // Arrange
            SeedData();
            var userId = 2;
            var expectedCount = 2;

            // Act
            var actual = _service.GetActiveUserBans(userId);

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method does not return right quantity");
            Assert.IsTrue(actual.All(b => b.UserId == userId && b.DateTo >= DateTime.Now && b.DateFrom <= DateTime.Now), "Method returns wrong bans");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task UpdateAsyncTest_InvalidModel_ThrowsArgumentException(int modelNumber)
        {
            // Arrange
            SeedData();
            var model = _invalidBans.ElementAt(modelNumber);
            var toUpdate = await _service.GetByIdAsync(1);
            toUpdate.AdminId = model.AdminId;
            toUpdate.UserId = model.UserId;
            toUpdate.DateFrom = DateTime.Now;
            toUpdate.DateTo = model.DateTo;
            toUpdate.Hammer = model.Hammer;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(toUpdate),
                "Method does not throw an ArgumentException if model is invalid");
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void UpdateAsyncTest_NotExistingModel_ThrowsInvalidOperationException(int id)
        {
            // Arrange
            SeedData();
            var model = _validBan;
            var toUpdate = new BanModel()
            {
                Id = id,
                AdminId = model.AdminId,
                UserId = model.UserId,
                DateFrom = DateTime.Now,
                DateTo = model.DateTo,
                Hammer = model.Hammer
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(toUpdate),
                "Method does not throw an InvalidOperationException if model does not exist");
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var expected = _validBan;
            var id = 2;
            var toUpdate = await _context.Bans.SingleAsync(b => b.Id == id);
            toUpdate.AdminId = expected.AdminId;
            toUpdate.UserId = expected.UserId;
            toUpdate.DateFrom = DateTime.Now;
            toUpdate.DateTo = expected.DateTo;
            toUpdate.Hammer = expected.Hammer;

            // Act
            await _service.UpdateAsync(_mapper.Map<BanModel>(toUpdate));

            // Assert
            var actual = await _context.Bans.SingleAsync(b => b.Id == id);
            Assert.AreEqual(expected.AdminId, actual.AdminId, "Method does not change model's AdminId");
            Assert.AreEqual(expected.UserId, actual.UserId, "Method does not change model's UserId");
            Assert.AreEqual(expected.DateTo, actual.DateTo, "Method does not change model's DateTo");
            Assert.AreEqual(expected.Hammer, actual.Hammer, "Method does not change model's Hammer");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IsValidTest_InvalidModel_ReturnsFalseWithError(int modelNumber)
        {
            // Arrange
            var model = _invalidBans.ElementAt(modelNumber);

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsFalse(actual, "Method does not return false if model is invalid");
            Assert.IsFalse(string.IsNullOrEmpty(error), "Method does not write proper error message");
        }

        [Test]
        public void IsValidTest_InvalidModel_ReturnsTrue()
        {
            // Arrange
            var model = _validBan;

            // Act
            var actual = _service.IsValid(model, out string? error);

            // Assert
            Assert.IsTrue(actual, "Method does not return true if model is valid");
            Assert.IsTrue(string.IsNullOrEmpty(error), "Error should be left null if model is valid");
        }

        [Test]
        public void GetAdminBans_ReturnsAllBans()
        {
            // Arrange
            SeedData();
            var adminId = 1;
            var expectedCount = 3;

            // Act
            var actual = _service.GetAdminBans(adminId);

            // Assert
            Assert.AreEqual(expectedCount, actual.Count(), "Method does not return right quantity");
            Assert.IsTrue(actual.All(b => b.AdminId == adminId), "Method returns wrong bans");
        }
    }
}
