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
    public class RefreshTokenServiceTests
    {
        private readonly IMapper _mapper = UnitTestHelper.CreateMapperProfile();

        private ApplicationDbContext _context = null!;

        private IRefreshTokenService _service = null!;

        private readonly IEnumerable<RefreshTokenModel> _invalidTokens = new RefreshTokenModel[]
        {
            new RefreshTokenModel           // index 0
            {
                Token = "sAmple1",
                UserId = 0
            },
            new RefreshTokenModel           // index 1
            {
                Token = "sAmplf%^$#fs32",
                UserId = -1
            },
            new RefreshTokenModel           // index 2
            {
                Token = "sAmplf%^$#fs32",
                UserId = 7
            },
            new RefreshTokenModel           // index 3
            {
                Token = string.Empty,
                UserId = 7
            },
            new RefreshTokenModel           // index 4
            {
                Token = null!,
                UserId = 7
            }
        };

        private readonly IEnumerable<RefreshTokenModel> _validTokens = new RefreshTokenModel[]
        {
            new RefreshTokenModel           // index 0
            {
                Token = "sAmple1",
                UserId = 1
            },
            new RefreshTokenModel           // index 1
            {
                Token = "sAmplf%^$#fs32",
                UserId = 2
            }
        };


        [SetUp]
        public void Setup()
        {
            _context = new ApplicationDbContext(UnitTestHelper.GetUnitTestDbOptions());
            SeedData();
            _service = new RefreshTokenService(_context, _mapper);
        }

        private void SeedData()
        {
            var tokens = new RefreshToken[]
            {
                new RefreshToken        // id 1
                {
                    Token = "ToKeN111",
                    UserId = 1
                },
                new RefreshToken        // id 2
                {
                    Token = "ToKeN221",
                    UserId = 3
                },
                new RefreshToken        // id 3
                {
                    Token = "ToKeN411",
                    UserId = 4
                },
                new RefreshToken        // id 4
                {
                    Token = "ToKeN222",
                    UserId = 3
                }
            };
            _context.RefreshTokens.AddRange(tokens);
            _context.SaveChanges();
        }

        private readonly IEnumerable<RefreshTokenModel> _invalidForUpdateTokens = new RefreshTokenModel[]
        {
            new RefreshTokenModel        // id 3
            {
                Id = 3,
                Token = "ToKeN411",
                UserId = 3       // changed
            },
            new RefreshTokenModel        // id 4
            {
                Id = 4,
                Token = "ToKeN222",
                UserId = 3,
                ExpiryTime = DateTime.Now.AddDays(7)     // changed
            }
        };

        private readonly RefreshTokenModel _validForUpdateToken = new()      // id 1
        {
            Id = 1,
            Token = "NewToken111",              // changed
            UserId = 1
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public async Task AddAsyncTest_ValidModel_AddsToDb(int index)
        {
            // Arrange
            var model = _validTokens.ElementAt(index);
            var expectedToken = model.Token;
            var expectedUserId = model.UserId;
            var expectedCount = _context.RefreshTokens.Count() + 1;

            // Act
            await _service.AddAsync(model);

            // Assert
            var actualCount = _context.RefreshTokens.Count();
            var actual = _context.RefreshTokens.Last();
            Assert.AreEqual(expectedCount, actualCount, "Method does not add a model to DB");
            Assert.AreEqual(expectedToken, actual.Token, "Method does not add model with needed information (token)");
            Assert.AreEqual(expectedUserId, actual.UserId, "Method does not add model with needed information (user Id)");
            Assert.AreNotEqual(model.Id, 0, "Method does not set id to the model");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AddAsyncTest_InvalidModel_ThrowsArgumentException(int index)
        {
            // Arrange
            var model = _invalidTokens.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.AddAsync(model));
        }

        [Test]
        public async Task FindAsyncTest_ReturnsCorrectElement()
        {
            // Arrange
            var element = _context.RefreshTokens.First();
            var expectedId = element.Id;
            var expectedTime = element.ExpiryTime;

            // Act
            var actual = await _service.FindAsync(element.Token, element.UserId);

            // Assert
            Assert.AreNotEqual(null, actual);
            Assert.AreEqual(expectedId, actual!.Id, "Method returned a wrong element (different ids)");
            Assert.AreEqual(expectedTime, actual!.ExpiryTime, "Method returned a wrong element (different expiry times)");
        }

        [Test]
        [TestCase("ToKeN111", 0)]
        [TestCase("THISTOKENDOESNOTEXIST", 1)]
        [TestCase("THISTOKENDOESNOTEXIST", 7)]
        public async Task FindAsyncTest_ReturnsNull(string token, int userId)
        {
            // Arrange
            RefreshTokenModel? expected = null;

            // Act
            var actual = await _service.FindAsync(token, userId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        public void DeleteByIdAsync_InvalidId_ThrowsInvalidOperationException(int id)
        {
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
            var expectedCount = _context.RefreshTokens.Count() - 1;

            // Act
            await _service.DeleteByIdAsync(id);

            // Assert
            var actualCount = _context.RefreshTokens.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete element");
            Assert.IsFalse(_context.RefreshTokens.Any(t => t.Id == id), "Method deletes wrong element");
        }

        [Test]
        public async Task DeleteUserTokensAsyncTest_DeletesAllElements()
        {
            // Arrange
            var userId = 3;
            var expectedCount = _context.RefreshTokens.Count() - 2;

            // Act
            await _service.DeleteUserTokensAsync(userId);

            // Assert
            var actualCount = _context.RefreshTokens.Count();
            Assert.AreEqual(expectedCount, actualCount, "Method does not delete all elements");
            Assert.IsFalse(_context.RefreshTokens.Any(t => t.UserId == userId), "Method deletes wrong elements");
        }

        [Test]
        public void UpdateAsyncTest_InvalidModel_ThrowsArgumentException()
        {
            // Arrange
            SeedData();
            var model = _invalidTokens.First();

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
            var model = _invalidForUpdateTokens.ElementAt(index);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsyncTest_ValidModel_UpdatesModel()
        {
            // Arrange
            SeedData();
            var model = _validForUpdateToken;
            var expectedToken = model.Token;

            // Act
            await _service.UpdateAsync(model);

            // Assert
            var actual = await _context.RefreshTokens.SingleAsync(r => r.Id == model.Id);
            Assert.AreEqual(expectedToken, actual.Token, "Method does not update model");
        }

    }
}
